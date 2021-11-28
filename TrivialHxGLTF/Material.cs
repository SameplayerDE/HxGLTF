using Newtonsoft.Json;

namespace TrivialHxGLTF
{
    public class Material
    {
        [JsonProperty("name")]
        public string Name;
        [JsonProperty("metallicFactor")]
        public int MetallicFactor;
        [JsonProperty("alphaMode")]
        public string AlphaMode;
        [JsonProperty("doubleSided")]
        public bool DoubleSided;
    }
}