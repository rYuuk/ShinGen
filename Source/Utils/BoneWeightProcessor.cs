using System.Diagnostics;
using AssimpMesh = Silk.NET.Assimp.Mesh;

namespace OpenGLEngine
{
    public class BoneWeightProcessor
    {
        public Dictionary<string, BoneInfo> BoneInfoMap { get; }
        public int BoneCounter { get; private set; }

        public BoneWeightProcessor()
        {
            BoneInfoMap = new Dictionary<string, BoneInfo>();
        }

        public unsafe BoneWeight[]? ProcessBoneWeight(int vertexCount, AssimpMesh* mesh)
        {
            if (mesh->MNumBones == 0)
            {
                return default;
            }

            var boneWeights = new SortedList<int, BoneWeight>();

            for (var i = 0; i < vertexCount; i++)
            {
                boneWeights.Add(i, new BoneWeight());
            }

            for (var i = 0; i < mesh->MNumBones; i++)
            {
                int boneID;
                var bone = mesh->MBones[i];

                string boneName = bone->MName;

                if (BoneInfoMap.ContainsKey(boneName))
                {
                    boneID = BoneInfoMap[boneName].ID;
                }
                else
                {
                    var newBoneInfo = new BoneInfo()
                    {
                        ID = BoneCounter,
                        Offset = bone->MOffsetMatrix
                    };

                    BoneInfoMap.Add(boneName, newBoneInfo);
                    boneID = BoneCounter;
                    BoneCounter++;
                }
                Debug.Assert(boneID != -1);

                var weights = bone->MWeights;
                var weightsCount = bone->MNumWeights;

                for (var j = 0; j < weightsCount; j++)
                {
                    var vertexId = (int) weights[j].MVertexId;
                    var weight = weights[j].MWeight;
                    SetVertexBoneData(boneWeights[vertexId], boneID, weight);
                }
            }

            return boneWeights.Values.ToArray();
        }

        private void SetVertexBoneData(BoneWeight boneWeight, int boneID, float weight)
        {
            for (var i = 0; i < BoneWeight.MAX_BONE_INFLUENCE; i++)
            {
                if (weight < 0)
                    continue;

                if (boneWeight.BoneIndex[i] >= 0)
                    continue;

                boneWeight.Weight[i] = weight;
                boneWeight.BoneIndex[i] = boneID;
                return;
            }
        }
    }
}
