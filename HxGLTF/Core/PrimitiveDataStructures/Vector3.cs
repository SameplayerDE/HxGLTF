namespace HxGLTF.Core.PrimitiveDataStructures;

public struct Vector3
{
    public float X;
    public float Y;
    public float Z;

    public Vector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }
    
    public static Vector3 Zero { get; } = new Vector3(0f, 0f, 0f);
    public static Vector3 One { get; } = new Vector3(1f, 1f, 1f);
}