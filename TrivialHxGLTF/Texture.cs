using Newtonsoft.Json;

namespace TrivialHxGLTF
{
    public class Texture
    {
        [JsonProperty("source")]
        public int Source;
        [JsonProperty("sampler")]
        public int Sampler;
    }
}