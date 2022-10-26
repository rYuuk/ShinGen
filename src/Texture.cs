using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace OpenGLEngine
{
    public class Texture
    {
        private readonly int handle;

        public Texture(string path)
        {
            handle = GL.GenTexture();
            
            Use(TextureUnit.Texture0);
            
            // stb_image loads from the top-left pixel, whereas OpenGL loads from the bottom-left,
            // causing the texture to be flipped vertically.
            // This will correct that, making the texture display properly.
            StbImage.stbi_set_flip_vertically_on_load(1);

            // Load the image.
            using (Stream stream = File.OpenRead(path))
            {
                ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
                
                GL.TexImage2D(TextureTarget.Texture2D, 
                    0,
                    PixelInternalFormat.Rgba,
                    image.Width, image.Height,
                    0, 
                    PixelFormat.Rgba, 
                    PixelType.UnsignedByte, 
                    image.Data);
            }

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        public void Use(TextureUnit unit)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, handle);
        }
    }
}
