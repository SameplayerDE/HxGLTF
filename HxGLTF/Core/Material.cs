using HxGLTF.Core.PrimitiveDataStructures;

namespace HxGLTF.Core
{
    public class Material
    {
        public string Name = string.Empty;
        public Texture? BaseColorTexture;
        public Texture? EmissiveTexture;
        public Texture? NormalTexture;
        
        public Color BasColorFactor = Color.White;
        public Color EmissiveFactor = Color.Black;
        public string AlphaMode = "OPAQUE";
        public float AlphaCutoff = 0.5f;
        public bool DoubleSided = false;
        
        // For KHR_materials_pbrSpecularGlossiness extension
        public Texture? DiffuseTexture;
        public Color DiffuseFactor = Color.White;
        public Color SpecularFactor = Color.Black;
        public float GlossinessFactor = 0.0f;
    }

    public class NormalTextureInfo
    {
        public Texture Texture;
    }
}