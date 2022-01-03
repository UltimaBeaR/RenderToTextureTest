using OpenTK;

namespace RenderToTextureTest.Rendering
{
    public class Camera
    {
        public Camera()
        {
            Transform = new Transform2D();
            Settings = new CameraSettings();
        }

        // Положение камеры
        public Transform2D Transform { get; private set; }

        // Настройки (проекция) камеры
        public CameraSettings Settings { get; private set; }

        /// <summary>
        /// Устанавливает текущее состояние камеры на основе двух переданных точек - в viewport и world варианте.
        /// Ставит такие настройки, чтобы камера так смотрела на заданные world точки, чтобы получались заданные viewport точки
        /// При этом используется текущее разрешение камеры, то есть viewport координаты должны быть заданы в текущем разрешении (Если надо получить
        /// в другом разрешении, то надо сначала установить текущее разрешение другое и потом уже вызывать этот метод)
        /// </summary>
        public void SetFromTwoPoints(Vector2 pointOneViewport, Vector2 pointTwoViewport, Vector2 pointOneWorld, Vector2 pointTwoWorld)
        {
            // Центр экрана
            Vector2 pointCenterViewport = new Vector2(Settings.Width / 2.0f, Settings.Height / 2.0f);

            // Получаем дистанцию между первой и второй точкой во вьюпортовой мере длины
            var oneToTwoDistanceViewport = (pointTwoViewport - pointOneViewport).Length;

            // Получаем вектор от первой до второй точки во вьюпорте
            Vector2 oneToTwoVectorViewport = pointTwoViewport - pointOneViewport;
            // Аналогично для мирового варианта
            Vector2 oneToTwoVectorWorld = pointTwoWorld - pointOneWorld;
            // Получаем вектор от первой точки до центра экрана во вьюпорте
            Vector2 oneToCenterVectorViewport = pointCenterViewport - pointOneViewport;

            // Теперь надо получить центр экрана в мировых координатах

            // Сначала получим центр экрана в независимых от координатной системы еденицах. Это можно сделать
            // Получив угол между вектором one-two и one-center и длину one-center как процент от one-two (скейл)

            float angleBetweenTwoAndCenter = Vector2Extensions.GetAngle(ref oneToTwoVectorViewport) - Vector2Extensions.GetAngle(ref oneToCenterVectorViewport);
            float scaleTwoCenter = oneToTwoDistanceViewport / oneToCenterVectorViewport.Length;

            // Теперь, имея описание точки(центра экрана), зависящее только от вектора от one к two, но не зависящее от коорд. системы, в которой определен этот вектор, можно сделать
            // обратное преобразование, но уже не во viewport координатную систему, а в world, таким образом получаем world соответствие центра экрана

            // Берем мировой one к two вектор, поворачиваем его на найденный угол и скейлим на найденный скейл, получив таким образом world вариант вектора one к center

            Vector2 oneToCenterVectorWorld = Vector2Extensions.FromAngle(
                Vector2Extensions.GetAngle(ref oneToTwoVectorWorld) + angleBetweenTwoAndCenter,
                oneToTwoVectorWorld.Length / scaleTwoCenter);

            // Прибавляем полученный мировой one->center вектор к мировому one и получаем мировую center. Это и есть мировой центр нашей камеры
            Vector2 pointCenterWorld = pointOneWorld + oneToCenterVectorWorld;

            Transform.Position = pointCenterWorld;

            Transform.Angle = Vector2Extensions.GetAngle(ref oneToTwoVectorWorld) + Vector2Extensions.GetAngle(ref oneToTwoVectorViewport);

            Settings.Zoom = oneToTwoVectorViewport.Length / oneToTwoVectorWorld.Length;
        }

        /// <summary>
        /// Для заданного objectTransform получает MVP(model * view * projection) матрицу
        /// Эту матрицу можно использовать для отображения объекта через шейдер с учетом состояния этой камеры
        /// </summary>
        public void CalculateMVPMatrixForObject(Transform2D objectTransform, out Matrix4 mvpMatrix)
        {
            Matrix4 viewProjectionMatrix;
            GetViewProjectionMatrix(out viewProjectionMatrix);

            // Конструируем mvp матрицу = model * view * projection
            mvpMatrix = objectTransform.Matrix * viewProjectionMatrix;
        }

        /// <summary>
        /// Получает матрицу вида * матрицу проекции из текущего состояния камеры
        /// </summary>
        /// <param name="viewProjectionMatrix"></param>
        public void GetViewProjectionMatrix(out Matrix4 viewProjectionMatrix)
        {
            float cameraZOffset = 10.0f;

            // Строим view матрицу на основе позиции камеры, которая находится в _transform (позиция камеры находится по z на той же плоскости
            // что и остальные объекты, поэтому мы дополнительно отодвигаем камеру по Z - чтобы объект попадал в кадр
            var viewMatrix = Matrix4.Invert(Matrix4.CreateTranslation(0, 0, cameraZOffset) * Transform.Matrix);

            viewProjectionMatrix = viewMatrix * Settings.Matrix;
        }

        /// <summary>
        /// Преобразует точку из мирового пространства во вьюпортовское (по сути экранное)
        /// </summary>
        public void CalculatePointWorldToViewport(float worldX, float worldY, out float viewportX, out float viewportY)
        {
            // ToDo: проверить правильность. во viewportY может быть косячок

            Matrix4 viewProjectionMatrix;
            GetViewProjectionMatrix(out viewProjectionMatrix);

            Vector3 worldPoint = new Vector3(worldX, worldY, 0);
            Vector3 result;
            Vector3.Transform(ref worldPoint, ref viewProjectionMatrix, out result);

            viewportX = ((result.X + 1.0f) / 2.0f) * Settings.Width;
            viewportY = ((1.0f - result.Y) / 2.0f) * Settings.Height;
        }

        /// <summary>
        /// Преобразует точку из вьюпортовского (по сути экранного) пространства в мировое
        /// </summary>
        public void CalculatePointViewportToWorld(float viewportX, float viewportY, out float worldX, out float worldY)
        {
            Matrix4 viewProjectionMatrixInverted;
            GetViewProjectionMatrix(out viewProjectionMatrixInverted);
            viewProjectionMatrixInverted = Matrix4.Invert(viewProjectionMatrixInverted);

            Vector3 viewportPoint = new Vector3(
                (2.0f * (viewportX / Settings.Width)) - 1.0f,
                (-2.0f * (viewportY / Settings.Height)) + 1.0f,
                0);

            Vector3 result;
            Vector3.Transform(ref viewportPoint, ref viewProjectionMatrixInverted, out result);

            worldX = result.X;
            worldY = result.Y;
        }
    }
}