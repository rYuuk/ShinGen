using System.Numerics;

namespace OpenGLEngine
{
    public class Animator
    {
        private readonly AnimationLoader animationLoader;

        public List<Matrix4x4> FinalBoneMatrices { get; }
        private double currentTime;

        public Animator(AnimationLoader animationLoader)
        {
            currentTime = 0.0f;
            this.animationLoader = animationLoader;

            FinalBoneMatrices = new List<Matrix4x4>();
            for (var i = 0; i < 200; i++)
                FinalBoneMatrices.Add(Matrix4x4.Identity);
        }

        public void UpdateAnimation(double deltaTime)
        {
            currentTime +=  deltaTime;
            if (currentTime >= 1)
            {
                currentTime = 0;
            }
            CalculateBoneTransform(animationLoader.RootAnimationNode, Matrix4x4.Identity);
        }

        public void CalculateBoneTransform(BoneAnimationNodeData animationNode, Matrix4x4 parentTransform)
        {
            var nodeName = animationNode.Name;
            var nodeTransform = animationNode.Transformation;
            var bone = animationLoader.FindBone(nodeName);

            if (bone != null)
            {
                nodeTransform = bone.Update(currentTime);
            }

            var globalTransformation = parentTransform * nodeTransform;

            var boneInfoMap = animationLoader.BoneInfoDict;
            if (boneInfoMap.ContainsKey(nodeName))
            {
                var index = boneInfoMap[nodeName].ID;
                var offset = boneInfoMap[nodeName].Offset;
                FinalBoneMatrices[index] = globalTransformation * offset;
            }

            for (var i = 0; i < animationNode.ChildrenCount; i++)
                CalculateBoneTransform(animationNode.Children[i], globalTransformation);
        }
    }
}
