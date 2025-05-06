using System;
using System.Collections.Generic;

namespace HxGLTF.Core
{
    public class MiMeType
    {
        public readonly static MiMeType Png = new MiMeType("image/png", 8, new byte[] {0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A});
        public readonly static MiMeType Jpeg   = new MiMeType("image/jpeg", 3, new byte[] {0xFF,0xD8,0xFF});
        
        private static readonly Dictionary<string, MiMeType> Types = new Dictionary<string, MiMeType>()
        {
            {Png.Id, Png},
            {Jpeg.Id, Jpeg}
        };
        
        public string Id { get; }
        public int PatternLength { get; }
        public byte[] PatternBytes { get; }

        private MiMeType(string id, int patternLength, byte[] patternBytes)
        {
            Id = id;
            PatternLength = patternLength;
            PatternBytes = patternBytes;
        }

        public static MiMeType FromString(string type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type), "Type cannot be null");
            }
            if (Types.TryGetValue(type, out var mimeType))
            {
                return mimeType;
            }
            throw new ArgumentException($"Unsupported MIME type: {type}", nameof(type));
        }
    }
    
    public class Image
    {
        public string? Uri;
        public BufferView? BufferView;
        public MiMeType? MiMeType;
        public string Name;
    }
}