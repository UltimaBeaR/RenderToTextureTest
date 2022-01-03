using System;
using OpenTK.Graphics.ES20;

namespace RenderToTextureTest.Rendering
{
    public class QuadModel
    {
        public QuadModel(float minX, float minY, float maxX, float maxY)
        {
            // vertexPosX, vertexPosY, vertexPosZ, texCoordU, texCoordV
            float[] vertexBufferData = {
                    minX, minY, 0, 0, 1,
                    maxX, minY, 0, 1, 1,
                    maxX, maxY, 0, 1, 0,
                    minX, maxY, 0, 0, 0,
                };

            ushort[] indexBufferData = {
                    0, 1, 2,
                    2, 3, 0,
                };

            GL.GenBuffers(1, out _vertexBufferId);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferId);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertexBufferData.Length * sizeof(float)), vertexBufferData, BufferUsage.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.GenBuffers(1, out _indexBufferId);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBufferId);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indexBufferData.Length * sizeof(ushort)), indexBufferData, BufferUsage.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        public void Draw()
        {
            const int vertexPositionIdx = 0;
            const int texCoordIdx = 1;

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferId);

            GL.VertexAttribPointer(vertexPositionIdx, 2, VertexAttribPointerType.Float, false, sizeof(float) * 5, IntPtr.Zero);
            GL.EnableVertexAttribArray(vertexPositionIdx);

            GL.VertexAttribPointer(texCoordIdx, 2, VertexAttribPointerType.Float, false, sizeof(float) * 5, new IntPtr(sizeof(float) * 3));
            GL.EnableVertexAttribArray(texCoordIdx);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBufferId);

            GL.DrawElements(BeginMode.Triangles, 6, DrawElementsType.UnsignedShort, IntPtr.Zero);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        private int _vertexBufferId;
        private int _indexBufferId;
    }
}