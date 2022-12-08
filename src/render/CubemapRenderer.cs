using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace OpenGLEngine
{
    public class CubemapRenderer
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

        private int textureID;

        public CubemapRenderer()
        {
            vertexArray = new VertexArray();
            shader = new Shader(
                "src/shaders/cubemap.vert",
                "src/shaders/cubemap.frag");
        }

        public void Load()
        {
            vertexArray.Load();

            var vertexBuffer = new VertexBuffer<float>(sizeof(float) * VertexData.Skybox.Length, VertexData.Skybox);
            vertexBuffer.Load();

            var vertexLayout = new VertexBufferLayout();
            vertexLayout.Push(0, 3);

            vertexArray.AddBufferLayout(vertexLayout);

            shader.Load();
            shader.SetInt("skybox", 0);

            textureID = TextureLoader.LoadCubemapFromPaths(cubeMapFaceTextures.Select(x => "Resources/Skybox/" + x).ToArray());
        }

        public void Draw(Matrix4 view, Matrix4 projection)
        {
            GL.DepthFunc(DepthFunction.Lequal);
            shader.Bind();
            shader.SetMatrix4("view", view);
            shader.SetMatrix4("projection", projection);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, textureID);

            vertexArray.Load();
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            vertexArray.UnLoad();

            GL.DepthFunc(DepthFunction.Less);
        }
    }
}
