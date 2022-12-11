using System.Numerics;
using Silk.NET.OpenGL;

namespace OpenGLEngine
{
    public class CubeRenderer
    {
        private readonly VertexArray vertexArray;
        private readonly Shader shader;

        public CubeRenderer()
        {
            vertexArray = RenderFactory.CreateVertexArray();
            shader = RenderFactory.CreateShader(
                "src/shaders/cubeShader.vert",
                "src/shaders/cubeShader.frag");
        }

        public void Load()
        {
            vertexArray.Load();

            RenderFactory.CreateBufferObject<float>(VertexData.CubeWithNormalAndTexCoord, BufferTargetARB.ArrayBuffer);

            var vertexLayout = new VertexBufferLayout();
            vertexLayout.Push(0, 3);
            vertexLayout.Push(1, 3);
            vertexLayout.Push(2, 2);

            vertexArray.AddBufferLayout(vertexLayout);
            vertexArray.UnLoad();

            shader.Load();
        }

        public void Draw(Matrix4x4 view, Matrix4x4 projection)
        {
            shader.Bind();
            shader.SetMatrix4("model", Matrix4x4.Identity);
            shader.SetMatrix4("view", view);
            shader.SetMatrix4("projection", projection);

            vertexArray.Load();
            RenderFactory.DrawArrays(36);
            vertexArray.UnLoad();
        }
    }
}
