using OpenTK.Graphics.OpenGL4;

namespace OpenGLEngine
{
    public class IndexBuffer : IDisposable
    {
        private readonly int rendererID;

        public IndexBuffer(int length, uint[] data)
        {
            rendererID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, rendererID);
            GL.BufferData(BufferTarget.ElementArrayBuffer, length * sizeof(uint), data, BufferUsageHint.StaticDraw);
        }

        public void Bind()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, rendererID);
        }

        public void UnBind()
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        public void Dispose()
        {
            GL.DeleteBuffer(rendererID);
        }
    }
}
