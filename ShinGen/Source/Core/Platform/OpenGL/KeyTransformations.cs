using System.Numerics;

namespace ShinGen.Core.OpenGL
{
    internal struct KeyPosition
    {
        public double TimeStamp;
        public Vector3 Position;
    }
    internal struct KeyRotation
    {
        public double TimeStamp;
        public Quaternion Rotation;
    }
    internal struct KeyScale
    {
        public double TimeStamp;
        public Vector3 Scale;
    }
}
