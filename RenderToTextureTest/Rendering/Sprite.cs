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
using OpenTK;

namespace RenderToTextureTest.Rendering
{
    class Sprite
    {
        public Texture Texture { get; set; }
        public QuadModel Model { get; set; }

        public Sprite(Texture texture, int mvpProgram, int mvpProgram_u_texture, int mvpProgram_u_mvp)
        {
            _mvpProgram = mvpProgram;
            _mvpProgram_u_texture = mvpProgram_u_texture;
            _mvpProgram_u_mvp = mvpProgram_u_mvp;
            Texture = texture;
            Model = new QuadModel(-(Texture.Width / 2), -(Texture.Height / 2), Texture.Width / 2, Texture.Height / 2);
        }

        public void Draw(Camera camera, Vector2 worldPosition, float angle = 0)
        {
            Transform2D modelTransform = new Transform2D();
            modelTransform.Position = worldPosition;
            modelTransform.Angle = angle;

            GL.UseProgram(_mvpProgram);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, Texture.Id);
            GL.Uniform1(_mvpProgram_u_texture, 0);

            Matrix4 mvpMatrix;
            camera.CalculateMVPMatrixForObject(modelTransform, out mvpMatrix);

            GL.UniformMatrix4(_mvpProgram_u_mvp, false, ref mvpMatrix);

            Model.Draw();
        }

        private int _mvpProgram_u_mvp;
        private int _mvpProgram_u_texture;
        private int _mvpProgram;
    }
}