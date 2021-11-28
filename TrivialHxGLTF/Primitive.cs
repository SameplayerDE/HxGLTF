using System.Collections.Generic;
using Newtonsoft.Json;

namespace TrivialHxGLTF
{
    public class Primitive
    {
        [JsonProperty("attributes")]
        public Dictionary<string, int> Attributes;
        [JsonProperty("targets")]
        public Dictionary<string, int>[]? Targets;
        [JsonProperty("indices")]
        public int? Indices; //Index Of Accessor
        [JsonProperty("material")]
        public int? Material; //Index Of Material
        [JsonProperty("mode")]
        public int? Mode; 
    }
}