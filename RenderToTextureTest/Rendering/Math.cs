using System;
using OpenTK;

namespace RenderToTextureTest.Rendering
{
    static class Vector2Extensions
    {
        public static float GetAngle(ref Vector2 vector) => (float)Math.Atan2(vector.Y, vector.X);
        public static Vector2 FromAngle(float angle, float length) => new Vector2((float)Math.Cos(angle) * length, (float)Math.Sin(angle) * length);
    }
}