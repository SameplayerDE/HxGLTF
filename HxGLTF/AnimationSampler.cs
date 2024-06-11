namespace HxGLTF
{
    public enum InterpolationAlgorithm
    {
        Linear,
        Step,
        Cubicspline
    }

    public class AnimationSampler
    {
        public Accessor Input;
        public Accessor Output;
        public InterpolationAlgorithm Interpolation = InterpolationAlgorithm.Linear;
    }
}