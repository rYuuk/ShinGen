using Silk.NET.OpenGL;

namespace OpenGLEngine
{
    public class MeshRenderer
    {
        private readonly Mesh mesh;
        private readonly VertexArray vertexArray;

        public MeshRenderer(Mesh mesh)
        {
            this.mesh = mesh;
            vertexArray = RenderFactory.CreateVertexArray();
        }

        public  void  SetupMesh()
       {
            vertexArray.Load();

            RenderFactory.CreateBufferObject<Vertex>(mesh.Vertices, BufferTargetARB.ArrayBuffer);
            RenderFactory.CreateBufferObject<uint>(mesh.Indices, BufferTargetARB.ElementArrayBuffer);

            var layout = new VertexBufferLayout();
            layout.Push(0, 3);
            layout.Push(1, 3);
            layout.Push(2, 2);

            vertexArray.AddBufferLayout(layout);
            vertexArray.UnLoad();
        }

        public void Draw(Shader shader)
        {
            for (var i = 0; i < mesh.Textures.Length; i++)
            {
                var name = mesh.Textures[i].Type;
                shader.SetInt(name, i);
                TextureLoader.LoadSlot(mesh.Textures[i].ID, i);
            }
            // Active texture slot 0 again
            TextureLoader.ActivateSlot(0);

            // Draw
            vertexArray.Load();
            RenderFactory.DrawElements(mesh.Indices.Length);
            vertexArray.UnLoad();
        }
    }
}
