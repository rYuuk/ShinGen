﻿using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace OpenGLEngine
{
    public class Game : GameWindow
    {
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
        
        public Game(int width, int height, string title) :
            base(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title })
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();

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
            vertexArray.UnBind();
            shader.Unbind();

            // Delete all the resources.
            vertexBuffer.Dispose();
            vertexArray.Dispose();
            shader.Dispose();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            renderer.Clear();

            Matrix4 lampMatrix = Matrix4.Identity;
            lampMatrix *= Matrix4.CreateScale(0.2f);
            lampMatrix *= Matrix4.CreateTranslation(lampPos);

            lampShader.Bind();
            lampShader.SetMatrix4("model", lampMatrix);
            lampShader.SetMatrix4("view", camera.GetViewMatrix());
            lampShader.SetMatrix4("projection", camera.GetProjectionMatrix());

            renderer.Draw(vertexArray, cubeData.Vertices, lampShader);

            diffuseMap.Bind();
            specularMap.Bind(1);
            
            shader.Bind();
            shader.SetMatrix4("view", camera.GetViewMatrix());
            shader.SetMatrix4("projection", camera.GetProjectionMatrix());
            shader.SetVector3("viewPos", camera.Position);

            shader.SetInt("material.diffuse", 0);
            shader.SetInt("material.specular", 1);
            shader.SetFloat("material.shininess", 32.0f);

            shader.SetVector3("light.position", lampPos);
            shader.SetVector3("light.ambient", new Vector3(0.2f));
            shader.SetVector3("light.diffuse", new Vector3(0.5f));
            shader.SetVector3("light.specular", new Vector3(1.0f));
            shader.SetFloat("light.constant",  1.0f);
            shader.SetFloat("light.linear", 0.09f);
            shader.SetFloat("light.quadratic", 0.032f);

            for (var i = 0; i < cubePositions.Length; i++)
            {
                Matrix4 model = Matrix4.Identity;
                model *= Matrix4.CreateTranslation(cubePositions[i]);
                var angle = 20.0f * i;
                model *= Matrix4.CreateFromAxisAngle(new Vector3(1.0f, 0.3f, 0.5f), angle);
                shader.SetMatrix4("model", model);
    
                renderer.Draw(vertexArray, cubeData.Vertices, shader);
            }

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
