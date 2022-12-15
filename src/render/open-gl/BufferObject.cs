using OpenTK.Graphics.OpenGL4;

namespace OpenGLEngine
{
    public class BufferObject<TDataType> : IDisposable
        where TDataType : struct
    {
        private readonly int handle;

        public BufferObject(int size, TDataType[] data, BufferTarget bufferType)
        {
            handle = GL.GenBuffer();
            GL.BindBuffer(bufferType, handle);
            GL.BufferData(bufferType, size, data, BufferUsageHint.StaticDraw);
        }

        public void Dispose()
        {
            GL.DeleteBuffer(handle);
        }
    }
}
