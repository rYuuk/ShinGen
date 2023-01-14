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
            currentTime += deltaTime * 250;
            if (currentTime >= animationLoader.Duration)
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

            var globalTransformation = nodeTransform * parentTransform;
            var boneInfoMap = animationLoader.BoneInfoMap;

            if (boneInfoMap.ContainsKey(nodeName))
            {
                var index = boneInfoMap[nodeName].ID;
                var offset = boneInfoMap[nodeName].Offset;
                FinalBoneMatrices[index] = offset * globalTransformation;
            }

            for (var i = 0; i < animationNode.Children.Count; i++)
                CalculateBoneTransform(animationNode.Children[i], globalTransformation);
        }
    }
}
