using System.Numerics;

namespace ShinGen
{
    public class Model : IDisposable
    {
        private readonly List<Mesh> meshes;

        public Vector3 Position = Vector3.Zero;
        public Vector3 Rotation = Vector3.Zero;
        public Vector3 Scale = Vector3.One;

        public List<Mesh> Meshes => meshes;
        public readonly Dictionary<string, BoneInfo> BoneInfoDict;
        public readonly int BoneCounter;

        public readonly Matrix4x4 GlobalInverseTransformation;
        protected readonly Shader Shader;

        private readonly Dictionary<Mesh, MeshRenderer> meshRendererMap;
        private Matrix4x4 modelMatrix;

        public Model(string path)
        {
            meshRendererMap = new Dictionary<Mesh, MeshRenderer>();

            var importer = new ModelImporter();
            meshes = importer.LoadModel(path);
            BoneInfoDict = importer.BoneInfoMap;
            BoneCounter = importer.BoneCount;

            GlobalInverseTransformation = importer.GlobalInverseTransformation;

            Shader = RenderFactory.CreateShader(
                "Source/Shaders/shader.vert",
                "Source/Shaders/shader.frag");
        }

        public void SetupMesh()
        {
            foreach (var mesh in meshes)
            {
                var meshRenderer = new MeshRenderer(mesh);
                meshRenderer.SetupMesh();
                meshRendererMap.Add(mesh, meshRenderer);
            }
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

        public void Draw(Matrix4x4 view, Matrix4x4 projection, Vector3 camPos)
        {
            SetMatrices(view, projection, camPos);

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

        private void SetMatrices(Matrix4x4 view, Matrix4x4 projection, Vector3 camPos)
        {
            Shader.Bind();
            Shader.SetMatrix4("view", view);
            Shader.SetMatrix4("projection", projection);
            Shader.SetVector3("camPos", camPos);

            modelMatrix = Matrix4x4.CreateScale(Scale);
            modelMatrix *= Matrix4x4.CreateFromYawPitchRoll(
                MathHelper.DegreesToRadians(Rotation.Y),
                MathHelper.DegreesToRadians(Rotation.X),
                MathHelper.DegreesToRadians(Rotation.Z));
            modelMatrix *= Matrix4x4.CreateTranslation(Position);
            Shader.SetMatrix4("model", modelMatrix);
        }
    }
}
