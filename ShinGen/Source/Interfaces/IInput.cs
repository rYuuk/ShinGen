using System.Numerics;
using Silk.NET.Input;

namespace ShinGen
{
    public interface IInput
    {
        void OnMouseMove(IMouse mouse, Vector2 position);
        void OnKeyUp(IKeyboard arg1, Key arg2, int arg3);
    }
}
