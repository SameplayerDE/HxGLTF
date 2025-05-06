namespace HxGLTF.Core
{
    public class Scene
    {
        public string? Name;
        public Node[]? Nodes;

        public int[]? NodesIndices;

        public bool HasName => Name != null;
        public bool HasNodes => Nodes != null;
    }
}