using OpenTK.Graphics.OpenGL4;

namespace OpenGLEngine
{
    public static class Renderer
    {
        public static void SetSettings()
        {
            Clear();

            // Enable depth testing so z-buffer can be checked for fragments and
            // only those which are in front be drawn.
            GL.Enable(EnableCap.DepthTest);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // Enabling culling of back face
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            // Enable Multisampling Anti-aliasing
            GL.Enable(EnableCap.Multisample);

            // Enable Gamma correction
            // GL.Enable(EnableCap.FramebufferSrgb);
        }

        public static void Clear()
        {
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public static void DrawArray(int count)
        {
            GL.DrawArrays(PrimitiveType.Triangles, 0, count);
        }

        public static void DrawElements(int size)
        {
            GL.DrawElements(PrimitiveType.Triangles, size, DrawElementsType.UnsignedInt, 0);
        }
    }
}
