using Newtonsoft.Json;

namespace TrivialHxGLTF
{
    public class Scene
    {
        [JsonProperty("name")]
        public string Name;
        [JsonProperty("nodes")]
        public int[] Nodes;
    }
}