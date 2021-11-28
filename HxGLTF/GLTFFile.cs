namespace HxGLTF
{
    // ReSharper disable once InconsistentNaming
    public class GLTFFile
    {

        public string FilePath;
        
        public Asset Asset;
        public Buffer[] Buffers;
        public BufferView[] BufferViews;
        public Accessor[] Accessors;
        public Sampler[] Samplers;
        public Image[] Images;
        public Texture[] Textures;
        public Material[] Materials;
        public Mesh[] Meshes;
    }
}