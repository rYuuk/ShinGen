using System.Numerics;

namespace ShinGen.Core
{
    internal struct BoneAnimationNodeData
    {
        public string Name;
        public Matrix4x4 Transformation;
        public List<BoneAnimationNodeData> Children;
    }
}
