using System.Numerics;

namespace OpenGLEngine
{
    public struct KeyPosition
    {
        public double TimeStamp;
        public Vector3 Position;
    }
    public struct KeyRotation
    {
        public double TimeStamp;
        public Quaternion Rotation;
    }
    public struct KeyScale
    {
        public double TimeStamp;
        public Vector3 Scale;
    }
}
