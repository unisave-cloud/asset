using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unisave.Utils
{
    /// <summary>
    /// Helper for getting unisave api urls
    /// </summary>
    public class ApiUrl
    {
        private string serverUrl;

        public ApiUrl(string serverUrl)
        {
            this.serverUrl = serverUrl;

            if (!this.serverUrl.EndsWith("/"))
                this.serverUrl += "/";
        }

        private string GameApiUrl(string relativeUrl)
        {
            return serverUrl + "api/game/v1.0/" + relativeUrl;
        }

        private string EditorApiUrl(string relativeUrl)
        {
            return serverUrl + "api/editor/v1.0/" + relativeUrl;
        }

        public string Register() => GameApiUrl("register");
        public string Login() => GameApiUrl("login");
        public string Logout() => GameApiUrl("logout");
        
        public string CallFacet() => GameApiUrl("call-facet");
        
        public string StartScriptUpload() => EditorApiUrl("upload-script/start");
        public string UploadScript() => EditorApiUrl("upload-script");
        public string FinishScriptUpload() => EditorApiUrl("upload-script/finish");
    }
}
