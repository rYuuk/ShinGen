using System.Numerics;

namespace OpenGLEngine
{
    public struct BoneAnimationNodeData
    {
        public readonly List<BoneAnimationNodeData> Children;

        public string Name = default;
        public Matrix4x4 Transformation = default;
        public uint ChildrenCount = default;

        public BoneAnimationNodeData()
        {
            Children = new List<BoneAnimationNodeData>();
        }
    }
}
