using Silk.NET.Assimp;

namespace ShinGen
{
    public static class AssimpTextureTypeShaderMap
    {
        public static readonly Dictionary<TextureType, ShaderTextures> GLTFMap = new Dictionary<TextureType, ShaderTextures>
        {
            { TextureType.BaseColor, ShaderTextures.albedoMap },
            { TextureType.Normals, ShaderTextures.normalMap },
            { TextureType.Unknown, ShaderTextures.metallicRoughnessMap },
            { TextureType.Emissive, ShaderTextures.emissiveMap }
        };
    }
}
