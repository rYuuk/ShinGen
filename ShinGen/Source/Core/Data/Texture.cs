namespace ShinGen.Core
{
    public enum ShaderTextures
    {
        albedoMap,
        normalMap,
        metallicRoughnessMap,
        emissiveMap,
    }

    public struct Texture
    {
        public uint ID;
        public ShaderTextures Type;
        public string Name;
    }
}
