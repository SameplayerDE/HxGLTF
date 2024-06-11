using Newtonsoft.Json;

namespace TrivialHxGLTF
{
    public class BaseColorTexture
    {
        [JsonProperty("index")]
        public int Index { get; set; }
        [JsonProperty("texCoord")]
        public int? TexCoord { get; set; }
    }
}