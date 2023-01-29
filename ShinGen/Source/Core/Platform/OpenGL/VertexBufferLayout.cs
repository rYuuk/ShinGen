using Silk.NET.OpenGL;

namespace ShinGen.Core.OpenGL
{
    internal enum ElementType
    {
        Float,
        Int
    }

    internal struct VertexBufferElement
    {
        public int Index;
        public VertexAttribPointerType Type;
        public int Count;
        public int Stride;
        public bool Normalized;
        public int Offset;

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

    internal class VertexBufferLayout
    {
        public List<VertexBufferElement> Elements { get; }

        public VertexBufferLayout()
        {
            Elements = new List<VertexBufferElement>();
        }

        public void Push(int index, int count, int offset, ElementType type = ElementType.Float, bool normalized = false)
        {
            var glEnumType = type == ElementType.Float ? VertexAttribPointerType.Float : VertexAttribPointerType.Int;
            Elements.Add(new VertexBufferElement
            {
                Index = index,
                Type = glEnumType,
                Count = count,
                Normalized = normalized,
                Offset = offset,
                Stride = count * VertexBufferElement.GetSizeOfType(glEnumType)
            });
        }
    }
}
