using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Linq;
using UnityEditor;
using UnityEngine;
using LightJson;

namespace Unisave.CodeUploader
{
    /// <summary>
    /// Uploads backend code to the server
    /// 
    /// Simple implementation (prototyping ;)), obtains backend folder path
    /// from preferences, traverses this folder for .cs files
    /// then uploads them one by one to the server
    /// 
    /// (no change checking, or caching so far)
    /// </summary>
    public class Uploader
    {
        private UnisavePreferences preferences;

        private string editorKey;

        public static Uploader CreateDefaultInstance()
        {
            return new Uploader(
                UnisavePreferences.LoadOrCreate(),
                EditorPrefs.GetString("unisave.editorKey", null)
            );
        }

        public Uploader(UnisavePreferences preferences, string editorKey)
        {
            this.preferences = preferences;
            this.editorKey = editorKey;
        }

        public void Run()
        {
            /*
                Prototype implementation, don't curse me
             */

            var files = new List<string>();
            TraverseFolder(files, "Assets/" + preferences.BackendFolder);

            // HACK: filter out only facets
            //files = files.Where(f => f.Contains("Facet")).ToList();

            // do the actual upload in the background
            new Thread(() => {
                Thread.CurrentThread.IsBackground = true; 
                
                foreach (string path in files)
                    UploadScript(path, File.ReadAllText(path));
            }).Start();
        }

        private void TraverseFolder(List<string> files, string path)
        {
            // branch on each subdirectory
            foreach (string dirPath in Directory.GetDirectories(path))
            {
                string dir = Path.GetFileName(dirPath);
                TraverseFolder(files, dirPath);
            }

            // select .cs files
            foreach (string filePath in Directory.GetFiles(path))
            {
                if (Path.GetExtension(filePath) == ".cs")
                {
                    files.Add(filePath);
                }
            }
        }

        private void UploadScript(string path, string code)
        {
            if (code == null)
            {
                Debug.LogError("Unisave: UploadScript called, but file does not exist.");
                return;
            }

            string payloadString = new JsonObject()
				.Add("gameToken", preferences.GameToken)
				.Add("editorKey", editorKey)
                .Add("scriptPath", path)
				.Add("scriptCode", code)
				.ToString();

            byte[] contentBytes = new UTF8Encoding().GetBytes(payloadString);

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(
                new Unisave.Utils.ApiUrl(preferences.ServerUrl).UploadScript()
            );
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = contentBytes.LongLength;
            request.GetRequestStream().Write(contentBytes, 0, contentBytes.Length);
    
            try
            {
                using(HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Debug.Log("Unisave: Uploaded file " + path);
                    //Debug.Log("Publish Response: " + (int)response.StatusCode + ", " + response.StatusDescription);
                    
                    if ((int)response.StatusCode == 200)
                    {
                        // ...
                    }

                    // PRINT RESPONSE:
                    //Debug.Log(new StreamReader(response.GetResponseStream()).ReadToEnd());
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }
    }
}
