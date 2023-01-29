using Silk.NET.OpenGL;

namespace ShinGen.Core.OpenGL
{
    internal class VertexArray : IDisposable
    {
        private readonly GL gl;
        private readonly uint handle;

        internal VertexArray(GL gl)
        {
            this.gl = gl;
            handle = gl.GenVertexArray();
        }

        public unsafe void AddBufferLayout(VertexBufferLayout layout)
        {
            Load();
            var elements = layout.Elements;
            foreach (var element in elements)
            {
                gl.EnableVertexAttribArray((uint) element.Index);

                if (element.Type == VertexAttribPointerType.Int)
                {
                    gl.VertexAttribIPointer(
                        (uint) element.Index,
                        element.Count,
                        VertexAttribIType.Int,
                        (uint) element.Stride,
                        (void*) element.Offset);
                }
                else
                {
                    gl.VertexAttribPointer(
                        (uint) element.Index,
                        element.Count,
                        element.Type,
                        element.Normalized,
                        (uint) element.Stride,
                        (void*) element.Offset);
                }
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
