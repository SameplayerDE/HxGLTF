namespace HxGLTF.Core;

public enum AnimationChannelTargetPath
{
    Translation,
    Rotation,
    Scale,
    Weights,
    Texture
}

public class AnimationChannelTarget
{
    public Node Node;
    public AnimationChannelTargetPath Path;
}