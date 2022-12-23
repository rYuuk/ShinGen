using Silk.NET.OpenGL;

namespace OpenGLEngine
{
    public struct VertexBufferElement
    {
        public int Index;
        public GLEnum Type;
        public int Count;
        public int Stride;
        public bool Normalized;
        public int Offset;

        public static int GetSizeOfType(GLEnum type)
        {
            return type switch
            {
                GLEnum.Float => sizeof(float),
                GLEnum.Int => sizeof(int),
                GLEnum.Byte => sizeof(byte),
                _ => 0
            };
        }
    }

    public class VertexBufferLayout
    {
        public List<VertexBufferElement> Elements { get; }

        public int Stride { get; private set; }

        public VertexBufferLayout()
        {
            Elements = new List<VertexBufferElement>();
        }

        public void Push(int index, int count, int offset,  GLEnum type = GLEnum.Float, bool normalized = false)
        {
            Elements.Add(new VertexBufferElement
            {
                Index = index,
                Type = type,
                Count = count,
                Normalized = normalized,
                Offset =  offset,
                Stride = count * VertexBufferElement.GetSizeOfType(type)
            });
        }
    }
}
