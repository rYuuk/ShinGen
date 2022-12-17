using System.Numerics;
using OpenGLEngine;
using Silk.NET.Assimp;
using Silk.NET.OpenGL;
using AssimpMesh = Silk.NET.Assimp.Mesh;
using Mesh = OpenGLEngine.Mesh;
using Texture = OpenGLEngine.Texture;

namespace OpenGLEngine
{
    public class ModelImporter : IDisposable
    {
        public ModelImporter(GL gl, string path, bool gamma = false)
        {
            this.gl = gl;
            assimp = Assimp.GetApi();
            LoadModel(path);
        }

        private readonly GL gl;
        private readonly Assimp assimp;
        private List<Texture> texturesLoaded = new List<Texture>();
        public string Directory { get; protected set; } = string.Empty;
        public List<Mesh> Meshes { get; protected set; } = new List<Mesh>();

        private void LoadModel(string path)
        {

            unsafe
            {
                var scene = assimp.ImportFile(path, (uint) PostProcessSteps.Triangulate);

                if (scene == null || scene->MFlags == Assimp.SceneFlagsIncomplete || scene->MRootNode == null)
                {
                    var error = assimp.GetErrorStringS();
                    throw new Exception(error);
                }

                Directory = path;

                ProcessNode(scene->MRootNode, scene);
            }
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
            // data to fill
            List<Vertex> vertices = new List<Vertex>();
            List<uint> indices = new List<uint>();
            List<Texture> textures = new List<Texture>();

            // walk through each of the mesh's vertices
            for (uint i = 0; i < mesh->MNumVertices; i++)
            {
                Vertex vertex = new Vertex();
                // vertex.BoneIds = new int[Vertex.MAX_BONE_INFLUENCE];
                // vertex.Weights = new float[Vertex.MAX_BONE_INFLUENCE];
                Vector3
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
                Face face = mesh->MFaces[i];
                // retrieve all indices of the face and store them in the indices vector
                for (uint j = 0; j < face.MNumIndices; j++)
                    indices.Add(face.MIndices[j]);
            }
            // process materials
            Material* material = scene->MMaterials[mesh->MMaterialIndex];
            // we assume a convention for sampler names in the shaders. Each diffuse texture should be named
            // as 'texture_diffuseN' where N is a sequential number ranging from 1 to MAX_SAMPLER_NUMBER. 
            // Same applies to other texture as the following list summarizes:
            // diffuse: texture_diffuseN
            // specular: texture_specularN
            // normal: texture_normalN

            foreach (TextureType t in Enum.GetValues(typeof(TextureType)))
            {
                var count = assimp.GetMaterialTextureCount(material, t);
                if (count == 1)
                {
                    AssimpString path;
                    var a = assimp.GetMaterialTexture(material, t, 0, &path, null, null, null, null, null, null);
                    int index = int.Parse(path.ToString()[1].ToString());
                    Console.WriteLine(t + " : " + scene->MTextures[index]->MFilename);
                    // Console.WriteLine( t +  " : " + a + "," + scene->MTextures[index]->MFilename);
                }
            }


            // 1. diffuse maps
            var diffuseMaps = LoadMaterialTextures(material, TextureType.Diffuse, "texture_diffuse");
            if (diffuseMaps.Any())
            {
                Console.WriteLine("wow1");
                textures.AddRange(diffuseMaps);

            }
            // 2. specular maps
            var specularMaps = LoadMaterialTextures(material, TextureType.Specular, "texture_specular");
            if (specularMaps.Any())
            {
                Console.WriteLine("wow2");
                textures.AddRange(specularMaps);
            }
            // 3. normal maps
            var normalMaps = LoadMaterialTextures(material, TextureType.Height, "texture_normal");
            if (normalMaps.Any())
            {
                Console.WriteLine("wow3");
                textures.AddRange(normalMaps);
            }
            // 4. height maps
            var heightMaps = LoadMaterialTextures(material, TextureType.Ambient, "texture_height");
            if (heightMaps.Any())
            {
                Console.WriteLine("wow4");
                textures.AddRange(heightMaps);
            }

            // return a mesh object created from the extracted mesh data
            var result = new Mesh();
            return result;
        }

        private unsafe List<Texture> LoadMaterialTextures(Material* mat, TextureType type, string typeName)
        {
            var textureCount = assimp.GetMaterialTextureCount(mat, type);
            List<Texture> textures = new List<Texture>();
            for (uint i = 0; i < textureCount; i++)
            {
                AssimpString path;
                assimp.GetMaterialTexture(mat, type, i, &path, null, null, null, null, null, null);
                bool skip = false;
                for (int j = 0; j < texturesLoaded.Count; j++)
                {
                    if (texturesLoaded[j].Path == path)
                    {
                        textures.Add(texturesLoaded[j]);
                        skip = true;
                        break;
                    }
                }
                if (!skip)
                {
                    var texture = new Texture();
                    texture.Path = path;
                    textures.Add(texture);
                    texturesLoaded.Add(texture);
                    // texture.Load();
                }
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
