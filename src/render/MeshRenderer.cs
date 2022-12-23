using System.Numerics;
using Silk.NET.OpenGL;

namespace OpenGLEngine
{
    public class MeshRenderer : IDisposable
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

            vertexBufferObject = RenderFactory.CreateBufferObject(BufferTargetARB.ArrayBuffer);

            var totalSize = mesh.SizeOfVertices + mesh.SizeOfNormals + mesh.SizeOfTexCoords;
            if (mesh.BoneWeights != null)
            {
                totalSize += mesh.BoneWeights.Length * (4 * sizeof(int) + 4 * sizeof(float));
            }

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

            if (mesh.BoneWeights != null)
            {
                var boneIndexes = mesh.BoneWeights.Select(x => x.BoneIndex).SelectMany(boneIndexes => boneIndexes).ToArray();
                offset += mesh.SizeOfTexCoords;
                vertexBufferObject.AddBufferSubData<int>(boneIndexes, offset);
                layout.Push(3, 4, offset, GLEnum.Int);

                var boneWeights = mesh.BoneWeights.Select(x => x.Weight).SelectMany(weights => weights).ToArray();
                offset += boneIndexes.Length * sizeof(int);
                vertexBufferObject.AddBufferSubData<float>(boneWeights, offset);
                layout.Push(4, 4, offset);
            }

            elementBufferObject = RenderFactory.CreateBufferObject(BufferTargetARB.ElementArrayBuffer);
            elementBufferObject.AddBufferData<uint>(mesh.Indices);

            vertexArray.AddBufferLayout(layout);
            vertexArray.UnLoad();
        }

        public void Draw(Shader shader)
        {
            shader.SetInt("useNormalMap", mesh.UseNormalMap ? 1 : 0);

            for (var i = 0; i < mesh.Textures.Length; i++)
            {
                var name = mesh.Textures[i].Type;
                shader.SetInt(name, i);
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
