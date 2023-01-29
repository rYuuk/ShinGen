using Silk.NET.Input;

namespace ShinGen
{
    public class Input
    {
        private readonly Camera camera;
        private readonly IKeyboard input;
        private readonly Action closed;

        public Input(Camera camera, IKeyboard input, Action closed)
        {
            this.camera = camera;
            this.input = input;
            this.closed = closed;
        }

        public void Update(float deltaTime)
        {
            if (input.IsKeyPressed(Key.Escape))
            {
                closed();
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

            if (input.IsKeyPressed(Key.ShiftRight))
            {
                camera.Position -= camera.Up * camera.Speed * deltaTime;
            }
        }
    }
}
