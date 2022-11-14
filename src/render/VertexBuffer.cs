using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace OpenGLEngine
{
    public class VertexBuffer : IDisposable
    {
        private readonly int size;
        private readonly float[] data;

        private int rendererID;

        public VertexBuffer(int size, float[] data)
        {
            this.size = size;
            this.data = data;
        }

        public void Load()
        {
            rendererID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, rendererID);
            GL.BufferData(BufferTarget.ArrayBuffer, size, data, BufferUsageHint.StaticDraw);
        }

        public void Bind()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, rendererID);
        }

        public void UnBind()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void Dispose()
        {
            GL.DeleteBuffer(rendererID);
        }
    }
}
