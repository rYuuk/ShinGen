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
        private RenderHelper renderHelper;

        private static GL gl = null!;

        private Input input;
        // Instance of the camera class to manage the view and projection matrix code.
        private Camera camera;

        private CubeRenderer cubeRenderer;
        private CubemapRenderer cubemapRenderer;

        private Shader shader;
        private Model model;

        private readonly Vector3[] lightPositions =
        {
            new Vector3(0.0f, 0.0f, 8.0f)
        };

        private readonly Vector3[] lightColors =
        {
            new Vector3(150.0f, 150.0f, 150.0f),
        };

        private bool isFocused = true;

        public ModelApplication()
        {
            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(800, 600);
            options.Title = "OpenGLEngine";
            window = Window.Create(options);
            RegisterEvents();
            window.Run();
        }

        public void Dispose()
        {
            UnRegisterEvents();
        }

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
        }

        private void OnLoad()
        {
            gl = GL.GetApi(window);

            var windowInput = window.CreateInput();

            RenderFactory.SetRenderer(gl);
            renderHelper = new RenderHelper(gl);
            TextureLoader.Init(gl);

            for (var i = 0; i < windowInput.Mice.Count; i++)
            {
                windowInput.Mice[i].Cursor.CursorMode = CursorMode.Raw;
                windowInput.Mice[i].Scroll += OnMouseWheel;
            }

            // Initialize the camera so that it is 3 units back from where the rectangle is.
            camera = new Camera(new Vector3(0, 0, 2), window.Size.X / (float) window.Size.Y, 1f, 0.1f);
            // To make the mouse cursor invisible and captured so to have proper FPS-camera movement.
            input = new Input(camera, windowInput.Keyboards[0], windowInput.Mice[0], window.Close);

            shader = new Shader(
                gl,
                "src/shaders/shader.vert",
                "src/shaders/shader.frag");

            cubeRenderer = new CubeRenderer();
            cubemapRenderer = new CubemapRenderer();

            renderHelper.LoadSettings();
            cubeRenderer.Load();
            cubemapRenderer.Load();

            shader.Load();

            // model = new Model("Resources/Backpack/backpack.obj");
            // model = new Model("Resources/Duck/Duck.gltf");
            // model = new Model("Resources/Duck/Duck.glb");
            // model = new Model("Resources/Avatar/MultiMesh/Avatar.glb");
            model = new Model("Resources/Avatar/SingleMesh/Avatar.glb");
            model.SetupMesh();
        }

        private void OnUnload()
        {
            shader.Unbind();
            shader.Dispose();
            model.Dispose();
        }

        private void OnRender(double time)
        {
            renderHelper.Clear();

            shader.Bind();
            shader.SetMatrix4("view", camera.GetViewMatrix());
            shader.SetMatrix4("projection", camera.GetProjectionMatrix());
            shader.SetVector3("camPos", camera.Position);

            var modelMatrix = Matrix4x4.CreateScale(1f);
            modelMatrix *= Matrix4x4.CreateTranslation(0.0f, 0.0f, 0f);
            shader.SetMatrix4("model", modelMatrix);

            for (var i = 0; i < lightPositions.Length; ++i)
            {
                var newPos = lightPositions[i] + new Vector3(MathF.Sin((float) time * 5.0f) * 5.0f, 0.0f, 0.0f);
                shader.SetVector3("lightPositions[" + i + "]", newPos);
                shader.SetVector3("lightColors[" + i + "]", lightColors[i]);
            }

            model.Draw(shader);

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
            // GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
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
