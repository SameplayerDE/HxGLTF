using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TrivialHxGLTF
{
    // ReSharper disable once InconsistentNaming
    public class GLTFLoader
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
            int i = 0;
            foreach (var buffer in glbFile.Buffers)
            {
                if (i == 0)
                {
                    if (buffer.Uri != string.Empty)
                    {
                        throw new Exception("uri of first buffer must be undefined");
                    }
                    buffer.Data = array;
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

            glbFile.Buffers[0].Data = array;
            return glbFile;
        }
    }
}