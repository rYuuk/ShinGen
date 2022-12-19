using System.Numerics;
using Silk.NET.Assimp;
using AssimpMesh = Silk.NET.Assimp.Mesh;

namespace OpenGLEngine
{
    public class ModelImporter
    {
        private readonly Assimp assimp;
        private List<Mesh> Meshes { get; } = new List<Mesh>();

        private readonly BoneWeightProcessor boneWeightProcessor;
        private readonly TextureImporter textureImporter;

        public ModelImporter()
        {
            assimp = Assimp.GetApi();
            boneWeightProcessor = new BoneWeightProcessor();
            textureImporter = new TextureImporter(assimp);
        }

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
            var vertices = ProcessVertices(mesh);
            var indices = ProcessIndices(mesh);
            var textures = textureImporter.ImportTextures(
                    scene->MTextures,
                    mesh->MName,
                    scene->MMaterials[mesh->MMaterialIndex])
                .ToList();

            boneWeightProcessor.ProcessBoneWeight(vertices, mesh);

            var result = new Mesh(
                mesh->MName,
                vertices.ToArray(),
                indices.ToArray(),
                textures.ToArray());

            return result;
        }

        private unsafe List<Vertex> ProcessVertices(AssimpMesh* mesh)
        {
            var vertices = new List<Vertex>();
            for (var i = 0; i < mesh->MNumVertices; i++)
            {
                var vertex = new Vertex();
                var vector = new Vector3();
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

                boneWeightProcessor.SetVertexBoneDataToDefault(vertex);
                vertices.Add(vertex);
            }

            return vertices;
        }

        private unsafe List<uint> ProcessIndices(AssimpMesh* mesh)
        {
            var indices = new List<uint>();

            for (uint i = 0; i < mesh->MNumFaces; i++)
            {
                var face = mesh->MFaces[i];
                for (uint j = 0; j < face.MNumIndices; j++)
                    indices.Add(face.MIndices[j]);
            }
            return indices;
        }
    }
}
