using SharpGLTF.Runtime;
using SharpGLTF.Schema2;
using SharpGLTF.Validation;
using GLTFMesh = SharpGLTF.Schema2.Mesh;
using GLTFMaterial = SharpGLTF.Schema2.Material;
using PrimitiveType = SharpGLTF.Schema2.PrimitiveType;

namespace OpenGLEngine
{
    public class GLTFImporter
    {
        private readonly List<Texture> loadedTextures;
        private readonly List<Mesh> meshes;

        public GLTFImporter()
        {
            meshes = new List<Mesh>();
            loadedTextures = new List<Texture>();
        }

        public List<Mesh> Import(string path) =>
            ParseGLTFMeshToNativeMesh(GetGLTFMeshes(path));

        public void Dispose()
        {
            for (var i = 0; i < loadedTextures.Count; i++)
                TextureLoader.Dispose(loadedTextures[i].ID);
        }

        private IEnumerable<GLTFMesh> GetGLTFMeshes(string path)
        {
            var gltfModel = ModelRoot.Load(path);

            var templates = gltfModel.LogicalScenes
                .Select(item => SceneTemplate.Create(item, new RuntimeOptions { IsolateMemory = false }))
                .ToArray();

            var srcMeshes = templates
                .SelectMany(item => item.LogicalMeshIds)
                .Distinct()
                .Select(idx => gltfModel.LogicalMeshes[idx]);

            return srcMeshes;
        }

        private List<Mesh> ParseGLTFMeshToNativeMesh(IEnumerable<GLTFMesh> gltfMeshes)
        {
            foreach (var gltfMesh in gltfMeshes)
            {
                var vertices = new List<Vertex>();
                var indices = new List<uint>();
                var textures = new List<Texture>();

                foreach (var gltfMeshPrim in gltfMesh.Primitives)
                {
                    var positionVertexAccessor = gltfMeshPrim.GetVertexAccessor("POSITION");
                    if (!IsValidPrimitives(positionVertexAccessor, gltfMeshPrim.DrawPrimitiveType))
                    {
                        continue;
                    }

                    var positions = positionVertexAccessor.AsVector3Array();
                    var normals = gltfMeshPrim.GetVertexAccessor("NORMAL").AsVector3Array();
                    var texCoord0 = gltfMeshPrim.GetVertexAccessor("TEXCOORD_0").AsVector2Array();
                    var joints0 = gltfMeshPrim.GetVertexAccessor("JOINTS_0")?.AsVector4Array();
                    var weights0 = gltfMeshPrim.GetVertexAccessor("WEIGHTS_0")?.AsVector4Array();

                    if (joints0 == null)
                    {
                        vertices.AddRange(positions.Select((position, i) => new Vertex
                        {
                            Position = position,
                            Normal = normals[i],
                            TexCoords = texCoord0[i]
                        }));
                    }
                    else
                    {
                        vertices.AddRange(positions.Select((position, i) => new Vertex
                        {
                            Position = position,
                            Normal = normals[i],
                            TexCoords = texCoord0[i],
                            Joints = joints0[i],
                            Weights = weights0![i]
                        }));
                    }

                    var triangleIndices = gltfMeshPrim.GetTriangleIndices().ToArray();
                    indices.AddRange(triangleIndices.SelectMany(i => new[] { (uint) i.A, (uint) i.B, (uint) i.C }));

                    foreach (var texture in GetTextures(gltfMesh.Name, gltfMeshPrim.Material))
                    {
                        if (textures.Exists(x => x.Path == texture.Path))
                            continue;

                        if (!loadedTextures.Contains(texture))
                        {
                            texture.Load();
                            loadedTextures.Add(texture);
                        }

                        textures.Add(texture);
                    }
                }

                meshes.Add(new Mesh(vertices.ToArray(), indices.ToArray(), textures.ToArray()));
            }
            return meshes;
        }

        private List<Texture> GetTextures(string meshName, GLTFMaterial gltfMaterial)
        {
            var textures = new List<Texture>();
            foreach (var materialChannel in gltfMaterial.Channels)
            {
                var textureKey = meshName + "_" + materialChannel.Key;
                Console.WriteLine(textureKey);

                if (materialChannel.Texture == null ||
                    textures.Exists(x => x.Path == textureKey))
                    continue;

                var param = materialChannel.Parameters.Aggregate(string.Empty,
                    (current, parameter) => current + (parameter.Name + " " + parameter.Value + ","));

                Console.WriteLine(meshName + ", " + textureKey + ", Param: " + param);

                var type = materialChannel.Key switch
                {
                    "BaseColor" => "albedoMap",
                    "MetallicRoughness" => "metallicMap",
                    "Normal" => "normalMap",
                    _ => string.Empty
                };

                if (type == string.Empty)
                    continue;

                var texture = new Texture
                {
                    Path = textureKey,
                    Type = type,
                    Bytes = materialChannel.Texture.PrimaryImage.Content.Content.ToArray()
                };

                textures.Add(texture);
            }

            return textures;
        }

        private bool IsValidPrimitives(Accessor position, PrimitiveType type)
        {
            if (position.Count < 3) return false;

            switch (type)
            {
                case PrimitiveType.POINTS:
                case PrimitiveType.LINES:
                case PrimitiveType.LINE_LOOP:
                case PrimitiveType.LINE_STRIP:
                    return false;
                case PrimitiveType.TRIANGLES:
                case PrimitiveType.TRIANGLE_STRIP:
                case PrimitiveType.TRIANGLE_FAN:
                default:
                    return true;
            }
        }
    }
}

