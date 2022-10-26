using OpenTK.Graphics.OpenGL4;

namespace OpenGLEngine
{
    public class VertexArray : IDisposable
    {
        private readonly int rendererID;

        public VertexArray()
        {
            rendererID = GL.GenVertexArray();
            GL.BindVertexArray(rendererID);
        }

        public void AddBuffer(VertexBuffer vertexBuffer, VertexBufferLayout layout)
        {
            Bind();
            List<VertexBufferElement> elements = layout.Elements;
            var offset = 0;
            foreach (VertexBufferElement element in elements)
            {
                GL.EnableVertexAttribArray(element.Index);
                GL.VertexAttribPointer(
                    element.Index,
                    element.Count,
                    element.Type,
                    element.Normalized,
                    layout.Stride,
                    offset);
                offset += element.Count * VertexBufferElement.GetSizeOfType(element.Type);
            }
        }

        public void Bind()
        {
            GL.BindVertexArray(rendererID);
        }

        public void UnBind()
        {
            GL.BindVertexArray(0);
        }

        public void Dispose()
        {
            GL.DeleteVertexArray(rendererID);
        }
    }
}
