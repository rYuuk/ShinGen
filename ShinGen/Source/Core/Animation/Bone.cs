using System.Diagnostics;
using System.Numerics;
using ShinGen.Core.OpenGL;

namespace ShinGen.Core
{
    internal class Bone
    {
        public string Name { get; }
        public int ID { get; }

        private readonly KeyPosition[] positions;
        private readonly KeyRotation[] rotations;
        private readonly KeyScale[] scales;

        public Bone(string name, int id, KeyPosition[] positions, KeyRotation[] rotations, KeyScale[] scales)
        {
            Name = name;
            ID = id;

            this.positions = positions;
            this.rotations = rotations;
            this.scales = scales;
        }

        public Matrix4x4 Update(double animationTime)
        {
            var translation = InterpolatePosition(animationTime);
            var rotation = InterpolateRotation(animationTime);
            var scale = InterpolateScale(animationTime);
            return scale * rotation * translation;
        }

        private int GetPositionIndex(double animationTime)
        {
            for (var i = 0; i < positions.Length - 1; i++)
            {
                if (animationTime < positions[i + 1].TimeStamp)
                    return i;
            }
            Debug.Fail("Index not found");
            return -1;
        }

        private int GetRotationIndex(double animationTime)
        {
            for (var i = 0; i < rotations.Length - 1; i++)
            {
                if (animationTime < rotations[i + 1].TimeStamp)
                    return i;
            }
            Debug.Fail("Index not found");
            return -1;
        }

        private int GetScaleIndex(double animationTime)
        {
            for (var i = 0; i < scales.Length - 1; i++)
            {
                if (animationTime < scales[i + 1].TimeStamp)
                    return i;
            }
            Debug.Fail("Index not found");
            return -1;
        }

        private double GetScaleFactor(double lastTimeStamp, double nextTimeStamp, double animationTime)
        {
            var midWayLength = animationTime - lastTimeStamp;
            var framesDiff = nextTimeStamp - lastTimeStamp;
            return midWayLength / framesDiff;
        }

        private Matrix4x4 InterpolatePosition(double animationTime)
        {
            if (positions.Length == 1)
                return Matrix4x4.CreateTranslation(positions[0].Position);

            var p0Index = GetPositionIndex(animationTime);
            var p1Index = p0Index + 1;
            var scaleFactor = GetScaleFactor(positions[p0Index].TimeStamp, positions[p1Index].TimeStamp, animationTime);

            var finalPosition = Vector3.Lerp(positions[p0Index].Position, positions[p1Index].Position, (float) scaleFactor);
            return Matrix4x4.CreateTranslation(finalPosition);
        }

        private Matrix4x4 InterpolateRotation(double animationTime)
        {
            if (rotations.Length == 1)
            {
                return Matrix4x4.CreateFromQuaternion(rotations[0].Rotation);
            }

            var p0Index = GetRotationIndex(animationTime);
            var p1Index = p0Index + 1;
            var scaleFactor = GetScaleFactor(rotations[p0Index].TimeStamp,
                rotations[p1Index].TimeStamp, animationTime);
            var finalRotation = Quaternion.Slerp(rotations[p0Index].Rotation, rotations[p1Index].Rotation, (float) scaleFactor);
            finalRotation = Quaternion.Normalize(finalRotation);
            return Matrix4x4.CreateFromQuaternion(finalRotation);
        }

        private Matrix4x4 InterpolateScale(double animationTime)
        {
            if (scales.Length == 1)
                return Matrix4x4.CreateScale(scales[0].Scale);

            var p0Index = GetScaleIndex(animationTime);
            var p1Index = p0Index + 1;
            var scaleFactor = GetScaleFactor(scales[p0Index].TimeStamp,
                scales[p1Index].TimeStamp, animationTime);
            var finalScale = Vector3.Lerp(scales[p0Index].Scale, scales[p1Index].Scale, (float) scaleFactor);
            return Matrix4x4.CreateScale(finalScale);
        }
    }
}
