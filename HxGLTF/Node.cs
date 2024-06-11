using Microsoft.Xna.Framework;

namespace HxGLTF
{
    public class Node
    {
        //public Camera Camera;
        public Node[]? Children;
        //public Skin Skin;
        public Matrix Matrix = Matrix.Identity;
        public Mesh? Mesh;
        public Quaternion Rotation = Quaternion.Identity;
        public Vector3 Scale = Vector3.One;
        public Vector3 Translation = Vector3.Zero;
        public string? Name;
    }
}
