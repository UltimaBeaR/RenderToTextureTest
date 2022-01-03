using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OpenTK.Graphics.ES20;
using Android.Graphics;

namespace RenderToTextureTest.Rendering
{
    public class Texture
    {
        public int Id { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public static Texture createEmpty(int width, int height,
            TextureWrapMode wrapMode = TextureWrapMode.ClampToEdge, TextureMinFilter minFilter = TextureMinFilter.Linear, TextureMagFilter magFilter = TextureMagFilter.Linear)
        {
            Texture res = new Texture() { Id = GL.GenTexture() };

            GL.BindTexture(TextureTarget.Texture2D, res.Id);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)minFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)magFilter);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)wrapMode);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)wrapMode);

            res.Width = width;
            res.Height = height;

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, OpenTK.Graphics.ES20.PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);

            GL.GenerateMipmap(TextureTarget.Texture2D);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            return res;
        }

        public static Texture createFromResource(Android.Content.Res.Resources resources, int resourceId,
            TextureWrapMode wrapMode = TextureWrapMode.ClampToEdge, TextureMinFilter minFilter = TextureMinFilter.Linear, TextureMagFilter magFilter = TextureMagFilter.Linear)
        {
            Texture res = new Texture() { Id = GL.GenTexture() };

            GL.BindTexture(TextureTarget.Texture2D, res.Id);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)minFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)magFilter);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)wrapMode);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)wrapMode);

            Bitmap b = BitmapFactory.DecodeResource(resources, resourceId, new BitmapFactory.Options() { InScaled = false });

            res.Width = b.Width;
            res.Height = b.Height;

            Android.Opengl.GLUtils.TexImage2D((int)All.Texture2D, 0, b, 0);
            b.Recycle();

            GL.GenerateMipmap(TextureTarget.Texture2D);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            return res;
        }
    }
}