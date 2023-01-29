namespace ShinGen.Core
{
    internal enum ShaderTextures
    {
        albedoMap,
        normalMap,
        metallicRoughnessMap,
        emissiveMap,
    }

    internal struct Texture
    {
        public uint ID;
        public ShaderTextures Type;
        public string Name;
    }
}
