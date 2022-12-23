using System.Numerics;
using Silk.NET.OpenGL;

namespace OpenGLEngine
{
    public class CubemapRenderer : IDisposable
    {
        private readonly string[] cubeMapFaceTextures =
        {
            "right.jpg",
            "left.jpg",
            "top.jpg",
            "bottom.jpg",
            "front.jpg",
            "back.jpg"
        };

        private readonly VertexArray vertexArray;
        private readonly Shader shader;
        private BufferObject vbo;


        private uint textureID;

        public CubemapRenderer()
        {
            vertexArray = RenderFactory.CreateVertexArray();
            shader = RenderFactory.CreateShader(
                "src/shaders/cubemap.vert",
                "src/shaders/cubemap.frag");
        }

        public void Load()
        {
            vertexArray.Load();

            vbo = RenderFactory.CreateBufferObject(BufferTargetARB.ArrayBuffer);
            vbo.AddBufferData<float>(DummyVertices.Skybox);

            var vertexLayout = new VertexBufferLayout();
            vertexLayout.Push(0, 3, 0);

            vertexArray.AddBufferLayout(vertexLayout);
            vertexArray.UnLoad();

            shader.SetInt("skybox", 0);

            textureID = TextureLoader.LoadCubemapFromPaths(cubeMapFaceTextures.Select(x => "Resources/Skybox/" + x).ToArray());
        }

        public void Draw(Matrix4x4 view, Matrix4x4 projection)
        {
            shader.Bind();
            shader.SetMatrix4("view", view);
            shader.SetMatrix4("projection", projection);
            
            TextureLoader.LoadSlot(textureID, 0);
            
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
