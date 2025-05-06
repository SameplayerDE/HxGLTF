using HxGLTF.Core.PrimitiveDataStructures;

namespace HxGLTF.Core
{
    public class Node
    {
        //public Camera Camera;
        public Node[]? Children;
        public Skin? Skin;
        public Matrix Matrix = Matrix.Identity;
        public Mesh? Mesh;
        public Quaternion Rotation = Quaternion.Identity;
        public Vector3 Scale = Vector3.One;
        public Vector3 Translation = Vector3.Zero;
        public string? Name;

        public int Index;

        public int MeshIndex = -1;
        public int SkinIndex = -1;

        public bool HasSkin => Skin != null;
        public bool HasMesh => Mesh != null;
        public bool HasName => Name != null;
        public bool HasChildren => Children != null;
    }
}
