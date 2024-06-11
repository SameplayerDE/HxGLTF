using System.Collections.Generic;

namespace HxGLTF
{

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
        
        /*
        public override bool Equals(object obj)
        {
            var item = obj is int i ? i : 0;
            return this.Id.Equals(item);
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
        */
        
        public static ComponentType FromInt(int type)
        {
            return _types.ContainsKey(type) ? _types[type] : null;
        }
    }
    
    public class Type
    {
        public readonly static Type Scalar = new Type("SCALAR", 1);
        public readonly static Type Vec2   = new Type("VEC2", 2);
        public readonly static Type Vec3   = new Type("VEC3", 3);
        public readonly static Type Vec4   = new Type("VEC4", 4);
        public readonly static Type Mat2   = new Type("MAT2", 4);
        public readonly static Type Mat3   = new Type("MAT3", 9);
        public readonly static Type Mat4   = new Type("MAT4", 16);

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

        /*
        public override bool Equals(object obj)
        {
            var item = obj as string;

            return item != string.Empty && this.Id.Equals(item);
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
        */
        
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

        public int TotalComponentCount => Type.NumberOfComponents * Count;
        public int BitsPerComponent => ComponentType.Bits;
        public int BytesPerComponent => ComponentType.Bits / 8;
        public int TotalByteCount => BytesPerComponent * TotalComponentCount;
    }
}