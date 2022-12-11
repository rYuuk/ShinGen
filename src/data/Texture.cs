namespace OpenGLEngine
{
    public struct Texture
    {
        public uint ID;
        public string Type;
        public string Path;

        public void Load()
        {
            ID = TextureLoader.LoadFromPath(Path);
        }
    }
}
