using Assimp;
using SharpGLTF.Runtime;
using SharpGLTF.Schema2;
using GLTFMesh = SharpGLTF.Schema2.Mesh;
using GLTFMaterial = SharpGLTF.Schema2.Material;
using PrimitiveType = SharpGLTF.Schema2.PrimitiveType;

namespace OpenGLEngine
{
    public class GLTFImporter : IDisposable
    {
        private string? directory;
        private readonly List<Texture> loadedTextures;
        private readonly List<Mesh> meshes;

        public GLTFImporter()
        {
            meshes = new List<Mesh>();
            loadedTextures = new List<Texture>();
        }

        public List<Mesh> Import(string path)
        {
            var gltfMeshes = GetMeshes(path);

            foreach (var gltfMesh in gltfMeshes)
            {
                var vertices = new List<Vertex>();
                var indices = new List<uint>();
                var textures = new List<Texture>();

                var gltfMeshPrims = GetValidPrimitives(gltfMesh);
                foreach (var gltfMeshPrim in gltfMeshPrims)
                {
                    var positions = gltfMeshPrim.GetVertexAccessor("POSITION").AsVector3Array();
                    var normals = gltfMeshPrim.GetVertexAccessor("NORMAL").AsVector3Array();
                    var texCoord0 = gltfMeshPrim.GetVertexAccessor("TEXCOORD_0").AsVector2Array();
                    var tangents = gltfMeshPrim.GetVertexAccessor("TANGENT")?.AsVector4Array();
                    var color0 = gltfMeshPrim.GetVertexAccessor("COLOR_0")?.AsColorArray();
                    
                    vertices.AddRange(positions.Select((t, i) => new Vertex()
                    {
                        Position = t,
                        Normal = normals[i],
                        TexCoords = texCoord0[i]
                    }));
                    foreach (var tri in gltfMeshPrim.GetTriangleIndices())
                    {
                        indices.Add((uint) tri.A);
                        indices.Add((uint) tri.B);
                        indices.Add((uint) tri.C);
                    }

                    foreach (var texture in GetTextures(gltfMesh.Name, gltfMeshPrim.Material))
                    {
                        if (textures.Exists(x => x.Path == texture.Path))
                        {
                            continue;
                        }
                        texture.Load();
                        textures.Add(texture);
                    }
                }

                meshes.Add(new Mesh(vertices.ToArray(), indices.ToArray(), textures.ToArray()));
            }
            return meshes;
        }


        public void Dispose()
        {
        }

        private IEnumerable<GLTFMesh> GetMeshes(string path)
        {
            var importer = new AssimpContext();
             var scene = importer.ImportFile(path, PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs);
             Console.WriteLine(scene.Meshes[0].Vertices.Count);
             Console.WriteLine(scene.Materials[0].Name);
             // Console.WriteLine(scene.Materials[0].GetMaterialTexture());

            var srcModel = ModelRoot.Load(path);

            var templates = srcModel.LogicalScenes
                .Select(item => SceneTemplate.Create(item, new RuntimeOptions() { IsolateMemory = true }))
                .ToArray();

            var srcMeshes = templates
                .SelectMany(item => item.LogicalMeshIds)
                .Distinct()
                .Select(idx => srcModel.LogicalMeshes[idx]);
            directory = Path.GetDirectoryName(path);

            return srcMeshes;
        }

        private List<Texture> GetTextures(string meshName, GLTFMaterial gltfMaterial)
        {
            var textures = new List<Texture>();
            foreach (var materialChannel in gltfMaterial.Channels)
            {
                var param = materialChannel.Parameters.Aggregate(string.Empty,
                    (current, parameter) => current + (parameter.Name + " " + parameter.Value + ","));

                Console.WriteLine(materialChannel.Key + " : " + param);
                if (materialChannel.Texture == null)
                {
                    continue;
                }
              
                var extension = materialChannel.Texture.PrimaryImage.Content.IsJpg ? ".jpg" : ".png";
                var texturePath = directory + "/" + meshName + "_" + materialChannel.Key + extension;
                Console.WriteLine(meshName + " " + materialChannel.Key + ", " + texturePath);

                if (!File.Exists(texturePath))
                {
                    materialChannel.Texture.PrimaryImage.Content.SaveToFile(texturePath);
                }

                if (textures.Exists(x => x.Path == texturePath))
                {
                    continue;
                }

                var type = string.Empty;

                if (materialChannel.Key.Contains("BaseColor"))
                {
                    type = "albedoMap";
                }
                else if (materialChannel.Key.Contains("Roughness"))
                {
                    type = "roughnessMap";
                }
                else if (materialChannel.Key.Contains("Normal"))
                {
                    type = "normalMap";
                }
                else if (materialChannel.Key.Contains("Occlusion"))
                {
                    type = "aoMap";
                }
                else if (materialChannel.Key.Contains("Emissive"))
                {
                }

                var texture = new Texture()
                {
                    Path = texturePath,
                    Type = type
                };

                textures.Add(texture);
            }

            return textures;
        }

        private static IEnumerable<MeshPrimitive> GetValidPrimitives(GLTFMesh gltfMesh)
        {
            foreach (var gltfMeshPrim in gltfMesh.Primitives)
            {
                var ppp = gltfMeshPrim.GetVertexAccessor("POSITION");
                if (ppp.Count < 3) continue;

                switch (gltfMeshPrim.DrawPrimitiveType)
                {
                    case PrimitiveType.POINTS:
                    case PrimitiveType.LINES:
                    case PrimitiveType.LINE_LOOP:
                    case PrimitiveType.LINE_STRIP:
                        continue;
                    default:
                        yield return gltfMeshPrim;
                        break;
                }

            }
        }
    }
}
