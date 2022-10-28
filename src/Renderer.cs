using OpenTK.Graphics.OpenGL4;

namespace OpenGLEngine
{
    public class Renderer
    {
        public Renderer(bool enableDepthTest = true)
        {
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            if (enableDepthTest)
            {
                // Enable depth testing so z-buffer can be checked for fragments and
                // only those which are in front be drawn.
                GL.Enable(EnableCap.DepthTest);
            }
        }

        public void Clear()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public void Draw(VertexArray vertexArray, float[] vertices, Shader shader)
        {
            vertexArray.Bind();
            shader.Bind();

            GL.DrawArrays(PrimitiveType.Triangles, 0, vertices.Length);
        }
    }
}
