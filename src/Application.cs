using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace OpenGLEngine
{
    public class Application : IDisposable
    {
        private readonly GameWindow window;
        private Renderer renderer;
        private CubeData cubeData;

        private VertexArray vertexArray;
        private VertexArray vertexArrayLamp;
        private VertexBuffer vertexBuffer;

        private Shader lampShader;
        private Shader shader;

        private Texture diffuseMap;
        private Texture specularMap;

        private Input input;
        // Instance of the camera class to manage the view and projection matrix code.
        private Camera camera;

        private readonly Vector3 lampPos = new Vector3(1.2f, 1.0f, 2.0f);

        private readonly Vector3[] cubePositions =
        {
            new Vector3(0.0f, 0.0f, 0.0f),
            new Vector3(2.0f, 5.0f, -15.0f),
            new Vector3(-1.5f, -2.2f, -2.5f),
            new Vector3(-3.8f, -2.0f, -12.3f),
            new Vector3(2.4f, -0.4f, -3.5f),
            new Vector3(-1.7f, 3.0f, -7.5f),
            new Vector3(1.3f, -2.0f, -2.5f),
            new Vector3(1.5f, 2.0f, -2.5f),
            new Vector3(1.5f, 0.2f, -1.5f),
            new Vector3(-1.3f, 1.0f, -1.5f)
        };

        private readonly Vector3[] pointLightPositions =
        {
            new Vector3(0.7f, 0.2f, 2.0f),
            new Vector3(2.3f, -3.3f, -4.0f),
            new Vector3(-4.0f, 2.0f, -12.0f),
            new Vector3(0.0f, 0.0f, -3.0f)
        };

        public Application()
        {
            window = new GameWindow(GameWindowSettings.Default,
                new NativeWindowSettings() { Size = (800, 600), Title = "First Blood" });
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
            cubeData = new CubeData();
            renderer = new Renderer();
            diffuseMap = new Texture("resources/container2.png");
            specularMap = new Texture("resources/container2_specular.png");
            vertexBuffer = new VertexBuffer(cubeData.Vertices.Length * sizeof(float), cubeData.Vertices);

            vertexArray = new VertexArray();
            var layout = new VertexBufferLayout();
            layout.Push(0, 3);
            layout.Push(1, 3);
            layout.Push(2, 2);
            vertexArray.AddBuffer(vertexBuffer, layout);

            vertexArrayLamp = new VertexArray();
            var lampLayout = new VertexBufferLayout();
            lampLayout.Push(0, 3);
            lampLayout.Push(1, 3);
            lampLayout.Push(2, 2);

            vertexArrayLamp.AddBuffer(vertexBuffer, lampLayout);

            lampShader = new Shader(
                "src/shaders/shader.vert",
                "src/shaders/lighting.frag");

            shader = new Shader(
                "src/shaders/shader.vert",
                "src/shaders/shader.frag");

            // Initialize the camera so that it is 3 units back from where the rectangle is.
            camera = new Camera(Vector3.UnitZ * 3, window.Size.X / (float) window.Size.Y, 1.5f, 0.2f);

            // To make the mouse cursor invisible and captured so to have proper FPS-camera movement.
            window.CursorState = CursorState.Grabbed;

            input = new Input(camera, window.KeyboardState, window.MouseState, window.Close);
        }

        private void OnUnload()
        {
            // Unbind all the resources by binding the targets to 0/null.
            vertexBuffer.UnBind();
            vertexArray.UnBind();
            shader.Unbind();

            // Delete all the resources.
            vertexBuffer.Dispose();
            vertexArray.Dispose();
            shader.Dispose();
        }

        private void OnRender(FrameEventArgs obj)
        {
            renderer.Clear();
            diffuseMap.Bind();
            specularMap.Bind(1);

            shader.Bind();
            shader.SetMatrix4("view", camera.GetViewMatrix());
            shader.SetMatrix4("projection", camera.GetProjectionMatrix());
            shader.SetVector3("viewPos", camera.Position);

            shader.SetInt("material.diffuse", 0);
            shader.SetInt("material.specular", 1);
            shader.SetFloat("material.shininess", 32.0f);

            // Directional light
            shader.SetVector3("dirLight.direction", new Vector3(-0.2f, -1.0f, -0.3f));
            shader.SetVector3("dirLight.ambient", new Vector3(0.05f, 0.05f, 0.05f));
            shader.SetVector3("dirLight.diffuse", new Vector3(0.4f, 0.4f, 0.4f));
            shader.SetVector3("dirLight.specular", new Vector3(0.5f, 0.5f, 0.5f));

            // Point lights
            for (int i = 0; i < pointLightPositions.Length; i++)
            {
                shader.SetVector3($"pointLights[{i}].position", pointLightPositions[i]);
                shader.SetVector3($"pointLights[{i}].ambient", new Vector3(0.05f, 0.05f, 0.05f));
                shader.SetVector3($"pointLights[{i}].diffuse", new Vector3(0.8f, 0.8f, 0.8f));
                shader.SetVector3($"pointLights[{i}].specular", new Vector3(1.0f, 1.0f, 1.0f));
                shader.SetFloat($"pointLights[{i}].constant", 1.0f);
                shader.SetFloat($"pointLights[{i}].linear", 0.09f);
                shader.SetFloat($"pointLights[{i}].quadratic", 0.032f);
            }

            // Spot light
            shader.SetVector3("spotLight.position", camera.Position);
            shader.SetVector3("spotLight.direction", camera.Front);
            shader.SetVector3("spotLight.ambient", new Vector3(0.0f, 0.0f, 0.0f));
            shader.SetVector3("spotLight.diffuse", new Vector3(1.0f, 1.0f, 1.0f));
            shader.SetVector3("spotLight.specular", new Vector3(1.0f, 1.0f, 1.0f));
            shader.SetFloat("spotLight.constant", 1.0f);
            shader.SetFloat("spotLight.linear", 0.09f);
            shader.SetFloat("spotLight.quadratic", 0.032f);
            shader.SetFloat("spotLight.cutOff", MathF.Cos(MathHelper.DegreesToRadians(12.5f)));
            shader.SetFloat("spotLight.outerCutOff", MathF.Cos(MathHelper.DegreesToRadians(17.5f)));

            for (var i = 0; i < cubePositions.Length; i++)
            {
                Matrix4 model = Matrix4.Identity;
                model *= Matrix4.CreateTranslation(cubePositions[i]);
                var angle = 20.0f * i;
                model *= Matrix4.CreateFromAxisAngle(new Vector3(1.0f, 0.3f, 0.5f), angle);
                shader.SetMatrix4("model", model);

                renderer.Draw(vertexArray, cubeData.Vertices, shader);
            }

            lampShader.Bind();
            lampShader.SetMatrix4("view", camera.GetViewMatrix());
            lampShader.SetMatrix4("projection", camera.GetProjectionMatrix());

            for (var i = 0; i < pointLightPositions.Length; i++)
            {
                var lampMatrix = Matrix4.CreateScale(0.2f);
                lampMatrix *= Matrix4.CreateTranslation(pointLightPositions[i]);
                lampShader.SetMatrix4("model", lampMatrix);
                renderer.Draw(vertexArray, cubeData.Vertices, lampShader);
            }

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
