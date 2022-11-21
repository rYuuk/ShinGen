using OpenTK.Graphics.OpenGL4;

namespace OpenGLEngine
{
    public class IndexBuffer : IDisposable
    {
        private readonly int length;
        private readonly uint[] data;
        private readonly int rendererID;

        public IndexBuffer(int length, uint[] data)
        {
            this.length = length;
            this.data = data;
            rendererID = GL.GenBuffer();
        }

        public void Load()
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, rendererID);
            GL.BufferData(BufferTarget.ElementArrayBuffer, length * sizeof(uint), data, BufferUsageHint.StaticDraw);
        }

        public void UnLoad()
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        public void Dispose()
        {
            GL.DeleteBuffer(rendererID);
        }
    }
}
