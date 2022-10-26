using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGLEngine
{
    public class Input
    {
        // A boolean set to true to detect whether or not the mouse has been moved for the first time.
        private bool firstMove;
        // The last position of the mouse so we can calculate the mouse offset easily.
        private Vector2 lastPos;

        public void Update(ref Camera camera, KeyboardState input, float deltaTime, MouseState mouse, Action? onClose)
        {
            if (input.IsKeyDown(Keys.Escape))
            {
                onClose?.Invoke();
            }

            if (input.IsKeyDown(Keys.W))
            {
                camera.Position += camera.Front * camera.Speed * deltaTime; // Forward
            }

            if (input.IsKeyDown(Keys.S))
            {
                camera.Position -= camera.Front * camera.Speed * deltaTime; // Backwards
            }
            if (input.IsKeyDown(Keys.A))
            {
                camera.Position -= camera.Right * camera.Speed * deltaTime; // Left
            }
            if (input.IsKeyDown(Keys.D))
            {
                camera.Position += camera.Right * camera.Speed * deltaTime; // Right
            }
            if (input.IsKeyDown(Keys.Space))
            {
                camera.Position += camera.Up * camera.Speed * deltaTime; // Up
            }
            if (input.IsKeyDown(Keys.LeftShift))
            {
                camera.Position -= camera.Up * camera.Speed * deltaTime; // Down
            }

            if (input.IsKeyDown(Keys.Space))
            {
                camera.Position += camera.Up * camera.Speed * deltaTime;
            }

            if (input.IsKeyDown(Keys.LeftShift))
            {
                camera.Position -= camera.Up * camera.Speed * deltaTime;
            }

            if (firstMove)
            {
                lastPos = new Vector2(mouse.X, mouse.Y);
                firstMove = false;
            }
            else
            {
                var deltaX = mouse.X - lastPos.X;
                var deltaY = mouse.Y - lastPos.Y;
                lastPos = new Vector2(mouse.X, mouse.Y);

                // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
                camera.Yaw += deltaX * camera.Sensitivity;
                // Reversed since y-coordinates range from bottom to top
                camera.Pitch -= deltaY * camera.Sensitivity;
            }
        }
    }
}
