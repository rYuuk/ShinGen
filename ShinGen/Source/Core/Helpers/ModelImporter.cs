using System.Numerics;
using Silk.NET.Assimp;
using AssimpMesh = Silk.NET.Assimp.Mesh;

namespace ShinGen.Core
{
    internal class ModelImporter
    {
        private readonly Assimp assimp;

        private readonly BoneWeightProcessor boneWeightProcessor;
        private readonly TextureImporter textureImporter;

        public Dictionary<string, BoneInfo> BoneInfoMap => boneWeightProcessor.BoneInfoMap;
        public int BoneCount => boneWeightProcessor.BoneCounter;

        private List<Mesh> Meshes { get; } = new List<Mesh>();

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
                var scene = assimp.ImportFile(path, (uint) (
                    PostProcessSteps.Triangulate |
                    PostProcessSteps.FlipUVs |
                    PostProcessSteps.ImproveCacheLocality
                ));

                if (scene == null || scene->MFlags == Assimp.SceneFlagsIncomplete || scene->MRootNode == null)
                {
                    var error = assimp.GetErrorStringS();
                    throw new Exception(error);
                }

                ProcessNode(scene->MRootNode, scene, Matrix4x4.Identity);
            }

            return Meshes;
        }

        private unsafe void ProcessNode(Node* node, Scene* scene, Matrix4x4 parentTransformation)
        {

            var globalTransformation = node->MTransformation * parentTransformation;

            for (var i = 0; i < node->MNumMeshes; i++)
            {
                var mesh = scene->MMeshes[node->MMeshes[i]];
                Meshes.Add(ProcessMesh(mesh, scene, globalTransformation));
            }

            for (var i = 0; i < node->MNumChildren; i++)
            {
                ProcessNode(node->MChildren[i], scene, globalTransformation);
            }
        }

        private unsafe Mesh ProcessMesh(AssimpMesh* mesh, Scene* scene, Matrix4x4 transformation)
        {
            var vertices = ProcessVertices(mesh);
            var indices = ProcessIndices(mesh);
            var textures = textureImporter.ImportTextures(
                    scene->MTextures,
                    mesh->MName,
                    scene->MMaterials[mesh->MMaterialIndex])
                .ToList();

            var boneWeights = boneWeightProcessor.ProcessBoneWeight((int) mesh->MNumVertices, mesh);

            var result = new Mesh(
                mesh->MName,
                vertices.Item1,
                vertices.Item2,
                vertices.Item3,
                boneWeights,
                indices.ToArray(),
                textures.ToArray(),
                transformation);

            return result;
        }

        private unsafe (Vector3[], Vector3[], Vector2[]) ProcessVertices(AssimpMesh* mesh)
        {
            var vertexCount = mesh->MNumVertices;

            var vertices = new Vector3[vertexCount];
            var normals = new Vector3[vertexCount];
            var texCoords = new Vector2[vertexCount];

            for (var i = 0; i < mesh->MNumVertices; i++)
            {
                var vertex = new Vector3();
                vertex.X = mesh->MVertices[i].X;
                vertex.Y = mesh->MVertices[i].Y;
                vertex.Z = mesh->MVertices[i].Z;
                vertices[i] = vertex;

                if (mesh->MNormals != null)
                {
                    var normal = new Vector3();
                    normal.X = mesh->MNormals[i].X;
                    normal.Y = mesh->MNormals[i].Y;
                    normal.Z = mesh->MNormals[i].Z;
                    normals[i] = normal;
                }

                if (mesh->MTextureCoords[0] != null)
                {
                    var texCoord = new Vector2
                    {
                        X = mesh->MTextureCoords[0][i].X,
                        Y = mesh->MTextureCoords[0][i].Y
                    };
                    texCoords[i] = texCoord;
                }
                else
                    texCoords[i] = Vector2.Zero;
            }

            return (vertices.ToArray(), normals.ToArray(), texCoords.ToArray());
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
