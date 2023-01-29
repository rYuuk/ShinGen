using System.Numerics;
using Silk.NET.Input;

namespace ShinGen
{
    public class RotateComponent : IComponent, IInput
    {
        private float lastPos;
        private bool firstMove;

        private float mouseRotationY;

        public Transform Transform { get; set; }

        public void OnMouseMove(IMouse mouse, Vector2 position)
        {
            // if (position.X < window.Size.X / 2f)
            // {
            // return;
            // }

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
                    var rotation = Transform.Rotation;
                    Transform.Rotation = rotation with
                    {
                        Y = mouseRotationY
                    };
                }
            }
        }

        public void OnKeyUp(IKeyboard arg1, Key arg2, int arg3)
        {
            
        }

    }
}
