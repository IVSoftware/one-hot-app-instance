 To satisfy the first clause of your question **Stop a WinForms app from running a second time**, one conventional approach that detects a reentrant instance of the app is by setting a [Mutex](https://learn.microsoft.com/en-us/dotnet/api/system.threading.mutex?view=net-7.0). 

Subsequent attempts to invoke the static entry point `static void Main()` will be unable to enter the mutex block. If the goal is to prevent the new instance from running, take this opportunity to leave the running instance alone and prevent new one from running, per my original answer.

To satisfy the second clause of the question **if it does, close the first instance**, we can take a different approach. using the _original no mutex_ version of `Program.cs`,  make the the main form class broadcast a WM_USER message after taking care to innoculate itself from the effects. Other window instances of MainForm will now close themselves.

```
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
```

___

Previous answer, using `Mutex` to avoid a new instance (per the first part of the question).

```
internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        if(Mutex.WaitOne(TimeSpan.Zero, true))
        {
            try
            {
                ApplicationConfiguration.Initialize();
                Application.Run(new MainForm());
            }
            finally
            {
                Mutex.ReleaseMutex();
            }
        }
        else
        {
            MessageBox.Show(Form.ActiveForm, "The app is already running!");
        }
    }
    static Mutex Mutex = new Mutex(
        true,
        // Use Tools\Create Guid to generate an arbitrary identifier.
        "{FD4DF30D-C22A-499C-A814-EA2A21BDE585}"
    );
}
```