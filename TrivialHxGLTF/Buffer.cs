using System;
using Newtonsoft.Json;

namespace TrivialHxGLTF
{
    public class Buffer
    {
        [JsonProperty("byteLenght")]
        public int? ByteLength;
        [JsonProperty("uri")]
        public string? Uri;

    }
}