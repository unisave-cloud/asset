using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEngine;
using LightJson;

namespace Unisave.CodeUploader
{
    public class Uploader
    {
        public const string FolderName = "UnisaveCloud";

        private List<string> FilesToUpload = new List<string>();

        private UnisavePreferences preferences;

        private string editorKey;

        public static Uploader CreateDefaultInstance()
        {
            return new Uploader(
                UnisavePreferences.LoadPreferences(),
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
            TraverseFolder("Assets", false);

            // do the actual upload in the background
            new Thread(() => {
                Thread.CurrentThread.IsBackground = true; 
                
                foreach (string path in FilesToUpload)
                    UploadScript(path, File.ReadAllText(path));
            }).Start();
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

        private void UploadScript(string path, string code)
        {
            if (code == null)
            {
                Debug.LogError("Unisave: UploadScript called, but file does not exist.");
                return;
            }

            string payloadString = new JsonObject()
				.Add("scriptPath", path)
				.Add("scriptCode", code)
				.Add("gameToken", preferences.gameToken)
				//.Add("buildGUID", Application.buildGUID)
				//.Add("version", Application.version)
				.Add("editorKey", editorKey)
				.ToString();

            byte[] contentBytes = new UTF8Encoding().GetBytes(payloadString);

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(
                new Uri(new Uri(preferences.serverApiUrl), "upload-script")
            );
            request.Method = "PUT";
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
                }
            }
            catch(Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }
    }
}
