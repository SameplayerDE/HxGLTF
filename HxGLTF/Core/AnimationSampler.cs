namespace HxGLTF.Core
{
    public enum InterpolationAlgorithm
    {
        Linear,
        Step,
        Cubicspline
    }

    public class AnimationSampler
    {
        public int Index;
        public Accessor Input; // time 
        public Accessor Output; // data
        public InterpolationAlgorithm Interpolation = InterpolationAlgorithm.Linear; // algorithm
    }
}