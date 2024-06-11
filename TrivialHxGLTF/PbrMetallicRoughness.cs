using Newtonsoft.Json;

namespace TrivialHxGLTF
{
    public class PbrMetallicRoughness
    {
        [JsonProperty("baseColorTexture")] 
        public BaseColorTexture? BaseColorTexture;
        [JsonProperty("baseColorFactor")]
        public float[] BaseColorFactor = { 1f, 1f, 1f, 1f };
        [JsonProperty("roughnessFactor")] public float RoughnessFactor = 1;
        [JsonProperty("metallicFactor")] public float MetallicFactor = 1;
    }
}