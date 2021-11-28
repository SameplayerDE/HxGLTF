using Newtonsoft.Json;

namespace TrivialHxGLTF
{
    public class BufferView
    {
        [JsonProperty("buffer")]
        public int? Buffer;
        [JsonProperty("byteLength")]
        public int? ByteLength;
        [JsonProperty("byteOffset")]
        public int? ByteOffset;
        [JsonProperty("byteStride")]
        public int? ByteStride;
    }
}