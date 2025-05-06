using HxGLTF.Core.PrimitiveDataStructures;

namespace HxGLTF.Core
{
    public class Skin
    {
        //public Accessor? InverseBindMatrices;
        public Matrix[]? InverseBindMatrices;
        public Node? Skeleton;
        public Node[]? Joints;
        public string? Name;

        public int? SkeletonIndex;
        public int[]? JointsIndices;

        public bool HasSkeleton => Skeleton != null;
        public bool HasName => Name != null;
    }
}
