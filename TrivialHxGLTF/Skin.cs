using System.Collections.Generic;
using Newtonsoft.Json;

namespace TrivialHxGLTF
{
    public class Skin
    {
        [JsonProperty("skeleton")]
        public int? Skeleton; //Index Of Common Root
        
        [JsonProperty("joints", Required = Required.Always)]
        public int[] Joints; //Indices Of Nodes

        [JsonProperty("inverseBindMatrices")]
        public int? InverseBindMatrices; //Index Of Accessor
    }
}