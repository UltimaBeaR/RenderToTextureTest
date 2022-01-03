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
using OpenTK.Graphics;

namespace RenderToTextureTest.Rendering
{
    /// <summary>
    /// Запекающая камера - камера, которая может запекать отрисованную сцену в свою внутреннюю текстуру
    /// </summary>
    class BakingCamera : IDisposable
    {
        /// <summary>
        /// Текстура в которую идет запекание. Можно использовать ее в отрисовках, если в этот момент не идет запекание в нее
        /// </summary>
        public Texture Texture { get; private set; }

        /// <summary>
        /// Камера, через которую идет запекание (печется то что снимает эта камера)
        /// </summary>
        public Camera Camera { get; }

        /// <summary>
        /// Начало запекания. После этого вызова, все вызовы отрисовки будут идти в этот объект
        /// </summary>
        public void BeginBaking(bool clearContents, Color4 clearColor)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _framebufferId);
            GL.Viewport(0, 0, Texture.Width, Texture.Height);

            if (clearContents)
            {
                GL.ClearColor(clearColor);
                GL.Clear(ClearBufferMask.ColorBufferBit);
            }
        }

        /// <summary>
        /// Конец запекания. После этого вызова, вызовы отрисовки будут идти туда, куда шли до вызова <see cref="BeginBaking"/>
        /// </summary>
        public void EndBaking()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            // Биндим текстуру в которую шла отрисовка перед тем как сгенерить ей мипмапы
            GL.BindTexture(TextureTarget.Texture2D, Texture.Id);
            GL.GenerateMipmap(TextureTarget.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                GL.DeleteFramebuffers(1, ref _framebufferId);
                GL.DeleteTexture(Texture.Id);

                _disposed = true;
            }
        }

        public BakingCamera(int textureWidth, int textureHeight)
        {
            // Создаем фреймбуффер и текстуру

            GL.GenFramebuffers(1, out _framebufferId);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _framebufferId);

            Texture = Texture.createEmpty(textureWidth, textureHeight, TextureWrapMode.ClampToEdge, TextureMinFilter.Linear, TextureMagFilter.Nearest);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferSlot.ColorAttachment0, TextureTarget.Texture2D, Texture.Id, 0);

            var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);

            if (status != FramebufferErrorCode.FramebufferComplete)
                throw new Exception("frame buffer is not complete");

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            // Создаем камеру
            Camera = new Camera();
            Camera.Settings.IsFlippedY = true;
            Camera.Settings.Width = textureWidth;
            Camera.Settings.Height = textureHeight;
        }

        ~BakingCamera()
        {
            Dispose();
        }

        /// <summary>
        /// Буффер с кадром, для реализации техники render to texture
        /// </summary>
        private int _framebufferId;
        private bool _disposed = false;
    }
}