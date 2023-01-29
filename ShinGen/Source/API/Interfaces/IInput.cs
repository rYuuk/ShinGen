using System.Numerics;
using Silk.NET.Input;

namespace ShinGen
{
    public interface IMouseInput
    {
        void OnMouseMove(IMouse mouse, Vector2 position);
    }

    public interface IKeyboardInput
    {
        void OnKeyUp(IKeyboard arg1, Key arg2, int arg3);

    }
}
