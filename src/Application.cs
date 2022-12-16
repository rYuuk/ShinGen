using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGLEngine
{
    public class ModelApplication : IDisposable
    {
        private readonly GameWindow window;

        private readonly Input input;
        // Instance of the camera class to manage the view and projection matrix code.
        private readonly Camera camera;

        // private readonly CubeRenderer cubeRenderer;
        private readonly CubemapRenderer cubemapRenderer;

        private readonly Shader avatarShader;
        private readonly Shader platformShader;
        private Model avatar = null!;
        private Model platform = null!;

        private readonly Vector3[] lightPositions =
        {
            new Vector3(0.0f, 1.0f, 8.0f),
        };

        private readonly Vector3[] lightColors =
        {
            new Vector3(255.0f, 255.0f, 255.0f),
        };

        public ModelApplication()
        {
            window = new GameWindow(
                GameWindowSettings.Default,
                new NativeWindowSettings
                    { Size = (1280, 720), Title = "Model Loader" }
            );

            avatarShader = new Shader(
                "src/shaders/shader.vert",
                "src/shaders/shader.frag");
            platformShader = new Shader(
                "src/shaders/shader.vert",
                "src/shaders/shader.frag");
            // cubeRenderer = new CubeRenderer();
            cubemapRenderer = new CubemapRenderer();

            // Initialize the camera so that it is 3 units back from where the rectangle is.
            camera = new Camera(new Vector3(-1, 0.8f, 3f), (float) window.Size.X / (float) window.Size.Y, 1f, 0.1f);
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
            Renderer.SetSettings();
            // cubeRenderer.Load();            
            cubemapRenderer.Load();

            avatarShader.Load();
            platformShader.Load();

            // avatar = new Model("Resources/Duck.glb");
            // avatar = new Model("Resources/Avatar/MultiMesh/Avatar.glb");
            avatar = new Model("Resources/Avatar/SingleMesh/Avatar2.glb");
            avatar.SetupMesh();

            platform = new Model("Resources/platform.glb");
            platform.SetupMesh();

            window.CursorState = CursorState.Normal;
        }

        private void OnUnload()
        {
            avatarShader.Unbind();
            avatarShader.Dispose();
            platformShader.Unbind();
            platformShader.Dispose();
            avatar.Dispose();
            platform.Dispose();
        }

        private double degrees;

        private void OnRender(FrameEventArgs obj)
        {
            Renderer.Clear();

            avatarShader.Bind();
            avatarShader.SetMatrix4("view", camera.GetViewMatrix());
            avatarShader.SetMatrix4("projection", camera.GetProjectionMatrix());
            avatarShader.SetVector3("camPos", camera.Position);

            var modelMatrix = Matrix4.CreateScale(1f);
            modelMatrix *= Matrix4.CreateTranslation(0.0f, 0.0f, 0f);
            modelMatrix *= Matrix4.CreateRotationY((float) MathHelper.DegreesToRadians(degrees));
            avatarShader.SetMatrix4("model", modelMatrix);

            for (var i = 0; i < lightPositions.Length; ++i)
            {
                avatarShader.SetVector3("directionLight[" + i + "].Position", lightPositions[i]);
                avatarShader.SetVector3("directionLight[" + i + "].Color", lightColors[i]);
            }

            avatar.Draw(avatarShader);

            platformShader.Bind();
            platformShader.SetMatrix4("view", camera.GetViewMatrix());
            platformShader.SetMatrix4("projection", camera.GetProjectionMatrix());
            platformShader.SetVector3("camPos", camera.Position);

            modelMatrix = Matrix4.CreateScale(new Vector3(0.012f, 0.012f, 0.012f));
            modelMatrix *= Matrix4.CreateFromQuaternion(new Quaternion(MathHelper.DegreesToRadians(90), 0, 0));
            modelMatrix *= Matrix4.CreateTranslation(0.0f, -0.2f, 0f);
            platformShader.SetMatrix4("model", modelMatrix);

            for (var i = 0; i < lightPositions.Length; ++i)
            {
                platformShader.SetVector3("directionLight[" + i + "].Position", lightPositions[i]);
                platformShader.SetVector3("directionLight[" + i + "].Color", lightColors[i]);
            }

            platform.Draw(platformShader);

            // cubeRenderer.Draw(camera.GetViewMatrix(), camera.GetProjectionMatrix());
            cubemapRenderer.Draw(camera.GetViewMatrix().ClearTranslation(), camera.GetProjectionMatrix());

            // draw in wireframe
            // GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            window.SwapBuffers();
        }


        private void Update(FrameEventArgs e)
        {
            if (!window.IsFocused)
            {
                return;
            }

            RotateAvatarWithMouse();

            input.Update((float) e.Time);

        }

        private bool firstMove;
        private float lastPos;

        private void RotateAvatarWithMouse()
        {
            if (window.MouseState.IsButtonDown(MouseButton.Button1))
            {
                if (firstMove)
                {
                    lastPos = window.MouseState.X;
                    firstMove = false;
                }
                else
                {
                    var deltaX = window.MouseState.X - lastPos;
                    lastPos = window.MouseState.X;

                    degrees += deltaX * 5;
                }
            }
        }

        private void OnMouseWheel(MouseWheelEventArgs e)
        {
            camera.Fov -= e.OffsetY;
        }

        private void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, window.Size.X, window.Size.Y);

            // Update the aspect ratio once the window has been resized.
            camera.AspectRatio = window.Size.X / (float) window.Size.Y;
        }
    }
}
