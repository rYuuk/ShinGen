namespace OpenGLEngine
{
    public struct Mesh
    {
        public readonly Vertex[] Vertices;
        public readonly uint[] Indices;
        public readonly Texture[] Textures;

        public Mesh(Vertex[] vertices, uint[] indices, Texture[] textures)
        {
            Vertices = vertices;
            Indices = indices;
            Textures = textures;
        }
    }
}
