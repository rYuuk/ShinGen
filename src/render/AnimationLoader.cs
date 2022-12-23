using System.Diagnostics;
using Silk.NET.Assimp;

namespace OpenGLEngine
{
    public class AnimationLoader
    {
        public readonly List<Bone> Bones;

        public double Duration { get; }
        public double TicksPerSecond { get; }
        public Dictionary<string, BoneInfo> BoneInfoDict { get; private set; }

        public BoneAnimationNodeData RootAnimationNode;

        public AnimationLoader(string path, Model model)
        {
            Bones = new List<Bone>();
            var assimp = Assimp.GetApi();

            unsafe
            {
                var scene = assimp.ImportFile(path, (uint) PostProcessSteps.Triangulate);
                Debug.Assert(scene != null && scene->MRootNode != null);
                var assimpAnimation = scene->MAnimations[0];
                Duration = assimpAnimation->MDuration;
                TicksPerSecond = assimpAnimation->MTicksPerSecond;
                RootAnimationNode = new BoneAnimationNodeData();
                ReadHierarchyData(ref RootAnimationNode, scene->MRootNode);
                ReadMissingBones(assimpAnimation, model);
            }
        }

        public Bone? FindBone(string name)
        {
            return Bones.FirstOrDefault(x => x.Name == name);
        }

        private unsafe void ReadHierarchyData(ref BoneAnimationNodeData boneAnimationNodeData, Node* node)
        {
            Debug.Assert(node != null);

            var name = node->MName.ToString();
            if (name.Contains("mixamorig"))
            {
                name = name.Substring("mixamorig_".Length);
            }
            boneAnimationNodeData.Name = name;
            boneAnimationNodeData.Transformation = node->MTransformation;
            boneAnimationNodeData.ChildrenCount = node->MNumChildren;

            for (var i = 0; i < node->MNumChildren; i++)
            {
                var newNode = new BoneAnimationNodeData();
                ReadHierarchyData(ref newNode, node->MChildren[i]);
                boneAnimationNodeData.Children.Add(newNode);
            }
        }

        private unsafe void ReadMissingBones(Animation* assimpAnimation, Model model)
        {
            var size = assimpAnimation->MNumChannels;

            var boneInfoDict = new Dictionary<string, BoneInfo>(model.BoneInfoDict);
            var boneCount = model.BoneCounter;

            for (var i = 0; i < size; i++)
            {
                var channel = assimpAnimation->MChannels[i];
                var boneName = channel->MNodeName.ToString().Substring("mixamorig_".Length);

                if (!boneInfoDict.ContainsKey(boneName))
                {
                    boneInfoDict.Add(boneName, new BoneInfo
                        { ID = boneCount });
                    boneCount++;
                }
                Bones.Add(new Bone(
                        boneName,
                        boneInfoDict[boneName].ID,
                        GetPositionKeys(channel, channel->MNumPositionKeys),
                        GetRotationKeys(channel, channel->MNumRotationKeys),
                        GetScaleKeys(channel, channel->MNumScalingKeys)
                    )
                );
            }
            BoneInfoDict = boneInfoDict;
        }

        private double max;

        private unsafe KeyPosition[] GetPositionKeys(NodeAnim* nodeAnim, uint positionsCount)
        {
            var positions = new KeyPosition[positionsCount];
            for (var i = 0; i < positionsCount; i++)
            {
                var position = nodeAnim->MPositionKeys[i].MValue;
                var timestamp = nodeAnim->MPositionKeys[i].MTime;

                if (timestamp > max)
                    max = timestamp;
                
                positions[i] = new KeyPosition
                {
                    TimeStamp = timestamp,
                    Position = position
                };
            }

            return positions;
        }

        private unsafe KeyRotation[] GetRotationKeys(NodeAnim* nodeAnim, uint rotationsCount)
        {
            var rotations = new KeyRotation[rotationsCount];
            for (var i = 0; i < rotationsCount; i++)
            {
                var rotation = nodeAnim->MRotationKeys[i].MValue;
                var timestamp = nodeAnim->MRotationKeys[i].MTime;

                if (timestamp > max)
                    max = timestamp;
                
                rotations[i] = new KeyRotation()
                {
                    TimeStamp = timestamp,
                    Rotation = rotation
                };
            }

            return rotations;
        }

        private unsafe KeyScale[] GetScaleKeys(NodeAnim* nodeAnim, uint scaleCount)
        {
            var scales = new KeyScale[scaleCount];
            for (var i = 0; i < scaleCount; i++)
            {
                var scale = nodeAnim->MScalingKeys[i].MValue;
                var timestamp = nodeAnim->MScalingKeys[i].MTime;

                if (timestamp > max)
                    max = timestamp;
                
                scales[i] = new KeyScale
                {
                    TimeStamp = timestamp,
                    Scale = scale
                };
            }

            return scales;
        }
    }

}
