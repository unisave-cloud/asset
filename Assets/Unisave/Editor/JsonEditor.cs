using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using LightJson;
using LightJson.Serialization;
using System.Linq;

namespace Unisave
{
    public class JsonEditor
    {
        public enum JsonType
        {
            Null,
            Bool,
            Num,
            Str,
            Arr,
            Obj,
        }

        private Font fontCache = null;
        private Font MonospaceFont
        {
            get
            {
                if (fontCache == null)
                    fontCache = Resources.Load<Font>("FiraCode-Regular");
                
                return fontCache;
            }
        }

        private JsonObject jsonValue = new JsonObject()
            .Add("foo", "bar")
            .Add("baz", JsonValue.Null)
            .Add("bj", new JsonObject().Add("CJ", true))
            .Add("lorem", 42);

        private string jsonString;

        private bool displayText = false;

        /// <summary>
        /// Sets the edited value
        /// </summary>
        public void SetValue(JsonObject value)
        {
            this.jsonValue = value;
        }

        /// <summary>
        /// Render editor GUI
        /// </summary>
        public JsonObject OnGUI(JsonObject valueIn)
        {
            SetValue(valueIn);

            // EditorGUILayout.BeginVertical(
            //     new GUIStyle {
            //         border = new RectOffset(10, 10, 20, 20),
            //     }
            // );

            EditorGUILayout.BeginHorizontal();
            SetDisplayText(
                GUILayout.Toolbar(
                    displayText ? 0 : 1,
                    new string[] { "Text", "UI"}
                ) == 0
            );
            EditorGUILayout.EndHorizontal();

            if (displayText)
                RenderText();
            else
                RenderUI();

            // EditorGUILayout.EndVertical();

            return jsonValue;
        }

        private void SetDisplayText(bool newValue)
        {
            if (displayText == newValue)
                return;

            displayText = newValue;

            if (displayText)
            {
                jsonString = jsonValue.ToString(true).Replace("\t", "    ");
            }
            else
            {
                try
                {
                    jsonValue = JsonReader.Parse(jsonString).AsJsonObject;
                }
                catch (JsonParseException e)
                {
                    displayText = true;
                    Debug.LogException(e);
                }
            }
        }

        private void RenderText()
        {
            var style = EditorStyles.textArea;
            style.font = MonospaceFont;
            string newJsonString = GUILayout.TextArea(jsonString, style);

            if (newJsonString != jsonString)
            {
                try
                {
                    jsonValue = JsonReader.Parse(jsonString).AsJsonObject;
                }
                catch (JsonParseException) { }
            }

            jsonString = newJsonString;
        }

        private void RenderUI()
        {
            //int inset = 1;

            GUILayout.Label("{", EditorStyles.boldLabel);

            RenderObjectBody(jsonValue, 1);

            GUILayout.Label("}", EditorStyles.boldLabel);
        }

        private void RenderObjectBody(JsonObject obj, int indent)
        {
            Dictionary<string, string> keysToRename = new Dictionary<string, string>();
            Dictionary<string, JsonValue> keysToModify = new Dictionary<string, JsonValue>();
            List<string> keysToRemove = new List<string>();

            foreach (var pair in obj)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(30f * indent);
                var nk = EditorGUILayout.TextField(pair.Key, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
                if (nk != pair.Key)
                    keysToRename.Add(pair.Key, nk);
                
                if (GUILayout.Button("x", GUILayout.Width(25f)))
                    keysToRemove.Add(pair.Key);

                var tp = GetJsonType(pair.Value);
                var newTp = (JsonType)EditorGUILayout.EnumPopup(tp, GUILayout.Width(50f));

                if (newTp != tp)
                {
                    keysToModify[pair.Key] = ChangeTypeTo(newTp, pair.Value);
                    tp = newTp;
                }

                RenderPrimitiveField(tp, pair, keysToModify);
                
                EditorGUILayout.EndHorizontal();

                if (pair.Value.IsJsonObject)
                    RenderObjectBody(pair.Value.AsJsonObject, indent + 1);
            }

            foreach (var pair in keysToRename)
                obj.Rename(pair.Key, pair.Value);

            foreach (var pair in keysToModify)
                obj[pair.Key] = pair.Value;

            foreach(string key in keysToRemove)
                obj.Remove(key);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(30f * indent);
            var add = GUILayout.Button("+", GUILayout.Width(25f));
            EditorGUILayout.EndHorizontal();

            if (add)
            {
                for (int i = 1; i < 1_000; i++)
                {
                    string name = "New field " + i;
                    if (!obj.ContainsKey(name))
                    {
                        obj[name] = "";
                        break;
                    }
                }
            }
        }

        private JsonType GetJsonType(JsonValue val)
        {
            if (val.IsNull) return JsonType.Null;
            if (val.IsBoolean) return JsonType.Bool;
            if (val.IsNumber) return JsonType.Num;
            if (val.IsString) return JsonType.Str;
            if (val.IsJsonArray) return JsonType.Arr;
            if (val.IsJsonObject) return JsonType.Obj;

            throw new System.Exception("Unknown JSON value type: " + val.ToString());
        }

        private JsonValue ChangeTypeTo(JsonType tp, JsonValue val)
        {
            switch (tp)
            {
                case JsonType.Null: return JsonValue.Null;
                case JsonType.Bool: return val.AsBoolean;
                case JsonType.Num: return val.AsNumber;
                case JsonType.Str: return val.AsString ?? "";
                case JsonType.Arr: return new JsonArray();
                case JsonType.Obj: return new JsonObject();
            }

            throw new System.Exception("Unknown JSON value type: " + tp.ToString());
        }

        private void RenderPrimitiveField(
            JsonType tp, KeyValuePair<string, JsonValue> pair, Dictionary<string, JsonValue> keysToModify
        )
        {
            if (tp == JsonType.Null)
            {
                EditorGUILayout.LabelField("null");
            }
            else if (tp == JsonType.Bool)
            {
                var v = EditorGUILayout.Toggle(pair.Value.AsBoolean);
                if (v != pair.Value.AsBoolean)
                    keysToModify.Add(pair.Key, v);
            }
            else if (tp == JsonType.Num)
            {
                var v = EditorGUILayout.DoubleField(pair.Value.AsNumber);
                if (v != pair.Value.AsNumber)
                    keysToModify.Add(pair.Key, v);
            }
            else if (tp == JsonType.Str)
            {
                var v = EditorGUILayout.TextField(pair.Value.AsString);
                if (v != pair.Value.AsString)
                    keysToModify.Add(pair.Key, v);
            }
        }
    }
}
