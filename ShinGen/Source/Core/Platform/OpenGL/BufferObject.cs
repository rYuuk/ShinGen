using Silk.NET.OpenGL;

namespace ShinGen.Core.OpenGL
{
    internal class BufferObject
    {
        private readonly uint handle;
        private readonly GL gl;
        private readonly BufferTargetARB bufferType;

        internal BufferObject(GL glRender, BufferTargetARB bufferType)
        {
            gl = glRender;
            this.bufferType = bufferType;

            handle = gl.GenBuffer();
            gl.BindBuffer(bufferType, handle);
        }

        public unsafe void AddBufferData(int size)
        {
            gl.BufferData(bufferType, (uint) size, null, GLEnum.StaticDraw);
        }

        public unsafe void AddBufferData<TDataType>(Span<TDataType> data) where TDataType : unmanaged
        {
            fixed (void* dataPointer = data)
            {
                gl.BufferData(bufferType, (nuint) (data.Length * sizeof(TDataType)), dataPointer, BufferUsageARB.StaticDraw);
            }
        }

        public unsafe void AddBufferSubData<TDataType>(Span<TDataType> data, int offset) where TDataType : unmanaged
        {
            fixed (void* dataPointer = data)
            {
                gl.BufferSubData(bufferType, offset, (nuint) (data.Length * sizeof(TDataType)), dataPointer);
            }
        }

        public void Dispose()
        {
            gl.DeleteBuffer(handle);
        }
    }
}
