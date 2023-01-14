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

        private static GL gl = null!;

        private Input input = null!;
        // Instance of the camera class to manage the view and projection matrix code.
        private Camera camera = null!;

        // private CubeRenderer cubeRenderer = null!;
        private CubemapRenderer cubemapRenderer = null!;

        private Shader avatarShader = null!;
        private Model avatar = null!;
        private Animator animator = null!;

        private Shader platformShader = null!;
        private Model platform = null!;

        private bool enableAnimation = false;
        private bool debugShowBones = false;

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
        private int displayBoneIndex;

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

        private IKeyboard windowInputKeyboard;

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

            // Initialize the camera so that it is 3 units back from where the rectangle is.
            camera = new Camera(new Vector3(-1, 0.8f, 3), window.Size.X / (float) window.Size.Y, 1f, 0.1f);
            // To make the mouse cursor invisible and captured so to have proper FPS-camera movement.
            windowInputKeyboard = windowInput.Keyboards[0];
            input = new Input(camera, windowInputKeyboard, windowInput.Mice[0], window.Close);

            avatarShader = new Shader(
                gl,
                "src/shaders/shader.vert",
                "src/shaders/shader.frag");

            Console.WriteLine($"Avatar Shader Loaded: {ElapsedSeconds}s");

            platformShader = new Shader(
                gl,
                "src/shaders/shader.vert",
                "src/shaders/shader.frag");

            Console.WriteLine($"Platform Shader Loaded: {ElapsedSeconds}s");

            // cubeRenderer = new CubeRenderer();

            cubemapRenderer = new CubemapRenderer();
            Console.WriteLine($"Cubemap Shader Loaded: {ElapsedSeconds}s");

            // cubeRenderer.Load();
            cubemapRenderer.Load();
            Console.WriteLine($"Cubemap loaded: {ElapsedSeconds}s");

            // avatar = new Model("Resources/Avatar/MultiMesh/Avatar.glb");
            avatar = new Model("Resources/Avatar/SingleMesh/Avatar.glb");
            avatar.SetupMesh();

            if (enableAnimation)
            {
                var animation = new AnimationLoader("resources/Avatar/SingleMesh/Walking.dae", avatar);
                animator = new Animator(animation);
            }

            Console.WriteLine($"Avatar loaded: {ElapsedSeconds}s");

            platform = new Model("Resources/Platform.glb");
            platform.SetupMesh();

            Console.WriteLine($"Platform loaded: {ElapsedSeconds}s");
            stopwatch.Stop();
        }

        private void OnKeyUp(IKeyboard arg1, Key arg2, int arg3)
        {
            if (debugShowBones && arg2 == Key.B)
            {
                displayBoneIndex++;
                if (displayBoneIndex > 66)
                {
                    displayBoneIndex = 0;
                }
            }
        }

        private void OnUnload()
        {
            avatarShader.Unbind();
            avatarShader.Dispose();
            avatar.Dispose();
            platform.Dispose();
            cubemapRenderer.Dispose();
        }

        private void OnRender(double time)
        {
            RenderHelper.Clear();

            if (enableAnimation)
            {
                animator.UpdateAnimation(time);
            }

            avatarShader.Bind();

            avatarShader.SetMatrix4("view", camera.GetViewMatrix());
            avatarShader.SetMatrix4("projection", camera.GetProjectionMatrix());
            avatarShader.SetVector3("camPos", camera.Position);

            avatarShader.SetInt("enableAnimation", enableAnimation ? 1 : 0);
            if (enableAnimation)
            {
                var transforms = animator.FinalBoneMatrices;
                for (var i = 0; i < transforms.Count; i++)
                {
                    avatarShader.SetMatrix4("finalBonesMatrices[" + i + "].matrix", transforms[i]);
                }
            }

            var modelMatrix = Matrix4x4.CreateScale(1f);
            modelMatrix *= Matrix4x4.CreateRotationY(MathHelper.DegreesToRadians(degrees));
            modelMatrix *= Matrix4x4.CreateTranslation(0.0f, 0.0f, 0f);
            avatarShader.SetMatrix4("model", modelMatrix);

            for (var i = 0; i < lightPositions.Length; i++)
            {
                avatarShader.SetVector3("lightPositions[" + i + "]", lightPositions[i]);
                avatarShader.SetVector3("lightColors[" + i + "]", lightColors[i]);
            }

            avatarShader.SetInt("displayBones", debugShowBones ? 1 : 0);
            if (debugShowBones)
            {
                avatarShader.SetInt("displayBoneIndex", displayBoneIndex);
            }

            avatar.Draw(avatarShader);

            platformShader.Bind();
            platformShader.SetMatrix4("view", camera.GetViewMatrix());
            platformShader.SetMatrix4("projection", camera.GetProjectionMatrix());
            platformShader.SetVector3("camPos", camera.Position);

            modelMatrix = Matrix4x4.CreateScale(0.1f);
            modelMatrix *= Matrix4x4.CreateTranslation(0.0f, -0.12f, 0.0f);
            platformShader.SetMatrix4("model", modelMatrix);

            for (var i = 0; i < lightPositions.Length; i++)
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
