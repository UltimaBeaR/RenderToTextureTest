using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;

namespace RenderToTextureTest
{
    [Activity(Label = "RenderToTextureTest", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            RequestWindowFeature(WindowFeatures.NoTitle);

            SetContentView(Resource.Layout.Main);
            _paintingView = FindViewById<PaintingView>(Resource.Id.paintingview);
        }

        public override bool DispatchKeyEvent(KeyEvent e)
        {
            return _paintingView.OnKeyDown(e.KeyCode, e);
        }

        PaintingView _paintingView;
    }
}

