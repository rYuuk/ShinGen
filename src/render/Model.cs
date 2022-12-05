namespace OpenGLEngine
{
    public class Model : IDisposable
    {
        private readonly List<Mesh> meshes;
        private readonly List<MeshRenderer> meshRenderers;

        private readonly GLTFImporter importer;
        
        public Model(string path)
        {
            meshRenderers = new List<MeshRenderer>();
            importer = new GLTFImporter();
            meshes = importer.Import(path);
        }

        public void SetupMesh()
        {
            foreach (var mesh in meshes)
            {
                var meshRender = new MeshRenderer(mesh);
                meshRender.SetupMesh();
                meshRenderers.Add(meshRender);
            }
        }

        public void Draw(Shader shader, Renderer renderer)
        {
            foreach (var meshRenderer in meshRenderers)
            {
                meshRenderer.Draw(shader, renderer);
            }
        }

        public void Dispose()
        {
            importer.Dispose();
        }
    }
}
