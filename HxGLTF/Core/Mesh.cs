using System.Collections.Generic;

namespace HxGLTF.Core
{
    public enum VertexType
    {
        Position,
        PositionNormal,
        PositionNormalTexture
    }

    public class MeshPrimitive
    {
        public Dictionary<string, Accessor> Attributes;
        public Dictionary<string, Accessor>[] Targets;
        public Accessor? Indices;
        public Material? Material;
        public int Mode = 4; //TODO Create Mode Class

        public bool HasIndices => Indices != null;
    }

    public class Mesh
    {
        public string? Name;
        public int Index;
        public MeshPrimitive[] Primitives;
    }
}
