using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;
using Mono.CecilX;
using Mono.CecilX.Cil;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityAssembly = UnityEditor.Compilation.Assembly;

namespace Unisave.Weaver
{
    public class Weaver
    {
        public const string UnisaveFrameworkAssemblyName = "Unisave.Framework";
        public const string UnisaveWeaverAssemblyName = "Unisave.Weaver";

        private string assemblyPath;

        private ModuleDefinition CorLibModule;
        private AssemblyDefinition UnisaveFrameworkAssembly;
        private AssemblyDefinition Assembly; // weaved assembly

        // types
        private TypeReference ControllerType;
        private TypeReference VoidType;
        private TypeReference ObjectType;
        private MethodReference Controller_ActionCalled;
        
        public Weaver(string assemblyPath)
        {
            this.assemblyPath = assemblyPath;
        }

        public void Weave()
        {
            string unisaveFrameworkPath = FindUnisaveFramework();

            if (string.IsNullOrEmpty(unisaveFrameworkPath))
            {
                Debug.LogError("Failed to find Unisave Framework assembly");
                return;
            }

            if (!File.Exists(unisaveFrameworkPath))
                return; // not compiled yet

            using (UnisaveFrameworkAssembly = AssemblyDefinition.ReadAssembly(unisaveFrameworkPath))
            using (DefaultAssemblyResolver asmResolver = new DefaultAssemblyResolver())
            using (Assembly = AssemblyDefinition.ReadAssembly(
                assemblyPath, new ReaderParameters {
                    ReadWrite = true,
                    ReadSymbols = true,
                    AssemblyResolver = asmResolver
                }
            ))
            {
                foreach (string path in GetPathsForResolver())
                    asmResolver.AddSearchDirectory(path);

                FindTypes();

                ModuleDefinition module = Assembly.MainModule;
                
                foreach (TypeDefinition td in module.Types)
                {
                    if (td.IsClass && td.BaseType.CanBeResolved())
                    {
                        WeaveTypeDefinition(td);
                    }
                }

                // save weaved assembly
                Assembly.Write(new WriterParameters { WriteSymbols = true });
            }
        }

        private string FindUnisaveFramework()
        {
            foreach (UnityAssembly assembly in CompilationPipeline.GetAssemblies())
                if (assembly.name == UnisaveFrameworkAssemblyName)
                    return assembly.outputPath;
            
            return "";
        }

        private IEnumerable<string> GetPathsForResolver()
        {
            HashSet<string> dependencyPaths = new HashSet<string>();
            dependencyPaths.Add(Path.GetDirectoryName(assemblyPath));
            foreach (UnityAssembly unityAsm in CompilationPipeline.GetAssemblies())
            {
                if (unityAsm.outputPath != assemblyPath)
                    continue;

                foreach (string unityAsmRef in unityAsm.compiledAssemblyReferences)
                    dependencyPaths.Add(Path.GetDirectoryName(unityAsmRef));
            }

            // extra
            dependencyPaths.Add(Path.GetDirectoryName(assemblyPath));

            return dependencyPaths;
        }

        void SetupCorLib()
        {
            AssemblyNameReference name = AssemblyNameReference.Parse("mscorlib");
            ReaderParameters parameters = new ReaderParameters
            {
                AssemblyResolver = Assembly.MainModule.AssemblyResolver,
            };
            CorLibModule = Assembly.MainModule.AssemblyResolver.Resolve(name, parameters).MainModule;
        }

        TypeReference ImportCorLibType(string fullName)
        {
            TypeDefinition type = CorLibModule.GetType(fullName) ?? CorLibModule.ExportedTypes.First(t => t.FullName == fullName).Resolve();
            if (type != null)
            {
                return Assembly.MainModule.ImportReference(type);
            }
            Debug.LogError("Failed to import mscorlib type: " + fullName + " because Resolve failed.");
            return null;
        }

        private void FindTypes()
        {
            SetupCorLib();
            VoidType = ImportCorLibType("System.Void");
            ObjectType = ImportCorLibType("System.Object");
            ControllerType = UnisaveFrameworkAssembly.MainModule.GetType("Unisave.Framework.Controller");
            
            Controller_ActionCalled = Assembly.MainModule.ImportReference(
                ControllerType.Resolve().Methods.Where(x => x.Name == "CallAction").First()
            );
        }

        private void WeaveTypeDefinition(TypeDefinition td)
        {
            // process only controllers
            if (!td.IsDerivedFrom(ControllerType))
                return;

            if (IsControllerProcessed(td))
                return;

            foreach (MethodDefinition md in td.Methods)
            {
                if (md.CustomAttributes.Any(x => x.AttributeType.FullName == "Unisave.ActionAttribute"))
                {
                    ILProcessor proc = md.Body.GetILProcessor();
                    Instruction first = proc.Body.Instructions.First();
                    
                    proc.InsertBefore(first, proc.Create(OpCodes.Nop)); // start
                    proc.InsertBefore(first, proc.Create(OpCodes.Ldarg_0)); // this
                    proc.InsertBefore(first, proc.Create(OpCodes.Ldstr, md.Name)); // actionName

                    // create args array
                    proc.InsertBefore(first, proc.Create(OpCodes.Ldc_I4, md.Parameters.Count)); // size
                    proc.InsertBefore(first, proc.Create(OpCodes.Newarr, ObjectType)); // create empty array
                    
                    // push args one by one
                    int i = 0;
                    foreach (ParameterDefinition pd in md.Parameters)
                    {
                        proc.InsertBefore(first, proc.Create(OpCodes.Dup));
                        proc.InsertBefore(first, proc.Create(OpCodes.Ldc_I4, i));

                        proc.InsertBefore(first, proc.Create(OpCodes.Ldarg, i + 1)); // shifted by "this"

                        // boxing for value types
                        if (pd.ParameterType.IsValueType)
                            proc.InsertBefore(first, proc.Create(OpCodes.Box, pd.ParameterType));

                        proc.InsertBefore(first, proc.Create(OpCodes.Stelem_Ref));

                        i++;
                    }
                    
                    proc.InsertBefore(first, proc.Create(OpCodes.Call, Controller_ActionCalled)); // call

                    // returned value is on the stack, now jump if this value is true:

                    // jump to ret
                    Instruction target = proc.Body.Instructions.Where(x => x.OpCode.Code == Code.Ret).First();
                    proc.InsertBefore(first, proc.Create(OpCodes.Brtrue, target));
                }
            }

            MarkControllerAsProcessed(td);
        }

        private const string ProcessedFunctionName = "UnisaveProcessed";

        private bool IsControllerProcessed(TypeDefinition td)
        {
            return td.Methods.Any(method => method.Name == ProcessedFunctionName);
        }

        private void MarkControllerAsProcessed(TypeDefinition td)
        {
            if (!IsControllerProcessed(td))
            {
                MethodDefinition versionMethod = new MethodDefinition(ProcessedFunctionName, MethodAttributes.Private, VoidType);
                ILProcessor worker = versionMethod.Body.GetILProcessor();
                worker.Append(worker.Create(OpCodes.Ret));
                td.Methods.Add(versionMethod);
            }
        }
    }
}
