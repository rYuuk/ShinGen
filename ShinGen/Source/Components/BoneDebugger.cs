using System.Numerics;
using Silk.NET.Input;

namespace ShinGen
{
    public class BoneDebugger : IComponent, IInput
    {
        public AnimatedModel animatedModel;

        public Transform Transform { get; set; }


        public void OnMouseMove(IMouse mouse, Vector2 position)
        {
        }

        public void OnKeyUp(IKeyboard arg1, Key arg2, int arg3)
        {
            if (AnimatedModel.DEBUG_BONES && arg2 == Key.B)
            {
                animatedModel.DebugBoneIndex++;
                if (animatedModel.DebugBoneIndex > 66)
                {
                    animatedModel.DebugBoneIndex = 0;
                }
            }
        }

    }
}
