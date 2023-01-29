using System.Numerics;
using ShinGen.Core.OpenGL;

namespace ShinGen.Core
{
    internal class MeshRenderer : IDisposable
    {
        private readonly Mesh mesh;
        private readonly VertexArray vertexArray;

        private BufferObject vertexBufferObject = null!;
        private BufferObject elementBufferObject = null!;

        public MeshRenderer(Mesh mesh)
        {
            this.mesh = mesh;
            vertexArray = RenderFactory.CreateVertexArray();
        }

        public void SetupMesh()
        {
            vertexArray.Load();
            var layout = new VertexBufferLayout();

            vertexBufferObject = RenderFactory.CreateBufferObject(BufferType.ArrayBuffer);

            var totalSize = mesh.SizeOfVertices +
                            mesh.SizeOfNormals +
                            mesh.SizeOfTexCoords +
                            mesh.SizeOfBoneWeights;

            vertexBufferObject.AddBufferData(totalSize);

            var offset = 0;
            vertexBufferObject.AddBufferSubData<Vector3>(mesh.Vertices, offset);
            layout.Push(0, 3, offset);

            offset += mesh.SizeOfVertices;
            vertexBufferObject.AddBufferSubData<Vector3>(mesh.Normals, offset);
            layout.Push(1, 3, offset);

            offset += mesh.SizeOfNormals;
            vertexBufferObject.AddBufferSubData<Vector2>(mesh.TexCoords, offset);
            layout.Push(2, 2, offset);

            if (mesh.HaveBones)
            {
                offset += mesh.SizeOfTexCoords;
                vertexBufferObject.AddBufferSubData<int>(mesh.FlattenedBoneIndices, offset);
                layout.Push(3, 4, offset, ElementType.Int);

                offset += mesh.FlattenedBoneIndices.Length * sizeof(int);
                vertexBufferObject.AddBufferSubData<float>(mesh.FlattenedBoneWeights, offset);
                layout.Push(4, 4, offset);
            }

            elementBufferObject = RenderFactory.CreateBufferObject(BufferType.ElementBuffer);
            elementBufferObject.AddBufferData<uint>(mesh.Indices);

            vertexArray.AddBufferLayout(layout);
            vertexArray.UnLoad();
        }

        public void Draw(Shader shader)
        {
            shader.SetInt("material.useNormalMap", mesh.UseNormalMap ? 1 : 0);
            shader.SetInt("material.useMetallicRoughnessMap", mesh.UseMetallicRoughnessMap ? 1 : 0);
            shader.SetInt("material.useEmissiveMap", mesh.UseEmissiveMap ? 1 : 0);

            for (var i = 0; i < mesh.Textures.Length; i++)
            {
                var name = mesh.Textures[i].Type;
                shader.SetInt($"material.{name}", i);
                TextureLoader.LoadSlot(mesh.Textures[i].ID, i);
            }

            // Draw
            vertexArray.Load();
            RenderFactory.DrawElements(mesh.Indices.Length);
            vertexArray.UnLoad();
        }

        public void Dispose()
        {
            vertexArray.Dispose();
            vertexBufferObject.Dispose();
            elementBufferObject.Dispose();
            for (var i = 0; i < mesh.Textures.Length; i++)
            {
                TextureLoader.Dispose(mesh.Textures[i].ID);
            }
            GC.SuppressFinalize(this);
        }
    }
}
