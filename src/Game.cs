using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace OpenGLEngine
{
    public class Game : GameWindow
    {
        private readonly float[] vertices =
        {
            -0.5f, -0.5f, -0.5f, 0.0f, 0.0f,
            0.5f, -0.5f, -0.5f, 1.0f, 0.0f,
            0.5f, 0.5f, -0.5f, 1.0f, 1.0f,
            0.5f, 0.5f, -0.5f, 1.0f, 1.0f,
            -0.5f, 0.5f, -0.5f, 0.0f, 1.0f,
            -0.5f, -0.5f, -0.5f, 0.0f, 0.0f,

            -0.5f, -0.5f, 0.5f, 0.0f, 0.0f,
            0.5f, -0.5f, 0.5f, 1.0f, 0.0f,
            0.5f, 0.5f, 0.5f, 1.0f, 1.0f,
            0.5f, 0.5f, 0.5f, 1.0f, 1.0f,
            -0.5f, 0.5f, 0.5f, 0.0f, 1.0f,
            -0.5f, -0.5f, 0.5f, 0.0f, 0.0f,

            -0.5f, 0.5f, 0.5f, 1.0f, 0.0f,
            -0.5f, 0.5f, -0.5f, 1.0f, 1.0f,
            -0.5f, -0.5f, -0.5f, 0.0f, 1.0f,
            -0.5f, -0.5f, -0.5f, 0.0f, 1.0f,
            -0.5f, -0.5f, 0.5f, 0.0f, 0.0f,
            -0.5f, 0.5f, 0.5f, 1.0f, 0.0f,

            0.5f, 0.5f, 0.5f, 1.0f, 0.0f,
            0.5f, 0.5f, -0.5f, 1.0f, 1.0f,
            0.5f, -0.5f, -0.5f, 0.0f, 1.0f,
            0.5f, -0.5f, -0.5f, 0.0f, 1.0f,
            0.5f, -0.5f, 0.5f, 0.0f, 0.0f,
            0.5f, 0.5f, 0.5f, 1.0f, 0.0f,

            -0.5f, -0.5f, -0.5f, 0.0f, 1.0f,
            0.5f, -0.5f, -0.5f, 1.0f, 1.0f,
            0.5f, -0.5f, 0.5f, 1.0f, 0.0f,
            0.5f, -0.5f, 0.5f, 1.0f, 0.0f,
            -0.5f, -0.5f, 0.5f, 0.0f, 0.0f,
            -0.5f, -0.5f, -0.5f, 0.0f, 1.0f,

            -0.5f, 0.5f, -0.5f, 0.0f, 1.0f,
            0.5f, 0.5f, -0.5f, 1.0f, 1.0f,
            0.5f, 0.5f, 0.5f, 1.0f, 0.0f,
            0.5f, 0.5f, 0.5f, 1.0f, 0.0f,
            -0.5f, 0.5f, 0.5f, 0.0f, 0.0f,
            -0.5f, 0.5f, -0.5f, 0.0f, 1.0f
        };

        private readonly uint[] indices =
        {
            0, 1, 3,
            1, 2, 3
        };

        private VertexArray vertexArray;
        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;
        private Shader shader;
        private Texture texture;
        private Texture texture2;
        private double time;
        private Renderer renderer;

        private Input input;
        // Instance of the camera class to manage the view and projection matrix code.
        private Camera camera;

        public Game(int width, int height, string title) :
            base(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title })
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            // Enable depth testing so z-buffer can be checked for fragments and only those which are in front be drawn.
            GL.Enable(EnableCap.DepthTest);

            renderer = new Renderer();
            vertexArray = new VertexArray();
            vertexBuffer = new VertexBuffer(vertices.Length * sizeof(float), vertices);

            var layout = new VertexBufferLayout();
            layout.Push(0, 3);
            layout.Push(1, 2);

            vertexArray.AddBuffer(vertexBuffer, layout);
            indexBuffer = new IndexBuffer(indices.Length, indices);

            shader = new Shader(
                "src/shaders/shader.vert",
                "src/shaders/shader.frag");
            shader.Bind();
          
            texture2 = new Texture("Resources/awesomeface.png");
            texture2.Bind(1);
            shader.SetInt("texture1", 1);

            texture = new Texture("Resources/container.jpg");
            texture.Bind();
            shader.SetInt("texture0", 0);
            
            // Initialize the camera so that it is 3 units back from where the rectangle is.
            camera = new Camera(Vector3.UnitZ * 3, Size.X / (float) Size.Y, 1.5f, 0.2f);

            // To make the mouse cursor invisible and captured so to have proper FPS-camera movement.
            CursorState = CursorState.Grabbed;

            input = new Input();
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            // Unbind all the resources by binding the targets to 0/null.
            vertexBuffer.UnBind();
            indexBuffer.UnBind();
            vertexArray.UnBind();
            shader.Unbind();

            // Delete all the resources.
            vertexBuffer.Dispose();
            indexBuffer.Dispose();
            vertexArray.Dispose();
            shader.Dispose();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            time += 8 * args.Time;

            renderer.Clear();

            // Determines the position of the model in the world.
            var model = Matrix4.CreateRotationX((float) MathHelper.DegreesToRadians(time));
            shader.SetMatrix4("model", model);
            shader.SetMatrix4("view", camera.GetViewMatrix());
            shader.SetMatrix4("projection", camera.GetProjectionMatrix());

            renderer.Draw(vertexArray, vertices, shader);
            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (!IsFocused)
            {
                return;
            }

            input.Update(ref camera, KeyboardState, (float) e.Time, MouseState, Close);
        }

        // This manage all the zooming of the camera.
        // This is simply done by changing the FOV of the camera.
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            camera.Fov -= e.OffsetY;
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, e.Width, e.Height);

            // Update the aspect ratio once the window has been resized.
            camera.AspectRatio = Size.X / (float) Size.Y;
        }
    }
}
