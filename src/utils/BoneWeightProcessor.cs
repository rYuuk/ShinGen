using System.Collections;
using System.Diagnostics;
using System.Numerics;
using AssimpMesh = Silk.NET.Assimp.Mesh;

namespace OpenGLEngine
{
    public struct BoneInfo
    {
        public int ID;
        public Matrix4x4 Offset;

    }

    public class BoneWeightProcessor
    {
        public Dictionary<string, BoneInfo> BoneInfoDict { get; }
        public int BoneCounter { get; private set; }

        public BoneWeightProcessor()
        {
            BoneInfoDict = new Dictionary<string, BoneInfo>();
        }

        // public void SetVertexBoneDataToDefault(BoneWeight boneWeight)
        // {
        //     for (var i = 0; i < Vertex.MAX_BONE_INFLUENCE; i++)
        //     {
        //         boneWeight.BoneIndex[i] = -1;
        //         boneWeight.Weight[i] = 0.0f;
        //     }
        // }

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

                if (BoneInfoDict.ContainsKey(boneName))
                {
                    boneID = BoneInfoDict[boneName].ID;
                }
                else
                {
                    var newBoneInfo = new BoneInfo()
                    {
                        ID = BoneCounter,
                        Offset = bone->MOffsetMatrix
                    };

                    BoneInfoDict.Add(boneName, newBoneInfo);
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
            
            foreach (var ibw in boneWeights)
            {
                // Console.WriteLine(ibw.Key + ",  " +
                //                   ibw.Value.BoneIndex[0] + "," +
                //                   ibw.Value.BoneIndex[1] + "," +
                //                   ibw.Value.BoneIndex[2] + "," +
                //                   ibw.Value.BoneIndex[3] + ", " +
                //                   mesh->MBones[ibw.Value.BoneIndex[0]]->MName +  ", " +
                //                   ibw.Value.Weight[0] + "," +
                //                   ibw.Value.Weight[1] + "," +
                //                   ibw.Value.Weight[2] + "," +
                //                   ibw.Value.Weight[3]);
            }

            return boneWeights.Values.ToArray();
        }

        private void SetVertexBoneData(BoneWeight boneWeight, int boneID, float weight)
        {
            for (var i = 0; i < Vertex.MAX_BONE_INFLUENCE; i++)
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
