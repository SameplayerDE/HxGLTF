using Newtonsoft.Json;

namespace TrivialHxGLTF
{
    public class AnimationChannelTarget
    {
        [JsonProperty("node")]
        public int Node; //Index Of Node

        [JsonProperty("path")]
        public string Path; //What To Edit
    }
}