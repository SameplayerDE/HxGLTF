using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;

namespace HxGLTF
{
    public class GLTFLoader
    {
        public static void Load(string path)
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
            if (!extension.Equals(".gltf"))
            {
                throw new FileLoadException("file could not be loaded, wrong file type");
            }

            LoadFromFile(path);

        }

        private static void LoadFromFile(string path)
        {
            var o1 = JObject.Parse(File.ReadAllText(path));

            var jAsset = o1["asset"];
            var jScenes = o1["scenes"];
            var jNodes = o1["nodes"];
            var jMeshes = o1["meshes"];
            var jBufferViews = o1["bufferViews"];
            var jBuffers = o1["buffers"];

            if (jBuffers == null || jBufferViews == null || jAsset == null)
            {
                throw new Exception();
            }
            
            var asset = new Asset
            {
                Version = (string)jAsset["version"]
            };

            var buffers = new Buffer[jBuffers.Count()];
            for (var i = 0; i < jBuffers.Count(); i++)
            {
                var jToken = jBuffers[i];
                
                var buffer = new Buffer
                {
                    Uri = (string)jToken?["uri"],
                    ByteLength = (int)jToken?["byteLength"]
                };

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
                
                    buffer.Bytes = File.ReadAllBytes(buffer.Uri);
                }
                else
                {
                    var combinedPath = Path.Combine(Path.GetDirectoryName(path) ?? string.Empty, buffer.Uri);
                    if (!File.Exists(combinedPath))
                    {
                        throw new FileNotFoundException();
                    }
                
                    buffer.Bytes = File.ReadAllBytes(combinedPath);
                }
                
                buffers[i] = buffer;
            }
            
            var bufferViews = new BufferView[jBufferViews.Count()];
            for (var i = 0; i < jBufferViews.Count(); i++)
            {
                var jToken = jBufferViews[i];
                
                var bufferView = new BufferView
                {
                    Buffer = buffers[(int)jToken?["buffer"]],
                    ByteLength = (int)jToken?["byteLength"],
                    ByteOffset = (int)jToken?["byteOffset"]
                };

                if (jToken?["byteStride"] != null)
                {
                    bufferView.ByteStride = (int)jToken["byteStride"];
                }
                
                bufferViews[i] = bufferView;
            }
        }
    }
}