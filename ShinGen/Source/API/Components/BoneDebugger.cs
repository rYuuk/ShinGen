using System.Numerics;
using ShinGen.Core;
using Silk.NET.Input;

namespace ShinGen
{
    // TODO Fix this component
    public class BoneDebugger : IComponent, IKeyboardInput
    {
        private AnimatedModel animatedModel = null!;

        public Transform Transform { get; set; } = null!;


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
