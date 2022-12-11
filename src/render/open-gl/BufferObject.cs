using Silk.NET.OpenGL;

namespace OpenGLEngine
{
    public class BufferObject<TDataType> : IDisposable
        where TDataType : unmanaged
    {
        private readonly uint handle;
        private readonly BufferTargetARB bufferType;
        private readonly GL gl;

        public unsafe BufferObject(GL gl, Span<TDataType> data, BufferTargetARB bufferType)
        {
            this.gl = gl;
            this.bufferType = bufferType;

            handle = this.gl.GenBuffer();
            Bind();
            fixed (void* dataPointer = data)
            {
                this.gl.BufferData(bufferType, (nuint) (data.Length * sizeof(TDataType)), dataPointer, BufferUsageARB.StaticDraw);
            }
        }

        public void Bind()
        {
            gl.BindBuffer(bufferType, handle);
        }

        public void Dispose()
        {
            gl.DeleteBuffer(handle);
        }
    }
}
