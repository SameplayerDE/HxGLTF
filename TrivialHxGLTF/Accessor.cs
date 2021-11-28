using System.Collections.Generic;
using Newtonsoft.Json;

namespace TrivialHxGLTF
{
    public class Accessor
    {
        [JsonProperty("bufferView")]
        public int BufferView;
        [JsonProperty("byteOffset")]
        public int ByteOffset;
        [JsonProperty("componentType")]
        public int ComponentType;
        [JsonProperty("count")]
        public int Count;
        [JsonProperty("type")]
        public string Type;
        [JsonProperty("min")]
        public List<double>? Min { get; set; }
        [JsonProperty("max")]
        public List<double>? Max { get; set; }
        [JsonProperty("normalized")]
        public bool? Normalized { get; set; }
    }
}