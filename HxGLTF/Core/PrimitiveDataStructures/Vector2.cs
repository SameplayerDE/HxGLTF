namespace HxGLTF.Core.PrimitiveDataStructures;

public struct Vector2
{
    public float X;
    public float Y;

    public Vector2(float x, float y)
    {
        X = x;
        Y = y;
    }
    
    public static Vector2 Zero { get; } = new Vector2(0f, 0f);
    public static Vector2 One { get; } = new Vector2(1f, 1f);
}