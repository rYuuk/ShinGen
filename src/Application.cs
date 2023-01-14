using System.Diagnostics;
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

        private GL gl = null!;

        private Input input = null!;
        private Camera camera = null!;

        // private CubeRenderer cubeRenderer = null!;
        private CubemapRenderer cubemapRenderer = null!;

        private AnimatedModel avatar = null!;
        private Model platform = null!;

        private bool firstMove;
        private float lastPos;

        private readonly Stopwatch stopwatch;
        private float ElapsedSeconds => stopwatch.ElapsedMilliseconds / 1000f;

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
            stopwatch = new Stopwatch();
            stopwatch.Start();

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
            Console.WriteLine($"Loading started: {ElapsedSeconds}s");

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

            windowInput.Keyboards[0].KeyUp += OnKeyUp;

            camera = new Camera(new Vector3(-1, 0.8f, 3), window.Size.X / (float) window.Size.Y, 1f, 0.1f);
            input = new Input(camera, windowInput.Keyboards[0], windowInput.Mice[0], window.Close);

            // cubeRenderer = new CubeRenderer();
            // cubeRenderer.Load();

            cubemapRenderer = new CubemapRenderer();
            cubemapRenderer.Load();

            Console.WriteLine($"Cubemap loaded: {ElapsedSeconds}s");

            // avatar = new Model("Resources/Avatar/MultiMesh/Avatar.glb");
            avatar = new AnimatedModel("Resources/Avatar/SingleMesh/Avatar.glb");
            avatar.SetupMesh();

            Console.WriteLine($"Avatar loaded: {ElapsedSeconds}s");

            platform = new Model("Resources/Platform.glb");
            platform.SetupMesh();

            Console.WriteLine($"Platform loaded: {ElapsedSeconds}s");
            stopwatch.Stop();
        }

        private void OnUnload()
        {
            avatar.Dispose();
            platform.Dispose();
            cubemapRenderer.Dispose();
        }

        private void OnRender(double time)
        {
            RenderHelper.Clear();

            avatar.Rotation = new Vector3(0, MathHelper.DegreesToRadians(degrees), 0);
            avatar.Light(lightPositions, lightColors);
            avatar.Animate(time);

            avatar.SetMatrices(camera.GetViewMatrix(), camera.GetProjectionMatrix(), camera.Position);
            avatar.Draw();

            platform.Scale = Vector3.One * 0.1f;
            platform.Position = new Vector3(0.0f, -0.12f, 0.0f);
            platform.Light(lightPositions, lightColors);
            platform.SetMatrices(camera.GetViewMatrix(), camera.GetProjectionMatrix(), camera.Position);
            platform.Draw();

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
                    degrees += deltaX * 10;
                }
            }
        }

        private void OnKeyUp(IKeyboard arg1, Key arg2, int arg3)
        {
            if (AnimatedModel.DEBUG_BONES && arg2 == Key.B)
            {
                avatar.DebugBoneIndex++;
                if (avatar.DebugBoneIndex > 66)
                {
                    avatar.DebugBoneIndex = 0;
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
