using System;

namespace HxGLTF.Core.PrimitiveDataStructures;

public struct Color
{
    public uint PackedValue;

    public byte R
    {
        get => (byte)(PackedValue & 0xFF);
        set => PackedValue = (PackedValue & 0xFFFFFF00) | value;
    }

    public byte G
    {
        get => (byte)((PackedValue >> 8) & 0xFF);
        set => PackedValue = (PackedValue & 0xFFFF00FF) | ((uint)value << 8);
    }

    public byte B
    {
        get => (byte)((PackedValue >> 16) & 0xFF);
        set => PackedValue = (PackedValue & 0xFF00FFFF) | ((uint)value << 16);
    }

    public byte A
    {
        get => (byte)((PackedValue >> 24) & 0xFF);
        set => PackedValue = (PackedValue & 0x00FFFFFF) | ((uint)value << 24);
    }

    public Color(byte r, byte g, byte b, byte a = 255)
    {
        PackedValue = (uint)(a << 24 | b << 16 | g << 8 | r);
    }

    public Color(float r, float g, float b, float a = 1f)
        : this((byte)(r * 255f), (byte)(g * 255f), (byte)(b * 255f), (byte)(a * 255f)) { }

    public Color(int r, int g, int b)
        : this((byte)System.Math.Clamp(r, 0, 255), (byte)Math.Clamp(g, 0, 255), (byte)Math.Clamp(b, 0, 255)) { }

    public Color(int r, int g, int b, int a)
        : this((byte)Math.Clamp(r, 0, 255), (byte)Math.Clamp(g, 0, 255), (byte)Math.Clamp(b, 0, 255), (byte)Math.Clamp(a, 0, 255)) { }

    public static Color White { get; } = new Color(255, 255, 255);
    public static Color Black { get; } = new Color(0, 0, 0);

    public Vector3 ToVector3() => new Vector3(R / 255f, G / 255f, B / 255f);
    public Vector4 ToVector4() => new Vector4(R / 255f, G / 255f, B / 255f, A / 255f);
}