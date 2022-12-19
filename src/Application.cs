using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace OpenGLEngine
{
    public class ModelApplication : IDisposable
    {
        private readonly IWindow window;

        private static GL gl = null!;

        private Input input = null!;
        // Instance of the camera class to manage the view and projection matrix code.
        private Camera camera = null!;

        private CubeRenderer cubeRenderer = null!;
        private CubemapRenderer cubemapRenderer = null!;

        private Shader avatarShader = null!;
        private Model avatar = null!;

        private Shader platformShader = null!;
        private Model platform = null!;

        private bool firstMove;
        private float lastPos;

        private readonly Vector3[] lightPositions =
        {
            new Vector3(0.0f, 0.0f, 10.0f)
        };

        private readonly Vector3[] lightColors =
        {
            new Vector3(150.0f, 150.0f, 150.0f),
        };

        private bool isFocused = true;
        private float degrees;

        public ModelApplication()
        {
            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(1280, 720);
            options.Title = "OpenGLEngine";
            options.Samples = 4;

            window = Window.Create(options);
            RegisterEvents();
            window.Run();
        }

        public void Dispose() =>
            UnRegisterEvents();

        private void RegisterEvents()
        {
            window.Load += OnLoad;
            window.Render += OnRender;
            window.Update += OnUpdate;
            window.Closing += OnUnload;
            window.Resize += OnResize;
            window.FocusChanged += OnFocusChanged;
        }

        private void UnRegisterEvents()
        {
            window.Load -= OnLoad;
            window.Render -= OnRender;
            window.Update -= OnUpdate;
            window.Closing -= OnUnload;
            window.Resize -= OnResize;
            window.FocusChanged -= OnFocusChanged;
        }

        private void OnLoad()
        {
            gl = GL.GetApi(window);

            var windowInput = window.CreateInput();

            RenderHelper.SetRenderer(gl);
            RenderFactory.SetRenderer(gl);
            TextureLoader.SetRenderer(gl);

            RenderHelper.LoadSettings();

            for (var i = 0; i < windowInput.Mice.Count; i++)
            {
                windowInput.Mice[i].Cursor.CursorMode = CursorMode.Raw;
                windowInput.Mice[i].MouseMove += OnMouseMove;
                windowInput.Mice[i].Scroll += OnMouseWheel;
            }

            // Initialize the camera so that it is 3 units back from where the rectangle is.
            camera = new Camera(new Vector3(-1, 0.8f, 3), window.Size.X / (float) window.Size.Y, 1f, 0.1f);
            // To make the mouse cursor invisible and captured so to have proper FPS-camera movement.
            input = new Input(camera, windowInput.Keyboards[0], windowInput.Mice[0], window.Close);

            avatarShader = new Shader(
                gl,
                "src/shaders/shader.vert",
                "src/shaders/shader.frag");

            platformShader = new Shader(
                gl,
                "src/shaders/shader.vert",
                "src/shaders/shader.frag");

            cubeRenderer = new CubeRenderer();
            cubemapRenderer = new CubemapRenderer();

            // cubeRenderer.Load();
            cubemapRenderer.Load();

            avatar = new Model("Resources/Avatar/MultiMesh/Avatar.glb");
            // avatar = new Model("Resources/Avatar/SingleMesh/Avatar.glb");
            avatar.SetupMesh();

            platform = new Model("Resources/Platform.glb");
            platform.SetupMesh();
        }

        private void OnUnload()
        {
            avatarShader.Unbind();
            avatarShader.Dispose();
            avatar.Dispose();
        }

        private void OnRender(double time)
        {
            RenderHelper.Clear();

            avatarShader.Bind();
            avatarShader.SetMatrix4("view", camera.GetViewMatrix());
            avatarShader.SetMatrix4("projection", camera.GetProjectionMatrix());
            avatarShader.SetVector3("camPos", camera.Position);

            var modelMatrix = Matrix4x4.CreateScale(1f);
            modelMatrix *= Matrix4x4.CreateTranslation(0.0f, 0.0f, 0f);
            modelMatrix *= Matrix4x4.CreateRotationY(MathHelper.DegreesToRadians(degrees));
            avatarShader.SetMatrix4("model", modelMatrix);

            for (var i = 0; i < lightPositions.Length; ++i)
            {
                avatarShader.SetVector3("lightPositions[" + i + "]", lightPositions[i]);
                avatarShader.SetVector3("lightColors[" + i + "]", lightColors[i]);
            }

            avatar.Draw(avatarShader);

            platformShader.Bind();
            platformShader.SetMatrix4("view", camera.GetViewMatrix());
            platformShader.SetMatrix4("projection", camera.GetProjectionMatrix());
            platformShader.SetVector3("camPos", camera.Position);

            modelMatrix = Matrix4x4.CreateScale(0.1f);
            modelMatrix *= Matrix4x4.CreateTranslation(0.0f, -0.12f, 0.0f);
            platformShader.SetMatrix4("model", modelMatrix);

            for (var i = 0; i < lightPositions.Length; ++i)
            {
                platformShader.SetVector3("lightPositions[" + i + "]", lightPositions[i]);
                platformShader.SetVector3("lightColors[" + i + "]", lightColors[i]);
            }

            platform.Draw(platformShader);

            // cubeRenderer.Draw(camera.GetViewMatrix(), camera.GetProjectionMatrix());

            var view = camera.GetViewMatrix();
            var viewWithoutTranslation = view with
            {
                M14 = 0,
                M24 = 0,
                M34 = 0,
                M41 = 0,
                M42 = 0,
                M43 = 0,
                M44 = 0
            };

            gl.DepthFunc(DepthFunction.Lequal);
            cubemapRenderer.Draw(viewWithoutTranslation, camera.GetProjectionMatrix());
            gl.DepthFunc(DepthFunction.Less);

            // draw in wireframe
            // gl.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
        }

        private void OnMouseMove(IMouse mouse, Vector2 position)
        {
            if (mouse.IsButtonPressed(MouseButton.Left))
            {
                if (firstMove)
                {
                    lastPos = position.X;
                    firstMove = false;
                }
                else
                {
                    var deltaX = position.X - lastPos;
                    lastPos = position.X;

                    degrees += deltaX * 5;
                }
            }
        }

        private void OnUpdate(double time)
        {
            if (!isFocused)
            {
                return;
            }
            input.Update((float) time);
        }

        private void OnMouseWheel(IMouse mouse, ScrollWheel scrollWheel)
        {
            camera.Fov = Math.Clamp(camera.Fov - scrollWheel.Y, 1.0f, 45f);
        }

        private void OnResize(Vector2D<int> size)
        {
            gl.Viewport(0, 0, (uint) size.X, (uint) size.Y);

            // Update the aspect ratio once the window has been resized.
            camera.AspectRatio = window.Size.X / (float) window.Size.Y;
        }

        private void OnFocusChanged(bool focus)
        {
            isFocused = focus;
        }
    }
}
