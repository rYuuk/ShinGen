namespace OpenGLEngine
{
    public struct Mesh
    {
        public readonly string Name;
        public readonly Vertex[] Vertices;
        public readonly uint[] Indices;
        public readonly Texture[] Textures;
        public readonly bool UseNormalMap;
        
        public Mesh( string name,Vertex[] vertices, uint[] indices, Texture[] textures)
        {
            Vertices = vertices;
            Indices = indices;
            Textures = textures;
            Name = name;
            UseNormalMap = Textures.Any(x => x.Type == "normalMap");
        }
    }
}
