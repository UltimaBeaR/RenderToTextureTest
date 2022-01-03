using System;
using System.Text;
using OpenTK.Graphics.ES20;
using OpenTK.Platform.Android;

using Android.Util;
using Android.Content;
using OpenTK;
using Android.Graphics;
using Android.Views;
using Android.Runtime;
using System.Collections.Generic;

namespace RenderToTextureTest
{
    using OpenTK.Graphics;
    using Rendering;

    class PaintingView : AndroidGameView
    {
        public PaintingView(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            Init();
        }

        public PaintingView(IntPtr handle, JniHandleOwnership transfer)
            : base(handle, transfer)
        {
            Init();
        }

        private void Init()
        {
            Resize += delegate
            {
                GL.Viewport(0, 0, _viewportWidth, _viewportHeight);

                //Matrix4.CreateOrthographic((float)_viewportWidth * zoom, (float)_viewportHeight * zoom, )

                // Проекцию (учет зума и aspect-а экрана), модель (смещение квада) и камеру (поворот, смещение)
                // нужно перемножить в modelViewProjection матрицу и передавать в вершинный шейдер как uniform.
                // далее в шейдере матрица множится на вершину и получается красота. Надо только понять как получить нужную mpv из параметров которые мне нужны 
            };

            //Touch += PaintingView_Touch;
        }

        //private void PaintingView_Touch(object sender, TouchEventArgs e)
        //{
        //    const int maximumTouchCount = 2;

        //    var motionEvent = e.Event;

        //    var pointerIndex = ((int)(motionEvent.Action & MotionEventActions.PointerIdMask) >>
        //                        (int)MotionEventActions.PointerIdShift);
        //    var pointerId = motionEvent.GetPointerId(pointerIndex);
        //    var action = (motionEvent.Action & MotionEventActions.Mask);
        //    var pointCnt = motionEvent.PointerCount;

        //    if (pointCnt <= maximumTouchCount)
        //    {
        //        if (pointerIndex <= maximumTouchCount - 1)
        //        {
        //            for (var i = 0; i < pointCnt; i++)
        //            {
        //                var id = motionEvent.GetPointerId(i);
        //                //_x[id] = (int)motionEvent.GetX(i);
        //                //_y[id] = (int)motionEvent.GetY(i);
        //            }

        //            switch (action)
        //            {
        //                case MotionEventActions.Down:
        //                case MotionEventActions.PointerDown:
        //                case MotionEventActions.Move:
        //                    //_isTouch[pointerId] = true;
        //                    break;
        //                default:
        //                    //_isTouch[pointerId] = false;
        //                    break;
        //            }
        //        }
        //    }

        //    System.Diagnostics.Trace.WriteLine($"{pointerIndex} {pointerId}");

        //    e.Handled = false;
        //}

        private Dictionary<int, PointerState> _pointerState = new Dictionary<int, PointerState>();

        public class PointerState
        {
            public float x;
            public float y;
            public bool isTouch;
        }

        public override bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            if (e.KeyCode == Keycode.VolumeDown || e.KeyCode == Keycode.VolumeUp)
            {
                // do what you want with the power button
                return true;
            }

            return base.OnKeyDown(keyCode, e);
        }

        private void SetPointerXY(int id, float x, float y)
        {
            PointerState state;
            if (!_pointerState.TryGetValue(id, out state))
                _pointerState[id] = new PointerState() { isTouch = false, x = x, y = y };
            else
            {
                state.x = x;
                state.y = y;
            }

        }

        private void SetPointerIsTouch(int id, bool isTouch)
        {
            PointerState state;
            if (!_pointerState.TryGetValue(id, out state))
                _pointerState[id] = new PointerState() { isTouch = isTouch, x = 0, y = 0 };
            else
                state.isTouch = isTouch;
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            const int maximumTouchCount = 2;

            var motionEvent = e;

            var pointerIndex = ((int)(motionEvent.Action & MotionEventActions.PointerIdMask) >>
                                (int)MotionEventActions.PointerIdShift);
            var pointerId = motionEvent.GetPointerId(pointerIndex);
            var action = (motionEvent.Action & MotionEventActions.Mask);
            var pointCnt = motionEvent.PointerCount;

            if (pointCnt <= maximumTouchCount)
            {
                if (pointerIndex <= maximumTouchCount - 1)
                {
                    for (var i = 0; i < pointCnt; i++)
                    {
                        SetPointerXY(motionEvent.GetPointerId(i), motionEvent.GetX(i), motionEvent.GetY(i));
                    }

                    switch (action)
                    {
                        case MotionEventActions.Down:
                        case MotionEventActions.PointerDown:
                        case MotionEventActions.Move:
                            SetPointerIsTouch(pointerId, true);
                            break;
                        default:
                            SetPointerIsTouch(pointerId, false);
                            break;
                    }
                }
            }

            OnPointerStateChanged();

            //string str = "";
            //foreach (var dddd in _pointerState)
            //    str += $"{{{dddd.Key} [{dddd.Value.isTouch} = {dddd.Value.x}, {dddd.Value.y}]}} ";

            //System.Diagnostics.Trace.WriteLine(str);

            return true;
        }

        protected override void OnResize(EventArgs e)
        {
            _viewportHeight = Height;
            _viewportWidth = Width;

            _camera.Settings.Width = _viewportWidth;
            _camera.Settings.Height = _viewportHeight;

            // the surface change event makes your context
            // not be current, so be sure to make it current again
            MakeCurrent();
        }

        // This method is called everytime the context needs
        // to be recreated. Use it to set any egl-specific settings
        // prior to context creation
        protected override void CreateFrameBuffer()
        {
            ContextRenderingApi = OpenTK.Graphics.GLVersion.ES2;

            // the default GraphicsMode that is set consists of (16, 16, 0, 0, 2, false)
            try
            {
                Log.Verbose("GLTriangle", "Loading with default settings");

                // if you don't call this, the context won't be created
                base.CreateFrameBuffer();
                return;
            }
            catch (Exception ex)
            {
                Log.Verbose("GLTriangle", "{0}", ex);
            }

            // this is a graphics setting that sets everything to the lowest mode possible so
            // the device returns a reliable graphics setting.
            try
            {
                Log.Verbose("GLTriangle", "Loading with custom Android settings (low mode)");
                GraphicsMode = new AndroidGraphicsMode(0, 0, 0, 0, 0, false);

                // if you don't call this, the context won't be created
                base.CreateFrameBuffer();
                return;
            }
            catch (Exception ex)
            {
                Log.Verbose("GLTriangle", "{0}", ex);
            }
            throw new Exception("Can't load egl, aborting");
        }

        private Camera _camera;

        // This gets called when the drawing surface has been created
        // There is already a GraphicsContext and Surface at this point,
        // following the standard OpenTK/GameWindow logic
        //
        // Android will only render when it refreshes the surface for
        // the first time, so if you don't call Run, you need to hook
        // up the Resize delegate or override the OnResize event to
        // get the updated bounds and re-call your rendering code.
        // This will also allow non-Run-loop code to update the screen
        // when the device is rotated.
        protected override void OnLoad(EventArgs e)
        {
            // This is completely optional and only needed
            // if you've registered delegates for OnLoad
            base.OnLoad(e);

            _viewportHeight = Height; _viewportWidth = Width;

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            _camera = new Camera();

            _camera.Settings.Zoom = 1f;
            _camera.Settings.Width = _viewportWidth;
            _camera.Settings.Height = _viewportHeight;

            //_camera.Transform.X = 192;
            //_camera.Transform.Y = 192;
            //_camera.Transform.Angle = MathHelper.Pi / 4;




            _quadProgram = LoadProgram(@"
                // uniform mat4 mvp_matrix;

                attribute vec4 a_position;
                attribute vec2 a_texCoord;

                varying vec2 v_texCoord;

                void main()
                {
                    v_texCoord = a_texCoord;
                    gl_Position = a_position; //< mvp_matrix * a_position;
                }
            ", @"
                precision mediump float;

                uniform sampler2D u_texture;

                varying vec2 v_texCoord;

                void main()
                {
                  gl_FragColor = texture2D(u_texture, v_texCoord);
                }
            ");
            _quadProgram_u_texture = GL.GetUniformLocation(_quadProgram, "u_texture");

            _simpleQuadProgram = LoadProgram(@"
                attribute vec4 a_position;
                attribute vec2 a_texCoord;

                void main()
                {
                    gl_Position = a_position;
                }
            ", @"
                precision mediump float;

                void main()
                {
                  gl_FragColor = vec4(0.0, 1.0, 1.0, 1.0); //< sky blue
                }
            ");

            _mvpQuadProgram = LoadProgram(@"
                uniform mat4 u_mvp;

                attribute vec4 a_position;
                attribute vec2 a_texCoord;

                varying vec2 v_texCoord;

                void main()
                {
                    v_texCoord = a_texCoord;
                    gl_Position = u_mvp * a_position;
                }
            ", @"
                precision mediump float;

                uniform sampler2D u_texture;

                varying vec2 v_texCoord;

                void main()
                {
                  gl_FragColor = texture2D(u_texture, v_texCoord);
                }
            ");
            _mvpQuadProgram_u_mvp = GL.GetUniformLocation(_mvpQuadProgram, "u_mvp");
            _mvpQuadProgram_u_texture = GL.GetUniformLocation(_mvpQuadProgram, "u_texture");


            int brushProgram = LoadProgram(@"
                uniform mat4 u_mvp;

                attribute vec4 a_position;
                attribute vec2 a_texCoord;

                varying vec2 v_texCoord;

                void main()
                {
                    v_texCoord = a_texCoord;
                    gl_Position = u_mvp * a_position;
                }
            ", @"
                precision mediump float;

                uniform sampler2D u_texture;

                varying vec2 v_texCoord;

                void main()
                {
                  float a = v_texCoord[0] - 0.5;
                  float b = v_texCoord[1] - 0.5;

                  float c = sqrt(a * a + b * b) * 2.0;

                  gl_FragColor = vec4(0, 0, 0, c < 1.0 ? 1.0 - c : 0.0);
                }
            ");
            int brushProgram_u_mvp = GL.GetUniformLocation(_mvpQuadProgram, "u_mvp");
            int brushProgram_u_texture = GL.GetUniformLocation(_mvpQuadProgram, "u_texture");

            _brushSprite = new Sprite(Texture.createFromResource(Context.Resources, Resource.Drawable.brush), brushProgram, brushProgram_u_texture, brushProgram_u_mvp);






            _quadTexture2 = Texture.createFromResource(Context.Resources, Resource.Drawable.brush);
            _quadTexture3 = Texture.createFromResource(Context.Resources, Resource.Drawable.brush);

            _quadModel = new QuadModel(-0.5f, -0.5f, 0.5f, 0.5f);

            CreateOffscreenTexture();
            _offscreenSprite = new Sprite(_quadTexture, _mvpQuadProgram, _mvpQuadProgram_u_texture, _mvpQuadProgram_u_mvp);

            


            _bakingCamera1 = new BakingCamera(1024, 1024);
            _bakingCamera1.Camera.Transform.Position = new Vector2(-512f, 0f);
            _bakingCamera1.BeginBaking(true, Color4.Transparent);
            _bakingCamera1.EndBaking();

            _bakingCamera2 = new BakingCamera(1024, 1024);
            _bakingCamera2.Camera.Transform.Position = new Vector2(512f, 0);
            _bakingCamera2.BeginBaking(true, Color4.Transparent);
            _bakingCamera2.EndBaking();


            Run();
        }

        private void CreateOffscreenTexture()
        {
            GL.GenFramebuffers(1, out _frameBufferId);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _frameBufferId);

            _quadTexture = Texture.createFromResource(Context.Resources, Resource.Drawable.brush);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferSlot.ColorAttachment0, TextureTarget.Texture2D, _quadTexture.Id, 0);

            var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);

            if (status != FramebufferErrorCode.FramebufferComplete)
                throw new Exception("frame buffer is not complete");

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        private void DrawOffscreenTexture()
        {

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _frameBufferId);
            GL.Viewport(0, 0, _quadTexture.Width, _quadTexture.Height); //< при 64 на 64 все мелкое, при увеличении начинает съезжать вправо-вниз и увеличиваться


            // Происходит какая-то херня. на эмуляторе все норм - заполняется красным рендер текстура, а на реальном устройстве какбудто с альфаблендингом сверху слой красный сделали просто поверх
            // старой текстуры
            // Что то с порядком команд как будто, при этом при загрузке если это делать - результат тот же

            //GL.Disable(EnableCap.DepthTest);
            //GL.Disable(EnableCap.ScissorTest);
            //GL.Disable(EnableCap.StencilTest);

            //GL.Flush();
            //GL.Finish();

            GL.ClearColor(1f, 0f, 0f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit);




            GL.UseProgram(_quadProgram);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _quadTexture2.Id);
            GL.Uniform1(_quadProgram_u_texture, 0);
            _quadModel.Draw();

            //GL.UseProgram(_simpleQuadProgram);
            //_quadModel.Draw();



            //GL.Flush();
            //GL.Finish();            

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            // Биндим текстуру в которую шла отрисовка перед тем как сгенерить ей мипмапы
            GL.BindTexture(TextureTarget.Texture2D, _quadTexture.Id);
            GL.GenerateMipmap(TextureTarget.Texture2D);
        }

        private int LoadProgram(string vertexShaderSource, string fragmentShaderSource)
        {
            int vertexShader = LoadShader(ShaderType.VertexShader, vertexShaderSource);
            int fragmentShader = LoadShader(ShaderType.FragmentShader, fragmentShaderSource);
            int resId = GL.CreateProgram();
            if (resId == 0)
                throw new InvalidOperationException("Unable to create program");

            GL.AttachShader(resId, vertexShader);
            GL.AttachShader(resId, fragmentShader);

            GL.BindAttribLocation(resId, 0, "a_position");
            GL.BindAttribLocation(resId, 1, "a_texCoord");
            GL.LinkProgram(resId);

            int linked;
            GL.GetProgram(resId, ProgramParameter.LinkStatus, out linked);
            if (linked == 0)
            {
                // link failed
                int length;
                GL.GetProgram(resId, ProgramParameter.InfoLogLength, out length);
                if (length > 0)
                {
                    var log = new StringBuilder(length);
                    GL.GetProgramInfoLog(resId, length, out length, log);
                    Log.Debug("GL2", "Couldn't link program: " + log.ToString());
                }

                GL.DeleteProgram(resId);
                throw new InvalidOperationException("Unable to link program");
            }

            return resId;
        }

        private int LoadShader(ShaderType shaderType, string source)
        {
            int shader = GL.CreateShader(shaderType);
            if (shader == 0)
                throw new InvalidOperationException("Unable to create shader");

            int length = 0;
            GL.ShaderSource(shader, 1, new string[] { source }, (int[])null);
            GL.CompileShader(shader);

            int compiled;
            GL.GetShader(shader, ShaderParameter.CompileStatus, out compiled);
            if (compiled == 0)
            {
                GL.GetShader(shader, ShaderParameter.InfoLogLength, out length);
                if (length > 0)
                {
                    var log = new StringBuilder(length);
                    GL.GetShaderInfoLog(shader, length, out length, log);
                    Log.Debug("GL2", "Couldn't compile shader: " + log.ToString());
                }

                GL.DeleteShader(shader);
                throw new InvalidOperationException("Unable to compile shader of type : " + shaderType.ToString());
            }

            return shader;
        }

        //private float angle = -50.0f;

        private List<Vector2> _points = new List<Vector2>();

        private void RenderToAreas(Action<BakingCamera> rendreringAction)
        {
            _bakingCamera1.BeginBaking(false, Color4.AliceBlue);
            rendreringAction(_bakingCamera1);
            _bakingCamera1.EndBaking();

            _bakingCamera2.BeginBaking(false, Color4.AliceBlue);
            rendreringAction(_bakingCamera2);
            _bakingCamera2.EndBaking();
        }

        private void RenderAreas()
        {
            DrawBakingCamera(_bakingCamera1);
            DrawBakingCamera(_bakingCamera2);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            //DrawOffscreenTexture();



            //DrawRT();








            GL.Viewport(0, 0, _viewportWidth, _viewportHeight);

            GL.ClearColor(0.7f, 0.7f, 0.7f, 1);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            //_offscreenSprite.Draw(_camera, new Vector2(0, 0));

            RenderAreas();


            //angle += 0.1f;
            //DrawWithMatrices(0, 0, 0);
            //DrawWithMatrices(192, 192, angle);
            //DrawWithMatrices(-500, -1000, 0);






            //foreach (var pointerStateElement in _pointerState)
            //{
            //    if (pointerStateElement.Value.isTouch)
            //        DrawFinger(pointerStateElement.Key, (float)pointerStateElement.Value.x / _viewportWidth, (float)pointerStateElement.Value.y / _viewportHeight);
            //}

            SwapBuffers();
        }

        private void DrawRT()
        {
            GL.UseProgram(_quadProgram);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _quadTexture.Id);
            GL.Uniform1(_quadProgram_u_texture, 0);
            _quadModel.Draw();
        }

        private void DrawFinger(int id, float x, float y)
        {
            x = x * 2.0f - 1.0f;

            y = 1.0f - y;
            y = y * 2.0f - 1.0f;

            const float size = 0.3f;

            var model = new QuadModel(x - size, y - size, x + size, y + size);

            GL.UseProgram(_quadProgram);
            GL.ActiveTexture(TextureUnit.Texture0);


            Texture textureDesc = id == 0 ? _quadTexture2 : _quadTexture3;

            GL.BindTexture(TextureTarget.Texture2D, textureDesc.Id);


            GL.Uniform1(_quadProgram_u_texture, 0);
            model.Draw();
        }

        //int _cameraMove_fingerOneId;
        //float _cameraMove_fingerOneStartX;
        //float _cameraMove_fingerOneStartY;
        //float _cameraMove_fingerOneEndX;
        //float _cameraMove_fingerOneEndY;

        //int _cameraMove_fingerTwoId;
        //float _cameraMove_fingerTwoStartX;
        //float _cameraMove_fingerTwoStartY;
        //float _cameraMove_fingerTwoEndX;
        //float _cameraMove_fingerTwoEndY;

        int _cameraMove_fingerOneId;
        float _cameraMove_worldFingerOneX;
        float _cameraMove_worldFingerOneY;

        int _cameraMove_fingerTwoId;
        float _cameraMove_worldFingerTwoX;
        float _cameraMove_worldFingerTwoY;


        bool _cameraMove_isMoving;

        bool _noFingers;

        /// <summary>
        /// Возникает при любых изменениях с состоянием пальев
        /// </summary>
        private void OnPointerStateChanged()
        {
            // ToDo: Происходит херня с id-шниками пальцев видимо. Если приложить 3 пальца а потом как-то поснимать поставить пальцы
            // то все нахер заглючивает. (подзреваю что id-шники прописаны одни, пальца прикоснуто 2, но один из пальцев не тем id-шником является какие прописаны на one и two пальцы)

            int touchingFingersCount = 0;
            foreach (var finger in _pointerState)
            {
                if (finger.Value.isTouch)
                    touchingFingersCount++;
            }

            if (touchingFingersCount == 0)
            {
                _noFingers = true;
            }

            if (!_cameraMove_isMoving && touchingFingersCount == 1)
            {
                float viewportX = 0, viewportY = 0;
                foreach (var finger in _pointerState)
                {
                    if (finger.Value.isTouch)
                    {
                        viewportX = finger.Value.x;
                        viewportY = finger.Value.y;
                        break;
                    }
                }

                float worldX, worldY;
                _camera.CalculatePointViewportToWorld(viewportX, viewportY, out worldX, out worldY);

                if (_noFingers || _points.Count == 0)
                {
                    AddNewPoint(new Vector2(worldX, worldY));
                }
                else {

                    var oldPoint = _points[_points.Count - 1];
                    var newPoint = new Vector2(worldX, worldY);

                    var oldToNewVector = newPoint - oldPoint;

                    const float worldDistanceBetweenPoints = 8.0f;

                    var pointsCountToBeDrawn = (int)Math.Floor(oldToNewVector.Length / worldDistanceBetweenPoints);

                    oldToNewVector.Normalize();

                    List<Vector2> points = new List<Vector2>();

                    for (int i = 0; i < pointsCountToBeDrawn; i++)
                        points.Add(oldPoint + (oldToNewVector * (i * worldDistanceBetweenPoints)));

                    AddNewPoints(points);
                }

                _noFingers = false;
            }

            if (_cameraMove_isMoving)
            {
                if (touchingFingersCount < 2)
                {
                    // Заканчиваем движение (Один из пальцев убрали и было движение

                    _cameraMove_isMoving = false;
                }
                else
                {

                    var finger = _pointerState[_cameraMove_fingerOneId];
                    System.Diagnostics.Debug.Assert(finger.isTouch);
                    float viewportOneX = finger.x, viewportOneY = finger.y;

                    finger = _pointerState[_cameraMove_fingerTwoId];
                    System.Diagnostics.Debug.Assert(finger.isTouch);
                    float viewportTwoX = finger.x, viewportTwoY = finger.y;

                    _camera.SetFromTwoPoints(
                        new Vector2(viewportOneX, viewportOneY), new Vector2(viewportTwoX, viewportTwoY),
                        new Vector2(_cameraMove_worldFingerOneX, _cameraMove_worldFingerOneY), new Vector2(_cameraMove_worldFingerTwoX, _cameraMove_worldFingerTwoY));
                }
            }
            else
            {
                if (touchingFingersCount == 2)
                {
                    // Начинаем движение (Стало 2 пальца, и до этого не было движения)

                    _cameraMove_isMoving = true;

                    // Прописываем текущее состояние пальцев в начальное и запоминаем id пальцев

                    float viewportOneX = 0, viewportOneY = 0, viewportTwoX = 0, viewportTwoY = 0;

                    bool firstFinger = true;
                    foreach (var finger in _pointerState)
                    {
                        if (firstFinger)
                        {
                            _cameraMove_fingerOneId = finger.Key;
                            viewportOneX = finger.Value.x;
                            viewportOneY = finger.Value.y;
                            firstFinger = false;
                        }
                        else
                        {
                            _cameraMove_fingerTwoId = finger.Key;
                            viewportTwoX = finger.Value.x;
                            viewportTwoY = finger.Value.y;
                        }
                    }

                    // Запоминаем мировые позиции двух пальцев при начале двжиения
                    _camera.CalculatePointViewportToWorld(viewportOneX, viewportOneY, out _cameraMove_worldFingerOneX, out _cameraMove_worldFingerOneY);
                    _camera.CalculatePointViewportToWorld(viewportTwoX, viewportTwoY, out _cameraMove_worldFingerTwoX, out _cameraMove_worldFingerTwoY);
                }
            }
        }

        private void AddNewPoint(Vector2 point)
        {
            _points.Add(point);

            RenderToAreas(bakingCamera =>
            {
                _brushSprite.Draw(bakingCamera.Camera, point, _camera.Transform.Angle);
            });
        }

        private void AddNewPoints(IEnumerable<Vector2> points)
        {
            _points.AddRange(points);

            RenderToAreas(bakingCamera =>
            {
                foreach (var point in points)
                    _brushSprite.Draw(bakingCamera.Camera, point, _camera.Transform.Angle);
            });
        }

        private void DrawBakingCamera(BakingCamera bakingCamera)
        {
            Transform2D modelTransform = bakingCamera.Camera.Transform;

            GL.UseProgram(_mvpQuadProgram);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, bakingCamera.Texture.Id);
            GL.Uniform1(_mvpQuadProgram_u_texture, 0);

            if (_quadModelForMatrix_Baking == null)
                _quadModelForMatrix_Baking = new QuadModel(
                    -(bakingCamera.Camera.Settings.Width / 2),
                    -(bakingCamera.Camera.Settings.Height / 2),
                    bakingCamera.Camera.Settings.Width / 2,
                    bakingCamera.Camera.Settings.Height / 2);

            Matrix4 mvpMatrix;
            _camera.CalculateMVPMatrixForObject(modelTransform, out mvpMatrix);

            GL.UniformMatrix4(_mvpQuadProgram_u_mvp, false, ref mvpMatrix);

            _quadModelForMatrix_Baking.Draw();
        }

        Sprite _brushSprite;
        Sprite _offscreenSprite;

        BakingCamera _bakingCamera1;
        BakingCamera _bakingCamera2;

        Texture _quadTexture;
        Texture _quadTexture2;
        Texture _quadTexture3;
        QuadModel _quadModel;
        QuadModel _quadModelForMatrix_Baking;
        int _quadProgram;
        int _simpleQuadProgram;
        int _mvpQuadProgram;
        int _quadProgram_u_texture;
        int _mvpQuadProgram_u_mvp;
        int _mvpQuadProgram_u_texture;
        int _frameBufferId;

        int _viewportWidth, _viewportHeight;
    }
}
