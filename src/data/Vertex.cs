using OpenTK.Mathematics;

namespace OpenGLEngine
{
    public struct Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TexCoords;

        public static int GetSize() => (2 * Vector3.SizeInBytes) + Vector2.SizeInBytes;
    }
}
