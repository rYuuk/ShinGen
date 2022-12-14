namespace OpenGLEngine
{
    public struct Texture
    {
        public uint ID;
        public string Type;
        public string Path;
        public byte[] Bytes;
        
        public void Load()
        {
            ID = TextureLoader.LoadFromBytes(Bytes);
        }
    }
}
