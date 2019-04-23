using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Unisave.CodeUploader
{
    public class Uploader
    {
        public const string FolderName = "UnisaveCloud";

        private List<string> FilesToUpload = new List<string>();

        public void Run()
        {
            TraverseFolder("Assets", false);

            foreach (string i in FilesToUpload)
                Debug.Log(i);
        }

        private void TraverseFolder(string path, bool isCloud)
        {
            foreach (string dirPath in Directory.GetDirectories(path))
            {
                string dir = Path.GetFileName(dirPath);
                TraverseFolder(dirPath, isCloud || dir == FolderName);
            }

            if (!isCloud)
                return;

            foreach (string filePath in Directory.GetFiles(path))
            {
                if (Path.GetExtension(filePath) == ".cs")
                {
                    FilesToUpload.Add(filePath);
                }
            }
        }
    }
}
