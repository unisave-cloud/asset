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
        private string absoluteEndpointUrl;

        public ApiUrl(string absoluteEndpointUrl)
        {
            this.absoluteEndpointUrl = absoluteEndpointUrl;

            if (!this.absoluteEndpointUrl.EndsWith("/"))
                this.absoluteEndpointUrl += "/";
        }

        /// <summary>
        /// Create url from a relative url (do not start relative url with slash)
        /// </summary>
        public string Url(string relativeUrl)
        {
            return absoluteEndpointUrl + relativeUrl;
        }

        public string Login() => Url("login");

        public string Logout() => Url("logout");

        public string Register() => Url("register");

        public string CallFacet() => Url("call-facet");
    }
}
