using OpenTK.Graphics.OpenGL4;

namespace OpenGLEngine
{
    public class VertexBuffer<T> : IDisposable where T : struct
    {
        private readonly int size;
        private readonly T[] data;

        private readonly int rendererID;

        public VertexBuffer(int size, T[] data)
        {
            this.size = size;
            this.data = data;
            rendererID = GL.GenBuffer();
        }

        public void Load()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, rendererID);
            GL.BufferData(BufferTarget.ArrayBuffer, size, data, BufferUsageHint.StaticDraw);
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
