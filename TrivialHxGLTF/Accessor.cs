using System.Collections.Generic;
using Newtonsoft.Json;

namespace TrivialHxGLTF
{
    public class Accessor
    {
        [JsonProperty("bufferView")]
        public int BufferView;
        [JsonProperty("byteOffset")]
        public int ByteOffset;
        [JsonProperty("componentType")]
        public int ComponentType;
        [JsonProperty("count")]
        public int Count;
        [JsonProperty("type")]
        public string Type;
        [JsonProperty("min")]
        public List<double>? Min { get; set; }
        [JsonProperty("max")]
        public List<double>? Max { get; set; }
        [JsonProperty("normalized")]
        public bool? Normalized { get; set; }
    }

    public static class AccessorExtension
    {
        public static int ComponentTypeBitsAmount(this Accessor accessor)
        {
            var bitsPerComponent = -1;
            switch (accessor.ComponentType)
            {
                case 5120:
                case 5121:
                    bitsPerComponent = 8;
                    break;
                case 5122:
                case 5123:
                    bitsPerComponent = 16;
                    break;
                case 5125:
                case 5126:
                    bitsPerComponent = 32;
                    break;
            }
            return bitsPerComponent;
        }
        
        public static int TypeComponentAmount(this Accessor accessor)
        {
            var numberOfComponents = 0;
            switch (accessor.Type)
            {
                case "SCALAR":
                    numberOfComponents = 1;
                    break;
                case "VEC2":
                    numberOfComponents = 2;
                    break;
                case "VEC3":
                    numberOfComponents = 3;
                    break;
                case "VEC4":
                case "MAT2":
                    numberOfComponents = 4;
                    break;
                case "MAT3":
                    numberOfComponents = 9;
                    break;
                case "MAT4":
                    numberOfComponents = 16;
                    break;
            }
            return numberOfComponents;
        }
        
    }
    
}