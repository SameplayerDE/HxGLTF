using Newtonsoft.Json;

namespace TrivialHxGLTF
{
    public class AnimationChannel
    {
        [JsonProperty("sampler")]
        public int Sampler; //Index Of AnimationSampler

        [JsonProperty("target")]
        public AnimationChannelTarget Target; //Index Of AnimationChannelTarget
    }
}