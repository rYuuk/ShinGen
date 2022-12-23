namespace OpenGLEngine
{
    public class Model : IDisposable
    {
        private readonly List<Mesh> meshes;
        private readonly List<MeshRenderer> meshRenderers;
        private readonly ModelImporter importer;

        public Dictionary<string, BoneInfo> BoneInfoDict => importer.BoneInfoDict;
        public int BoneCounter => importer.BoneCounter;

        public Model(string path)
        {
            meshRenderers = new List<MeshRenderer>();
            importer = new ModelImporter();
            meshes = importer.LoadModel(path);
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
