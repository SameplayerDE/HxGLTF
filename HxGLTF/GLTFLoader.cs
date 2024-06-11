using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace HxGLTF
{
    public class GLTFLoader
    {
        public static GLTFFile Load(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path), "File path cannot be null or empty.");
            }

            ValidateFileExists(path);

            using (var fileStream = File.OpenRead(path))
            {
                var extension = Path.GetExtension(path);
                ValidateFileType(extension);

                byte[]? glbBytes = null;
                string? json = null;
                byte[]? binary = null;

                if (extension.Equals(".glb"))
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

        private static void ValidateFileExists(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("File not found.", path);
            }
        }

        private static void ValidateFileType(string extension)
        {
            if (!extension.Equals(".gltf") && !extension.Equals(".glb"))
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

        private static GLTFFile LoadFromJsonWithBinary(string path, string json, byte[]? binary)
        {
            var jObject = JObject.Parse(json);

            // Überprüfe die erforderlichen Elemente
            if (jObject["asset"] == null || jObject["buffers"] == null || jObject["bufferViews"] == null || jObject["accessors"] == null)
            {
                throw new ArgumentException("Die GLTF-Datei enthält nicht alle erforderlichen Elemente.");
            }

            var asset = LoadAsset(jObject["asset"]);
            var buffers = LoadBuffers(path, jObject["buffers"]);
            var bufferViews = LoadBufferViews(jObject["bufferViews"], buffers);
            var accessors = LoadAccessors(jObject["accessors"], bufferViews);
            var samplers = LoadSamplers(jObject["samplers"]);
            var images = LoadImages(path, jObject["images"], bufferViews);
            var textures = LoadTextures(jObject["textures"], samplers, images);
            var materials = LoadMaterials(jObject["materials"], textures);
            var meshes = LoadMeshes(jObject["meshes"], accessors, materials);
            var nodes = LoadNodes(jObject["nodes"], meshes);
            var animations = jObject["animations"] != null ? LoadAnimations(jObject["animations"], accessors, nodes) : null;
            var skins = jObject["skins"] != null ? LoadSkins(jObject["skins"], accessors, nodes) : null;

            return new GLTFFile()
            {
                Path = path,
                Asset = asset,
                Buffers = buffers,
                BufferViews = bufferViews,
                Accessors = accessors,
                Images = images,
                Samplers = samplers,
                Textures = textures,
                Materials = materials,
                Meshes = meshes,
                Nodes = nodes,
                //Animations = animations,
                //Skins = skins,
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
                    ByteOffset = (int)jToken["byteOffset"],
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
                    ComponentType = ComponentType.FromInt((int)jToken["componentType"]),
                    Type = Type.FromSting((string)jToken["type"])
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
                var jToken = (JObject)jMaterials[i];
                var material = new Material
                {
                    Name = (string)(jToken["name"] ?? string.Empty),
                    AlphaMode = (string)(jToken["alphaMode"] ?? string.Empty),
                    DoubleSided = (bool)(jToken["doubleSided"] ?? false)
                };

                if (jToken.ContainsKey("pbrMetallicRoughness"))
                {
                    var pbr = (JObject)jToken["pbrMetallicRoughness"];
                    if (pbr.ContainsKey("baseColorTexture"))
                    {
                        var baseColorTexture = (JObject)pbr["baseColorTexture"];
                        //material.PbrMetallicRoughness.BaseColorTexture = textures[(int)baseColorTexture["index"]];
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
                    animationChannelTarget.Path = jAnimationChannelTargetPath.ToString();

                    animationChannel.Target = animationChannelTarget;

                    animationChannels[c] = animationChannel;
                }

                animation.Samplers = animationSamplers;
                animation.Channels = animationChannels;

                animations[a] = animation;
            }

            return animations;
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

                var jNodeName = jNode["name"];
                if (jNodeName != null && jNodeName.Type == JTokenType.String)
                {
                    node.Name = jNodeName.ToString();
                }

                var jNodeMesh = jNode["mesh"];
                if (jNodeMesh != null)
                {
                    node.Mesh = meshes[jNodeMesh.Value<int>()];
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

                var jSkinInverseBindMatrices = jSkin["inverseBindMatrices"];
                if (jSkinInverseBindMatrices != null)
                {
                    var inverseBindMatricesIndex = jSkinInverseBindMatrices.Value<int>();
                    skin.InverseBindMatrices = accessors[inverseBindMatricesIndex];
                }

                var jSkinSkeleton = jSkin["skeleton"];
                if (jSkinSkeleton != null)
                {
                    skin.Skeleton = nodes[jSkinSkeleton.Value<int>()];
                }

                skin.Joints = skinJoints;

                skins[a] = skin;
            }
            return skins;
        }
    }
}
