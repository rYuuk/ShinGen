using System.Numerics;

namespace ShinGen
{
    public class ModelRendererComponent : IComponent, IRenderer
    {
        public Transform Transform { get; set; } = null!;

        public Model Model { get; private set; } = null!;
        public bool IsLoaded { get; private set; }

        private Light[] lights = null!;

        public void SetPath(string path)
        {
            Model = new Model(path);
        }

        public void Load()
        {
            Model.Load();
            IsLoaded = true;
        }

        public void SetLights(Light[] sceneLights)
        {
            lights = sceneLights;
        }

        public void Render(Matrix4x4 view, Matrix4x4 projection, Vector3 camPos)
        {
            Model.Light(lights);
            Model.Draw(Transform.ModelMatrix, view, projection, camPos);
        }

        public void Dispose()
        {
            Model.Dispose();
        }
    }
}
