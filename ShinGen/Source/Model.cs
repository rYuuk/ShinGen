using System.Numerics;

namespace ShinGen
{
    public class Model : IDisposable
    {
        private readonly string path;
        private List<Mesh> meshes;

        public List<Mesh> Meshes => meshes;
        public Dictionary<string, BoneInfo> BoneInfoDict;
        public int BoneCounter;

        public Matrix4x4 GlobalInverseTransformation;
        protected Shader Shader;

        private readonly Dictionary<Mesh, MeshRenderer> meshRendererMap;

        public bool IsLoaded;
        
        public Model(string path)
        {
            this.path = path;
            meshRendererMap = new Dictionary<Mesh, MeshRenderer>();
        }

        public void Load()
        {
            var importer = new ModelImporter();
            meshes = importer.LoadModel(path);
            BoneInfoDict = importer.BoneInfoMap;
            BoneCounter = importer.BoneCount;

            GlobalInverseTransformation = importer.GlobalInverseTransformation;

            Shader = RenderFactory.CreateShader(
                "Source/Shaders/shader.vert",
                "Source/Shaders/shader.frag");
            SetupMesh();
        }

        private void SetupMesh()
        {
            foreach (var mesh in meshes)
            {
                var meshRenderer = new MeshRenderer(mesh);
                meshRenderer.SetupMesh();
                meshRendererMap.Add(mesh, meshRenderer);
            }

            IsLoaded = true;
        }

        public void Light(Vector3[] lightPositions, Vector3[] lightColors)
        {
            Shader.Bind();
            for (var i = 0; i < lightPositions.Length; i++)
            {
                Shader.SetVector3("lightPositions[" + i + "]", lightPositions[i]);
                Shader.SetVector3("lightColors[" + i + "]", lightColors[i]);
            }
        }

        public void Draw(Matrix4x4 model, Matrix4x4 view, Matrix4x4 projection, Vector3 camPos)
        {
            Shader.Bind();
            Shader.SetMatrix4("model", model);
            Shader.SetMatrix4("view", view);
            Shader.SetMatrix4("projection", projection);
            Shader.SetVector3("camPos", camPos);

            foreach (var mesh in meshRendererMap)
            {
                Shader.SetMatrix4("modelTransformation", mesh.Key.Transformation, true);
                mesh.Value.Draw(Shader);
            }
        }

        public void Dispose()
        {
            foreach (var meshRenderer in meshRendererMap.Values)
            {
                meshRenderer.Dispose();
            }
            Shader.Unbind();
            Shader.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
