using System.Numerics;
using ShinGen.Core.OpenGL;

namespace ShinGen.Core
{
    internal class CubemapRenderer : IDisposable
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
        private BufferObject vertexBufferObject = null!;

        private uint textureID;

        public CubemapRenderer()
        {
            vertexArray = RenderFactory.CreateVertexArray();
            shader = RenderFactory.CreateShader(
                "Source/Core/shaders/cubemap.vert",
                "Source/Core/shaders/cubemap.frag");
        }

        public void Load()
        {
            vertexArray.Load();

            vertexBufferObject = RenderFactory.CreateBufferObject(BufferType.ArrayBuffer);
            vertexBufferObject.AddBufferData<float>(DummyVertices.Skybox);

            var vertexLayout = new VertexBufferLayout();
            vertexLayout.Push(0, 3, 0);

            vertexArray.AddBufferLayout(vertexLayout);
            vertexArray.UnLoad();

            shader.SetInt("skybox", 0);

            textureID = TextureLoader.LoadCubemapFromPaths(cubeMapFaceTextures.Select(x => "Resources/Skybox/" + x).ToArray());
        }

        public void Draw(Matrix4x4 view, Matrix4x4 projection)
        {
            var viewWithoutTranslation = view with
            {
                M14 = 0,
                M24 = 0,
                M34 = 0,
                M41 = 0,
                M42 = 0,
                M43 = 0,
                M44 = 0
            };

            shader.Bind();
            shader.SetMatrix4("view", viewWithoutTranslation);
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
            vertexBufferObject.Dispose();
        }
    }
}
