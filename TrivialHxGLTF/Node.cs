namespace TrivialHxGLTF
{
    public class Node
    {
        public string Name;
        public Node[] Children;
        public float[] Translation = new float[3];
        public float[] Scale = new float[3];
        public float[] Rotation = new float[4];
    }
}