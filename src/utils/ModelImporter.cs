using System.Numerics;
using Silk.NET.Assimp;
using AssimpMesh = Silk.NET.Assimp.Mesh;

namespace OpenGLEngine
{
    public class ModelImporter : IDisposable
    {
        public ModelImporter()
        {
            assimp = Assimp.GetApi();
        }

        private readonly Assimp assimp;
        private List<Texture> texturesLoaded = new List<Texture>();
        private List<Mesh> Meshes { get; } = new List<Mesh>();

        public List<Mesh> LoadModel(string path)
        {
            unsafe
            {
                var scene = assimp.ImportFile(path, (uint) (PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs));

                if (scene == null || scene->MFlags == Assimp.SceneFlagsIncomplete || scene->MRootNode == null)
                {
                    var error = assimp.GetErrorStringS();
                    throw new Exception(error);
                }

                ProcessNode(scene->MRootNode, scene);
            }

            return Meshes;
        }

        private unsafe void ProcessNode(Node* node, Scene* scene)
        {
            for (var i = 0; i < node->MNumMeshes; i++)
            {
                var mesh = scene->MMeshes[node->MMeshes[i]];
                Meshes.Add(ProcessMesh(mesh, scene));

            }

            for (var i = 0; i < node->MNumChildren; i++)
            {
                ProcessNode(node->MChildren[i], scene);
            }
        }

        private unsafe Mesh ProcessMesh(AssimpMesh* mesh, Scene* scene)
        {
            var vertices = new List<Vertex>();
            var indices = new List<uint>();
            var textures = new List<Texture>();

            for (uint i = 0; i < mesh->MNumVertices; i++)
            {
                var vertex = new Vertex();
                // vertex.BoneIds = new int[Vertex.MAX_BONE_INFLUENCE];
                // vertex.Weights = new float[Vertex.MAX_BONE_INFLUENCE];
                var  vector = new Vector3();
                vector.X = mesh->MVertices[i].X;
                vector.Y = mesh->MVertices[i].Y;
                vector.Z = mesh->MVertices[i].Z;
                vertex.Position = vector;

                if (mesh->MNormals != null)
                {
                    vector.X = mesh->MNormals[i].X;
                    vector.Y = mesh->MNormals[i].Y;
                    vector.Z = mesh->MNormals[i].Z;
                    vertex.Normal = vector;
                }

                if (mesh->MTextureCoords[0] != null)
                {
                    var vec = new Vector2()
                    {
                        X = mesh->MTextureCoords[0][i].X,
                        Y = mesh->MTextureCoords[0][i].Y
                    };
                    vertex.TexCoords = vec;
                }
                else
                    vertex.TexCoords = new Vector2(0.0f, 0.0f);

                vertices.Add(vertex);
            }

            for (uint i = 0; i < mesh->MNumFaces; i++)
            {
                var face = mesh->MFaces[i];
                for (uint j = 0; j < face.MNumIndices; j++)
                    indices.Add(face.MIndices[j]);
            }

            var material = scene->MMaterials[mesh->MMaterialIndex];

            textures.AddRange(LoadMaterialTextures(material, mesh->MName, TextureType.BaseColor, "albedoMap", scene));
            textures.AddRange(LoadMaterialTextures(material, mesh->MName, TextureType.Normals, "normalMap", scene));
            textures.AddRange(LoadMaterialTextures(material, mesh->MName, TextureType.Unknown, "metallicMap", scene));

            // return a mesh object created from the extracted mesh data
            var result = new Mesh(vertices.ToArray(), indices.ToArray(), textures.ToArray());

            return result;
        }

        private unsafe List<Texture> LoadMaterialTextures(Material* mat, string meshName, TextureType type, string typeName, Scene* scene)
        {
            var textureCount = assimp.GetMaterialTextureCount(mat, type);
            var textures = new List<Texture>();
            for (uint i = 0; i < textureCount; i++)
            {
                AssimpString path;
                assimp.GetMaterialTexture(mat, type, i, &path, null, null, null, null, null, null);
                var skip = false;
                var index = int.Parse(path.ToString()[1].ToString());
                var assimpTexture = scene->MTextures[index];
                var textureName = meshName + assimpTexture->MFilename;

                Console.WriteLine(meshName);
                for (var j = 0; j < texturesLoaded.Count; j++)
                {
                    if (texturesLoaded[j].Name != textureName) continue;
                    textures.Add(texturesLoaded[j]);
                    skip = true;
                    break;
                }
                if (skip) continue;

                var texture = new Texture();
                texture.ID = TextureLoader.LoadFromBytes(assimpTexture->PcData, assimpTexture->MWidth, assimpTexture->MHeight);
                texture.Name = path;
                texture.Type = typeName;

                texturesLoaded.Add(texture);
                textures.Add(texture);
            }
            return textures;
        }

        public void Dispose()
        {
            foreach (var texture in texturesLoaded)
            {
                TextureLoader.Dispose(texture.ID);
            }
            texturesLoaded = null!;
        }
    }
}
