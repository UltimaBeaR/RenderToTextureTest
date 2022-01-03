using OpenTK;

namespace RenderToTextureTest.Rendering
{
    /// <summary>
    /// Описывает положение двумерной точки в мировом пространстве (Соответсвует 3d матрице, описывающей translation, rotation, scale)
    /// </summary>
    public class Transform2D
    {
        /// <summary>
        /// Позиция точки в пространстве
        /// </summary>
        public Vector2 Position
        {
            get { return _position; }

            set
            {
                if (_position != value)
                {
                    _position = value;
                    UpdateMatrix();
                }
            }
        }

        /// <summary>
        /// Угол поворота точки (в радианах) против часовой стрелки
        /// ToDo: сделать проворот в диапазоне 0 - 2PI при установке значения
        /// </summary>
        public float Angle
        {
            get { return _angle; }

            set
            {
                float newVal = Wrap(value, 0, MathHelper.TwoPi);

                if (_angle != newVal)
                {
                    _angle = newVal;
                    UpdateMatrix();
                }
            }
        }

        /// <summary>
        /// OpengGL матрица, соответсвующая состоянию данного transform-а
        /// </summary>
        public Matrix4 Matrix { get { return _matrix; } }

        public Transform2D()
        {
            UpdateMatrix();
        }

        private void UpdateMatrix()
        {
            Matrix4 rotationMatrix;
            Matrix4.CreateRotationZ(_angle, out rotationMatrix);

            Matrix4 translationMatrix;
            Matrix4.CreateTranslation(_position.X, _position.Y, 0, out translationMatrix);

            _matrix = rotationMatrix * translationMatrix;
        }

        private float Wrap(float val, float min, float max)
        {
            float normalized = ((val - min) / (max - min)) % 1.0f;

            if (normalized < 0)
                normalized = 1.0f + normalized;

            return min + (normalized * (max - min));
        }

        // значения для публичных свойств

        private Vector2 _position;
        private float _angle;

        // кэш матрицы, которая соответсвует публичному состоянию
        private Matrix4 _matrix;
    }
}