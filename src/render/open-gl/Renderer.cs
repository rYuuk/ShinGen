using OpenTK.Graphics.OpenGL4;

namespace OpenGLEngine
{
    public class Renderer
    {
        private readonly bool enableDepthTest;

        public Renderer(bool enableDepthTest = true)
        {
            this.enableDepthTest = enableDepthTest;
        }

        public void Load()
        {
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);

            if (enableDepthTest)
            {
                // Enable depth testing so z-buffer can be checked for fragments and
                // only those which are in front be drawn.
                GL.Enable(EnableCap.DepthTest);

                // GL.Enable(EnableCap.Blend);
                // GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                
                GL.Enable(EnableCap.CullFace);
                GL.CullFace(CullFaceMode.Back);
            }
        }

        public void Clear()
        {
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public void Draw(VertexArray vertexArray, float[] vertices, Shader shader)
        {
            vertexArray.Load();
            shader.Bind();

            GL.DrawArrays(PrimitiveType.Triangles, 0, vertices.Length);
        }

        public void Draw(VertexArray vertexArray, int size)
        {
            vertexArray.Load();
            GL.DrawElements(PrimitiveType.Triangles, size, DrawElementsType.UnsignedInt, 0);
            vertexArray.UnLoad();
        }
    }
}
