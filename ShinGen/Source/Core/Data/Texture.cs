namespace ShinGen.Core
{
    public enum ShaderTextures
    {
        albedoMap,
        normalMap,
        metallicRoughnessMap,
        emissiveMap
    }

    public struct Texture
    {
        public ShaderTextures Type;
        public string Name;
        public unsafe byte* TextureData;
        public uint Height;
        public uint Width;
    }
}
