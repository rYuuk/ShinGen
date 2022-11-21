using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace OpenGLEngine
{
    public class ModelApplication : IDisposable
    {
        private readonly GameWindow window;
        private readonly Renderer renderer;

        private readonly Shader shader;

        private readonly Input input;
        // Instance of the camera class to manage the view and projection matrix code.
        private readonly Camera camera;

        private Model? model;

        public ModelApplication()
        {
            window = new GameWindow(
                GameWindowSettings.Default,
                new NativeWindowSettings() { Size = (800, 600), Title = "Model Loader" }
            );

            renderer = new Renderer();
            
            shader = new Shader(
                "src/shaders/shader.vert",
                "src/shaders/shader.frag");


            // Initialize the camera so that it is 3 units back from where the rectangle is.
            camera = new Camera(Vector3.UnitZ * 3, window.Size.X / (float) window.Size.Y, 1.5f, 0.2f);
            // To make the mouse cursor invisible and captured so to have proper FPS-camera movement.
            input = new Input(camera, window.KeyboardState, window.MouseState, window.Close);

            RegisterEvents();
        }

        public void Run()
        {
            window.Run();
        }

        public void Dispose()
        {
            UnRegisterEvents();
        }

        private void RegisterEvents()
        {
            window.Load += Load;
            window.RenderFrame += OnRender;
            window.UpdateFrame += Update;
            window.Unload += OnUnload;
            window.MouseWheel += OnMouseWheel;
            window.Resize += OnResize;
        }

        private void UnRegisterEvents()
        {
            window.Load -= Load;
            window.RenderFrame -= OnRender;
            window.UpdateFrame -= Update;
            window.Unload -= OnUnload;
            window.MouseWheel -= OnMouseWheel;
            window.Resize -= OnResize;
        }

        private void Load()
        {
            renderer.Load();
            shader.Load();
            model = new Model("Resources/Backpack/backpack.obj");
            model.SetupMesh();

            window.CursorState = CursorState.Grabbed;
        }

        private void OnUnload()
        {
            shader.Unbind();
            shader.Dispose();
        }

        private void OnRender(FrameEventArgs obj)
        {
            renderer.Clear();

            shader.Bind();
            shader.SetMatrix4("view", camera.GetViewMatrix());
            shader.SetMatrix4("projection", camera.GetProjectionMatrix());

            var modelMatrix = Matrix4.CreateScale(1f);
            modelMatrix *= Matrix4.CreateTranslation(0.0f, 0.0f, 0.0f);
            shader.SetMatrix4("model", modelMatrix);
            model?.Draw(shader);

            window.SwapBuffers();
        }

        private void Update(FrameEventArgs e)
        {
            if (!window.IsFocused)
            {
                return;
            }

            input.Update((float) e.Time);

        }

        private void OnMouseWheel(MouseWheelEventArgs e)
        {
            camera.Fov -= e.OffsetY;
        }

        private void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, e.Width, e.Height);

            // Update the aspect ratio once the window has been resized.
            camera.AspectRatio = window.Size.X / (float) window.Size.Y;
        }
    }
}
