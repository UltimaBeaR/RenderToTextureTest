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

        // ��������� ������
        public Transform2D Transform { get; private set; }

        // ��������� (��������) ������
        public CameraSettings Settings { get; private set; }

        /// <summary>
        /// ������������� ������� ��������� ������ �� ������ ���� ���������� ����� - � viewport � world ��������.
        /// ������ ����� ���������, ����� ������ ��� �������� �� �������� world �����, ����� ���������� �������� viewport �����
        /// ��� ���� ������������ ������� ���������� ������, �� ���� viewport ���������� ������ ���� ������ � ������� ���������� (���� ���� ��������
        /// � ������ ����������, �� ���� ������� ���������� ������� ���������� ������ � ����� ��� �������� ���� �����)
        /// </summary>
        public void SetFromTwoPoints(Vector2 pointOneViewport, Vector2 pointTwoViewport, Vector2 pointOneWorld, Vector2 pointTwoWorld)
        {
            // ����� ������
            Vector2 pointCenterViewport = new Vector2(Settings.Width / 2.0f, Settings.Height / 2.0f);

            // �������� ��������� ����� ������ � ������ ������ �� ����������� ���� �����
            var oneToTwoDistanceViewport = (pointTwoViewport - pointOneViewport).Length;

            // �������� ������ �� ������ �� ������ ����� �� ��������
            Vector2 oneToTwoVectorViewport = pointTwoViewport - pointOneViewport;
            // ���������� ��� �������� ��������
            Vector2 oneToTwoVectorWorld = pointTwoWorld - pointOneWorld;
            // �������� ������ �� ������ ����� �� ������ ������ �� ��������
            Vector2 oneToCenterVectorViewport = pointCenterViewport - pointOneViewport;

            // ������ ���� �������� ����� ������ � ������� �����������

            // ������� ������� ����� ������ � ����������� �� ������������ ������� ��������. ��� ����� �������
            // ������� ���� ����� �������� one-two � one-center � ����� one-center ��� ������� �� one-two (�����)

            float angleBetweenTwoAndCenter = Vector2Extensions.GetAngle(ref oneToTwoVectorViewport) - Vector2Extensions.GetAngle(ref oneToCenterVectorViewport);
            float scaleTwoCenter = oneToTwoDistanceViewport / oneToCenterVectorViewport.Length;

            // ������, ���� �������� �����(������ ������), ��������� ������ �� ������� �� one � two, �� �� ��������� �� �����. �������, � ������� ��������� ���� ������, ����� �������
            // �������� ��������������, �� ��� �� �� viewport ������������ �������, � � world, ����� ������� �������� world ������������ ������ ������

            // ����� ������� one � two ������, ������������ ��� �� ��������� ���� � ������� �� ��������� �����, ������� ����� ������� world ������� ������� one � center

            Vector2 oneToCenterVectorWorld = Vector2Extensions.FromAngle(
                Vector2Extensions.GetAngle(ref oneToTwoVectorWorld) + angleBetweenTwoAndCenter,
                oneToTwoVectorWorld.Length / scaleTwoCenter);

            // ���������� ���������� ������� one->center ������ � �������� one � �������� ������� center. ��� � ���� ������� ����� ����� ������
            Vector2 pointCenterWorld = pointOneWorld + oneToCenterVectorWorld;

            Transform.Position = pointCenterWorld;

            Transform.Angle = Vector2Extensions.GetAngle(ref oneToTwoVectorWorld) + Vector2Extensions.GetAngle(ref oneToTwoVectorViewport);

            Settings.Zoom = oneToTwoVectorViewport.Length / oneToTwoVectorWorld.Length;
        }

        /// <summary>
        /// ��� ��������� objectTransform �������� MVP(model * view * projection) �������
        /// ��� ������� ����� ������������ ��� ����������� ������� ����� ������ � ������ ��������� ���� ������
        /// </summary>
        public void CalculateMVPMatrixForObject(Transform2D objectTransform, out Matrix4 mvpMatrix)
        {
            Matrix4 viewProjectionMatrix;
            GetViewProjectionMatrix(out viewProjectionMatrix);

            // ������������ mvp ������� = model * view * projection
            mvpMatrix = objectTransform.Matrix * viewProjectionMatrix;
        }

        /// <summary>
        /// �������� ������� ���� * ������� �������� �� �������� ��������� ������
        /// </summary>
        /// <param name="viewProjectionMatrix"></param>
        public void GetViewProjectionMatrix(out Matrix4 viewProjectionMatrix)
        {
            float cameraZOffset = 10.0f;

            // ������ view ������� �� ������ ������� ������, ������� ��������� � _transform (������� ������ ��������� �� z �� ��� �� ���������
            // ��� � ��������� �������, ������� �� ������������� ���������� ������ �� Z - ����� ������ ������� � ����
            var viewMatrix = Matrix4.Invert(Matrix4.CreateTranslation(0, 0, cameraZOffset) * Transform.Matrix);

            viewProjectionMatrix = viewMatrix * Settings.Matrix;
        }

        /// <summary>
        /// ����������� ����� �� �������� ������������ �� ������������� (�� ���� ��������)
        /// </summary>
        public void CalculatePointWorldToViewport(float worldX, float worldY, out float viewportX, out float viewportY)
        {
            // ToDo: ��������� ������������. �� viewportY ����� ���� �������

            Matrix4 viewProjectionMatrix;
            GetViewProjectionMatrix(out viewProjectionMatrix);

            Vector3 worldPoint = new Vector3(worldX, worldY, 0);
            Vector3 result;
            Vector3.Transform(ref worldPoint, ref viewProjectionMatrix, out result);

            viewportX = ((result.X + 1.0f) / 2.0f) * Settings.Width;
            viewportY = ((1.0f - result.Y) / 2.0f) * Settings.Height;
        }

        /// <summary>
        /// ����������� ����� �� �������������� (�� ���� ���������) ������������ � �������
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