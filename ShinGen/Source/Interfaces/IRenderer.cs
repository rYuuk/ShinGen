using System.Numerics;

namespace ShinGen
{
    public interface IRenderer : IDisposable
    {
        bool IsLoaded { get; }

        void Load();
        void SetLights(Vector3[] lP, Vector3[] lC);
        void Render(Matrix4x4 view, Matrix4x4 projection, Vector3 camPos);
    }
}
