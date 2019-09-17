using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using LightJson;
using LightJson.Serialization;
using Unisave.Exceptions;
using Unisave.Utils;
using Unisave.Serialization;

namespace Unisave.Editor.BackendUploading
{
    /// <summary>
    /// Uploads backend folder to the server
    /// </summary>
    public class Uploader
    {
        private readonly UnisavePreferences preferences;

        private readonly ApiUrl apiUrl;

        /// <summary>
        /// Creates the default instance of the uploader
        /// that uses correct preferences.
        /// </summary>
        public static Uploader GetDefaultInstance()
        {
            // NOTE: There's not an easy way to keep the value.
            // Static field gets reset when the game is started.
            // It would have to use editor preferences which is an overkill.
            
            return new Uploader(
                UnisavePreferences.LoadOrCreate()
            );
        }

        private Uploader(UnisavePreferences preferences)
        {
            this.preferences = preferences;

            apiUrl = new ApiUrl(preferences.ServerUrl);
        }

        /// <summary>
        /// This method gets called after recompilation by the hook.
        /// It triggers the upload if preferences are set up that way.
        /// </summary>
        public void RunAutomaticUpload()
        {
            if (!preferences.AutomaticCodeUploading)
                return;

            NewRun();
        }

        /// <summary>
        /// Performs all the uploading
        /// </summary>
        public void NewRun()
        {
            var backendFolders = new string[] {
                "Assets/" + preferences.BackendFolder
            };

            // get list of files to be uploaded
            var files = new List<BackendFile>();
            files.AddRange(CSharpFile.FindFiles(backendFolders));
            files.AddRange(SOFile.FindFiles(backendFolders));

            // HERE START BACKGROUND THREAD

            // compute file hashes
            files.ForEach(f => f.ComputeHash());
            
            // compute backend hash
            string backendHash = Unisave.Editor.Hash.CompositeMD5(
                files.Select(f => f.Hash)
            );
            
            Debug.Log(backendHash);
            Debug.Log(string.Join("\n", files.Select(x => x.ToString())));

            // send all file paths, hashes and global hash to the server
            // and initiate the upload

            // send individual files the server has asked for,
            // access file.ContentForUpload()

            // finish the upload and print result of the compilation
        }

        /// <summary>
        /// Performs all the uploading
        /// </summary>
        public void Run()
        {
            // get the list of all files to upload
            var files = new List<string>();
            TraverseFolder(files, "Assets/" + preferences.BackendFolder);

            // compute hashes for those files
            Dictionary<string, string> hashes = ComputeHashesForFiles(files);

            // compute global hash
            string globalHash = ComputeGlobalHash(files, hashes);

            // FUTURE FEATURE?
            // save global hash in unisave preferences and upload it on login / registration
            // to replace the role of buildGUID? Maybe.

            // upload hashes to server and see what needs to be uploaded
            JsonObject uploadStartResponse = HttpPostRequest(
                apiUrl.StartScriptUpload(),
                new JsonObject()
                    .Add("gameToken", preferences.GameToken)
				    .Add("editorKey", preferences.EditorKey)
                    .Add("assetVersion", UnisaveServer.AssetVersion)
                    .Add("files", Serializer.ToJson(files))
                    .Add("hashes", Serializer.ToJson(hashes))
                    .Add("globalHash", globalHash)
            );

            // upload requested files
            var filesToUpload = Serializer.FromJson<string[]>(uploadStartResponse["filesToUpload"]);

            foreach (string file in filesToUpload)
            {
                // NOTE: read bytes not text to make sure line endings match and hash matches the server one
                // (ReadAllText does some fancy line ending conversions)
                string code = Encoding.UTF8.GetString(File.ReadAllBytes(file));

                JsonObject uploadResponse = HttpPostRequest(
                    apiUrl.UploadScript(),
                    new JsonObject()
                        .Add("gameToken", preferences.GameToken)
				        .Add("editorKey", preferences.EditorKey)
                        .Add("scriptPath", file)
				        .Add("scriptCode", code)
                );

                if (uploadResponse["code"].AsString == "ok")
                {
                    Debug.Log("Unisave uploaded file: " + file);
                }
                else
                {
                    Debug.LogError("Unisave file upload failed: " + file);
                }
            }

            // finish upload (let the server compile all scripts)
            JsonObject uploadFinishResponse = HttpPostRequest(
                apiUrl.FinishScriptUpload(),
                new JsonObject()
                    .Add("gameToken", preferences.GameToken)
                    .Add("editorKey", preferences.EditorKey)
            );

            if (!uploadFinishResponse["compilationSucceeded"].AsBoolean)
            {
                Debug.LogError("Server compile error:\n" + uploadFinishResponse["compilationMessage"]);
            }

            // save upload time
            preferences.LastCodeUploadAt = DateTime.Now;
        }

        /// <summary>
        /// Searches for files to upload
        /// </summary>
        private void TraverseFolder(List<string> files, string path)
        {
            // NOTE: keep everything ordered alphabetically, because it affects the global hash

            if (!Directory.Exists(path))
                throw new UnisaveException(
                    $"Backend folder '{path}' does not exist.\n" +
                    "Check your Unisave preferences for the 'Backend assets folder' field. Make sure it's set up properly."
                );

            // branch on each subdirectory
            foreach (string dirPath in Directory.GetDirectories(path).OrderBy(f => f))
            {
                string dir = Path.GetFileName(dirPath);
                TraverseFolder(files, dirPath);
            }

            // select .cs files
            foreach (string filePath in Directory.GetFiles(path).OrderBy(f => f))
            {
                if (Path.GetExtension(filePath) == ".cs")
                {
                    files.Add(filePath);
                }
            }
        }

        /// <summary>
        /// Computes hashes for all files that are going to be uploaded
        /// </summary>
        private Dictionary<string, string> ComputeHashesForFiles(List<string> files)
        {
            Dictionary<string, string> hashes = new Dictionary<string, string>();
            
            foreach (var file in files)
                hashes[file] = ComputeHash(file);
                
            return hashes;
        }

        /// <summary>
        /// Computes MD5 hash of a file
        /// </summary>
        private string ComputeHash(string file)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(file))
                {
                    byte[] hash = md5.ComputeHash(stream);

                    return Convert.ToBase64String(hash);
                }
            }
        }

        /// <summary>
        /// Computes global hash over all files
        /// </summary>
        private string ComputeGlobalHash(List<string> files, Dictionary<string, string> hashes)
        {
            StringBuilder hashConcatenation = new StringBuilder();
            
            foreach (string file in files)
                hashConcatenation.Append(hashes[file]);

            using (var md5 = MD5.Create())
            {
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(hashConcatenation.ToString())))
                {
                    byte[] hash = md5.ComputeHash(stream);
                    return Convert.ToBase64String(hash);
                }
            }
        }

        /// <summary>
        /// Blocking HTTP request via .NET framework
        /// </summary>
        private JsonObject HttpPostRequest(string url, JsonObject payload)
        {
            byte[] payloadBytes = new UTF8Encoding().GetBytes(payload.ToString());

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Accept = "application/json";
            request.ContentLength = payloadBytes.LongLength;
            request.GetRequestStream().Write(payloadBytes, 0, payloadBytes.Length);

            string responseString = null;

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (var sr = new StreamReader(response.GetResponseStream()))
                        responseString = sr.ReadToEnd();

                    if ((int)response.StatusCode != 200)
                        throw new UnisaveException(
                            "Server responded with non 200 response:\n" +
                            responseString
                        );
                    
                    return JsonReader.Parse(responseString);
                }
            }
            catch (Exception e)
            {
                if (e is WebException)
                {
                    if ((int)((HttpWebResponse)((WebException)e).Response).StatusCode == 401)
                        throw new UnisaveException(
                            "Server response to code uploader was 401 unauthorized.\n"
                            + "Check that your game token and editor key are correctly set up."
                        );

                    if ((int)((HttpWebResponse)((WebException)e).Response).StatusCode == 429)
                        throw new UnisaveException(
                            "Server refuses code uploader requests due to their amount. This happens when Unity editor does a lot of recompiling.\n"
                            + "Simply wait for a while and this problem will go away."
                        );
                }

                Debug.LogError("Server sent:\n" + responseString);
                throw e;
            }
        }
    }
}
