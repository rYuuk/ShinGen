using System.Numerics;

namespace OpenGLEngine
{
    public struct BoneWeight
    {
        public const int MAX_BONE_INFLUENCE = 4;
        public readonly int[] BoneIndex;
        public readonly float[] Weight;

        public BoneWeight()
        {
            BoneIndex = new[] { -1, -1, -1, -1 };
            Weight = new[] { 0.0f, 0.0f, 0.0f, 0.0f };
        }
    }

    public struct Mesh
    {
        public readonly string Name;
        public readonly Vector3[] Vertices;
        public readonly Vector3[] Normals;
        public readonly Vector2[] TexCoords;
        public readonly uint[] Indices;
        public readonly BoneWeight[]? BoneWeights;
        public readonly Texture[] Textures;
        public readonly bool UseNormalMap;

        public Mesh(string name, Vector3[] vertices, Vector3[] normals, Vector2[] texCoords, BoneWeight[]? boneWeights, uint[] indices,
            Texture[] textures)
        {
            Vertices = vertices;
            Normals = normals;
            TexCoords = texCoords;
            Indices = indices;
            Textures = textures;
            BoneWeights = boneWeights;
            Name = name;
            UseNormalMap = Textures.Any(x => x.Type == "normalMap");
        }

        public unsafe int SizeOfVertices => sizeof(Vector3) * Vertices.Length;
        public unsafe int SizeOfNormals => sizeof(Vector3) * Normals.Length;
        public unsafe int SizeOfTexCoords => sizeof(Vector2) * TexCoords.Length;
    }
}
