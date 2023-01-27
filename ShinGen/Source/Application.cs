using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace ShinGen
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

        private bool firstMove;
        private float lastPos;

        private readonly Vector3[] lightPositions =
        {
            new Vector3(0.0f, 4.0f, 6.0f)
        };

        private readonly Vector3[] lightColors =
        {
            new Vector3(255.0f, 255.0f, 255.0f),
        };

        private bool isFocused = true;
        private float mouseRotationY;

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

            uiController = new UIController(gl, window, windowInput);
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

            uiController.RestartStopwatch();
            cubemapRenderer = new CubemapRenderer();
            cubemapRenderer.Load();
            uiController.AddTimedLog("Cube map loading completed");
            
            uiController.RestartStopwatch();
            avatar = new AnimatedModel("Resources/Avatar/MultiMesh/Avatar.glb");
            // avatar = new AnimatedModel("Resources/Avatar/SingleMesh/Avatar.glb");
            avatar.SetupMesh();
            uiController.AddTimedLog($"Avatar model loading completed");

            uiController.RestartStopwatch();
            // room = new Model("Resources/OfficeRoom.glb");
            room = new Model("Resources/EpicRoom.glb");
            room.SetupMesh();
            uiController.AddTimedLog($"Room model loading completed");
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
            room.Draw(camera.GetViewMatrix(), camera.GetProjectionMatrix(), camera.Position);

            gl.DepthFunc(DepthFunction.Lequal);
            cubemapRenderer.Draw(camera.GetViewMatrix(5), camera.GetProjectionMatrix());
            gl.DepthFunc(DepthFunction.Less);

            if (avatarDownloadStatus is AvatarDownloadStatus.Downloaded)
            {
                uiController.RestartStopwatch();
                avatarDownloadStatus = AvatarDownloadStatus.None;
                avatar = new AnimatedModel(DOWNLOADED_AVATAR_FILE_PATH);
                avatar.SetupMesh();
                uiController.AddTimedLog($"Avatar model loading completed");
            }

            avatar.Position = new Vector3(0.6f, 0.1f, 0.2f);
            avatar.Scale = Vector3.One * 0.5f;
            avatar.Rotation = new Vector3(0, mouseRotationY, 0);
            avatar.Light(lightPositions, lightColors);
            avatar.Animate(deltaTime);

            avatar.Draw(camera.GetViewMatrix(), camera.GetProjectionMatrix(), camera.Position);

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
                uiController.AddTimedLog($"Download cancelled.");
                avatarDownloadStatus = AvatarDownloadStatus.None;
                ctx.Cancel();
                return;
            }
            ctx = new CancellationTokenSource();
            avatarDownloadStatus = AvatarDownloadStatus.InProgress;

            uiController.RestartStopwatch();
            uiController.StartProgressLog();
            var progress = new Progress<float>(progress => uiController.AddProgressLog("Downloading...", progress));
            await WebRequestDispatcher.DownloadRequest(path, DOWNLOADED_AVATAR_FILE_PATH, progress, ctx.Token);
            if (ctx.IsCancellationRequested)
            {
                return;
            }
            uiController.AddTimedLog($"Downloaded at - {DOWNLOADED_AVATAR_FILE_PATH}");
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
                    mouseRotationY += deltaX * 10;
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
