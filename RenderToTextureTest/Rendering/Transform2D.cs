using OpenTK;

namespace RenderToTextureTest.Rendering
{
    /// <summary>
    /// ��������� ��������� ��������� ����� � ������� ������������ (������������ 3d �������, ����������� translation, rotation, scale)
    /// </summary>
    public class Transform2D
    {
        /// <summary>
        /// ������� ����� � ������������
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
        /// ���� �������� ����� (� ��������) ������ ������� �������
        /// ToDo: ������� �������� � ��������� 0 - 2PI ��� ��������� ��������
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
        /// OpengGL �������, �������������� ��������� ������� transform-�
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

        // �������� ��� ��������� �������

        private Vector2 _position;
        private float _angle;

        // ��� �������, ������� ������������ ���������� ���������
        private Matrix4 _matrix;
    }
}