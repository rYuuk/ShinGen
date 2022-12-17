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

            // walk through each of the mesh's vertices
            for (uint i = 0; i < mesh->MNumVertices; i++)
            {
                var vertex = new Vertex();
                // vertex.BoneIds = new int[Vertex.MAX_BONE_INFLUENCE];
                // vertex.Weights = new float[Vertex.MAX_BONE_INFLUENCE];
                var
                    vector = new Vector3(); // we declare a placeholder vector since assimp uses its own vector class that doesn't directly convert to glm's vec3 class so we transfer the data to this placeholder glm::vec3 first.
                // positions
                vector.X = mesh->MVertices[i].X;
                vector.Y = mesh->MVertices[i].Y;
                vector.Z = mesh->MVertices[i].Z;
                vertex.Position = vector;
                // normals

                if (mesh->MNormals != null)
                {
                    vector.X = mesh->MNormals[i].X;
                    vector.Y = mesh->MNormals[i].Y;
                    vector.Z = mesh->MNormals[i].Z;
                    vertex.Normal = vector;
                }
                // texture coordinates
                if (mesh->MTextureCoords[0] != null) // does the mesh contain texture coordinates?
                {
                    var vec = new Vector2();
                    // a vertex can contain up to 8 different texture coordinates. We thus make the assumption that we won't 
                    // use models where a vertex can have multiple texture coordinates so we always take the first set (0).
                    vec.X = mesh->MTextureCoords[0][i].X;
                    vec.Y = mesh->MTextureCoords[0][i].Y;
                    vertex.TexCoords = vec;
                    // tangent
                    if (mesh->MTangents != null)
                    {
                        vector.X = mesh->MTangents[i].X;
                        vector.Y = mesh->MTangents[i].Y;
                        vector.Z = mesh->MTangents[i].Z;
                        // vertex.Tangent = vector;
                    }
                    // bitangent
                    if (mesh->MBitangents != null)
                    {
                        vector.X = mesh->MBitangents[i].X;
                        vector.Y = mesh->MBitangents[i].Y;
                        vector.Z = mesh->MBitangents[i].Z;
                        // vertex.Bitangent = vector;
                    }
                }
                else
                    vertex.TexCoords = new Vector2(0.0f, 0.0f);

                vertices.Add(vertex);
            }
            // now wak through each of the mesh's faces (a face is a mesh its triangle) and retrieve the corresponding vertex indices.
            for (uint i = 0; i < mesh->MNumFaces; i++)
            {
                var face = mesh->MFaces[i];
                // retrieve all indices of the face and store them in the indices vector
                for (uint j = 0; j < face.MNumIndices; j++)
                    indices.Add(face.MIndices[j]);
            }
            // process materials
            var material = scene->MMaterials[mesh->MMaterialIndex];
            // we assume a convention for sampler names in the shaders. Each diffuse texture should be named
            // as 'texture_diffuseN' where N is a sequential number ranging from 1 to MAX_SAMPLER_NUMBER. 
            // Same applies to other texture as the following list summarizes:
            // diffuse: texture_diffuseN
            // specular: texture_specularN
            // normal: texture_normalN

            // 1. diffuse maps
            var diffuseMaps = LoadMaterialTextures(material, mesh->MName, TextureType.BaseColor, "albedoMap", scene);
            if (diffuseMaps.Any())
            {
                textures.AddRange(diffuseMaps);

            }
            // 2. specular maps
            var specularMaps = LoadMaterialTextures(material, mesh->MName, TextureType.Normals, "normalMap", scene);
            if (specularMaps.Any())
            {
                textures.AddRange(specularMaps);
            }
            // 3. normal maps
            var normalMaps = LoadMaterialTextures(material, mesh->MName, TextureType.Unknown, "metallicMap", scene);
            if (normalMaps.Any())
            {
                textures.AddRange(normalMaps);
            }

            // return a mesh object created from the extracted mesh data
            var result = new Mesh(vertices.ToArray(), indices.ToArray(), textures.ToArray());

            return result;
        }

        private unsafe List<Texture> LoadMaterialTextures(Material* mat, string meshName, TextureType type, string typeName, Scene* scene)
        {
            var textureCount = assimp.GetMaterialTextureCount(mat, type);
            List<Texture> textures = new List<Texture>();
            for (uint i = 0; i < textureCount; i++)
            {
                AssimpString path;
                assimp.GetMaterialTexture(mat, type, i, &path, null, null, null, null, null, null);
                var skip = false;
                var index = int.Parse(path.ToString()[1].ToString());
                var assimpTexture = scene->MTextures[index];
                var textureName = meshName + assimpTexture->MFilename;

                for (int j = 0; j < texturesLoaded.Count; j++)
                {
                    if (texturesLoaded[j].Path == textureName)
                    {
                        textures.Add(texturesLoaded[j]);
                        skip = true;
                        break;
                    }
                }
                if (skip) continue;

                var texture = new Texture();
                texture.ID = TextureLoader.LoadFromBytes(assimpTexture->PcData, assimpTexture->MWidth, assimpTexture->MHeight);
                texture.Path = path;
                texture.Type = typeName;
                // texture.Width = assimpTexture->MWidth;
                // texture.Height = assimpTexture->MHeight;
                // texture.PixelData = assimpTexture->PcData;

                texturesLoaded.Add(texture);
                textures.Add(texture);
            }
            return textures;
        }

        private float[] BuildVertices(List<Vertex> vertexCollection)
        {
            var vertices = new List<float>();

            foreach (var vertex in vertexCollection)
            {
                vertices.Add(vertex.Position.X);
                vertices.Add(vertex.Position.Y);
                vertices.Add(vertex.Position.Z);
                vertices.Add(vertex.TexCoords.X);
                vertices.Add(vertex.TexCoords.Y);
            }

            return vertices.ToArray();
        }

        private uint[] BuildIndices(List<uint> indices)
        {
            return indices.ToArray();
        }

        public void Dispose()
        {
            foreach (var mesh in Meshes)
            {
                // mesh.Dispose();
            }

            texturesLoaded = null;
        }
    }
}
