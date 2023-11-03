using System.Diagnostics;

namespace one_hot_app_instance
{
    internal static class Program
    {
#if PREVENT_NEW_INSTANCES
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

#else
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
#endif
    }
}