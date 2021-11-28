using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;

namespace HxGLTF
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
        
        // ReSharper disable once InconsistentNaming
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
            
            if (chunkType1 == 0x004E4942) //Binary
            {
                
            }
            else if (chunkType0 == 0x4E4F534A) //Json
            {
                
            }

            return LoadFromJsonWithByteArray(path, json, array);
        }
        
        // ReSharper disable once InconsistentNaming
        private static GLTFFile LoadFromJsonWithByteArray(string path, string json, byte[] array)
        {
            var o1 = JObject.Parse(json);

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
            var jAccessors = o1["accessors"];

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
                    Uri = (string)(jToken?["uri"] ?? string.Empty),
                    ByteLength = (int)jToken?["byteLength"]
                };
                
                if (i == 0)
                {
                    if (buffer.Uri != string.Empty)
                    {
                        throw new Exception("uri of first buffer must be undefined");
                    }
                    buffer.Bytes = array;
                    buffers[i] = buffer;
                    continue;
                }
                
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
                    ByteOffset = (int)jToken?["byteOffset"],
                    ByteStride = (int)(jToken?["byteStride"] ?? 0)
                };
                bufferViews[i] = bufferView;
            }
            
            var accessors = new Accessor[jAccessors.Count()];
            for (var i = 0; i < jAccessors.Count(); i++)
            {
                var jToken = jAccessors[i];
                
                var accessor = new Accessor
                {
                    BufferView = bufferViews[(int)jToken?["bufferView"]],
                    ByteOffset = (int)(jToken?["byteOffset"] ?? 0),
                    Count = (int)jToken?["count"],
                    ComponentType = ComponentType.FromInt((int)jToken?["componentType"]),
                    Type = Type.FromSting((string)jToken?["type"])
                };
                accessors[i] = accessor;
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
                        throw new FileNotFoundException("images specified in glb file could not be found");
                    }
                }
                else
                {
                    var combinedPath = Path.Combine(Path.GetDirectoryName(path) ?? string.Empty, image.Uri);
                    if (!File.Exists(combinedPath))
                    {
                        throw new FileNotFoundException("images specified in glb file could not be found");
                    }
                }
                
                images[i] = image;
            }

            var meshes = new Mesh[jMeshes.Count()];
            for (var i = 0; i < jMeshes.Count(); i++)
            {
                var jMeshToken = jMeshes[i];
                var jMeshPrimitiveToken = jMeshToken?["primitives"];
                
                var primitives = new Primitive[jMeshPrimitiveToken.Count()];
                for (var j = 0; j < jMeshPrimitiveToken.Count(); j++)
                {

                    var jPrimitiveObject = (JObject)jMeshPrimitiveToken?[j];
                    var jMeshPrimitiveAttribute = (JObject)jPrimitiveObject["attributes"];
                    var attributes = new Attribute[jMeshPrimitiveAttribute.Count];

                    var x = 0;
                    foreach (var attributeData in jMeshPrimitiveAttribute)
                    {
                        var attribute = new Attribute()
                        {
                            Type = attributeData.Key,
                            Accessor = accessors[(int)attributeData.Value]
                        };
                        attributes[x] = attribute;
                        x++;
                    }
                    var primitive = new Primitive()
                    { 
                        Attributes = attributes,
                        Indices = (int)(jPrimitiveObject["indices"] ?? null),
                        Material = (int)(jPrimitiveObject["material"] ?? null)
                    };
                    primitives[j] = primitive;
                    //Console.WriteLine((string)jMeshPrimitiveToken);
                }
                
                var mesh = new Mesh()
                {
                    Name = (string)(jMeshToken?["name"] ?? string.Empty),
                    Primitives = primitives
                };
                
                meshes[i] = mesh;
            }

            return new GLTFFile()
            {
                Asset = asset,
                Buffers = buffers,
                BufferViews = bufferViews,
                Accessors = accessors,
                Images = images,
                Samplers = samplers,
                Meshes = meshes
            };
        }

        // ReSharper disable once InconsistentNaming
        private static GLTFFile LoadFromGLTFFile(string path)
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
            var jAccessors = o1["accessors"];

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
                var jObject = jBufferViews[i];
                
                var bufferView = new BufferView
                {
                    Buffer = buffers[(int)jObject?["buffer"]],
                    ByteLength = (int)jObject?["byteLength"],
                    ByteOffset = (int)jObject?["byteOffset"],
                    ByteStride = (int)(jObject?["byteStride"] ?? 0)
                };
                bufferViews[i] = bufferView;
            }
            
            var accessors = new Accessor[jAccessors.Count()];
            for (var i = 0; i < jAccessors.Count(); i++)
            {
                var jObject = jAccessors[i];
                
                var accessor = new Accessor
                {
                    BufferView = bufferViews[(int)jObject?["bufferView"]],
                    ByteOffset = (int)(jObject?["byteOffset"] ?? 0),
                    Count = (int)jObject?["count"],
                    ComponentType = ComponentType.FromInt((int)jObject?["componentType"]),
                    Type = Type.FromSting((string)jObject?["type"])
                };
                accessors[i] = accessor;
            }
            
            var samplers = new Sampler[jSamplers.Count()];
            for (var i = 0; i < jSamplers.Count(); i++)
            {
                var jObject = jSamplers[i];
                
                var sampler = new Sampler()
                {
                    WrapS = (int)jObject?["wrapS"],
                    WrapT = (int)jObject?["wrapT"],
                    MinFilter = (int)jObject?["minFilter"],
                    MagFilter = (int)jObject?["magFilter"]
                };
                
                samplers[i] = sampler;
            }
            
            var images = new Image[jImages.Count()];
            for (var i = 0; i < jImages.Count(); i++)
            {
                var jObject = jImages[i];
                
                var image = new Image
                {
                    Uri = (string)jObject?["uri"]
                };

                if (image.Uri == null)
                {
                    throw new Exception();
                }

                if (Path.IsPathRooted(image.Uri))
                {
                    if (!File.Exists(image.Uri))
                    {
                        throw new FileNotFoundException("images specified in gltf file could not be found");
                    }
                }
                else
                {
                    var combinedPath = Path.Combine(Path.GetDirectoryName(path) ?? string.Empty, image.Uri);
                    if (!File.Exists(combinedPath))
                    {
                        throw new FileNotFoundException("images specified in gltf file could not be found");
                    }
                }
                
                images[i] = image;
            }
            
            var meshes = new Mesh[jMeshes.Count()];
            for (var i = 0; i < jMeshes.Count(); i++)
            {
                var jMeshToken = jMeshes[i];
                var jMeshPrimitiveToken = jMeshToken?["primitives"];
                
                var primitives = new Primitive[jMeshPrimitiveToken.Count()];
                for (var j = 0; j < jMeshPrimitiveToken.Count(); j++)
                {

                    var jPrimitiveObject = (JObject)jMeshPrimitiveToken?[j];
                    var jMeshPrimitiveAttribute = (JObject)jPrimitiveObject["attributes"];
                    var attributes = new Attribute[jMeshPrimitiveAttribute.Count];

                    var x = 0;
                    foreach (var attributeData in jMeshPrimitiveAttribute)
                    {
                        var attribute = new Attribute()
                        {
                            Type = attributeData.Key,
                            Accessor = accessors[(int)attributeData.Value]
                        };
                        attributes[x] = attribute;
                        x++;
                    }
                    var primitive = new Primitive()
                    { 
                        Attributes = attributes,
                        Indices = (int)(jPrimitiveObject["indices"] ?? null),
                        Material = (int)(jPrimitiveObject["material"] ?? null)
                    };
                    primitives[j] = primitive;
                    //Console.WriteLine((string)jMeshPrimitiveToken);
                }
                
                var mesh = new Mesh()
                {
                    Name = (string)(jMeshToken?["name"] ?? string.Empty),
                    Primitives = primitives
                };
                
                meshes[i] = mesh;
            }

            return new GLTFFile()
            {
                Asset = asset,
                Buffers = buffers,
                BufferViews = bufferViews,
                Accessors = accessors,
                Images = images,
                Samplers = samplers,
                Meshes = meshes
            };
        }
    }
}