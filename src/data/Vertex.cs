using System.Numerics;

namespace OpenGLEngine
{
    public struct Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TexCoords;

        public static int GetSize()
        {
            unsafe
            {
                return 2 * sizeof(Vector3) + sizeof(Vector2);
            }
        }
    }
}
