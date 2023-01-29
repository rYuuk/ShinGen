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

        private readonly Light[] lights =
        {
            new Light
            {
                Position = new Vector3(0.0f, 4.0f, 6.0f),
                Color = new Vector3(255.0f, 255.0f, 255.0f),
            }
        };

        private bool isFocused = true;

        private readonly List<GameObject> gameObjects;
        private readonly List<GameObject> disposableGameObjects;
        private readonly List<ICanvas> canvases;

        private ImGuiController imGui = null!;
        private IInputContext windowInput = null!;

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

            imGui = new ImGuiController(gl, window, windowInput);

            foreach (var mouse in windowInput.Mice)
            {
                mouse.Cursor.CursorMode = CursorMode.Normal;
                mouse.Scroll += OnMouseWheel;
            }

            camera = new Camera(new Vector3(0, 1.6f, 3), window.Size.X / (float) window.Size.Y, 2f, 0.1f);
            input = new Input(camera, windowInput.Keyboards[0], window.Close);

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

        private void OnRender(double deltaTime)
        {
            RenderHelper.Clear();

            foreach (var gameObject in gameObjects.ToList())
            {
                foreach (var component in gameObject.GetAllComponents())
                {
                    if (component is IRenderer renderer)
                    {
                        if (!renderer.IsLoaded)
                        {
                            renderer.Load();
                        }
                        renderer.SetLights(lights);
                        renderer.Render(camera.GetViewMatrix(), camera.GetProjectionMatrix(), camera.Position);
                    }

                    if (component is IInput inputComponent)
                    {
                        foreach (var mouse in windowInput.Mice)
                        {
                            mouse.MouseMove += inputComponent.OnMouseMove;
                        }

                        foreach (var keyboard in windowInput.Keyboards)
                        {
                            keyboard.KeyUp += inputComponent.OnKeyUp;
                        }
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

            imGui.Update((float) deltaTime);
            foreach (var canvas in canvases)
            {
                canvas.Render(deltaTime);
            }
            imGui.Render();

            // draw in wireframe
            // gl.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
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
