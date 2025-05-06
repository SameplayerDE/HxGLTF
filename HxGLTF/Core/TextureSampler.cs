namespace HxGLTF.Core
{
    // public enum TextureWrapMode
    // {
    //     Repeat = 10497,
    //     ClampToEdge = 33071,
    //     MirroredRepeat = 33648
    // }
    
    public class TextureSampler
    {
        public int WrapS = 10497;
        public int WrapT = 10497;
        public int? MagFilter = null;
        public int? MinFilter = null;
        public string? Name = null;
    }
}