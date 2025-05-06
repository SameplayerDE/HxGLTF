using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace HxGLTF.Core;

public class ComponentDataType
{
    public static ComponentDataType T5120 = new ComponentDataType(5120, 8);  //Byte
    public static ComponentDataType T5121 = new ComponentDataType(5121, 8);  //UByte
    public static ComponentDataType T5122 = new ComponentDataType(5122, 16); //Short
    public static ComponentDataType T5123 = new ComponentDataType(5123, 16); //UShort
    public static ComponentDataType T5125 = new ComponentDataType(5125, 32); //UInt
    public static ComponentDataType T5126 = new ComponentDataType(5126, 32); //Float
        
    private static Dictionary<int, ComponentDataType> _types = new Dictionary<int, ComponentDataType>()
    {
        {T5120.Id, T5120},
        {T5121.Id, T5121},
        {T5122.Id, T5122},
        {T5123.Id, T5123},
        {T5125.Id, T5125},
        {T5126.Id, T5126}
    };

    public int Id { get; }
    public int Bits { get; }

    private ComponentDataType(int id, int bits)
    {
        Id = id;
        Bits = bits;
    }
        
        
    public static ComponentDataType FromInt(int type)
    {
        return _types.ContainsKey(type) ? _types[type] : null;
    }
}
    
public class StructureType
{
    public readonly static StructureType Scalar = new StructureType("SCALAR", 1);
    public readonly static StructureType Vec2   = new StructureType("VEC2", 2);  // vector2
    public readonly static StructureType Vec3   = new StructureType("VEC3", 3);  // vector3
    public readonly static StructureType Vec4   = new StructureType("VEC4", 4);  // vector4
    public readonly static StructureType Mat2   = new StructureType("MAT2", 4);  // 2x2 matrix
    public readonly static StructureType Mat3   = new StructureType("MAT3", 9);  // 3x3 matrix
    public readonly static StructureType Mat4   = new StructureType("MAT4", 16); // 4x4 matrix

    private static Dictionary<string, StructureType> _types = new Dictionary<string, StructureType>()
    {
        {Scalar.Id, Scalar},
        {Vec2.Id, Vec2},
        {Vec3.Id, Vec3},
        {Vec4.Id, Vec4},
        {Mat2.Id, Mat2},
        {Mat3.Id, Mat3},
        {Mat4.Id, Mat4},
    };

    public string Id { get; }
    public int NumberOfComponents { get; }

    private StructureType(string id, int numberOfComponents)
    {
        Id = id;
        NumberOfComponents = numberOfComponents;
    }
        
    public static StructureType FromSting(string type)
    {
        if (type == null)
        {
            return null;
        }
        return _types.ContainsKey(type.ToUpper()) ? _types[type.ToUpper()] : null;
    }
}
    
public class Accessor
{
    public BufferView BufferView;
    public int ByteOffset = 0;
    public ComponentDataType DataType;
    public bool Normalized = false;
    public int Count;
    public StructureType StructureType;
    public int Max;
    public int Min;
    public string Name;

    public int TotalComponentCount => StructureType.NumberOfComponents * Count;
    public int BitsPerComponent => DataType.Bits;
    public int BytesPerComponent => DataType.Bits / 8;
    public int TotalByteCount => BytesPerComponent * TotalComponentCount;
}

public static class AccessorReader
{
    public static float[] ReadData(Accessor accessor)
    {
        if (accessor == null || accessor.BufferView == null || accessor.BufferView.Buffer == null || accessor.DataType == null || accessor.StructureType == null)
        {
            throw new ArgumentNullException(nameof(accessor), "Accessor, BufferView, Buffer, ComponentType, or Type is null.");
        }


        float[] data = new float[accessor.TotalComponentCount];
        switch (accessor.DataType.Id)
        {
            case 5120:
                ReadDataInternal<sbyte>(data, accessor);
                break;
            case 5121:
                ReadDataInternal<byte>(data, accessor);
                break;
            case 5122:
                ReadDataInternal<short>(data, accessor);
                break;
            case 5123:
                ReadDataInternal<ushort>(data, accessor);
                break;
            case 5125:
                ReadDataInternal<uint>(data, accessor);
                break;
            case 5126:
                ReadDataInternal<float>(data, accessor);
                break;
            default:
                throw new ArgumentException("Unsupported component type.");
        }
        return data;
    }

    private static float ReadValue<T>(ReadOnlySpan<byte> bufferSpan, int byteIndex, bool debug = false) where T : struct
    {
        if (typeof(T) == typeof(sbyte))
        {
            return (sbyte)bufferSpan[byteIndex];
        }

        if (typeof(T) == typeof(byte))
        {
            return bufferSpan[byteIndex];
        }

        if (typeof(T) == typeof(short))
        {
            return BitConverter.ToInt16(bufferSpan.Slice(byteIndex, sizeof(short)));
        }
        if (typeof(T) == typeof(ushort))
        {
            return BitConverter.ToUInt16(bufferSpan.Slice(byteIndex, sizeof(ushort)));
        }
        if (typeof(T) == typeof(uint))
        {
            return BitConverter.ToUInt32(bufferSpan.Slice(byteIndex, sizeof(uint)));
        }
        if (typeof(T) == typeof(float))
        {
            return BitConverter.ToSingle(bufferSpan.Slice(byteIndex, sizeof(float)));
        }
        throw new ArgumentException("Unsupported data type.");
    }

    private static void ReadDataInternal<T>(float[] data, Accessor accessor) where T : struct
    {
        ReadOnlySpan<byte> bufferSpan = accessor.BufferView.Buffer.Bytes.Span;
        int totalOffset = accessor.ByteOffset + accessor.BufferView.ByteOffset;
        int displacement = accessor.BufferView.ByteStride;
        int bytesPerComp = accessor.BytesPerComponent;
        int compCount = accessor.StructureType.NumberOfComponents;

        if (displacement != 0)
        {
            /*
             * The data of the attributes that are stored in a single bufferView may be stored as an Array-Of-Structures.
             * A single bufferView may, for example, contain the data for vertex positions and for vertex normals in an interleaved fashion.
             * In this case, the byteOffset of an accessor defines the start of the first relevant data element for the respective attribute,
             * and the bufferView defines an additional byteStride property. This is the number of bytes between the start of one element
             * of its accessors, and the start of the next one.
             */
            for (int i = 0; i < accessor.Count; i++)
            {
                for (int j = 0; j < compCount; j++)
                {
                    int elementOffset = totalOffset + i * displacement + j * bytesPerComp;
                    data[i * compCount + j] = accessor.Normalized 
                        ? ReadValueNormalized<T>(bufferSpan, elementOffset)
                        : ReadValue<T>(bufferSpan, elementOffset);
                }
            }
        }
        else
        {
            int index = 0;
            int totalBytesLoop = compCount * bytesPerComp * accessor.Count;
            for (int a = 0; a < totalBytesLoop; a += bytesPerComp)
            {
                int byteIndex = totalOffset + a;
                if (byteIndex >= bufferSpan.Length)
                {
                    throw new Exception("Index out of range while reading buffer data.");
                }
                data[index++] = accessor.Normalized 
                    ? ReadValueNormalized<T>(bufferSpan, byteIndex)
                    : ReadValue<T>(bufferSpan, byteIndex);
            }
        }
    }
    
    private static float ReadValueNormalized<T>(ReadOnlySpan<byte> bufferSpan, int byteIndex) where T : struct
    {
        float rawValue = ReadValue<T>(bufferSpan, byteIndex);
        return NormalizeComponentValue<T>(rawValue, true);
    }
    
    private static float NormalizeComponentValue<T>(float rawValue, bool normalize) where T : struct
    {
        return 0f;

        /*return typeof(T) switch
        {
            var t when t == typeof(byte) => rawValue / 255f,
            var t when t == typeof(sbyte) => Math.Max(rawValue / 127f, -1f),
            var t when t == typeof(ushort) => rawValue / 65535f,
            var t when t == typeof(short) => Math.Max(rawValue / 32767f, -1f),
            var t when t == typeof(uint) => rawValue / uint.MaxValue,
            var t when t == typeof(int) => Math.Max(rawValue / int.MaxValue, -1f),
            var t when t == typeof(float) => rawValue, // Already normalized
            _ => throw new ArgumentException($"Unsupported normalization for type {typeof(T)}")
        };*/
    }
    
    public static int[] ReadIndices(Accessor accessor)
    {
        if (accessor.BufferView?.Buffer == null)
            throw new ArgumentNullException(nameof(accessor));
        
        int count      = accessor.Count;
        int compBytes  = accessor.BytesPerComponent;
        int stride     = accessor.BufferView.ByteStride != 0 
            ? accessor.BufferView.ByteStride 
            : compBytes;
        int baseOffset = accessor.BufferView.ByteOffset + accessor.ByteOffset;
        ReadOnlySpan<byte> span = accessor.BufferView.Buffer.Bytes.Span;
        var indices = new int[count];

        for (int i = 0; i < count; i++)
        {
            int off = baseOffset + i * stride;
            indices[i] = accessor.DataType.Id switch
            {
                5121 => span[off],                                 // UNSIGNED_BYTE
                5123 => BitConverter.ToUInt16(span.Slice(off,2)),  // UNSIGNED_SHORT
                5125 => (int)BitConverter.ToUInt32(span.Slice(off,4)), // UNSIGNED_INT
                _    => throw new NotSupportedException($"Index-Typ {accessor.DataType.Id} nicht unterstützt")
            };
        }
        return indices;
    }
}

public class AccessorReaderContext
{
    private readonly ConcurrentDictionary<Accessor, float[]> _cache = new();
    private const bool _useCache = true;
    
    public float[] Read(Accessor accessor)
    {
        if (_useCache)
        {
            if (_cache.TryGetValue(accessor, out var cached))
            {
                return cached;
            }
        }
        var data = AccessorReader.ReadData(accessor);
        if (_useCache)
        {
            _cache[accessor] = data;
        }
        return data;
    }
}

/**
 * using System.Diagnostics;

namespace HxGLTF;

public class ComponentType
{
    public static ComponentType T5120 = new ComponentType(5120, 8);  //Byte
    public static ComponentType T5121 = new ComponentType(5121, 8);  //UByte
    public static ComponentType T5122 = new ComponentType(5122, 16); //Short
    public static ComponentType T5123 = new ComponentType(5123, 16); //UShort
    public static ComponentType T5125 = new ComponentType(5125, 32); //UInt
    public static ComponentType T5126 = new ComponentType(5126, 32); //Float
        
    private static Dictionary<int, ComponentType> _types = new Dictionary<int, ComponentType>()
    {
        {T5120.Id, T5120},
        {T5121.Id, T5121},
        {T5122.Id, T5122},
        {T5123.Id, T5123},
        {T5125.Id, T5125},
        {T5126.Id, T5126}
    };

    public int Id { get; }
    public int Bits { get; }

    private ComponentType(int id, int bits)
    {
        Id = id;
        Bits = bits;
    }
        
        
    public static ComponentType FromInt(int type)
    {
        return _types.ContainsKey(type) ? _types[type] : null;
    }
}
    
public class Type
{
    public readonly static Type Scalar = new Type("SCALAR", 1);
    public readonly static Type Vec2   = new Type("VEC2", 2);  // vector2
    public readonly static Type Vec3   = new Type("VEC3", 3);  // vector3
    public readonly static Type Vec4   = new Type("VEC4", 4);  // vector4
    public readonly static Type Mat2   = new Type("MAT2", 4);  // 2x2 matrix
    public readonly static Type Mat3   = new Type("MAT3", 9);  // 3x3 matrix
    public readonly static Type Mat4   = new Type("MAT4", 16); // 4x4 matrix

    private static Dictionary<string, Type> _types = new Dictionary<string, Type>()
    {
        {Scalar.Id, Scalar},
        {Vec2.Id, Vec2},
        {Vec3.Id, Vec3},
        {Vec4.Id, Vec4},
        {Mat2.Id, Mat2},
        {Mat3.Id, Mat3},
        {Mat4.Id, Mat4},
    };

    public string Id { get; }
    public int NumberOfComponents { get; }

    private Type(string id, int numberOfComponents)
    {
        Id = id;
        NumberOfComponents = numberOfComponents;
    }
        
    public static Type FromSting(string type)
    {
        if (type == null)
        {
            return null;
        }
        return _types.ContainsKey(type.ToUpper()) ? _types[type.ToUpper()] : null;
    }
}
    
public class Accessor
{
    public BufferView BufferView;
    public int ByteOffset = 0;
    public ComponentType ComponentType;
    public bool Normalized = false;
    public int Count;
    public Type Type;
    public int Max;
    public int Min;
    public string Name;

    public int TotalComponentCount => Type.NumberOfComponents * Count;
    public int BitsPerComponent => ComponentType.Bits;
    public int BytesPerComponent => ComponentType.Bits / 8;
    public int TotalByteCount => BytesPerComponent * TotalComponentCount;
}

public static class AccessorReader
{

    private static readonly Dictionary<Accessor, float[]> _cache = new();
    public static bool UseCache = true;
    
    public static float[] ReadData(Accessor accessor)
    {
#if DEBUG
        var sw = Stopwatch.StartNew();
#endif
        
        if (accessor == null || accessor.BufferView == null || accessor.BufferView.Buffer == null || accessor.ComponentType == null || accessor.Type == null)
        {
            throw new ArgumentNullException(nameof(accessor), "Accessor, BufferView, Buffer, ComponentType, or Type is null.");
        }

        if (UseCache)
        {
            if (_cache.TryGetValue(accessor, out var cached))
            {
                return cached;
            }
        }

        float[] data = new float[accessor.TotalComponentCount];

        switch (accessor.ComponentType.Id)
        {
            case 5120: ReadDataInternal<sbyte>(data, accessor); break;
            case 5121: ReadDataInternal<byte>(data, accessor); break;
            case 5122: ReadDataInternal<short>(data, accessor); break;
            case 5123: ReadDataInternal<ushort>(data, accessor); break;
            case 5125: ReadDataInternal<uint>(data, accessor); break;
            case 5126: ReadDataInternal<float>(data, accessor); break;
            default: throw new ArgumentException("Unsupported component type.");
        }

        if (UseCache)
        {
            _cache[accessor] = data;
        }

        return data;
    }

    private static void ReadDataInternal<T>(float[] result, Accessor accessor) where T : struct
    {
        ReadOnlySpan<byte> buffer = accessor.BufferView.Buffer.Bytes.Span;

        int baseOffset = accessor.BufferView.ByteOffset + accessor.ByteOffset;
        int componentSizeInBytes = accessor.BytesPerComponent;
        int componentsPerElement = accessor.Type.NumberOfComponents;
        int totalElements = accessor.Count;
        int elementStride = accessor.BufferView.ByteStride;
        bool isInterleaved = elementStride != 0;

        if (isInterleaved)
        {
            for (int elementIndex = 0; elementIndex < totalElements; elementIndex++)
            {
                for (int componentIndex = 0; componentIndex < componentsPerElement; componentIndex++)
                {
                    int byteOffset = baseOffset + elementIndex * elementStride + componentIndex * componentSizeInBytes;
                    float rawValue = ReadRawComponentValue<T>(buffer, byteOffset);
                    float normalizedValue = NormalizeComponentValue<T>(rawValue, accessor.Normalized);
                    result[elementIndex * componentsPerElement + componentIndex] = normalizedValue;
                }
            }
        }
        else
        {
            int totalComponents = totalElements * componentsPerElement;
            for (int componentFlatIndex = 0; componentFlatIndex < totalComponents; componentFlatIndex++)
            {
                int byteOffset = baseOffset + componentFlatIndex * componentSizeInBytes;
                float rawValue = ReadRawComponentValue<T>(buffer, byteOffset);
                float normalizedValue = NormalizeComponentValue<T>(rawValue, accessor.Normalized);
                result[componentFlatIndex] = normalizedValue;
            }
        }
    }

    private static float ReadRawComponentValue<T>(ReadOnlySpan<byte> buffer, int byteOffset) where T : struct
    {
        return typeof(T) switch
        {
            var t when t == typeof(sbyte)  => (sbyte)buffer[byteOffset],
            var t when t == typeof(byte)   => buffer[byteOffset],
            var t when t == typeof(short)  => BitConverter.ToInt16(buffer.Slice(byteOffset, sizeof(short))),
            var t when t == typeof(ushort) => BitConverter.ToUInt16(buffer.Slice(byteOffset, sizeof(ushort))),
            var t when t == typeof(uint)   => BitConverter.ToUInt32(buffer.Slice(byteOffset, sizeof(uint))),
            var t when t == typeof(float)  => BitConverter.ToSingle(buffer.Slice(byteOffset, sizeof(float))),
            _ => throw new ArgumentException($"Unsupported component type: {typeof(T)}")
        };
    }

    private static float NormalizeComponentValue<T>(float rawValue, bool normalize) where T : struct
    {
        if (!normalize) return rawValue;

        return typeof(T) switch
        {
            var t when t == typeof(byte)   => rawValue / 255f,
            var t when t == typeof(sbyte)  => Math.Max(rawValue / 127f, -1f),
            var t when t == typeof(ushort) => rawValue / 65535f,
            var t when t == typeof(short)  => Math.Max(rawValue / 32767f, -1f),
            var t when t == typeof(uint)   => rawValue / uint.MaxValue,
            var t when t == typeof(int)    => Math.Max(rawValue / int.MaxValue, -1f),
            var t when t == typeof(float)  => rawValue, // Already normalized
            _ => throw new ArgumentException($"Unsupported normalization for type {typeof(T)}")
        };
    }

    public static int[] ReadIndices(Accessor accessor)
    {
        if (accessor.BufferView?.Buffer == null)
        {
            throw new ArgumentNullException(nameof(accessor));
        }

        int count = accessor.Count;
        int componentSize = accessor.BytesPerComponent;
        int stride = accessor.BufferView.ByteStride != 0
            ? accessor.BufferView.ByteStride
            : componentSize;
        int baseOffset = accessor.BufferView.ByteOffset + accessor.ByteOffset;
        ReadOnlySpan<byte> buffer = accessor.BufferView.Buffer.Bytes.Span;

        var indices = new int[count];

        for (int i = 0; i < count; i++)
        {
            int byteOffset = baseOffset + i * stride;

            indices[i] = accessor.ComponentType.Id switch
            {
                5121 => buffer[byteOffset], // UNSIGNED_BYTE
                5123 => BitConverter.ToUInt16(buffer.Slice(byteOffset, 2)), // UNSIGNED_SHORT
                5125 => (int)BitConverter.ToUInt32(buffer.Slice(byteOffset, 4)), // UNSIGNED_INT
                _ => throw new NotSupportedException($"Index component type {accessor.ComponentType.Id} is not supported.")
            };
        }

        return indices;
    }
}
**/