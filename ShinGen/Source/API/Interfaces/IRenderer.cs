using System.Numerics;

namespace ShinGen
{
    public struct Light
    {
        public Vector3 Position;
        public Vector3 Color;
    }

    public interface IRenderer : IDisposable
    {
        bool IsLoaded { get; }

        void Load();
        void SetLights(Light[] lights);
        void Render(Matrix4x4 view, Matrix4x4 projection, Vector3 camPos);
    }
}
