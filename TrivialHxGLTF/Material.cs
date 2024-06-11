using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TrivialHxGLTF
{
    public class Material
    {
        [JsonProperty("name")]
        public string Name;
        [JsonProperty("metallicFactor")]
        public int MetallicFactor;
        [JsonProperty("emissiveFactor")]
        public float[] EmissiveFactor = new float[] {0, 0, 0};
        [JsonProperty("emissiveTexture")]
        public TextureInfo EmissiveTexture;
        [JsonProperty("pbrMetallicRoughness")]
        public PbrMetallicRoughness PbrMetallicRoughness;
        [JsonProperty("normalTexture")] public NormalTextureInfo NormalTexture;
        [JsonProperty("alphaMode")]
        public string AlphaMode = "OPAQUE";
        [JsonProperty("alphaCutoff")] public float AlphaCutoff = 0.5f;
        [JsonProperty("doubleSided")]
        public bool DoubleSided = false;
        [JsonProperty("extensions")] public Dictionary<string, JObject>? Extensions;
    }
}