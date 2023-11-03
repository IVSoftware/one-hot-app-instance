using System.Runtime.InteropServices;

namespace one_hot_app_instance
{
    public partial class MainForm : Form
    {
        SemaphoreSlim sslimNotMe = new SemaphoreSlim(0, 1);
        public MainForm() => InitializeComponent();
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            Text = "Creation Time " +  DateTime.Now.ToString(@"hh\:mm\:ss tt");
            if (!PostMessage((IntPtr)HWND_BROADCAST, WM_SWAP_FOR_NEW_INSTANCE, IntPtr.Zero, IntPtr.Zero))
            {
                MessageBox.Show("Failed to post message");
            }
        }
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_SWAP_FOR_NEW_INSTANCE)
            {
                try
                {
                    if (sslimNotMe.Wait(0))
                    {
                        Close();
                    }
                }
                finally
                {
                    sslimNotMe.Release();
                }
            }
            base.WndProc(ref m);
        }

        public static int WM_SWAP_FOR_NEW_INSTANCE { get; } = RegisterWindowMessage("{FC0A54D8-F999-493A-B8D1-DF4E8FCF08E4}");

        #region Dll Imports
        private const int HWND_BROADCAST = 0xFFFF;
        [DllImport("user32")]
        private static extern int RegisterWindowMessage(string message);

        [DllImport("user32")]
        private static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

        #endregion Dll Imports
    }
}