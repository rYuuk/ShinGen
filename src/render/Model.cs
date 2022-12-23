namespace OpenGLEngine
{
    public class Model : IDisposable
    {
        private readonly List<Mesh> meshes;
        private readonly List<MeshRenderer> meshRenderers;

        public readonly Dictionary<string, BoneInfo> BoneInfoDict;
        public readonly int BoneCounter;

        public Model(string path)
        {
            meshRenderers = new List<MeshRenderer>();
            var importer = new ModelImporter();
            meshes = importer.LoadModel(path);
            BoneInfoDict = importer.BoneInfoMap;
            BoneCounter = importer.BoneCount;
        }

        public void SetupMesh()
        {
            foreach (var meshRender in meshes.Select(mesh => new MeshRenderer(mesh)))
            {
                meshRender.SetupMesh();
                meshRenderers.Add(meshRender);
            }
        }

        public void Draw(Shader shader)
        {
            foreach (var meshRenderer in meshRenderers)
            {
                meshRenderer.Draw(shader);
            }
        }

        public void Dispose()
        {
            foreach (var meshRenderer in meshRenderers)
            {
                meshRenderer.Dispose();
            }
            GC.SuppressFinalize(this);
        }
    }
}
