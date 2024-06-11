#nullable enable
using Newtonsoft.Json;

namespace TrivialHxGLTF
{
    public class Node
    {
        [JsonProperty("mesh")] public int? Mesh; //Index Of Mesh
        [JsonProperty("skin")] public int? Skin; //Index Of Skin
        [JsonProperty("children")] public int[]? Children; //Indices Of Childen
        [JsonProperty("name")] public string? Name;
        [JsonProperty("translation")] public float[]? Translation = new float[] { 0, 0, 0 };
        [JsonProperty("scale")] public float[]? Scale = new float[] { 1, 1, 1 };
        [JsonProperty("rotation")] public float[]? Rotation = new float[] { 0, 0, 0, 1 };
        [JsonProperty("matrix")]
        public float[]? Matrix = new float[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
    }
}