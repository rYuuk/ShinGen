using Silk.NET.OpenGL;

namespace OpenGLEngine
{
    public class RenderHelper
    {
        private readonly GL gl;

        public RenderHelper(GL gl)
        {
            this.gl = gl;
        }

        public void LoadSettings()
        {
            gl.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);

            // Enable depth testing so z-buffer can be checked for fragments and
            // only those which are in front be drawn.
            gl.Enable(EnableCap.DepthTest);

            // GL.Enable(EnableCap.Blend);
            // gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // Enabling culling of back face
            // gl.Enable(EnableCap.CullFace);
            // gl.CullFace(CullFaceMode.Back);

            // Enable Multisampling Anti-aliasing
            gl.Enable(EnableCap.Multisample);

            // Enable Gamma correction
            // GL.Enable(EnableCap.FramebufferSrgb);
        }

        public void Clear()
        {
            gl.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            gl.Clear((uint) (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
        }
    }
}
