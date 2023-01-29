namespace ShinGen.Core
{
    public class AnimatedModel : Model
    {
        /// If <c cref="ModelImporter"/> has PostProcessSteps.PreTransformVertices
        /// while importing model,the animations are removed.
        public const bool ENABLE_ANIMATION = false;
        public const bool DEBUG_BONES = false;
        
        private readonly Animator animator;

        public int DebugBoneIndex;

        public AnimatedModel(string path) : base(path)
        {
            if (ENABLE_ANIMATION)
            {
                var animation = new AnimationLoader("resources/Avatar/SingleMesh/Walking.dae", BoneInfoDict, BoneCounter);
                animator = new Animator(animation);
            }
        }

        public void Animate(double time)
        {
            if (ENABLE_ANIMATION)
            {
                animator.UpdateAnimation(time);
            }

            Shader.Bind();
            Shader.SetInt("displayBones", DEBUG_BONES ? 1 : 0);
            if (DEBUG_BONES)
            {
                Shader.SetInt("displayBoneIndex", DebugBoneIndex);
            }
            
            Shader.SetInt("enableAnimation", ENABLE_ANIMATION ? 1 : 0);
            if (ENABLE_ANIMATION)
            {
                var transforms = animator.FinalBoneMatrices;
                for (var i = 0; i < transforms.Count; i++)
                {
                    Shader.SetMatrix4("finalBonesMatrices[" + i + "].matrix", transforms[i]);
                }
            }
        }
    }
}
