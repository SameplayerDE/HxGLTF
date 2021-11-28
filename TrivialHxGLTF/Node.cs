#nullable enable
using Newtonsoft.Json;

namespace TrivialHxGLTF
{
    public class Node
    {
        [JsonProperty("mesh")]
        public int? Mesh; //Index Of Mesh
        [JsonProperty("skin")]
        public int? Skin; //Index Of Skin
        [JsonProperty("children")]
        public int[]? Children; //Indices Of Childen
        [JsonProperty("name")]
        public string? Name;
        [JsonProperty("translation")]
        public float[]? Translation;
        [JsonProperty("scale")]
        public float[]? Scale;
        [JsonProperty("rotation")]
        public float[]? Rotation;
    }
}