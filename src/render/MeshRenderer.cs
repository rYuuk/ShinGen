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

            var vertexBuffer = new VertexBuffer<Vertex>(Vertex.GetSize() * mesh.Vertices.Length, mesh.Vertices);
            vertexBuffer.Load();

            var indexBuffer = new IndexBuffer(mesh.Indices.Length, mesh.Indices);
            indexBuffer.Load();

            var layout = new VertexBufferLayout();
            layout.Push(0, 3);
            layout.Push(1, 3);
            layout.Push(2, 2);

            vertexArray.AddBufferLayout(layout);
            vertexArray.UnLoad();
        }

        public void Draw(Shader shader, Renderer renderer)
        {
            var diffuseNr = 1;
            var specularNr = 1;
            var normalNr = 1;
            var heightNr = 1;

            for (var i = 0; i < mesh.Textures.Length; i++)
            {
                var name = mesh.Textures[i].Type;
                // number = name switch
                // {
                //     "texture_diffuse" => diffuseNr++.ToString(),
                //     "texture_specular" => specularNr++.ToString(),
                //     "texture_normal" => normalNr++.ToString(),
                //     "texture_height" => heightNr++.ToString(),
                //     _ => number
                // };

                shader.SetInt(name, i);
                TextureLoader.LoadSlot(i, mesh.Textures[i].ID);
            }
            // Active texture slot 0 again
            TextureLoader.ActivateSlot(0);

            // Draw
            renderer.Draw(vertexArray, mesh.Indices.Length);
        }
    }
}
