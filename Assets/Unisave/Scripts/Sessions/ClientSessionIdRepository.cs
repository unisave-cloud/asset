using System;
using LightJson;
using LightJson.Serialization;
using Unisave.Serialization;
using UnityEngine;

namespace Unisave.Sessions
{
    /// <summary>
    /// Stores session id that is used for connecting to the server
    /// </summary>
    public class ClientSessionIdRepository
    {
        /// <summary>
        /// In how many minutes the session id expires
        /// </summary>
        private const double ExpiryMinutes = 30;

        private const string PlayerPrefsKey = "Unisave.SessionId";
        
        private string id;
        private bool loaded;
        
        /// <summary>
        /// Returns the stored session id
        /// </summary>
        public string GetSessionId()
        {
            if (!loaded)
                LoadSessionId();

            return id;
        }

        private void LoadSessionId()
        {
            var raw = PlayerPrefs.GetString(PlayerPrefsKey);
            JsonObject json = null;

            if (string.IsNullOrWhiteSpace(raw))
                raw = "{}";

            try
            {
                json = JsonReader.Parse(raw);
            }
            catch (JsonParseException e)
            {
                Debug.LogException(e);
            }
            
            if (json == null)
                json = new JsonObject();
            
            // check expiry
            var storedAt = Serializer.FromJson<DateTime>(json["StoredAt"]);
            if ((DateTime.UtcNow - storedAt).TotalMinutes > ExpiryMinutes)
            {
                id = null;
                loaded = true;
                return;
            }

            id = json["SessionId"].AsString;
            loaded = true;
        }
        
        /// <summary>
        /// Sets the session ID to be remembered
        /// </summary>
        public void StoreSessionId(string sessionId)
        {
            PlayerPrefs.SetString(PlayerPrefsKey, new JsonObject()
                .Add("StoredAt", Serializer.ToJson(DateTime.UtcNow))
                .Add("SessionId", sessionId)
                .ToString()
            );
            PlayerPrefs.Save();

            id = sessionId;
            loaded = true;
        }
    }
}