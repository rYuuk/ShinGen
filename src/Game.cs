using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace OpenGLEngine
{
    public class Game : GameWindow
    {
        private CubeData cubeData;
        private VertexArray vertexArray;
        private VertexArray vertexArrayLamp;
        private VertexBuffer vertexBuffer;
        private Shader lampShader;
        private Shader shader;
        private Renderer renderer;

        private Input input;
        // Instance of the camera class to manage the view and projection matrix code.
        private Camera camera;

        private readonly Vector3 lampPos = new Vector3(1.2f, 1.0f, 2.0f);

        public Game(int width, int height, string title) :
            base(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title })
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            cubeData = new CubeData();
            renderer = new Renderer();

            vertexBuffer = new VertexBuffer(cubeData.VerticesWithNormal.Length * sizeof(float), cubeData.VerticesWithNormal);

            vertexArray = new VertexArray();
            var layout = new VertexBufferLayout();
            layout.Push(0, 3);
            layout.Push(1, 3);
            vertexArray.AddBuffer(vertexBuffer, layout);

            vertexArrayLamp = new VertexArray();
            var lampLayout = new VertexBufferLayout();
            lampLayout.Push(0, 3);
            layout.Push(1, 3);

            vertexArrayLamp.AddBuffer(vertexBuffer, lampLayout);

            lampShader = new Shader(
                "src/shaders/shader.vert",
                "src/shaders/lighting.frag");

            shader = new Shader(
                "src/shaders/shader.vert",
                "src/shaders/shader.frag");

            // Initialize the camera so that it is 3 units back from where the rectangle is.
            camera = new Camera(Vector3.UnitZ * 3, Size.X / (float) Size.Y, 1.5f, 0.2f);

            // To make the mouse cursor invisible and captured so to have proper FPS-camera movement.
            CursorState = CursorState.Grabbed;

            input = new Input();
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            // Unbind all the resources by binding the targets to 0/null.
            vertexBuffer.UnBind();
            vertexArray.UnBind();
            shader.Unbind();

            // Delete all the resources.
            vertexBuffer.Dispose();
            vertexArray.Dispose();
            shader.Dispose();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            renderer.Clear();

            Matrix4 lampMatrix = Matrix4.Identity;
            lampMatrix *= Matrix4.CreateScale(0.2f);
            lampMatrix *= Matrix4.CreateTranslation(lampPos);

            lampShader.Bind();
            lampShader.SetMatrix4("model", lampMatrix);
            lampShader.SetMatrix4("view", camera.GetViewMatrix());
            lampShader.SetMatrix4("projection", camera.GetProjectionMatrix());

            renderer.Draw(vertexArray, cubeData.VerticesWithNormal, lampShader);

            shader.Bind();
            shader.SetMatrix4("model", Matrix4.Identity);
            shader.SetMatrix4("view", camera.GetViewMatrix());
            shader.SetMatrix4("projection", camera.GetProjectionMatrix());
            shader.SetVector3("viewPos", camera.Position);

            shader.SetVector3("material.ambient", new Vector3(1.0f, 0.5f, 0.31f));
            shader.SetVector3("material.diffuse", new Vector3(1.0f, 0.5f, 0.31f));
            shader.SetVector3("material.specular", new Vector3(0.5f, 0.5f, 0.5f));
            shader.SetFloat("material.shininess", 32.0f);
            shader.SetVector3("light.ambient",  new Vector3(0.2f, 0.2f, 0.2f));
            shader.SetVector3("light.diffuse",  new Vector3(0.5f, 0.5f, 0.5f)); // darken the light a bit to fit the scene
            shader.SetVector3("light.specular", new Vector3(1.0f, 1.0f, 1.0f));
            
            shader.SetVector3("light.position", lampPos);
            
            renderer.Draw(vertexArray, cubeData.VerticesWithNormal, shader);

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (!IsFocused)
            {
                return;
            }

            input.Update(ref camera, KeyboardState, (float) e.Time, MouseState, Close);
        }

        // This manage all the zooming of the camera.
        // This is simply done by changing the FOV of the camera.
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            camera.Fov -= e.OffsetY;
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, e.Width, e.Height);

            // Update the aspect ratio once the window has been resized.
            camera.AspectRatio = Size.X / (float) Size.Y;
        }
    }
}
