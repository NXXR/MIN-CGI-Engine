﻿using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;

namespace cgimin.engine.texture
{
    public class TextureManager
    {

        // Methode zum laden einer Textur
        public static int LoadTexture(string fullAssetPath)
        {
            // Textur wird generiert
            int returnTextureID = GL.GenTexture();

            // Textur wird "gebunden", folgende Befehle beziehen sich auf die gesetzte Textur (Statemachine)
            GL.BindTexture(TextureTarget.Texture2D, returnTextureID);

            Bitmap bmp = new Bitmap(fullAssetPath);
            int width = bmp.Width;
            int height = bmp.Height;

            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // Textur-Parameter, Pixelformat etc.
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmpData.Width, bmpData.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Nearest);

            bmp.UnlockBits(bmpData);

            // Mip-Map Daten werden generiert
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            // Textur-ID wird zurückgegeben
            return returnTextureID;
        }





    }
}