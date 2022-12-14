using Silk.NET.OpenGL;

namespace OpenGLEngine
{
    public static class RenderFactory
    {
        private static GL gl = null!;

        public static void SetRenderer(GL glRender) =>
            gl = glRender;

        public static VertexArray CreateVertexArray()
            => new VertexArray(gl);

        public static BufferObject<T> CreateBufferObject<T>(Span<T> data, BufferTargetARB bufferType) where T : unmanaged
            => new BufferObject<T>(gl, data, bufferType);

        public static Shader CreateShader(string vertexPath, string fragmentPath)
            => new Shader(gl, vertexPath, fragmentPath);

        public static void DrawArrays(int count)
            => gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)count);

        public static unsafe void DrawElements(int size)
            => gl.DrawElements(PrimitiveType.Triangles, (uint)size, DrawElementsType.UnsignedInt, null);
    }
}
