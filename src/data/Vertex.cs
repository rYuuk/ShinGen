using  System.Numerics;

namespace OpenGLEngine
{
    public struct Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TexCoords;
        public Vector4 Joints;
        public Vector4 Weights;

        public static int GetSize()
        {
            unsafe
            {
                return sizeof(Vertex);
            }
        }
    }
}
