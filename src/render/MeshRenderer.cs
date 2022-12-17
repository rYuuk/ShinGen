using Silk.NET.OpenGL;

namespace OpenGLEngine
{
    public class MeshRenderer : IDisposable
    {
        private readonly Mesh mesh;
        private readonly VertexArray vertexArray;

        private BufferObject<Vertex> vertexBufferObject = null!;
        private BufferObject<uint> elementBufferObject = null!;

        public MeshRenderer(Mesh mesh)
        {
            this.mesh = mesh;
            vertexArray = RenderFactory.CreateVertexArray();
        }

        public void SetupMesh()
        {
            vertexArray.Load();

            vertexBufferObject = RenderFactory.CreateBufferObject<Vertex>(mesh.Vertices, BufferTargetARB.ArrayBuffer);
            elementBufferObject = RenderFactory.CreateBufferObject<uint>(mesh.Indices, BufferTargetARB.ElementArrayBuffer);

            var layout = new VertexBufferLayout();
            layout.Push(0, 3);
            layout.Push(1, 3);
            layout.Push(2, 2);

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
