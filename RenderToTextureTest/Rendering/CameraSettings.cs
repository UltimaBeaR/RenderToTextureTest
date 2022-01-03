using OpenTK;

namespace RenderToTextureTest.Rendering
{
    /// <summary>
    /// Настройки камеры
    /// </summary>
    public class CameraSettings
    {
        /// <summary>
        /// Повертнута ли вся картинка на камере вверх ногами
        /// Эта опция нужна при рендере в текстуру, так как opengl рендерит вверх ногами во фреймбуфферы
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
        /// Коэффициент увеличения. Значение 1.0 будет значить что 1 еденица расстояния в мире будет равна 1 пикселю на экране.
        /// При коэффициенте 2.0, 1 еденица расстояния будет равна 0.5 пикселям и так далее.
        /// Значение может быть от 0 (исключая сам 0) и выше.
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
        /// Ширина области просмотра камеры в пикселах
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
        /// Высота области просмотра камеры в пикселах
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
        /// OpengGL матрица - это projection matrix в OpenGL
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

        // значения для публичных свойств

        private bool _isFlippedY;
        private float _zoom;
        private int _width;
        private int _height;

        // кэш матрицы, которая соответсвует публичному состоянию
        private Matrix4 _matrix;

        private static Matrix4 _flipYMatrix = Matrix4.Scale(1f, -1f, 1f);
    }
}