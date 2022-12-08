using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace OpenGLEngine
{
    public class CubeRenderer
    {
        private readonly VertexArray vertexArray;
        private readonly Shader shader;

        public CubeRenderer()
        {
            vertexArray = new VertexArray();
            shader = new Shader(
                "src/shaders/cubeShader.vert",
                "src/shaders/cubeShader.frag");
        }

        public void Load()
        {
            vertexArray.Load();

            var vertexBuffer = new VertexBuffer<float>(sizeof(float) * VertexData.CubeWithNormalAndTexCoord.Length, VertexData.CubeWithNormalAndTexCoord);
            vertexBuffer.Load();

            var vertexLayout = new VertexBufferLayout();
            vertexLayout.Push(0,3);
            vertexLayout.Push(1,3);
            vertexLayout.Push(2,2);

            vertexArray.AddBufferLayout(vertexLayout);
            vertexArray.UnLoad();
            
            shader.Load();
        }

        public void Draw(Matrix4 view, Matrix4 projection)
        {
            shader.Bind();
            shader.SetMatrix4("model",  Matrix4.Identity);
            shader.SetMatrix4("view", view);
            shader.SetMatrix4("projection", projection);

            vertexArray.Load();
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            vertexArray.UnLoad();
        }
    }
}
