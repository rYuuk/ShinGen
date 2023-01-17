using System.Numerics;

namespace OpenGLEngine
{
    public struct BoneAnimationNodeData
    {
        public string Name;
        public Matrix4x4 Transformation;
        public List<BoneAnimationNodeData> Children;
    }
}
