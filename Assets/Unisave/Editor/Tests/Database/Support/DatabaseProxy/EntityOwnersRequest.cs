using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LightJson;
using LightJson.Serialization;
using Parrot;

namespace Unisave.Editor.Tests.Database.Support.DatabaseProxy
{
    /// <summary>
    /// Represents a request for entity owners
    /// </summary>
    public class EntityOwnersRequest : IEnumerable<string>
    {
        private readonly string entityId;
        private readonly Client client;
        
        public EntityOwnersRequest(string entityId, Client client)
        {
            this.entityId = entityId;
            this.client = client;
        }
       
        /// <summary>
        /// Executes the actual request and returns a cursor
        /// </summary>
        public IEnumerator<string> GetEnumerator()
        {
            // start iteration
            client.SendTextMessage(205, new JsonObject()
                .Add("entity_id", entityId)
                .ToString()
            );
            
            // receive cursor ID and the first batch
            var response = JsonReader.Parse(
                client.ReceiveTextMessageType(206)
            );
            
            return new EntityOwnersCursor(client, response);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class EntityOwnersCursor : IEnumerator<string>
    {
        List<string> currentBatch = new List<string>();

        private int batchPosition = -1;

        private bool isFinalBatch;

        private bool cursorClosed;

        private string cursorId;

        private Client client;

        public string Current => currentBatch[batchPosition];

        object IEnumerator.Current => Current;
        
        public EntityOwnersCursor(Client client, JsonObject response)
        {
            this.client = client;
            
            HandleResponse(response);
        }

        private void HandleResponse(JsonObject response)
        {
            // load cursor ID (first response)
            if (cursorId == null && response.ContainsKey("cursor_id"))
                cursorId = response["cursor_id"].AsString;
            
            // load items
            currentBatch.Clear();
            currentBatch.AddRange(
                response["items"].AsJsonArray.Select(x => x.AsString)
            );
            batchPosition = -1;
            
            // is final?
            isFinalBatch = response["is_final"].AsBoolean;

            // close cursor if final
            if (isFinalBatch)
                cursorClosed = true;
        }
        
        public bool MoveNext()
        {
            // get next batch
            // (last returned element was the last element in batch)
            if (batchPosition >= currentBatch.Count - 1)
            {
                if (isFinalBatch)
                {
                    Dispose();
                    return false;
                }

                PullNextBatch();
                return MoveNext();
            }

            batchPosition++;
            return true;
        }

        private void PullNextBatch()
        {
            if (cursorClosed)
                throw new ObjectDisposedException(
                    "Cursor for enumerating entity owners has been disposed."
                );
            
            // request next batch
            client.SendTextMessage(207, new JsonObject()
                .Add("cursor_id", cursorId)
                .ToString()
            );
            
            var response = JsonReader.Parse(
                client.ReceiveTextMessageType(206)
            );

            HandleResponse(response);
        }

        public void Reset()
        {
            throw new InvalidOperationException();
        }

        public void Dispose()
        {
            if (cursorClosed)
                return;
            
            // close cursor
            client.SendTextMessage(208, new JsonObject()
                .Add("cursor_id", cursorId)
                .ToString()
            );

            cursorClosed = true;
        }
    }
}