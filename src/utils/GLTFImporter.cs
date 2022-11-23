using OpenTK.Mathematics;
using SharpGLTF.Runtime;
using SharpGLTF.Schema2;

namespace OpenGLEngine
{
    public class GLTFImporter
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
            var srcModel = ModelRoot.Load(path);

            var templates = srcModel.LogicalScenes
                .Select(item => SceneTemplate.Create(item, new RuntimeOptions() { IsolateMemory = true }))
                .ToArray();

            var srcMeshes = templates
                .SelectMany(item => item.LogicalMeshIds)
                .Distinct()
                .Select(idx => srcModel.LogicalMeshes[idx]);

            var vertices = new List<Vertex>();
            var indices = new List<uint>();

            foreach (SharpGLTF.Schema2.Mesh? srcMesh in srcMeshes)
            {
                var srcPrims = GetValidPrimitives(srcMesh);
                foreach (var srcPrim in srcPrims)
                {
                    var positions = srcPrim.GetVertexAccessor("POSITION")?.AsVector3Array();
                    var normals = srcPrim.GetVertexAccessor("NORMAL")?.AsVector3Array();
                    var texCoord0 = srcPrim.GetVertexAccessor("TEXCOORD_0")?.AsVector2Array();


                    vertices.AddRange(positions.Select((t, i) => new Vertex()
                    {
                        Position = t,
                        Normal = normals[i],
                        TexCoords = texCoord0[i]
                    }));
                    foreach (var tri in srcPrim.GetTriangleIndices())
                    {
                        indices.Add((uint) tri.A);
                        indices.Add((uint) tri.B);
                        indices.Add((uint) tri.C);

                    }
                }
            }

            var textures = new List<Texture>();

            foreach (Material material in srcModel.LogicalMaterials)
            {
                Console.WriteLine(material.Name);
                foreach (var materialChannel in material.Channels)
                {
                    var srcImage = materialChannel.Texture.PrimaryImage;
                    var bytes = materialChannel.Texture.PrimaryImage.Content.Content.ToArray();
                    materialChannel.Texture.PrimaryImage.Content.SaveToFile("Resources/Duck/" + materialChannel.Key + ".png");
                    // foreach (var jsonSerializable in materialChannel.Texture.PrimaryImage.Extras.ToJson())
                    // {
                        // Console.WriteLine(jsonSerializable.ToString());
                        
                    // }
                    var texture = new Texture()
                    {
                        ID = TextureLoader.LoadFromPath(srcImage.Content.SourcePath),
                        // ID = TextureLoader.LoadFromBytes( bytes),
                        Path = srcImage.Content.SourcePath,
                        Type = "texture_diffuse"
                    };
                    textures.Add(texture);
                }
            }

            meshes.Add(new Mesh(vertices.ToArray(), indices.ToArray(), textures.ToArray()));
            return meshes;
        }

        private static IEnumerable<MeshPrimitive> GetValidPrimitives(SharpGLTF.Schema2.Mesh srcMesh)
        {
            foreach (var srcPrim in srcMesh.Primitives)
            {
                var ppp = srcPrim.GetVertexAccessor("POSITION");
                if (ppp.Count < 3) continue;

                switch (srcPrim.DrawPrimitiveType)
                {
                    case PrimitiveType.POINTS:
                    case PrimitiveType.LINES:
                    case PrimitiveType.LINE_LOOP:
                    case PrimitiveType.LINE_STRIP:
                        continue;
                    default:
                        yield return srcPrim;
                        break;
                }

            }
        }

        void MeshPrimitiveReader(MeshPrimitive srcPrim, bool doubleSided)
        {
        }
    }
}
