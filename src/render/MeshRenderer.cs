using OpenTK.Graphics.OpenGL4;

namespace OpenGLEngine
{
    public class MeshRenderer
    {
        private readonly Mesh mesh;
        private readonly VertexArray vertexArray;

        public MeshRenderer(Mesh mesh)
        {
            this.mesh = mesh;
            vertexArray = new VertexArray();
        }

        public void SetupMesh()
        {
            vertexArray.Load();

            var vertexBuffer = new BufferObject<Vertex>(Vertex.GetSize() * mesh.Vertices.Length, mesh.Vertices, BufferTarget.ArrayBuffer);

            var indexBuffer = new BufferObject<uint>(mesh.Indices.Length * sizeof(uint), mesh.Indices, BufferTarget.ElementArrayBuffer);

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
                TextureLoader.LoadSlot(i, mesh.Textures[i].ID);
            }

            vertexArray.Load();
            // Draw
            Renderer.DrawElements(mesh.Indices.Length);
            vertexArray.UnLoad();
        }
    }
}
