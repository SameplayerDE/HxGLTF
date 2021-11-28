using System.Collections.Generic;
using System.Threading.Channels;
using Newtonsoft.Json;

namespace TrivialHxGLTF
{
    public class Animation
    {
        [JsonProperty("channels")]
        public AnimationChannel[] Channels;

        [JsonProperty("samplers")]
        public AnimationSampler[] Samplers;

        [JsonProperty("name")]
        public string Name;
    }
    
    public class AnimationSampler
    {
        [JsonProperty("input")]
        public int? Input; //Index Of Accessor
        [JsonProperty("output")]
        public int? Output; //Index Of Accessor
        [JsonProperty("interpolation")]
        public string? Interpolation; //Interpolation Mode
    }
    
}