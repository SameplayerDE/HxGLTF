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
            if (!extension.Equals(".gltf"))
            {
                throw new FileLoadException("file could not be loaded, wrong file type");
            }

            return LoadFromFile(path);
        }

        private static GLTFFile LoadFromFile(string path)
        {
            var o1 = JObject.Parse(File.ReadAllText(path));

            var jAsset = o1["asset"];
            var jScenes = o1["scenes"];
            var jNodes = o1["nodes"];
            var jMeshes = o1["meshes"];
            var jBufferViews = o1["bufferViews"];
            var jBuffers = o1["buffers"];
            var jImages = o1["images"];
            var jDummy = o1["dummy"];
            var jTextures = o1["textures"];
            var jSamplers = o1["samplers"];

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
            
            var samplers = new Sampler[jSamplers.Count()];
            for (var i = 0; i < jSamplers.Count(); i++)
            {
                var jToken = jSamplers[i];
                
                var sampler = new Sampler()
                {
                    WrapS = (int)jToken?["wrapS"],
                    WrapT = (int)jToken?["wrapT"],
                    MinFilter = (int)jToken?["minFilter"],
                    MagFilter = (int)jToken?["magFilter"]
                };
                
                samplers[i] = sampler;
            }
            
            var images = new Image[jImages.Count()];
            for (var i = 0; i < jImages.Count(); i++)
            {
                var jToken = jImages[i];
                
                var image = new Image
                {
                    Uri = (string)jToken?["uri"]
                };

                if (image.Uri == null)
                {
                    throw new Exception();
                }

                if (Path.IsPathRooted(image.Uri))
                {
                    if (!File.Exists(image.Uri))
                    {
                        throw new FileNotFoundException();
                    }
                }
                else
                {
                    var combinedPath = Path.Combine(Path.GetDirectoryName(path) ?? string.Empty, image.Uri);
                    if (!File.Exists(combinedPath))
                    {
                        throw new FileNotFoundException();
                    }
                }
                
                images[i] = image;
            }

            return new GLTFFile()
            {
                Asset = asset,
                Buffers = buffers,
                BufferViews = bufferViews,
                Images = images,
                Samplers = samplers
            };
        }
    }
}