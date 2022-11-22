﻿using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace OpenGLEngine
{
    // A helper class, much like Shader, meant to simplify loading textures.
    public static class TextureLoader
    {
        public static int LoadFromPath(string path)
        {
            var rendererID = GL.GenTexture();

            // Bind the handle
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, rendererID);

            // OpenGL has it's texture origin in the lower left corner instead of the top left corner,
            // so StbImageSharp will to flip the image when loading.
            StbImage.stbi_set_flip_vertically_on_load(1);

            // Open a stream to the file and pass it to StbImageSharp to load.
            using (Stream stream = File.OpenRead(path))
            {
                ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
            }

            // Liner mode means that OpenGL will try to blend pixels, meaning that textures scaled too far will look blurred.
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);

            // Set the wrapping mode. S is for the X axis, and T is for the Y axis.
            // This is set to Repeat which will repeat the textures when wrapped.
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat);

            // Generate mipmaps.
            // OpenGL will automatically switch between mipmaps when an object gets sufficiently far away.
            // This prevents moiré effects, as well as saving on texture bandwidth.
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return rendererID;
        }

        // Activate texture
        public static void ActivateSlot(int slot)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + slot);
        }
        
        // Multiple textures can be bound, if shader needs more than just one.
        public static void LoadSlot(int slot, int rendererID)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + slot);
            GL.BindTexture(TextureTarget.Texture2D, rendererID);
        }

        public static void Dispose(int rendererID)
        {
            GL.DeleteTexture(rendererID);
        }
    }
}