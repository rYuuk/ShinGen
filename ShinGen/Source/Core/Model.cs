using System.Numerics;
using ShinGen.Core.OpenGL;

namespace ShinGen.Core
{
    public class Model : IDisposable
    {
        private readonly string path;
        private List<Mesh> meshes = null!;

        internal List<Mesh> Meshes => meshes;
        internal Dictionary<string, BoneInfo> BoneInfoDict = null!;
        public int BoneCounter;

        public Matrix4x4 GlobalInverseTransformation;
        internal Shader Shader = null!;

        private readonly Dictionary<Mesh, MeshRenderer> meshRendererMap;

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
                "Source/Core/Shaders/shader.vert",
                "Source/Core/Shaders/shader.frag");
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
        }

        public void Light(Light[] lights)
        {
            Shader.Bind();
            for (var i = 0; i < lights.Length; i++)
            {
                Shader.SetVector3("lights[" + i + "].position", lights[i].Position);
                Shader.SetVector3("lights[" + i + "].color", lights[i].Color);
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
