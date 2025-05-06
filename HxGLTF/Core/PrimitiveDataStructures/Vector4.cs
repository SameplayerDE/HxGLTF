namespace HxGLTF.Core.PrimitiveDataStructures;

public struct Vector4
{
    public float X;
    public float Y;
    public float Z;
    public float W;

    public Vector4(float x, float y, float z, float w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }
    
    public static Vector4 Zero { get; } = new Vector4(0f, 0f, 0f, 0f);
    public static Vector4 One { get; } = new Vector4(1f, 1f, 1f, 1f);
}