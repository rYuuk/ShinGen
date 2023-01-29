using System.Numerics;

namespace ShinGen
{
    public class ModelRendererComponent : IComponent, IRenderer
    {
        private readonly Model model;
        public GameObject GameObject { get; set; }

        public Model Model => model;
        public bool IsLoaded { get; private set; }

        private Vector3[] lightPosiiton;
        private Vector3[] ligthColor;

        public ModelRendererComponent(string path)
        {
            model = new Model(path);
        }

        public void Load()
        {
            model.Load();
            IsLoaded = true;

        }

        public void SetLights(Vector3[] lP, Vector3[] lC)
        {
            lightPosiiton = lP;
            ligthColor = lC;
        }

        public void Render(Matrix4x4 view, Matrix4x4 projection, Vector3 camPos)
        {
            model.Light(lightPosiiton, ligthColor);
            model.Draw(GameObject.Transform.ModelMatrix, view, projection, camPos);
        }

        public void Dispose()
        {
            model.Dispose();
        }
    }
}
