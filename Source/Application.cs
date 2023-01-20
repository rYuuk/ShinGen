using System.Diagnostics;
using System.Numerics;
using OpenGLEngine.UI;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace OpenGLEngine
{
    public class ModelApplication : IDisposable
    {
        private const string DOWNLOADED_AVATAR_FILE_PATH = "Resources/Avatar/SingleMesh/DownloadedAvatar.glb";
        private readonly IWindow window;

        private GL gl = null!;

        private Input input = null!;
        private Camera camera = null!;

        private CubemapRenderer cubemapRenderer = null!;

        private AnimatedModel avatar = null!;
        private Model room = null!;
        private UIController uiController = null!;

        private readonly Stopwatch stopwatch;

        private bool firstMove;
        private float lastPos;

        private float ElapsedSeconds => stopwatch.ElapsedMilliseconds / 1000f;

        private readonly Vector3[] lightPositions =
        {
            new Vector3(0.0f, 5.0f, 8.0f)
        };

        private readonly Vector3[] lightColors =
        {
            new Vector3(255.0f, 255.0f, 255.0f),
        };

        private bool isFocused = true;
        private float degrees;

        private CancellationTokenSource ctx = null!;
        private AvatarDownloadStatus avatarDownloadStatus;

        private enum AvatarDownloadStatus
        {
            None,
            InProgress,
            Downloaded
        }

        public ModelApplication()
        {
            stopwatch = new Stopwatch();
            stopwatch.Start();

            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(Constants.WINDOW_WIDTH, Constants.WINDOW_HEIGHT);
            options.Title = Constants.TITLE;
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

            uiController = new UI.UIController(gl, window, windowInput);
            uiController.DownloadButtonClicked += DownloadAvatarAsync;

            for (var i = 0; i < windowInput.Mice.Count; i++)
            {
                windowInput.Mice[i].Cursor.CursorMode = CursorMode.Normal;
                windowInput.Mice[i].MouseMove += OnMouseMove;
                windowInput.Mice[i].Scroll += OnMouseWheel;
            }

            windowInput.Keyboards[0].KeyUp += OnKeyUp;

            camera = new Camera(new Vector3(0, 1.6f, 3), window.Size.X / (float) window.Size.Y, 2f, 0.1f);
            input = new Input(camera, windowInput.Keyboards[0], windowInput.Mice[0], window.Close);

            cubemapRenderer = new CubemapRenderer();
            cubemapRenderer.Load();

            avatar = new AnimatedModel("Resources/Avatar/MultiMesh/Avatar.glb");
            // avatar = new AnimatedModel("Resources/Avatar/SingleMesh/Avatar.glb");
            avatar.SetupMesh();

            room = new Model("Resources/OfficeRoom.glb");
            room = new Model("Resources/EpicRoom.glb");
            room.SetupMesh();

            stopwatch.Stop();
        }

        private void OnUnload()
        {
            avatar.Dispose();
            room.Dispose();
            cubemapRenderer.Dispose();
        }

        private void OnRender(double deltaTime)
        {
            RenderHelper.Clear();

            room.Scale = Vector3.One * 0.2f;
            room.Position = new Vector3(0.0f, 0, 0f);
            room.Rotation = new Vector3(0, -45, 0);
            room.Light(lightPositions, lightColors);
            room.SetMatrices(camera.GetViewMatrix(), camera.GetProjectionMatrix(), camera.Position);
            room.Draw();

            // gl.DepthFunc(DepthFunction.Lequal);
            // cubemapRenderer.Draw(camera.GetViewMatrix(), camera.GetProjectionMatrix());
            // gl.DepthFunc(DepthFunction.Less);

            if (avatarDownloadStatus is AvatarDownloadStatus.Downloaded)
            {
                avatarDownloadStatus = AvatarDownloadStatus.None;
                avatar = new AnimatedModel(DOWNLOADED_AVATAR_FILE_PATH);
                avatar.SetupMesh();
                uiController.AddLog($"Model loading completed : {ElapsedSeconds}s");
            }

            avatar.Position = new Vector3(0.6f, 0.1f, 0.2f);
            avatar.Scale = Vector3.One * 0.5f;
            avatar.Rotation = new Vector3(0, degrees, 0);
            avatar.Light(lightPositions, lightColors);
            avatar.Animate(deltaTime);

            avatar.SetMatrices(camera.GetViewMatrix(), camera.GetProjectionMatrix(), camera.Position);
            avatar.Draw();

            uiController.Begin(deltaTime);
            uiController.RenderAvatarLoaderWindow();
            uiController.RenderDebugWindow(deltaTime, avatar, room);
            uiController.RenderLogWindow();
            uiController.End();

            // draw in wireframe
            // gl.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
        }

        private async void DownloadAvatarAsync(string path)
        {
            if (avatarDownloadStatus == AvatarDownloadStatus.InProgress)
            {
                ctx.Cancel();
            }
            ctx = new CancellationTokenSource();
            avatarDownloadStatus = AvatarDownloadStatus.InProgress;
            
            var progress = new Progress<float>(progress => uiController.ProgressLog("Downloading...", progress));
            await WebRequestDispatcher.DownloadRequest(path, DOWNLOADED_AVATAR_FILE_PATH, progress, ctx.Token);
            
            uiController.AddLog($"Saved at path : {DOWNLOADED_AVATAR_FILE_PATH}, {ElapsedSeconds}s");
            avatarDownloadStatus = AvatarDownloadStatus.Downloaded;
        }

        private void OnMouseMove(IMouse mouse, Vector2 position)
        {
            if (position.X < window.Size.X / 2f)
            {
                return;
            }

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
            if (mouse.Position.X < window.Size.X / 2f)
            {
                return;
            }

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
