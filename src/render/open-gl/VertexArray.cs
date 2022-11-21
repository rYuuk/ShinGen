using OpenTK.Graphics.OpenGL4;

namespace OpenGLEngine
{
    public class VertexArray : IDisposable
    {
        private readonly int rendererID;

        public VertexArray()
        {
            rendererID = GL.GenVertexArray();
        }
        
        public void AddBufferLayout(VertexBufferLayout layout)
        {
            Load();
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

        public void Load()
        {
            GL.BindVertexArray(rendererID);
        }

        public void UnLoad()
        {
            GL.BindVertexArray(0);
        }

        public void Dispose()
        {
            GL.DeleteVertexArray(rendererID);
        }
    }
}
