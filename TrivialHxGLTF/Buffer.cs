using System;
using Newtonsoft.Json;

namespace TrivialHxGLTF
{
    public class Buffer
    {
        [JsonProperty("byteLength")]
        public int? ByteLength;
        [JsonProperty("uri")]
        public string? Uri;
        public byte[] Data;
    }
}