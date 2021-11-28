using System.Collections.Generic;
using Newtonsoft.Json;

namespace TrivialHxGLTF
{
    public class Image
    {
        [JsonProperty("uri")]
        public string Uri;
        [JsonProperty("bufferView")]
        public int? BufferView;
        [JsonProperty("mimeType")]
        public string? MiMeType;
    }
}