using System.Diagnostics;
using System.Numerics;
using AssimpMesh = Silk.NET.Assimp.Mesh;

namespace OpenGLEngine
{
    public class BoneWeightProcessor
    {
        public struct BoneInfo
        {
            public int ID;
            public Matrix4x4 Offset;

        };

        private const int MAX_BONE_INFLUENCE = 4;

        private readonly Dictionary<string, BoneInfo> boneInfoDict;
        private int boneCounter;

        public BoneWeightProcessor()
        {
            boneInfoDict = new Dictionary<string, BoneInfo>();
        }
        
        public unsafe void SetVertexBoneDataToDefault(Vertex vertex)
        {
            for (var i = 0; i < MAX_BONE_INFLUENCE; i++)
            {
                vertex.BoneIds[i] = -1;
                vertex.Weights[i] = 0.0f;
            }
        }

        public unsafe void ProcessBoneWeight(IReadOnlyList<Vertex> vertices, AssimpMesh* mesh)
        {
            for (var i = 0; i < mesh->MNumBones; i++)
            {
                int boneID;
                var bone = mesh->MBones[i];
                string boneName = bone->MName;

                if (boneInfoDict.ContainsKey(boneName))
                {
                    boneID = boneInfoDict[boneName].ID;
                }
                else
                {
                    var newBoneInfo = new BoneInfo()
                    {
                        ID = boneCounter,
                        Offset = bone->MOffsetMatrix
                    };

                    boneInfoDict.Add(boneName, newBoneInfo);
                    boneID = boneCounter;
                    boneCounter++;
                }
                Debug.Assert(boneID != -1);

                var weights = bone->MWeights;
                var weightsCount = bone->MNumWeights;

                for (var j = 0; j < weightsCount; j++)
                {
                    var vertexId = (int) weights[j].MVertexId;
                    var weight = weights[j].MWeight;
                    Debug.Assert(vertexId <= vertices.Count);
                    SetVertexBoneData(vertices[vertexId], boneID, weight);
                }
            }
        }

        private unsafe void SetVertexBoneData(Vertex vertex, int boneID, float weight)
        {
            for (var i = 0; i < MAX_BONE_INFLUENCE; ++i)
            {
                if (vertex.BoneIds[i] >= 0) continue;
                vertex.Weights[i] = weight;
                vertex.BoneIds[i] = boneID;
                break;
            }
        }
    }
}
