using Newtonsoft.Json;

namespace HxGLTF
{
    public class GLTFFile
    {

        public string Path;

        public Asset Asset;
        public Buffer[] Buffers;
        public BufferView[] BufferViews;
        public Accessor[] Accessors;
        public TextureSampler[] Samplers;
        public Image[] Images;
        public Node[] Nodes;
        public Texture[] Textures;
        public Material[] Materials;
        public Mesh[] Meshes;
        public Animation[] Animations;
        public Skin[] Skins;
    }
}