using System.Numerics;

namespace ShinGen
{
    public class ModelRendererComponent : IComponent, IRenderer
    {
        private readonly Model model;
        public GameObject GameObject { get; set; }

        public Model Model => model;
        public bool IsLoaded { get; private set; }

        private Light[] lights;


        public ModelRendererComponent(string path)
        {
            model = new Model(path);
        }

        public void Load()
        {
            model.Load();
            IsLoaded = true;

        }

        public void SetLights(Light[] sceneLights)
        {
            lights = sceneLights;
        }

        public void Render(Matrix4x4 view, Matrix4x4 projection, Vector3 camPos)
        {
            model.Light(lights);
            model.Draw(GameObject.Transform.ModelMatrix, view, projection, camPos);
        }

        public void Dispose()
        {
            model.Dispose();
        }
    }
}
