using System.Numerics;
using Silk.NET.OpenGL.Extensions.ImGui;

namespace ShinGen
{
    public interface ICanvas
    {
        ImGuiController UIController { get; set; }
        Vector2 WindowSize { get; set; }

        void Render(double deltaTime);
    }
}
