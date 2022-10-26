using OpenTK.Graphics.OpenGL4;

namespace OpenGLEngine
{
    public struct VertexBufferElement
    {
        public int Index;
        public VertexAttribPointerType Type;
        public int Count;
        public bool Normalized;

        public static int GetSizeOfType(VertexAttribPointerType type)
        {
            return type switch
            {
                VertexAttribPointerType.Float => sizeof(float),
                VertexAttribPointerType.Int => sizeof(int),
                VertexAttribPointerType.Byte => sizeof(byte),
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

        public void Push(int index, int count)
        {
            Elements.Add(new VertexBufferElement()
            {
                Index = index,
                Type = VertexAttribPointerType.Float,
                Count = count,
                Normalized = false
            });
            Stride += count * VertexBufferElement.GetSizeOfType(VertexAttribPointerType.Float);
        }
    }
}
