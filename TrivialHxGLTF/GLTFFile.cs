using Newtonsoft.Json;

namespace TrivialHxGLTF
{
    // ReSharper disable once InconsistentNaming
    public class GLTFFile
    {

        public string Path;
        
        [JsonProperty("asset")]
        public Asset Asset;

        [JsonProperty("bufferViews")]
        public BufferView[] BufferViews;

        [JsonProperty("accessors")]
        public Accessor[] Accessors;
        
        [JsonProperty("animations")]
        public Animation[]? Animations;

        [JsonProperty("meshes")]
        public Mesh[] Meshes;

        [JsonProperty("nodes")]
        public Node[] Nodes;

        [JsonProperty("skins")]
        public Skin[]? Skins;

        [JsonProperty("scenes")]
        public Scene[] Scenes;

        [JsonProperty("scene")]
        public int Scene;

        [JsonProperty("samplers")]
        public Sampler[] Samplers;

        [JsonProperty("images")]
        public Image[] Images;

        [JsonProperty("textures")]
        public Texture[] Textures;

        [JsonProperty("materials")]
        public Material[] Materials;

        [JsonProperty("buffers")]
        public Buffer[] Buffers;
    }
}