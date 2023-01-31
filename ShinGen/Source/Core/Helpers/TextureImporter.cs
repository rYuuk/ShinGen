using Silk.NET.Assimp;
using AssimpTexture = Silk.NET.Assimp.Texture;

namespace ShinGen.Core
{
    internal class TextureImporter
    {
        private readonly Assimp assimp;
        private readonly Dictionary<TextureType, ShaderTextures> shaderTexturesMap;
        private readonly List<Texture> texturesLoaded;

        public TextureImporter(Assimp assimp)
        {
            this.assimp = assimp;
            shaderTexturesMap = AssimpTextureTypeShaderMap.GLTFMap;
            texturesLoaded = new List<Texture>();
        }

        public unsafe IEnumerable<Texture> ImportTextures(AssimpTexture** assimpTextures, string meshName, Material* material)
        {
            var textures = new List<Texture>();

            foreach (var shaderTexture in shaderTexturesMap)
            {
                textures.AddRange(LoadMaterialTextures(meshName, assimpTextures, material, shaderTexture.Key, shaderTexture.Value));
            }

            return textures;
        }

        private unsafe IEnumerable<Texture> LoadMaterialTextures(
            string meshName,
            AssimpTexture** assimpTextures,
            Material* assimpMaterial,
            TextureType assimpTextureType,
            ShaderTextures shaderTexture)
        {
            var textureCount = assimp.GetMaterialTextureCount(assimpMaterial, assimpTextureType);
            var textures = new List<Texture>();
            for (uint i = 0; i < textureCount; i++)
            {
                AssimpString path;
                var success = assimp.GetMaterialTexture(assimpMaterial, assimpTextureType, i, &path, null, null, null, null, null, null);
                if (success != Return.Success)
                {
                    throw new Exception($"Loading texture of Assimp TextureType {assimpTextureType} and of {shaderTexture} failed");
                }
                var skip = false;
                var index = int.Parse(path.ToString()[1..]);
                var assimpTexture = assimpTextures[index];
                var textureName = meshName + "_" + shaderTexture + "_" + assimpTexture->MFilename;

                foreach (var tex in texturesLoaded.Where(tex => tex.Name == textureName))
                {
                    textures.Add(tex);
                    skip = true;
                    break;
                }

                if (skip) continue;
                var texture = new Texture
                {
                    Name = textureName,
                    Type = shaderTexture,
                    TextureData = (byte*) assimpTexture->PcData,
                    Width = assimpTexture->MWidth,
                    Height = assimpTexture->MHeight
                };
                
                texturesLoaded.Add(texture);
                textures.Add(texture);
            }
            return textures;
        }
    }
}
