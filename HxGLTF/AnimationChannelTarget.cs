using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HxGLTF
{

    public enum AnimationChannelTargetPath
    {
        Translation,
        Rotation,
        Scale,
        Weights
    }

    public class AnimationChannelTarget
    {
        public Node Node;
        public string Path;
    }
}
