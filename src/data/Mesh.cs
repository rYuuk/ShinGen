﻿using System.Numerics;

namespace OpenGLEngine
{
    public readonly struct Mesh
    {
        public readonly string Name;
        public readonly Vector3[] Vertices;
        public readonly Vector3[] Normals;
        public readonly Vector2[] TexCoords;
        public readonly uint[] Indices;
        public readonly BoneWeight[] BoneWeights;
        public readonly Texture[] Textures;
        public readonly bool UseNormalMap;
        public readonly bool HaveBones; 

        public Mesh(string name, Vector3[] vertices, Vector3[] normals, Vector2[] texCoords, BoneWeight[]? boneWeights, uint[] indices,
            Texture[] textures)
        {
            Vertices = vertices;
            Normals = normals;
            TexCoords = texCoords;
            Indices = indices;
            Textures = textures;
            BoneWeights = boneWeights ?? Array.Empty<BoneWeight>();
            HaveBones = boneWeights != null;
            Name = name;
            UseNormalMap = Textures.Any(x => x.Type == "normalMap");
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
