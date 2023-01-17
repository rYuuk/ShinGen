using Silk.NET.OpenGL;

namespace OpenGLEngine
{
    public static class RenderHelper
    {
        private static GL gl = null!;

        public static void SetRenderer(GL glRenderer) =>
            gl = glRenderer;

        public static void LoadSettings()
        {
            Clear();
            
            // Enable depth testing so z-buffer can be checked for fragments and
            // only those which are in front be drawn.
            gl.Enable(EnableCap.DepthTest);

            gl.Enable(EnableCap.Blend);
            gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // Enabling culling of back face
            gl.Enable(EnableCap.CullFace);
            gl.CullFace(CullFaceMode.Back);

            // Enable Multisampling Anti-aliasing
            gl.Enable(EnableCap.Multisample);

            // Enable Gamma correction
            // gl.Enable(EnableCap.FramebufferSrgb);
        }

        public static void Clear()
        {
            gl.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            gl.Clear((uint) (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
        }
    }
}
