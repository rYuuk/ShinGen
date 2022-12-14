using Silk.NET.OpenGL;

namespace OpenGLEngine
{
    public class BufferObject<TDataType> : IDisposable
        where TDataType : unmanaged
    {
        private readonly uint handle;
        private readonly GL gl;

        public unsafe BufferObject(GL glRender, Span<TDataType> data, BufferTargetARB bufferType)
        {
            gl = glRender;

            handle = gl.GenBuffer();
            gl.BindBuffer(bufferType, handle);

            fixed (void* dataPointer = data)
            {
                gl.BufferData(bufferType, (nuint) (data.Length * sizeof(TDataType)), dataPointer, BufferUsageARB.StaticDraw);
            }
        }

        public void Dispose()
        {
            gl.DeleteBuffer(handle);
        }
    }
}
