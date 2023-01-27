using System.Diagnostics;
using Silk.NET.Assimp;

namespace ShinGen
{
    public class AnimationLoader
    {
        public double Duration { get; }
        public double TicksPerSecond { get; }
        public Dictionary<string, BoneInfo> BoneInfoMap { get; private set; }

        public BoneAnimationNodeData RootAnimationNode { get; }

        private readonly List<Bone> bones;

        public AnimationLoader(string path,IDictionary<string, BoneInfo> modelBoneInfoDict, int boneCount)
        {
            var assimp = Assimp.GetApi();
            bones = new List<Bone>();

            unsafe
            {
                var scene = assimp.ImportFile(path, (uint) PostProcessSteps.Triangulate);
                Debug.Assert(scene != null && scene->MRootNode != null);
                var assimpAnimation = scene->MAnimations[0];
                Duration = assimpAnimation->MDuration;
                TicksPerSecond = assimpAnimation->MTicksPerSecond;
                RootAnimationNode = ReadHierarchyData(scene->MRootNode);
                ReadMissingBones(assimpAnimation, modelBoneInfoDict, boneCount);
            }
        }

        public Bone? FindBone(string name) => bones.FirstOrDefault(x => x.Name == name);

        private unsafe BoneAnimationNodeData ReadHierarchyData(Node* node)
        {
            Debug.Assert(node != null);

            var name = node->MName.ToString();

            if (name.Contains("mixamorig"))
            {
                name = name.Substring("mixamorig_".Length);
            }

            var boneAnimationNodeData = new BoneAnimationNodeData()
            {
                Name = name,
                Transformation = node->MTransformation,
                Children = new List<BoneAnimationNodeData>()
            };

            for (var i = 0; i < node->MNumChildren; i++)
            {
                boneAnimationNodeData.Children.Add(ReadHierarchyData(node->MChildren[i]));
            }

            return boneAnimationNodeData;
        }

        private unsafe void ReadMissingBones(Animation* assimpAnimation, IDictionary<string, BoneInfo> modelBoneInfoDict, int boneCount)
        {
            var size = assimpAnimation->MNumChannels;
            var boneInfoDict = new Dictionary<string, BoneInfo>(modelBoneInfoDict);

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
                bones.Add(new Bone(
                        boneName,
                        boneInfoDict[boneName].ID,
                        GetPositionKeys(channel, channel->MNumPositionKeys),
                        GetRotationKeys(channel, channel->MNumRotationKeys),
                        GetScaleKeys(channel, channel->MNumScalingKeys)
                    )
                );
            }
            BoneInfoMap = boneInfoDict;
        }

        private unsafe KeyPosition[] GetPositionKeys(NodeAnim* nodeAnim, uint positionsCount)
        {
            var positions = new KeyPosition[positionsCount];
            for (var i = 0; i < positionsCount; i++)
            {
                positions[i] = new KeyPosition
                {
                    TimeStamp = nodeAnim->MPositionKeys[i].MTime,
                    Position = nodeAnim->MPositionKeys[i].MValue
                };
            }

            return positions;
        }

        private unsafe KeyRotation[] GetRotationKeys(NodeAnim* nodeAnim, uint rotationsCount)
        {
            var rotations = new KeyRotation[rotationsCount];
            for (var i = 0; i < rotationsCount; i++)
            {
                rotations[i] = new KeyRotation()
                {
                    TimeStamp = nodeAnim->MRotationKeys[i].MTime,
                    Rotation = nodeAnim->MRotationKeys[i].MValue
                };
            }
            return rotations;
        }

        private unsafe KeyScale[] GetScaleKeys(NodeAnim* nodeAnim, uint scaleCount)
        {
            var scales = new KeyScale[scaleCount];
            for (var i = 0; i < scaleCount; i++)
            {
                scales[i] = new KeyScale
                {
                    TimeStamp = nodeAnim->MScalingKeys[i].MTime,
                    Scale = nodeAnim->MScalingKeys[i].MValue
                };
            }
            return scales;
        }
    }
}
