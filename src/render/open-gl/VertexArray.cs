using Silk.NET.OpenGL;

namespace OpenGLEngine
{
    public class VertexArray : IDisposable
    {
        private readonly GL gl;
        private readonly uint handle;

        public VertexArray(GL gl)
        {
            this.gl = gl;
            handle = gl.GenVertexArray();
        }
        
        public unsafe void AddBufferLayout(VertexBufferLayout layout)
        {
            Load();
            var elements = layout.Elements;
            var offset = 0;
            foreach (var element in elements)
            {
                gl.EnableVertexAttribArray((uint)element.Index);
                gl.VertexAttribPointer(
                    (uint)element.Index,
                    element.Count,
                    element.Type,
                    element.Normalized,
                    (uint)layout.Stride,
                    (void*)offset);
                offset += element.Count * VertexBufferElement.GetSizeOfType(element.Type);
            }
        }

        public void Load()
        {
            gl.BindVertexArray(handle);
        }

        public void UnLoad()
        {
            gl.BindVertexArray(0);
        }

        public void Dispose()
        {
            gl.DeleteVertexArray(handle);
        }
    }
}
