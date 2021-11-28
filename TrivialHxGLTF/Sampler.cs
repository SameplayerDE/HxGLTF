using Newtonsoft.Json;

namespace TrivialHxGLTF
{
    
    public class Sampler
    {
        [JsonProperty("wrapS")] 
        public int? WrapS = 10497;
        [JsonProperty("wrapT")]
        public int? WrapT = 10497;
        [JsonProperty("magFilter")]
        public int? MagFilter;
        [JsonProperty("minFilter")]
        public int? MinFilter;
        [JsonProperty("name")]
        public string? Name = string.Empty;
    }
}