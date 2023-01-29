using System.Numerics;
using Silk.NET.Input;

namespace ShinGen
{
    public interface IInput
    {
        void OnMouseMove(IMouse mouse, Vector2 position);
    }
}
