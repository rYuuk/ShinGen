using System.Numerics;
using ShinGen.Core.OpenGL;

namespace ShinGen.Core
{
    internal class CubeRenderer : IDisposable
    {
        private readonly VertexArray vertexArray;
        private readonly Shader shader;
        private BufferObject vbo;

        public CubeRenderer()
        {
            vertexArray = RenderFactory.CreateVertexArray();
            shader = RenderFactory.CreateShader(
                "Source/Core/Shaders/cubeShader.vert",
                "Source/Core/Shaders/cubeShader.frag");
        }

        public void Load()
        {
            vertexArray.Load();

            vbo = RenderFactory.CreateBufferObject(BufferType.ArrayBuffer);
            vbo.AddBufferData<float>(DummyVertices.CubeWithNormalAndTexCoord);

            var vertexLayout = new VertexBufferLayout();
            // TODO Fix offset
            vertexLayout.Push(0, 3, 0);
            vertexLayout.Push(1, 3, 0);
            vertexLayout.Push(2, 2, 0);

            vertexArray.AddBufferLayout(vertexLayout);
            vertexArray.UnLoad();
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

        public void Dispose()
        {
            vertexArray.Dispose();
            shader.Dispose();
            vbo.Dispose();
        }
    }
}
