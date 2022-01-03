using OpenTK;

namespace RenderToTextureTest.Rendering
{
    /// <summary>
    /// ��������� ������
    /// </summary>
    public class CameraSettings
    {
        /// <summary>
        /// ���������� �� ��� �������� �� ������ ����� ������
        /// ��� ����� ����� ��� ������� � ��������, ��� ��� opengl �������� ����� ������ �� ������������
        /// </summary>
        public bool IsFlippedY
        {
            get { return _isFlippedY; }

            set
            {
                if (_isFlippedY != value)
                {
                    _isFlippedY = value;
                    UpdateMatrix();
                }
            }
        }

        /// <summary>
        /// ����������� ����������. �������� 1.0 ����� ������� ��� 1 ������� ���������� � ���� ����� ����� 1 ������� �� ������.
        /// ��� ������������ 2.0, 1 ������� ���������� ����� ����� 0.5 �������� � ��� �����.
        /// �������� ����� ���� �� 0 (�������� ��� 0) � ����.
        /// </summary>
        public float Zoom
        {
            get { return _zoom; }

            set
            {
                const float minZoom = 0.0001f;

                if (value < minZoom)
                    value = minZoom;

                if (_zoom != value)
                {
                    _zoom = value;
                    UpdateMatrix();
                }
            }
        }

        /// <summary>
        /// ������ ������� ��������� ������ � ��������
        /// </summary>
        public int Width
        {
            get { return _width; }

            set
            {
                const int minWidth = 1;

                if (value < minWidth)
                    value = minWidth;

                if (_width != value)
                {
                    _width = value;
                    UpdateMatrix();
                }
            }
        }

        /// <summary>
        /// ������ ������� ��������� ������ � ��������
        /// </summary>
        public int Height
        {
            get { return _height; }

            set
            {
                const int minHeight = 1;

                if (value < minHeight)
                    value = minHeight;

                if (_height != value)
                {
                    _height = value;
                    UpdateMatrix();
                }
            }
        }

        /// <summary>
        /// OpengGL ������� - ��� projection matrix � OpenGL
        /// </summary>
        public Matrix4 Matrix { get { return _matrix; } }

        public CameraSettings()
        {
            _zoom = 1.0f;
            _width = 1;
            _height = 1;
            UpdateMatrix();
        }

        private void UpdateMatrix()
        {
            Matrix4.CreateOrthographic(_width / _zoom, _height / _zoom, 1.0f, 1000.0f, out _matrix);
            if (_isFlippedY)
                _matrix *= _flipYMatrix;
        }

        // �������� ��� ��������� �������

        private bool _isFlippedY;
        private float _zoom;
        private int _width;
        private int _height;

        // ��� �������, ������� ������������ ���������� ���������
        private Matrix4 _matrix;

        private static Matrix4 _flipYMatrix = Matrix4.Scale(1f, -1f, 1f);
    }
}