using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using HxGLTF.Core.PrimitiveDataStructures;
using HxGLTF.Core;
using Buffer = HxGLTF.Core.Buffer;

namespace HxGLTF
{
    public class GLTFLoader
    {
        public static void LoadSmart(string pathOrBaseName)
        {
            if (string.IsNullOrEmpty(pathOrBaseName))
            {
                throw new ArgumentNullException(nameof(pathOrBaseName), "File path or base name cannot be null or empty.");
            }
            string path = ValidateFileExists(pathOrBaseName);
            using (var fileStream = File.OpenRead(path))
            {
                var extension = Path.GetExtension(path);
                ValidateFileType(extension);

                byte[]? glbBytes = null;
                string? json = null;
                byte[]? binary = null;

                if (extension.Equals(".glb", StringComparison.OrdinalIgnoreCase))
                {
                    glbBytes = ExtractGLBBytes(fileStream);
                    json = ExtractJSONFromGLB(glbBytes);
                    binary = ExtractBinaryFromGLB(glbBytes, json.Length);
                }
                else
                {
                    json = ExtractJSONFromFile(fileStream);
                }

                LoadFromJsonWithBinarySmart(path, json, binary);
            }
        }

        private static void LoadFromJsonWithBinarySmart(string path, string json, byte[]? buffer = null)
        {
            var jObject = JObject.Parse(json);

            var jAsset = jObject["asset"];
            if (jAsset == null)
            {
                throw new Exception();
            }

            var jScenes = jObject["scenes"];
            if (jScenes == null)
            {
                return;
            }

            var sceneCount = jScenes.Count();
            var scenes = new Scene[sceneCount];

            for (int a = 0; a < sceneCount; a++)
            {
                var jScene = jScenes[a];
                if (jScene == null)
                {
                    throw new Exception();
                }
                var scene = new Scene();

                var jSceneName = jScene["name"];
                if (jSceneName != null)
                {
                    var sceneName = jSceneName.ToString();
                    scene.Name = sceneName;
                }

                var jSceneNodeIndices = jScene["nodes"];
                if (jSceneNodeIndices != null)
                {
                    var sceneNodeIndices = jSceneNodeIndices.Value<int[]>();
                    scene.NodesIndices = sceneNodeIndices;

                    var jNodes = jObject["nodes"];
                    if (jNodes == null)
                    {
                        throw new Exception();
                    }

                    foreach (var nodeIndex in scene.NodesIndices!)
                    {
                        var jNode = jNodes[nodeIndex];
                        if (jNode == null)
                        {
                            throw new Exception();
                        }
                        var node = new Node();

                        var jNodeName = jNode["name"];
                        if (jNodeName != null && jNodeName.Type == JTokenType.String)
                        {
                            node.Name = jNodeName.ToString();
                        }

                        var jNodeSkin = jNode["skin"];
                        if (jNodeSkin != null)
                        {
                            int skinIndex = jNodeSkin.Value<int>();
                            node.SkinIndex = skinIndex;
                        }

                        var jNodeMesh = jNode["mesh"];
                        if (jNodeMesh != null)
                        {
                            int meshIndex = jNodeMesh.Value<int>();
                            node.MeshIndex = meshIndex;
                        }
                    }

                }

                scenes[a] = scene;
            }


        }

        public static GLTFFile Load(string pathOrBaseName)
        {
            if (string.IsNullOrEmpty(pathOrBaseName))
            {
                throw new ArgumentNullException(nameof(pathOrBaseName), "File path or base name cannot be null or empty.");
            }

            string path = ValidateFileExists(pathOrBaseName);

            using (var fileStream = File.OpenRead(path))
            {
                var extension = Path.GetExtension(path);
                ValidateFileType(extension);

                byte[]? glbBytes = null;
                string? json = null;
                byte[]? binary = null;

                if (extension.Equals(".glb", StringComparison.OrdinalIgnoreCase))
                {
                    glbBytes = ExtractGLBBytes(fileStream);
                    json = ExtractJSONFromGLB(glbBytes);
                    binary = ExtractBinaryFromGLB(glbBytes, json.Length);
                }
                else
                {
                    json = ExtractJSONFromFile(fileStream);
                }
                
                return LoadFromJsonWithBinary(path, json, binary);
            }
        }

        private static string ValidateFileExists(string pathOrBaseName)
        {
            string gltfPath = pathOrBaseName.EndsWith(".gltf", StringComparison.OrdinalIgnoreCase) ? pathOrBaseName : pathOrBaseName + ".gltf";
            string glbPath = pathOrBaseName.EndsWith(".glb", StringComparison.OrdinalIgnoreCase) ? pathOrBaseName : pathOrBaseName + ".glb";

            bool gltfExists = File.Exists(gltfPath);
            bool glbExists = File.Exists(glbPath);

            // If exact file path is provided and exists
            if (File.Exists(pathOrBaseName))
            {
                return pathOrBaseName;
            }
            // If both .gltf and .glb exist and no extension was provided
            else if (gltfExists && glbExists && !Path.HasExtension(pathOrBaseName))
            {
                throw new InvalidOperationException("Both .gltf and .glb files exist. Specify the file extension explicitly.");
            }
            else if (gltfExists)
            {
                return gltfPath;
            }
            else if (glbExists)
            {
                return glbPath;
            }
            else
            {
                throw new FileNotFoundException("Neither .gltf nor .glb file found with the specified name.", pathOrBaseName);
            }
        }

        private static void ValidateFileType(string extension)
        {
            if (!extension.Equals(".gltf", StringComparison.OrdinalIgnoreCase) &&
                !extension.Equals(".glb", StringComparison.OrdinalIgnoreCase))
            {
                throw new FileLoadException("Invalid file type.");
            }
        }

        private static byte[] ExtractGLBBytes(FileStream fileStream)
        {
            var glbBytes = new byte[fileStream.Length];
            fileStream.Read(glbBytes, 0, glbBytes.Length);
            return glbBytes;
        }

        private static string ExtractJSONFromGLB(byte[] glbBytes)
        {
            if (glbBytes.Length < 20)
            {
                throw new Exception("Invalid GLB file format.");
            }

            var magic = BitConverter.ToUInt32(glbBytes, 0);
            if (magic != 0x46546C67)
            {
                throw new Exception("Invalid GLB file format.");
            }

            var chunkLength0 = BitConverter.ToUInt32(glbBytes, 12);
            if (glbBytes.Length < chunkLength0 + 20)
            {
                throw new Exception("Invalid GLB file format.");
            }

            return System.Text.Encoding.UTF8.GetString(glbBytes, 20, (int)chunkLength0);
        }

        private static string ExtractJSONFromFile(FileStream fileStream)
        {
            using (var reader = new StreamReader(fileStream))
            {
                return reader.ReadToEnd();
            }
        }

        private static byte[] ExtractBinaryFromGLB(byte[] glbBytes, int jsonLength)
        {
            if (glbBytes.Length < jsonLength + 28)
            {
                throw new Exception("Invalid GLB file format.");
            }

            var chunkLength1 = BitConverter.ToUInt32(glbBytes, 20 + jsonLength);
            var startIndex = 20 + jsonLength + 8;
            if (glbBytes.Length < startIndex + chunkLength1)
            {
                throw new Exception("Invalid GLB file format.");
            }

            var binChunkData = new byte[chunkLength1];
            Array.Copy(glbBytes, startIndex, binChunkData, 0, (int)chunkLength1);
            return binChunkData;
        }

        private static GLTFFile LoadFromJsonWithBinary(string path, string json, byte[] binary = null)
        {
            var jObject = JObject.Parse(json);

            // Überprüfe die erforderlichen Elemente
            if (jObject["asset"] == null)
            {
                throw new ArgumentException("gltf file is missing data.");
            }

            var asset = LoadAsset(jObject["asset"]);
            var extensionsUsed = LoadExtensionsUsed(jObject["extensionsUsed"]);
            var buffers = jObject["buffers"] != null ? LoadBuffers(path, jObject["buffers"], binary) : null;
            var bufferViews = jObject["bufferViews"] != null ? LoadBufferViews(jObject["bufferViews"], buffers) : null;
            var accessors = jObject["accessors"] != null ? LoadAccessors(jObject["accessors"], bufferViews) : null;
            var samplers = jObject["samplers"] != null ? LoadSamplers(jObject["samplers"]) : null;
            var images = jObject["images"] != null ? LoadImages(path, jObject["images"], bufferViews) : null;
            var textures = jObject["textures"] != null ? LoadTextures(jObject["textures"], samplers, images) : null;
            var materials = jObject["materials"] != null ? LoadMaterials(jObject["materials"], textures) : null;
            var meshes = jObject["meshes"] != null ? LoadMeshes(jObject["meshes"], accessors, materials) : null;
            var nodes = jObject["nodes"] != null ? LoadNodes(jObject["nodes"], meshes) : null;
            var animations = jObject["animations"] != null ? LoadAnimations(jObject["animations"], accessors, nodes) : null;
            var skins = jObject["skins"] != null ? LoadSkins(jObject["skins"], accessors, nodes) : null;
            var scenes = jObject["scenes"] != null ? LoadScenes(jObject["scenes"], nodes) : null;

            if (nodes != null && skins != null)
            {
                LinkNodesAndSkins(nodes, skins);
            }

            return new GLTFFile()
            {
                Path = path,
                Asset = asset,
                ExtensionsUsed = extensionsUsed,
                Buffers = buffers,
                BufferViews = bufferViews,
                Accessors = accessors,
                Images = images,
                Samplers = samplers,
                Textures = textures,
                Materials = materials,
                Meshes = meshes,
                Nodes = nodes,
                Animations = animations,
                Skins = skins,
                Scenes = scenes
            };
        }

        private static Asset LoadAsset(JToken jAsset)
        {
            var versionToken = jAsset["version"];
            if (versionToken == null)
            {
                throw new Exception("Version field is missing.");
            }

            var asset = new Asset
            {
                Version = versionToken.ToString(),
                Copyright = (string?)jAsset["copyright"],
                Generator = (string?)jAsset["generator"],
                MinVersion = (string?)jAsset["minVersion"]
            };

            return asset;
        }
        
        private static string[] LoadExtensionsUsed(JToken jExtensionsUsed)
        {
            if (jExtensionsUsed == null || jExtensionsUsed.Type != JTokenType.Array)
            {
                return Array.Empty<string>();
            }

            return jExtensionsUsed.Select(x => x.ToString()).ToArray();
        }
        
        public static Buffer[] LoadBuffers(string path, JToken jBuffers, byte[] array = null)
        {
            var buffers = new Buffer[jBuffers.Count()];
            for (var i = 0; i < jBuffers.Count(); i++)
            {
                var jToken = jBuffers[i];

                var buffer = new Buffer
                {
                    Uri = (string?)jToken?["uri"],
                    ByteLength = (int)jToken?["byteLength"]
                };

                if (string.IsNullOrEmpty(buffer.Uri))
                {
                    buffer.Bytes = array;
                }
                else
                {
                    if (buffer.Uri.StartsWith("data:"))
                    {
                        buffer.Bytes = LoadBase64Buffer(buffer.Uri);
                    }
                    else
                    {
                        buffer.Bytes = LoadBufferBytes(path, buffer.Uri);
                    }
                }
                buffers[i] = buffer;
            }
            return buffers;
        }

        private static byte[] LoadBufferBytes(string path, string uri)
        {
            var combinedPath = Path.IsPathRooted(uri) ? uri : Path.Combine(Path.GetDirectoryName(path) ?? string.Empty, uri);
            if (!File.Exists(combinedPath))
            {
                throw new FileNotFoundException();
            }
            return File.ReadAllBytes(combinedPath);
        }

        private static byte[] LoadBase64Buffer(string base64Uri)
        {
            var base64Data = base64Uri.Substring(base64Uri.IndexOf(",") + 1);
            return Convert.FromBase64String(base64Data);
        }

        private static BufferView[] LoadBufferViews(JToken jBufferViews, Buffer[] buffers)
        {
            var bufferViews = new BufferView[jBufferViews.Count()];
            for (var i = 0; i < jBufferViews.Count(); i++)
            {
                var jToken = jBufferViews[i];
                bufferViews[i] = new BufferView
                {
                    Buffer = buffers[(int)jToken["buffer"]],
                    ByteLength = (int)jToken["byteLength"],
                    ByteOffset = (int)(jToken["byteOffset"] ?? 0),
                    ByteStride = (int)(jToken["byteStride"] ?? 0)
                };
            }
            return bufferViews;
        }

        private static Accessor[] LoadAccessors(JToken jAccessors, BufferView[] bufferViews)
        {
            var accessors = new Accessor[jAccessors.Count()];
            for (var i = 0; i < jAccessors.Count(); i++)
            {
                var jToken = jAccessors[i];
                accessors[i] = new Accessor
                {
                    BufferView = bufferViews[(int)jToken["bufferView"]],
                    ByteOffset = (int)(jToken["byteOffset"] ?? 0),
                    Count = (int)jToken["count"],
                    Normalized = (bool?)jToken["normalized"] ?? false,
                    DataType = ComponentDataType.FromInt((int)jToken["componentType"]),
                    StructureType = StructureType.FromSting((string)jToken["type"]),
                    Name = (string?)jToken["name"] ?? null,
                };
            }
            return accessors;
        }

        private static TextureSampler[] LoadSamplers(JToken jSamplers)
        {
            if (jSamplers == null) return null;

            var samplers = new TextureSampler[jSamplers.Count()];
            for (var i = 0; i < jSamplers.Count(); i++)
            {
                var jSampler = jSamplers[i];

                if (!jSampler.HasValues)
                {
                    continue;
                }

                int wrapS = jSampler?["wrapS"] != null ? (int)jSampler["wrapS"] : 10497;
                int wrapT = jSampler?["wrapT"] != null ? (int)jSampler["wrapT"] : 10497;
                int? minFilter = jSampler?["minFilter"] != null ? (int?)jSampler["minFilter"] : null;
                int? magFilter = jSampler?["magFilter"] != null ? (int?)jSampler["magFilter"] : null;

                var sampler = new TextureSampler()
                {
                    WrapS = wrapS,
                    WrapT = wrapT,
                    MinFilter = minFilter,
                    MagFilter = magFilter
                };

                samplers[i] = sampler;
            }
            return samplers;
        }

        private static Image[] LoadImages(string path, JToken jImages, BufferView[] bufferViews)
        {
            var images = new Image[jImages.Count()];
            for (var i = 0; i < jImages.Count(); i++)
            {
                var jImage = jImages[i];

                var image = new Image
                {
                    Uri = (string?)jImage?["uri"]
                };

                var jImageName = jImage["name"];
                if (jImageName != null)
                {
                    image.Name = jImageName.ToString();
                }

                if (image.Uri == null)
                {
                    var bufferViewIndex = (int)jImage?["bufferView"];
                    var bufferView = bufferViews[bufferViewIndex];
                    var mimeType = MiMeType.FromString((string)jImage["mimeType"]);

                    image.BufferView = bufferView;
                    image.MiMeType = mimeType;
                }
                else
                {
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
                }

                images[i] = image;
            }
            return images;
        }

        private static Texture[] LoadTextures(JToken jTextures, TextureSampler[] samplers, Image[] images)
        {
            var textures = new Texture[jTextures.Count()];
            for (var i = 0; i < jTextures.Count(); i++)
            {
                var jToken = jTextures[i];
                textures[i] = new Texture
                {
                    Sampler = jToken["sampler"] != null ? samplers[(int)jToken["sampler"]] : null,
                    Source = images[(int)jToken["source"]]
                };
            }
            return textures;
        }

        private static Material[] LoadMaterials(JToken jMaterials, Texture[] textures)
        {
            var materials = new Material[jMaterials.Count()];
            for (var i = 0; i < jMaterials.Count(); i++)
            {
                var jMaterial = jMaterials[i];
                if (jMaterial == null)
                {
                    throw new Exception();
                }

                var material = new Material();
                
                var jName = jMaterial["name"];
                if (jName != null)
                {
                    material.Name = jName.ToString();
                }

                var jAlphaMode = jMaterial["alphaMode"];
                if (jAlphaMode != null)
                {
                    material.AlphaMode = jAlphaMode.ToString();
                }

                var jAlphaCutoff = jMaterial["alphaCutoff"];
                if (jAlphaCutoff != null)
                {
                    material.AlphaCutoff = jAlphaCutoff.ToObject<float>();
                }

                var jDoubleSided = jMaterial["doubleSided"];
                if (jDoubleSided != null)
                {
                    material.DoubleSided = jDoubleSided.ToObject<bool>();
                }

                var jEmissiveFactor = jMaterial["emissiveFactor"];
                if (jEmissiveFactor != null)
                {
                    var rgb = jEmissiveFactor.ToObject<float[]>();
                    material.EmissiveFactor = new Color(rgb[0], rgb[1], rgb[2]);
                }
                
                var jEmissiveTextureInfo = jMaterial["emissiveTexture"];
                if (jEmissiveTextureInfo != null)
                {
                    var jIndex = jEmissiveTextureInfo["index"];
                    if (jIndex == null)
                    {
                        throw new Exception();
                    }

                    var index = jIndex.ToObject<int>();
                    material.EmissiveTexture = textures[index];
                }
                
                var jNormalMap = jMaterial["normalTexture"];
                if (jNormalMap != null)
                {
                    var jIndex = jNormalMap["index"];
                    if (jIndex == null)
                    {
                        throw new Exception();
                    }

                    var index = jIndex.ToObject<int>();
                    material.NormalTexture = textures[index];
                }
                
                var jPbr = jMaterial["pbrMetallicRoughness"];
                if (jPbr != null)
                {
                    var jBaseColorFactor = jPbr["baseColorFactor"];
                    if (jBaseColorFactor != null)
                    {
                        var rgba = jBaseColorFactor.ToObject<float[]>();
                        //if (material.Name == "shade")
                        //{
                        //    material.BasColorFactor = new Color(0, 0, 0, rgba[3]);
                        //}
                        //else
                        //{
                        //    material.BasColorFactor = new Color(rgba[0], rgba[1], rgba[2], rgba[3]);
                        //}
                        material.BasColorFactor = new Color(rgba[0], rgba[1], rgba[2], rgba[3]);
                    }

                    var jMetallicFactor = jPbr["metallicFactor"];
                    if (jMetallicFactor != null)
                    {
                        
                    }

                    var jBaseColorTextureInfo = jPbr["baseColorTexture"];
                    if (jBaseColorTextureInfo != null)
                    {
                        var jIndex = jBaseColorTextureInfo["index"];
                        if (jIndex == null)
                        {
                            throw new Exception();
                        }

                        var index = jIndex.ToObject<int>();
                        material.BaseColorTexture = textures[index];
                    }
                }
                
                var jExtensions = jMaterial["extensions"];
                if (jExtensions != null)
                {
                    var jKHRMaterials = jExtensions["KHR_materials_pbrSpecularGlossiness"];
                    if (jKHRMaterials != null)
                    {
                        var jDiffuseTexture = jKHRMaterials["diffuseTexture"];
                        if (jDiffuseTexture != null)
                        {
                            var jIndex = jDiffuseTexture["index"];
                            if (jIndex == null)
                            {
                                throw new Exception("Invalid diffuse texture index in material " + i);
                            }

                            var index = jIndex.ToObject<int>();
                            material.DiffuseTexture = textures[index];
                        }

                        var jDiffuseFactor = jKHRMaterials["diffuseFactor"];
                        if (jDiffuseFactor != null)
                        {
                            var rgba = jDiffuseFactor.ToObject<float[]>();
                            material.DiffuseFactor = new Color(rgba[0], rgba[1], rgba[2], rgba[3]);
                        }

                        var jSpecularFactor = jKHRMaterials["specularFactor"];
                        if (jSpecularFactor != null)
                        {
                            var rgb = jSpecularFactor.ToObject<float[]>();
                            material.SpecularFactor = new Color(rgb[0], rgb[1], rgb[2], 1);
                        }

                        var jGlossinessFactor = jKHRMaterials["glossinessFactor"];
                        if (jGlossinessFactor != null)
                        {
                            material.GlossinessFactor = jGlossinessFactor.ToObject<float>();
                        }
                    }
                }

                materials[i] = material;
            }
            return materials;
        }

        private static Mesh[] LoadMeshes(JToken jMeshes, Accessor[] accessors, Material[] materials)
        {
            var meshes = new Mesh[jMeshes.Count()];

            for (var a = 0; a < jMeshes.Count(); a++)
            {
                var jMesh = jMeshes[a];
                if (jMesh == null)
                {
                    throw new Exception($"Mesh at index {a} is null.");
                }
                var mesh = new Mesh();
                mesh.Index = a;
                // Name
                var jMeshName = jMesh["name"];
                if (jMeshName != null && jMeshName.Type == JTokenType.String)
                {
                    mesh.Name = jMeshName.ToString();
                }

                var jMeshPrimitives = jMesh["primitives"];
                if (jMeshPrimitives == null)
                {
                    throw new Exception($"No primitives found for mesh at index {a}.");
                }

                var meshPrimitives = new MeshPrimitive[jMeshPrimitives.Count()];
                for (int b = 0; b < meshPrimitives.Length; b++)
                {
                    var jMeshPrimitive = jMeshPrimitives[b];
                    if (jMeshPrimitive == null)
                    {
                        throw new Exception($"Primitive at index {b} is null.");
                    }

                    var meshPrimitive = new MeshPrimitive();

                    // Indices
                    var jIndices = jMeshPrimitive["indices"];
                    if (jIndices != null)
                    {
                        var indicesIndex = jIndices.Value<int>();
                        meshPrimitive.Indices = accessors[indicesIndex];
                    }

                    // Material
                    var jMaterial = jMeshPrimitive["material"];
                    if (jMaterial != null)
                    {
                        var materialIndex = jMaterial.Value<int>();
                        meshPrimitive.Material = materials[materialIndex];
                    }

                    // Mode
                    var jMode = jMeshPrimitive["mode"];
                    if (jMode != null)
                    {
                        var mode = jMode.Value<int>();
                        meshPrimitive.Mode = mode;
                    }

                    // Attributes
                    var jAttributes = jMeshPrimitive["attributes"];
                    if (jAttributes == null)
                    {
                        throw new Exception("Attributes not found for mesh primitive.");
                    }
                    var attributes = new Dictionary<string, Accessor>();
                    foreach (JProperty jAttribute in jAttributes)
                    {
                        int attributeValue = jAttribute.Value.Value<int>();
                        var attributeName = jAttribute.Name;

                        attributes.Add(attributeName, accessors[attributeValue]);
                    }
                    meshPrimitive.Attributes = attributes;

                    meshPrimitives[b] = meshPrimitive;
                }

                mesh.Primitives = meshPrimitives;
                meshes[a] = mesh;
            }
            return meshes;
        }

        private static Animation[] LoadAnimations(JToken jAnimations, Accessor[] accessors, Node[] nodes)
        {
            var animations = new Animation[jAnimations.Count()];

            for (var a = 0; a < jAnimations.Count(); a++)
            {
                var jAnimation = jAnimations[a];
                if (jAnimation == null)
                {
                    throw new Exception($"Animation at index {a} is null.");
                }

                var animation = new Animation();

                var jAnimationName = jAnimation["name"];
                if (jAnimationName != null && jAnimationName.Type == JTokenType.String)
                {
                    animation.Name = jAnimationName.ToString();
                }

                var jAnimationSamplers = jAnimation["samplers"];
                if (jAnimationSamplers == null)
                {
                    throw new Exception("No samplers found for animation.");
                }

                var animationSamplers = new AnimationSampler[jAnimationSamplers.Count()];
                for (int b = 0; b < animationSamplers.Length; b++)
                {
                    var jAnimationSampler = jAnimationSamplers[b];
                    if (jAnimationSampler == null)
                    {
                        throw new Exception($"Sampler at index {b} is null.");
                    }

                    var animationSampler = new AnimationSampler();
                    animationSampler.Index = b;

                    // Input accessor
                    var jAnimationSamplerInput = jAnimationSampler["input"];
                    if (jAnimationSamplerInput == null)
                    {
                        throw new Exception("Input accessor not found for sampler.");
                    }
                    var inputIndex = jAnimationSamplerInput.Value<int>();
                    animationSampler.Input = accessors[inputIndex];

                    // Output accessor
                    var jAnimationSamplerOutput = jAnimationSampler["output"];
                    if (jAnimationSamplerOutput == null)
                    {
                        throw new Exception("Output accessor not found for sampler.");
                    }
                    var outputIndex = jAnimationSamplerOutput.Value<int>();
                    animationSampler.Output = accessors[outputIndex];

                    // Interpolation
                    var jAnimationSamplerInterpolation = jAnimationSampler["interpolation"];
                    if (jAnimationSamplerInterpolation != null)
                    {
                        var identifier = jAnimationSamplerInterpolation.ToString();
                        if (identifier == "STEP")
                        {
                            animationSampler.Interpolation = InterpolationAlgorithm.Step;
                        }
                        else if (identifier == "CUBICSPLINE")
                        {
                            animationSampler.Interpolation = InterpolationAlgorithm.Cubicspline;
                        }
                    }

                    animationSamplers[b] = animationSampler;
                }

                // Channels
                var jAnimationChannels = jAnimation["channels"];
                if (jAnimationChannels == null)
                {
                    throw new Exception("No channels found for animation.");
                }

                var animationChannels = new AnimationChannel[jAnimationChannels.Count()];
                for (int c = 0; c < animationChannels.Length; c++)
                {
                    var jAnimationChannel = jAnimationChannels[c];
                    if (jAnimationChannel == null)
                    {
                        throw new Exception($"Channel at index {c} is null.");
                    }

                    var animationChannel = new AnimationChannel();

                    // Sampler
                    var jAnimationChannelSampler = jAnimationChannel["sampler"];
                    if (jAnimationChannelSampler == null)
                    {
                        throw new Exception("Sampler not found for channel.");
                    }
                    var samplerIndex = jAnimationChannelSampler.Value<int>();
                    animationChannel.Sampler = animationSamplers[samplerIndex];

                    // Target
                    var jAnimationChannelTarget = jAnimationChannel["target"];
                    if (jAnimationChannelTarget == null)
                    {
                        throw new Exception("Target not found for channel.");
                    }
                    var animationChannelTarget = new AnimationChannelTarget();

                    var jAnimationChannelTargetNode = jAnimationChannelTarget["node"];
                    if (jAnimationChannelTargetNode == null)
                    {
                        throw new Exception("Node not found for channel target.");
                    }
                    var nodeIndex = jAnimationChannelTargetNode.Value<int>();
                    // Assuming nodes have been loaded previously and stored in a variable called 'nodes'
                    animationChannelTarget.Node = nodes[nodeIndex];

                    var jAnimationChannelTargetPath = jAnimationChannelTarget["path"];
                    if (jAnimationChannelTargetPath == null)
                    {
                        throw new Exception("Path not found for channel target.");
                    }
                    
                    if (Enum.TryParse<AnimationChannelTargetPath>(jAnimationChannelTargetPath.ToString(), ignoreCase: true, out var parsedPath))
                    {
                        animationChannelTarget.Path = parsedPath;
                    }
                    else
                    {
                        throw new ArgumentException($"Invalid path value: {jAnimationChannelTargetPath}", nameof(jAnimationChannelTargetPath));
                    }
                    animationChannel.Target = animationChannelTarget;

                    animationChannels[c] = animationChannel;
                }

                animation.Samplers = animationSamplers;
                animation.Channels = animationChannels;

                animations[a] = animation;
            }

            return animations;
        }

        private static Scene[] LoadScenes(JToken jScenes)
        {
            var sceneCount = jScenes.Count();
            var scenes = new Scene[sceneCount];

            for (int i = 0; i < sceneCount; i++)
            {
                var jScene = jScenes[i];
                if (jScene == null)
                {
                    throw new Exception();
                }
                var scene = new Scene();

                var jSceneName = jScene["name"];
                if (jSceneName != null)
                {
                    var sceneName = jSceneName.ToString();
                    scene.Name = sceneName;
                }

                var jSceneNodeIndices = jScene["nodes"];
                if (jSceneNodeIndices != null)
                {
                    var sceneNodeIndices = jSceneNodeIndices.Value<int[]>();
                    scene.NodesIndices = sceneNodeIndices;
                }

                scenes[i] = scene;
            }

            return scenes;
        }

        private static Scene[] LoadScenes(JToken jScenes, Node[] nodes)
        {
            var sceneCount = jScenes.Count();
            var scenes = new Scene[sceneCount];

            for (int i = 0; i < sceneCount; i++)
            {
                var jScene = jScenes[i];
                if (jScene == null)
                {
                    throw new Exception();
                }
                var scene = new Scene();

                var jSceneName = jScene["name"];
                if (jSceneName != null)
                {
                    var sceneName = jSceneName.ToString();
                    scene.Name = sceneName;
                }

                var jSceneNodeIndices = jScene["nodes"];
                if (jSceneNodeIndices != null)
                {
                    var sceneNodeIndices = jSceneNodeIndices.ToObject<int[]>();
                    scene.NodesIndices = sceneNodeIndices;

                    scene.Nodes = new Node[scene.NodesIndices!.Length];
                    for (int j = 0; j < scene.NodesIndices.Length; j++) 
                    {
                        int nodeIndex = scene.NodesIndices[j];
                        scene.Nodes[j] = nodes[nodeIndex];
                    }
                }

                scenes[i] = scene;
            }

            return scenes;
        }

        private static void LinkNodesAndSkins(Node[] nodes, Skin[] skins)
        {
            foreach (var node in nodes)
            {
                if (node.SkinIndex == -1)
                {
                    continue;
                }

                node.Skin = skins[node.SkinIndex];
            }
        }

        private static Node[] LoadNodes(JToken jNodes, Mesh[] meshes)
        {
            var nodes = new Node[jNodes.Count()];

            for (var a = 0; a < nodes.Length; a++)
            {
                var jNode = jNodes[a];
                if (jNode == null)
                {
                    throw new Exception();
                }

                var node = new Node();
                node.Index = a;
                
                var jNodeName = jNode["name"];
                if (jNodeName != null && jNodeName.Type == JTokenType.String)
                {
                    node.Name = jNodeName.ToString();
                }

                var jNodeSkin = jNode["skin"];
                if (jNodeSkin != null)
                {
                    int skinIndex = jNodeSkin.Value<int>();
                    node.SkinIndex = skinIndex;
                }

                var jNodeMesh = jNode["mesh"];
                if (jNodeMesh != null)
                {
                    int meshIndex = jNodeMesh.Value<int>();
                    node.MeshIndex = meshIndex;
                    node.Mesh = meshes[meshIndex];
                }

                var jNodeMatrix = jNode["matrix"];
                if (jNodeMatrix != null)
                {
                    var matrixValues = jNodeMatrix.ToObject<float[]>();
                    if (matrixValues == null)
                    {
                        throw new Exception();
                    }

                    if (matrixValues.Length != 16)
                    {
                        throw new Exception();
                    }

                    node.Matrix = new Matrix(
                        matrixValues[0], matrixValues[1], matrixValues[2], matrixValues[3],
                        matrixValues[4], matrixValues[5], matrixValues[6], matrixValues[7],
                        matrixValues[8], matrixValues[9], matrixValues[10], matrixValues[11],
                        matrixValues[12], matrixValues[13], matrixValues[14], matrixValues[15]);
                }

                var jNodeScale = jNode["scale"];
                if (jNodeScale != null)
                {
                    var scaleValues = jNodeScale.ToObject<float[]>();
                    if (scaleValues == null)
                    {
                        throw new Exception("Scale values for node are null.");
                    }

                    if (scaleValues.Length != 3)
                    {
                        throw new Exception("Scale values for node have invalid length.");
                    }

                    node.Scale = new Vector3(scaleValues[0], scaleValues[1], scaleValues[2]);
                }

                var jNodeRotation = jNode["rotation"];
                if (jNodeRotation != null)
                {
                    var rotationValues = jNodeRotation.ToObject<float[]>();
                    if (rotationValues == null)
                    {
                        throw new Exception("Rotation values for node are null.");
                    }

                    if (rotationValues.Length != 4)
                    {
                        throw new Exception("Rotation values for node have invalid length.");
                    }

                    node.Rotation = new Quaternion(rotationValues[0], rotationValues[1], rotationValues[2], rotationValues[3]);
                }

                var jNodeTranslation = jNode["translation"];
                if (jNodeTranslation != null)
                {
                    var translationValues = jNodeTranslation.ToObject<float[]>();
                    if (translationValues == null)
                    {
                        throw new Exception("Translation values for node are null.");
                    }

                    if (translationValues.Length != 3)
                    {
                        throw new Exception("Translation values for node have invalid length.");
                    }

                    node.Translation = new Vector3(translationValues[0], translationValues[1], translationValues[2]);
                }
                nodes[a] = node;
            }

            for (var a = 0; a < nodes.Length; a++)
            {
                var jNode = jNodes[a];
                if (jNode == null)
                {
                    throw new Exception("Node at index " + a + " is null.");
                }

                var jNodeChildren = jNode["children"];
                if (jNodeChildren != null && jNodeChildren.Type == JTokenType.Array)
                {
                    var childIndices = jNodeChildren.ToObject<int[]>();
                    if (childIndices != null)
                    {
                        var childNodes = new Node[childIndices.Length];
                        for (int i = 0; i < childIndices.Length; i++)
                        {
                            var childIndex = childIndices[i];
                            if (childIndex >= 0 && childIndex < nodes.Length)
                            {
                                childNodes[i] = nodes[childIndex];
                            }
                            else
                            {
                                throw new Exception("Invalid child index for node at index " + a + ".");
                            }
                        }
                        nodes[a].Children = childNodes;
                    }
                }
            }

            return nodes;
        }

        private static Skin[] LoadSkins(JToken jSkins, Accessor[] accessors, Node[] nodes)
        {
            var skins = new Skin[jSkins.Count()];

            for (var a = 0; a < skins.Length; a++)
            {
                var jSkin = jSkins[a];
                if (jSkin == null)
                {
                    throw new Exception();
                }

                var skin = new Skin();

                var jSkinName = jSkin["name"];
                if (jSkinName != null)
                {
                    skin.Name = jSkinName.ToString();
                }

                var jSkinJoints = jSkin["joints"];
                if (jSkinJoints == null)
                {
                    throw new Exception();
                }

                var skinJointsIndices = jSkinJoints.ToObject<int[]>();
                if (skinJointsIndices == null)
                {
                    throw new Exception();
                }

                var skinJoints = new Node[jSkinJoints.Count()];
                for (int b = 0; b < skinJointsIndices.Length; b++)
                {
                    skinJoints[b] = nodes[skinJointsIndices[b]];
                }
                skin.JointsIndices = skinJointsIndices;

                var jSkinInverseBindMatrices = jSkin["inverseBindMatrices"];
                if (jSkinInverseBindMatrices != null)
                {
                    var inverseBindMatricesIndex = jSkinInverseBindMatrices.Value<int>();
                    var accessor = accessors[inverseBindMatricesIndex];
                    
                    var matrixData = AccessorReader.ReadData(accessor);
                    var matrices = new List<Matrix>();
                    
                    for (var b = 0; b < matrixData.Length; b += 16)
                    {
                        //var matrix = new Matrix(
                        //    matrixData[b], matrixData[b + 4], matrixData[b + 8], matrixData[b + 12],
                        //    matrixData[b + 1], matrixData[b + 5], matrixData[b + 9], matrixData[b + 13],
                        //    matrixData[b + 2], matrixData[b + 6], matrixData[b + 10], matrixData[b + 14],
                        //    matrixData[b + 3], matrixData[b + 7], matrixData[b + 11], matrixData[b + 15]
                        //);
                        
                        var matrix = new Matrix(
                            matrixData[b], matrixData[b + 1], matrixData[b + 2], matrixData[b + 3],
                            matrixData[b + 4], matrixData[b + 5], matrixData[b + 6], matrixData[b + 7],
                            matrixData[b + 8], matrixData[b + 9], matrixData[b + 10], matrixData[b + 11],
                            matrixData[b + 12], matrixData[b + 13], matrixData[b + 14], matrixData[b + 15]
                        );
                        
                        matrices.Add(matrix);
                    }
                    skin.InverseBindMatrices = matrices.ToArray();
                }

                var jSkinSkeleton = jSkin["skeleton"];
                if (jSkinSkeleton != null)
                {
                    var skeletonIndex = jSkinSkeleton.Value<int>();
                    skin.SkeletonIndex = skeletonIndex;
                    skin.Skeleton = nodes[skeletonIndex];
                }

                skin.Joints = skinJoints;

                skins[a] = skin;
            }
            return skins;
        }
    }
}
