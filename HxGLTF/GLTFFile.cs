using HxGLTF.Core;
using Buffer = HxGLTF.Core.Buffer;

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
        public Scene[] Scenes;
        public string[] ExtensionsUsed;

        public bool HasBuffers => Buffers != null && Buffers.Length > 0;
        public bool HasBufferViews => BufferViews != null && BufferViews.Length > 0;
        public bool HasAccessors => Accessors != null && Accessors.Length > 0;
        public bool HasSamplers => Samplers != null && Samplers.Length > 0;
        public bool HasImages => Images != null && Images.Length > 0;
        public bool HasNodes => Nodes != null && Nodes.Length > 0;
        public bool HasTextures => Textures != null && Textures.Length > 0;
        public bool HasMaterials => Materials != null && Materials.Length > 0;
        public bool HasMeshes => Meshes != null && Meshes.Length > 0;
        public bool HasAnimations => Animations != null && Animations.Length > 0;
        public bool HasSkins => Skins != null && Skins.Length > 0;
        public bool HasScenes => Scenes != null && Scenes.Length > 0;
    }
}
