using System.Numerics;
using System.Runtime.InteropServices;

namespace OpenGLEngine
{
    [StructLayout(LayoutKind.Explicit, Size = 64)]
    public struct Vertex
    {
        [FieldOffset(0)]
        public Vector3 Position;
        [FieldOffset(12)]
        public Vector3 Normal;
        [FieldOffset(24)]
        public Vector2 TexCoords;
        [FieldOffset(32)]
        public readonly unsafe int* BoneIds;
        [FieldOffset(48)]
        public readonly unsafe float* Weights;

        public unsafe Vertex()
        {
            Position = default;
            Normal = default;
            TexCoords = default;

            fixed (int* boneIds = new int[4])
            {
                BoneIds = boneIds;
            }

            fixed (float* weights = new float[4])
            {
                Weights = weights;
            }
        }
    }
}
