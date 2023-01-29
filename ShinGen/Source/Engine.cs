using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

namespace ShinGen
{
    public class Engine : IDisposable
    {
        private readonly IWindow window;

        private GL gl = null!;

        private Input input = null!;
        private Camera camera = null!;

        private CubemapRenderer cubemapRenderer = null!;

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

        private CancellationTokenSource ctx = null!;

        private List<GameObject> gameObjects;
        private List<GameObject> disposableGameObjects;
        private List<ICanvas> canvases;

        private ImGuiController imGui;
        private IInputContext windowInput;

        public Engine()
        {
            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(Constants.WINDOW_WIDTH, Constants.WINDOW_HEIGHT);
            options.Title = Constants.TITLE;
            options.Samples = 4;

            window = Window.Create(options);
            RegisterEvents();

            gameObjects = new List<GameObject>();
            disposableGameObjects = new List<GameObject>();
            canvases = new List<ICanvas>();
        }

        public void Start()
        {
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
            windowInput = window.CreateInput();

            RenderHelper.SetRenderer(gl);
            RenderFactory.SetRenderer(gl);
            TextureLoader.SetRenderer(gl);

            RenderHelper.LoadSettings();

            // uiController = new UIController(gl, window, windowInput);
            // uiController.DownloadButtonClicked += DownloadAvatarAsync;
            imGui = new ImGuiController(gl, window, windowInput);

            for (var i = 0; i < windowInput.Mice.Count; i++)
            {
                windowInput.Mice[i].Cursor.CursorMode = CursorMode.Normal;
                // windowInput.Mice[i].MouseMove += OnMouseMove;
                windowInput.Mice[i].Scroll += OnMouseWheel;
            }

            windowInput.Keyboards[0].KeyUp += OnKeyUp;

            camera = new Camera(new Vector3(0, 1.6f, 3), window.Size.X / (float) window.Size.Y, 2f, 0.1f);
            input = new Input(camera, windowInput.Keyboards[0], windowInput.Mice[0], window.Close);

            cubemapRenderer = new CubemapRenderer();
            cubemapRenderer.Load();

            foreach (var gameObject in gameObjects)
            {
                foreach (var component in gameObject.GetAllComponents())
                {
                    if (component is IRenderer renderer)
                    {
                        renderer.Load();
                    }
                }

            }
        }

        public void AddGameObject(GameObject gameObject)
        {
            gameObjects.Add(gameObject);
        }

        public void DestroyGameObject(GameObject gameObject)
        {
            gameObjects.Remove(gameObject);
            disposableGameObjects.Add(gameObject);

        }

        public T CreateUI<T>() where T : ICanvas, new()
        {
            var canvas = new T();
            canvas.UIController = imGui;
            canvas.WindowSize = new Vector2(window.Size.X, window.Size.Y);
            canvases.Add(canvas);
            return canvas;
        }


        private void OnUnload()
        {
            foreach (var gameObject in gameObjects)
            {
                foreach (var component in gameObject.GetAllComponents())
                {
                    if (component is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
            }

            cubemapRenderer.Dispose();
        }

        private bool doOnce = true;

        private void OnRender(double deltaTime)
        {
            RenderHelper.Clear();

            foreach (var gameObject in gameObjects.ToList())
            {
                // windowInput.Mice[0].MouseMove += gameObject.OnMouseMove;
                foreach (var component in gameObject.GetAllComponents())
                {
                    if (component is IRenderer renderer)
                    {
                        if (!renderer.IsLoaded)
                        {
                            renderer.Load();
                        }
                        renderer.SetLights(lightPositions, lightColors);
                        renderer.Render(camera.GetViewMatrix(), camera.GetProjectionMatrix(), camera.Position);
                    }

                    if (component is IInput input)
                    {
                        windowInput.Mice[0].MouseMove += input.OnMouseMove;
                    }
                }
            }

            foreach (var gameObject in disposableGameObjects)
            {
                foreach (var component in gameObject.GetAllComponents())
                {
                    if (component is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
            }

            gl.DepthFunc(DepthFunction.Lequal);
            cubemapRenderer.Draw(camera.GetViewMatrix(5), camera.GetProjectionMatrix());
            gl.DepthFunc(DepthFunction.Less);

            if (doOnce)
            {
                doOnce = false;
                // uiController.RestartStopwatch();
                // avatarDownloadStatus = AvatarDownloadStatus.None;
                // avatar = new AnimatedModel(DOWNLOADED_AVATAR_FILE_PATH);
                // avatar.Load();
                // avatar.SetupMesh();
                // models.Add(avatar);
                // uiController.AddTimedLog($"Avatar model loading completed");
            }


            imGui.Update((float) deltaTime);
            foreach (var canvas in canvases)
            {
                canvas.Render(deltaTime);
            }
            imGui.Render();

            // draw in wireframe
            // gl.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
        }

        // private void OnMouseMove(IMouse mouse, Vector2 position)
        // {
        //     if (position.X < window.Size.X / 2f)
        //     {
        //         return;
        //     }
        //
        //     if (mouse.IsButtonPressed(MouseButton.Left))
        //     {
        //         if (firstMove)
        //         {
        //             lastPos = position.X;
        //             firstMove = false;
        //         }
        //         else
        //         {
        //             var deltaX = position.X - lastPos;
        //             lastPos = position.X;
        //             mouseRotationY += deltaX * 10;
        //         }
        //     }
        // }

        private void OnKeyUp(IKeyboard arg1, Key arg2, int arg3)
        {
            // if (AnimatedModel.DEBUG_BONES && arg2 == Key.B)
            // {
            //     avatar.DebugBoneIndex++;
            //     if (avatar.DebugBoneIndex > 66)
            //     {
            //         avatar.DebugBoneIndex = 0;
            //     }
            // }
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
