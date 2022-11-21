﻿using Assimp;
using OpenTK.Mathematics;
using AiMesh = Assimp.Mesh;

namespace OpenGLEngine
{
    public class Model : IDisposable
    {
        private readonly List<Mesh> meshes;
        private readonly List<MeshRenderer> meshRenderers;
        private readonly List<Texture> loadedTextures;

        private string? directory;

        public Model(string path)
        {
            meshes = new List<Mesh>();
            meshRenderers = new List<MeshRenderer>();
            loadedTextures = new List<Texture>();
            Load(path);
        }

        public void SetupMesh()
        {
            foreach (var mesh in meshes)
            {
                var meshRender = new MeshRenderer(mesh);
                meshRender.SetupMesh();
                meshRenderers.Add(meshRender);
            }
        }

        public void Draw(Shader shader, Renderer renderer)
        {
            foreach (var meshRenderer in meshRenderers)
            {
                meshRenderer.Draw(shader, renderer);
            }
        }

        private void Load(string path)
        {
            var importer = new AssimpContext();
            var scene = importer.ImportFile(path, PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs);

            if (scene == null || scene.SceneFlags == SceneFlags.Incomplete || scene.RootNode == null)
            {
                return;
            }

            directory = path.Substring(0, path.LastIndexOf('/'));
            Console.WriteLine(directory);
            ProcessNode(scene.RootNode, scene);
        }

        private void ProcessNode(Node rootNode, Scene scene)
        {
            for (var i = 0; i < rootNode.MeshCount; i++)
            {
                var mesh = scene.Meshes[rootNode.MeshIndices[i]];
                meshes.Add(ProcessMesh(mesh, scene));
            }

            for (int i = 0; i < rootNode.ChildCount; i++)
            {
                ProcessNode(rootNode.Children[i], scene);
            }
        }

        private Mesh ProcessMesh(AiMesh mesh, Scene scene)
        {
            var vertices = GetVertices(mesh);
            var indices = GetIndices(mesh);
            var textures = new List<Texture>();

            if (mesh.MaterialIndex >= 0)
            {
                var material = scene.Materials[mesh.MaterialIndex];
                var diffuseMaps = LoadMaterialTexture(material, TextureType.Diffuse, "texture_diffuse");
                textures.AddRange(diffuseMaps);
                var specularMaps = LoadMaterialTexture(material, TextureType.Specular, "texture_specular");
                textures.AddRange(specularMaps);
            }

            return new Mesh(vertices.ToArray(), indices.ToArray(), textures.ToArray());
        }

        private List<Vertex> GetVertices(AiMesh mesh)
        {
            var vertices = new List<Vertex>();
            for (var i = 0; i < mesh.VertexCount; i++)
            {
                var aiVertex = mesh.Vertices[i];
                var aiNormal = mesh.Normals[i];
                var vertex = new Vertex()
                {
                    Position = new Vector3(aiVertex.X, aiVertex.Y, aiVertex.Z),
                    Normal = new Vector3(aiNormal.X, aiNormal.Y, aiNormal.Z),
                };

                if (mesh.HasTextureCoords(0))
                {
                    var aiTexCoords = mesh.TextureCoordinateChannels[0][i];
                    vertex.TexCoords = new Vector2(aiTexCoords.X, aiTexCoords.Y);
                }
                else
                {
                    vertex.TexCoords = new Vector2(0, 0);
                }

                vertices.Add(vertex);
            }

            return vertices;
        }

        private List<uint> GetIndices(AiMesh mesh)
        {
            var indices = new List<uint>();
            for (var i = 0; i < mesh.FaceCount; i++)
            {
                var face = mesh.Faces[i];
                for (var j = 0; j < face.IndexCount; j++)
                {
                    indices.Add((uint) face.Indices[j]);
                }
            }

            return indices;
        }

        private List<Texture> LoadMaterialTexture(Material material, TextureType textureType, string typeName)
        {
            var textures = new List<Texture>();
            for (var i = 0; i < material.GetMaterialTextureCount(textureType); i++)
            {
                material.GetMaterialTexture(textureType, i, out TextureSlot textureSlot);

                var index = loadedTextures.FindIndex(x => x.Path == textureSlot.FilePath);
                if (index == -1)
                {
                    Texture texture;
                    var path = directory + "\\" + textureSlot.FilePath;
                    Console.WriteLine("Texture path: " + textureSlot.FilePath);
                    texture.ID = TextureLoader.LoadFromPath(path);
                    texture.Type = typeName;
                    texture.Path = textureSlot.FilePath;
                    textures.Add(texture);
                    loadedTextures.Add(texture);
                }
                else
                {
                    textures.Add(loadedTextures[index]);
                }
            }

            return textures;
        }

        public void Dispose()
        {
            for (var i = 0; i < loadedTextures.Count; i++)
            {
                TextureLoader.Dispose(loadedTextures[i].ID);
            }
        }
    }
}