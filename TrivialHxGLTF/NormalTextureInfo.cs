using Newtonsoft.Json;

namespace TrivialHxGLTF
{
    public class NormalTextureInfo : TextureInfo
    {
        [JsonProperty("scale")]
        public float Scale = 1;
    }
}