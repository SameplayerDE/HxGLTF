namespace HxGLTF.Core.PrimitiveDataStructures;

public struct Quaternion
{
    public float X;
    public float Y;
    public float Z;
    public float W;

    public Quaternion(float x, float y, float z, float w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }
    
    public static Quaternion Identity { get; } =  new Quaternion(0f, 0f, 0f, 1f);
}