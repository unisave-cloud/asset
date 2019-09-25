using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LightJson;
using LightJson.Serialization;
using Parrot;
using Unisave.Database;
using Unisave.Database.Query;

namespace Unisave.Editor.Tests.Database.Support.DatabaseProxy
{
    /// <summary>
    /// Represents an entity query request
    /// </summary>
    public class EntityQueryRequest : IEnumerable<RawEntity>
    {
        private readonly EntityQuery query;
        private readonly Client client;

        public EntityQueryRequest(EntityQuery query, Client client)
        {
            this.query = query;
            this.client = client;
        }
        
        public IEnumerator<RawEntity> GetEnumerator()
        {
            // start iteration
            client.SendTextMessage(213, new JsonObject()
                .Add("query", query.ToJson())
                .ToString()
            );
            
            // receive cursor ID and the first batch
            var response = JsonReader.Parse(
                client.ReceiveTextMessageType(214)
            );
            
            return new EntityQueryCursor(client, response);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class EntityQueryCursor : IEnumerator<RawEntity>
    {
        List<RawEntity> currentBatch = new List<RawEntity>();

        private int batchPosition = -1;

        private bool isFinalBatch;

        private bool cursorClosed;

        private string cursorId;

        private Client client;

        public RawEntity Current => currentBatch[batchPosition];

        object IEnumerator.Current => Current;
        
        public EntityQueryCursor(Client client, JsonObject response)
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
                response["items"].AsJsonArray.Select(
                    x => RawEntity.FromJson(x.AsJsonObject)
                )
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
                    "Cursor for enumerating entity query has been disposed."
                );
            
            // request next batch
            client.SendTextMessage(215, new JsonObject()
                .Add("cursor_id", cursorId)
                .ToString()
            );
            
            var response = JsonReader.Parse(
                client.ReceiveTextMessageType(214)
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
            client.SendTextMessage(216, new JsonObject()
                .Add("cursor_id", cursorId)
                .ToString()
            );

            cursorClosed = true;
        }
    }
}