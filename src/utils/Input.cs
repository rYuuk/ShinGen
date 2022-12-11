using System.Numerics;
using Silk.NET.Input;

namespace OpenGLEngine
{
    public class Input
    {
        private readonly Camera camera;
        private readonly IKeyboard input;
        private readonly IMouse mouse;
        private readonly Action closed;

        // A boolean set to true to detect whether or not the mouse has been moved for the first time.
        private bool firstMove;
        // The last position of the mouse so we can calculate the mouse offset easily.
        private Vector2 lastPos;

        public Input(Camera camera, IKeyboard input, IMouse mouse, Action closed)
        {
            this.camera = camera;
            this.input = input;
            this.mouse = mouse;
            this.closed = closed;
        }

        public void Update(float deltaTime)
        {
            if (input.IsKeyPressed(Key.Escape))
            {
                closed?.Invoke();
            }

            if (input.IsKeyPressed(Key.W))
            {
                camera.Position += camera.Front * camera.Speed * deltaTime; // Forward
            }

            if (input.IsKeyPressed(Key.S))
            {
                camera.Position -= camera.Front * camera.Speed * deltaTime; // Backwards
            }
            if (input.IsKeyPressed(Key.A))
            {
                camera.Position -= camera.Right * camera.Speed * deltaTime; // Left
            }
            if (input.IsKeyPressed(Key.D))
            {
                camera.Position += camera.Right * camera.Speed * deltaTime; // Right
            }
            if (input.IsKeyPressed(Key.Space))
            {
                camera.Position += camera.Up * camera.Speed * deltaTime; // Up
            }
            if (input.IsKeyPressed(Key.ShiftLeft))
            {
                camera.Position -= camera.Up * camera.Speed * deltaTime; // Down
            }

            if (input.IsKeyPressed(Key.Space))
            {
                camera.Position += camera.Up * camera.Speed * deltaTime;
            }

            if (input.IsKeyPressed(Key.ShiftRight))
            {
                camera.Position -= camera.Up * camera.Speed * deltaTime;
            }

            if (firstMove)
            {
                lastPos = new Vector2(mouse.Position.X, mouse.Position.Y);
                firstMove = false;
            }
            else
            {
                var deltaX = mouse.Position.X - lastPos.X;
                var deltaY = mouse.Position.Y - lastPos.Y;
                lastPos = new Vector2(mouse.Position.X, mouse.Position.Y);
                
                // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
                camera.Yaw += deltaX * camera.Sensitivity;
                // Reversed since y-coordinates range from bottom to top
                camera.Pitch -= deltaY * camera.Sensitivity;
            }
        }
    }
}
