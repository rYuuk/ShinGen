using OpenTK.Graphics.OpenGL4;

namespace OpenGLEngine
{
    public class Renderer
    {
        public void Clear()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }
        
        public void Draw(VertexArray vertexArray, float[] vertices,  Shader shader)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            vertexArray.Bind();
            shader.Bind();
            
            GL.DrawArrays(PrimitiveType.Triangles, 0, vertices.Length);
        }
    }
}
