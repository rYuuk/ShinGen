using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace OpenGLEngine
{
    public class MeshRenderer
    {
        private readonly Mesh mesh;
        private readonly VertexArray vertexArray;

        private int VAO, VBO, EBO;

        public MeshRenderer(Mesh mesh)
        {
            this.mesh = mesh;
            vertexArray = new VertexArray();
        }

        public void SetupMesh()
        {
            // var vertexBuffer = new VertexBuffer<Vertex>(Vertex.GetSize() * mesh.Vertices.Count, mesh.Vertices.ToArray());
            // vertexBuffer.Load();
            // vertexArray.Load();

            // var layout = new VertexBufferLayout();
            // layout.Push(0, 3);
            // layout.Push(1, 3);
            // layout.Push(2, 2);
            //
            // vertexArray.AddBuffer(layout);
            // vertexArray.UnLoad();

            VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, mesh.Vertices.Length * Vertex.GetSize(), mesh.Vertices.ToArray(), BufferUsageHint.StaticDraw);
            
            EBO = GL.GenBuffer();

            VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, mesh.Indices.Length * sizeof(uint), mesh.Indices.ToArray(), BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0,3,VertexAttribPointerType.Float,false,Vertex.GetSize(),0);
            GL.EnableVertexAttribArray(0);

            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1,3,VertexAttribPointerType.Float,false,Vertex.GetSize(),Vector3.SizeInBytes);
            
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2,2,VertexAttribPointerType.Float,false,Vertex.GetSize(),2*Vector3.SizeInBytes);
            
            GL.BindVertexArray(0);
        }

        public void Draw(Shader shader)
        {
            var diffuseNr = 1;
            var specularNr = 1;
            var normalNr = 1;
            var heightNr = 1;

            for (var i = 0; i < mesh.Textures.Length; i++)
            {
                var number = string.Empty;
                var name = mesh.Textures[i].Type;
                number = name switch
                {
                    "texture_diffuse" => diffuseNr++.ToString(),
                    "texture_specular" => specularNr++.ToString(),
                    "texture_normal" => normalNr++.ToString(),
                    "texture_height" => heightNr++.ToString(),
                    _ => number
                };

                shader.SetInt(name + number, i);
                TextureLoader.LoadSlot(i, mesh.Textures[i].ID);
            }
            // Active texture slot 0 again
            TextureLoader.ActivateSlot(0);

            // Draw

            // vertexArray.Load();
            GL.BindVertexArray(VAO);
            GL.DrawElements(PrimitiveType.Triangles, mesh.Indices.Length, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
            // vertexArray.UnLoad();
        }
    }
}
