using System.Numerics;

namespace ShinGen
{
    public readonly struct Mesh
    {
        public readonly string Name;
        public readonly Matrix4x4 Transformation;
        public readonly Vector3[] Vertices;
        public readonly Vector3[] Normals;
        public readonly Vector2[] TexCoords;
        public readonly BoneWeight[] BoneWeights;

        public readonly uint[] Indices;
        public readonly Texture[] Textures;

        public readonly bool HaveBones;
        public readonly bool UseNormalMap;
        public readonly bool UseMetallicRoughnessMap;
        public readonly bool UseEmissiveMap;

        public Mesh(string name, Vector3[] vertices, Vector3[] normals, Vector2[] texCoords, BoneWeight[]? boneWeights, uint[] indices,
            Texture[] textures, Matrix4x4 transformation)
        {
            Name = name;
            Vertices = vertices;
            Normals = normals;
            TexCoords = texCoords;
            HaveBones = boneWeights != null;
            BoneWeights = boneWeights ?? Array.Empty<BoneWeight>();
            Indices = indices;
            Textures = textures;
            Transformation = transformation;
            UseNormalMap = Textures.Any(x => x.Type == ShaderTextures.normalMap);
            UseMetallicRoughnessMap = Textures.Any(x => x.Type == ShaderTextures.metallicRoughnessMap);
            UseEmissiveMap = Textures.Any(x => x.Type == ShaderTextures.emissiveMap);
        }

        public unsafe int SizeOfVertices => sizeof(Vector3) * Vertices.Length;
        public unsafe int SizeOfNormals => sizeof(Vector3) * Normals.Length;
        public unsafe int SizeOfTexCoords => sizeof(Vector2) * TexCoords.Length;

        public int SizeOfBoneWeights =>
            sizeof(int) * sizeof(float) * 4 * BoneWeights.Length;

        public int[] FlattenedBoneIndices =>
            BoneWeights.Select(x => x.BoneIndex).SelectMany(indices => indices).ToArray();

        public float[] FlattenedBoneWeights =>
            BoneWeights.Select(x => x.Weight).SelectMany(weights => weights).ToArray();
    }
}
