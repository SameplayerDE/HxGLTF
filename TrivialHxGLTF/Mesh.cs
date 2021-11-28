using Newtonsoft.Json;

namespace TrivialHxGLTF
{
    public class Mesh
    {
        [JsonProperty("name")]
        public string Name;
        [JsonProperty("primitives")]
        public Primitive[] Primitives;
    }
}