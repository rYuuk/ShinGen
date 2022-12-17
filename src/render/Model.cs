namespace OpenGLEngine
{
    public class Model : IDisposable
    {
        private readonly List<Mesh> meshes;
        private readonly List<MeshRenderer> meshRenderers;

        private readonly ModelImporter importer;
        // private readonly GLTFImporter importer;
        
        public Model(string path)
        {
            meshRenderers = new List<MeshRenderer>();
            importer = new ModelImporter();
            // importer = new GLTFImporter();
            meshes =  importer.LoadModel(path);
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
            importer.Dispose();
        }
    }
}
