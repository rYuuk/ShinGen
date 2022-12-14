using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using StbImageSharp;

namespace OpenGLEngine
{
    // A helper class, meant to simplify loading textures.
    public static class TextureLoader
    {
        private static GL gl = null!;

        public static void SetRenderer(GL glRender) =>
            gl = glRender;

        public static uint LoadFromPath(string path)
        {
            var handle = gl.GenTexture();
            gl.BindTexture(TextureTarget.Texture2D, handle);

            unsafe
            {
                using var img = Image.Load<Rgba32>(path);
                gl.TexImage2D(
                    TextureTarget.Texture2D,
                    0,
                    InternalFormat.Rgba8,
                    (uint) img.Width,
                    (uint) img.Height,
                    0,
                    PixelFormat.Rgba,
                    PixelType.UnsignedByte,
                    null);

                img.ProcessPixelRows(accessor =>
                {
                    for (var y = 0; y < accessor.Height; y++)
                    {
                        fixed (void* data = accessor.GetRowSpan(y))
                        {
                            gl.TexSubImage2D(TextureTarget.Texture2D, 0, 0, y, (uint) accessor.Width, 1, PixelFormat.Rgba, PixelType.UnsignedByte,
                                data);
                        }
                    }
                });
            }

            SetParameters();
            return handle;
        }

        public static uint LoadFromBytes(byte[] bytes)
        {
            var handle = gl.GenTexture();
            gl.BindTexture(TextureTarget.Texture2D, handle);

            StbImage.stbi_set_flip_vertically_on_load(1);
            
            unsafe
            {
                using var img = Image.Load<Rgba32>(bytes);
                gl.TexImage2D(
                    TextureTarget.Texture2D,
                    0,
                    InternalFormat.Rgba,
                    (uint) img.Width,
                    (uint) img.Height,
                    0,
                    PixelFormat.Rgba,
                    PixelType.UnsignedByte,
                    null);

                img.ProcessPixelRows(accessor =>
                {
                    for (var y = 0; y < accessor.Height; y++)
                    {
                        fixed (void* data = accessor.GetRowSpan(y))
                        {
                            gl.TexSubImage2D(TextureTarget.Texture2D, 0, 0, y, (uint) accessor.Width, 1, PixelFormat.Rgba, PixelType.UnsignedByte,
                                data);
                        }
                    }
                });
            }

            SetParameters();
            return handle;
        }

        public static uint LoadCubemapFromPaths(string[] facePaths)
        {
            var handle = gl.GenTexture();
            gl.BindTexture(TextureTarget.TextureCubeMap, handle);

            unsafe
            {
                for (var i = 0; i < facePaths.Length; i++)
                {
                    using var img = Image.Load<Rgba32>(facePaths[i]);
                    gl.TexImage2D(
                        TextureTarget.TextureCubeMapPositiveX + i,
                        0,
                        InternalFormat.Rgba8,
                        (uint) img.Width,
                        (uint) img.Height,
                        0,
                        PixelFormat.Rgba,
                        PixelType.UnsignedByte,
                        null);

                    img.ProcessPixelRows(accessor =>
                    {
                        for (var y = 0; y < accessor.Height; y++)
                        {
                            fixed (void* data = accessor.GetRowSpan(y))
                            {
                                gl.TexSubImage2D(TextureTarget.TextureCubeMapPositiveX + i,
                                    0, 0, y,
                                    (uint) accessor.Width,
                                    1,
                                    PixelFormat.Rgba,
                                    PixelType.UnsignedByte,
                                    data);
                            }
                        }
                    });
                }
            }

            gl.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear);
            gl.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);
            gl.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int) TextureWrapMode.ClampToEdge);
            gl.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int) TextureWrapMode.ClampToEdge);
            gl.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int) TextureWrapMode.ClampToEdge);

            return handle;
        }

        // Multiple textures can be bound, if shader needs more than just one.
        public static void LoadSlot(uint handle, int slot)
        {
            gl.ActiveTexture(TextureUnit.Texture0 + slot);
            gl.BindTexture(TextureTarget.Texture2D, handle);
        }

        public static void Dispose(uint handle) =>
            gl.DeleteTexture(handle);

        private static void SetParameters()
        {
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.LinearMipmapLinear);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMinFilter.Linear);
            gl.GenerateMipmap(TextureTarget.Texture2D);
        }
    }
}
