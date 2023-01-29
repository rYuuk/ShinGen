using Silk.NET.Windowing;

namespace ShinGen
{
    public interface ICanvas
    {
        IWindow Window { get; set; }

        void Render(double deltaTime);
    }
}
