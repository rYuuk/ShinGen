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
}
