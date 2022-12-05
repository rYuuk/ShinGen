namespace OpenGLEngine
{
    public struct Texture
    {
        public int ID;
        public string Type;
        public string Path;

        public void Load()
        {
            ID = TextureLoader.LoadFromPath(Path);
        }
    }
}
