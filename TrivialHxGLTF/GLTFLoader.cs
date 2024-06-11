using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TrivialHxGLTF
{
    // ReSharper disable once InconsistentNaming
    public static class GLTFLoader
    {
        public static GLTFFile Load(string path)
        {
            if (Directory.Exists(path))
            {
                throw new Exception("passed directory path");
            }
            
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("file could not be found");
            }

            var extension = Path.GetExtension(path);
            if (!extension.Equals(".gltf") && !extension.Equals(".glb"))
            {
                throw new FileLoadException("file could not be loaded, wrong file type");
            }
            return extension.Equals(".glb") ? LoadFromGLBFile(path) : LoadFromGLTFFile(path);
        }

        private static GLTFFile LoadFromGLTFFile(string path)
        {
            var gltfFile = JsonConvert.DeserializeObject<GLTFFile>(File.ReadAllText(path));
            foreach (var buffer in gltfFile.Buffers)
            {
                if (buffer.Uri.StartsWith("data:"))
                {
                    //read bytes in the file
                }
                else
                {
                    if (buffer.Uri == null)
                    {
                        throw new Exception();
                    }

                    if (Path.IsPathRooted(buffer.Uri))
                    {
                        if (!File.Exists(buffer.Uri))
                        {
                            throw new FileNotFoundException();
                        }
                
                        buffer.Data = File.ReadAllBytes(buffer.Uri);
                    }
                    else
                    {
                        var combinedPath = Path.Combine(Path.GetDirectoryName(path) ?? string.Empty, buffer.Uri);
                        if (!File.Exists(combinedPath))
                        {
                            throw new FileNotFoundException();
                        }
                
                        buffer.Data = File.ReadAllBytes(combinedPath);
                    }
                }
            }

            gltfFile.Path = path;
            return gltfFile;
        }
        
        private static GLTFFile LoadFromGLBFile(string path)
        {
            var glbBytes = File.ReadAllBytes(path);
            var stream = new MemoryStream(glbBytes);

            var magic = BitConverter.ToUInt32(glbBytes, 0);

            if (magic != 0x46546C67)
            {
                throw new Exception("file is damaged");
            }

            var version = BitConverter.ToUInt32(glbBytes, 4);
            var length = BitConverter.ToUInt32(glbBytes, 8);
            
            var chunkLenght0 = BitConverter.ToUInt32(glbBytes, 12);
            var chunkType0 = BitConverter.ToUInt32(glbBytes, 16);
            
            var chunkLenght1 = BitConverter.ToUInt32(glbBytes, 20 + (int)chunkLenght0);
            var chunkType1 = BitConverter.ToUInt32(glbBytes, 20 + (int)chunkLenght0 + 4);
            
            stream.Position = 20;
            var chunkData0 = new byte[chunkLenght0];
            stream.Read(chunkData0, 0, (int)chunkLenght0);
            var json = System.Text.Encoding.UTF8.GetString(chunkData0);
            
            stream.Position = 20 + (int)chunkLenght0 + 8;
            var chunkData1 = new byte[chunkLenght1];
            stream.Read(chunkData1, 0, (int)chunkLenght1);
            var array = chunkData1;
            
            return LoadFromJsonWithByteArray(path, json, array);
        }

        private static GLTFFile LoadFromJsonWithByteArray(string path, string json, byte[] array)
        {
            var glbFile = JsonConvert.DeserializeObject<GLTFFile>(json);
            glbFile.Path = path;
            int i = 0;
            foreach (var buffer in glbFile.Buffers)
            {
                if (i == 0)
                {
                    if (!string.IsNullOrEmpty(buffer.Uri))
                    {
                        throw new Exception("uri of first buffer must be undefined");
                    }
                    buffer.Data = array;
                }
                else
                {
                    if (string.IsNullOrEmpty(buffer.Uri))
                    {
                        throw new Exception();
                    }

                    if (Path.IsPathRooted(buffer.Uri))
                    {
                        if (!File.Exists(buffer.Uri))
                        {
                            throw new FileNotFoundException();
                        }
                
                        buffer.Data = File.ReadAllBytes(buffer.Uri);
                    }
                    else
                    {
                        var combinedPath = Path.Combine(Path.GetDirectoryName(path) ?? string.Empty, buffer.Uri);
                        if (!File.Exists(combinedPath))
                        {
                            throw new FileNotFoundException();
                        }
                
                        buffer.Data = File.ReadAllBytes(combinedPath);
                    }
                }  
            }

            glbFile.Buffers[0].Data = array;
            return glbFile;
        }
        
        public static float[] ReadAccessor(this GLTFFile gltfFile, Accessor accessor)
        {
            
            var elementCount = accessor.Count;
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

            var bitsPerComponent = 0;

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
            
            var bytesPerComponent = bitsPerComponent / 8;
            var totalAmountOfBytes = bytesPerComponent * numberOfComponents * elementCount;// * (byteStride != 0 ? byteStride : 1);
            
            var bufferView = gltfFile.BufferViews[accessor.BufferView];
            var buffer = bufferView.Buffer;
            var byteStride = bufferView.ByteStride ?? 0;
            if (buffer == null) return null;
            var totalByteOffset = accessor.ByteOffset + (bufferView.ByteOffset ?? 0);

            var result = new List<float>();

            var stream = new MemoryStream(gltfFile.Buffers[buffer.Value].Data);
            stream.Position = totalByteOffset;
            var data = new byte[totalAmountOfBytes];
            stream.Read(data, 0, totalAmountOfBytes);
            
            
            /*
            Console.WriteLine("ComponentType: " + accessor.ComponentType);
            Console.WriteLine("ComponentTypeBits: " + bitsPerComponent);
            Console.WriteLine("ComponentTypeByte: " + bytesPerComponent);
            Console.WriteLine("Type: " + accessor.Type);
            Console.WriteLine("ComponentCount: " + numberOfComponents);
            Console.WriteLine("ElementCount: " + elementCount);
            Console.WriteLine("ByteStride: " + byteStride);
            Console.WriteLine("TotalAmountOfBytes: " + totalAmountOfBytes);
            Console.WriteLine("BufferViewByteAmount: " + bufferView.ByteLength);
            */

            //var bytes = new byte[numberOfComponents * bytesPerComponent];
            var bytes = new List<byte>();
            var value = 0.0f;
            for (var i = 0; i < totalAmountOfBytes; i += numberOfComponents * bytesPerComponent)
            {
                for (var k = 0; k < numberOfComponents * bytesPerComponent; k += bytesPerComponent)
                {
                    //Console.Write("    ");
                    bytes.Clear();
                    for (var j = 0; j < bytesPerComponent; j++)
                    {
                        //Console.Write($"0x{data[i + j + k]:X2} ");
                        //bytes[k] = data[i + j + k];
                        bytes.Add(data[i + j + k]);
                    }

                    switch (accessor.ComponentType)
                    {
                        case 5126:
                            value = BitConverter.ToSingle(bytes.ToArray(), 0);
                            break;
                        case 5125:
                            value = BitConverter.ToUInt32(bytes.ToArray(), 0);
                            break;
                        case 5123:
                            value = BitConverter.ToUInt16(bytes.ToArray(), 0);
                            break;
                        case 5122:
                            value = BitConverter.ToInt16(bytes.ToArray(), 0);
                            break;
                        case 5121:
                            try
                            {
                                value = Convert.ToSByte(bytes.ToArray()[0]);
                            }
                            catch (OverflowException)
                            {
                                value = Convert.ToByte(bytes.ToArray()[0]);
                            }

                            break;
                        case 5120:
                            value = Convert.ToByte(bytes.ToArray()[0]);
                            break;
                    }
                    result.Add(value);
                    //Console.Write($" = {value}");
                    //Console.Write($"\n");
                }
                //i += byteStride;
            }
            return result.ToArray();
        }
        
        public static float[] ReadAccessorIndexed(this GLTFFile gltfFile, Accessor dataAccessor, Accessor indexAccessor)
        {
            
            var data = gltfFile.ReadAccessor(dataAccessor);
            var indices = gltfFile.ReadAccessor(indexAccessor);

            //var result = new float[indexAccessor.Count * dataAccessor.TypeComponentAmount()];
            var result = new List<float>();
            for (var x = 0; x < indices.Length; x++)
            {
                var index = (ushort)indices[x];
                for (var j = 0; j < dataAccessor.TypeComponentAmount(); j++)
                {
                    var calculatedIndex = index * dataAccessor.TypeComponentAmount() + j;
                    var d = data[calculatedIndex];
                    result.Add(d);
                    //result[calculatedIndex] = d;
                }
            }

            return result.ToArray();
        }
        
    }
}