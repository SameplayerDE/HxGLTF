using System.Collections.Generic;

namespace HxGLTF
{

    public class ComponentType
    {
        public static ComponentType T5120 = new ComponentType(5120, 8);
        public static ComponentType T5121 = new ComponentType(5121, 8);
        public static ComponentType T5122 = new ComponentType(5122, 16);
        public static ComponentType T5123 = new ComponentType(5123, 16);
        public static ComponentType T5124 = new ComponentType(5124, 32);
        public static ComponentType T5125 = new ComponentType(5125, 32);
        
        private int _id;
        private int _bits;

        private ComponentType(int id, int bits)
        {
            _id = id;
            _bits = bits;
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
            {Scalar._id, Scalar},
            {Vec2._id, Vec2},
            {Vec3._id, Vec3},
            {Vec4._id, Vec4},
            {Mat2._id, Mat2},
            {Mat3._id, Mat3},
            {Mat4._id, Mat4},
        };

        private string _id;
        private int _bits;

        public string Id => _id;
        public int Bits => _bits;

        private Type(string id, int bits)
        {
            _id = id;
            _bits = bits;
        }

        public static Type FromSting(string type)
        {
            return _types.ContainsKey(type.ToUpper()) ? _types[type.ToUpper()] : null;
        }
    }
    
    public class Accessor
    {
        public BufferView BufferView;
        public int ByteOffset;
        public ComponentType ComponentType;
        public int Count;
        public Type Type;
    }
}