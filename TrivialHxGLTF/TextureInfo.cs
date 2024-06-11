using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TrivialHxGLTF
{
    public class TextureInfo
    {
        [JsonProperty("index", Required = Required.Always)] public int Index;
        [JsonProperty("texCoord")] public int TexCoord = 0;
        [JsonProperty("extensions")] public Dictionary<string, JObject>? Extensions;
    }
}