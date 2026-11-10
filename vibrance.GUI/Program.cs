using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using vibrance.GUI.AMD;
using vibrance.GUI.AMD.vendor;
using vibrance.GUI.AMD.vendor.utils;
using vibrance.GUI.common;
using vibrance.GUI.NVIDIA;

namespace vibrance.GUI
{
    static class Program
    {
        private const string ErrorGraphicsAdapterUnknown = "No supported AMD or NVIDIA graphics adapter with an available driver was detected. Press Yes to open the support profile in your browser.";
        private const string ErrorGraphicsAdapterAmbiguous = "Windows reports both an AMD and an NVIDIA graphics adapter with available drivers. This version of vibranceGUI can use only one graphics provider at a time.";
        private const string MessageBoxCaption = "vibranceGUI Error";

        [STAThread]
        static void Main(string[] args)
        {
#if SINGLE_EXE
            if (args.Contains("--verify-bundle"))
            {
                VerifyBundle();
                return;
            }
#endif
            UiLanguage.Load();
            bool result = false;
            Mutex mutex = new Mutex(true, "vibranceGUI~Mutex", out result);
            if (!result)
            {
                MessageBox.Show(UiLanguage.Translate("You can run vibranceGUI only once at a time!"),
                    UiLanguage.Translate(MessageBoxCaption), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            NativeMethods.SetDllDirectory(CommonUtils.GetVibrance_GUI_AppDataPath());

            GraphicsAdapter adapter = GraphicsAdapterHelper.GetAdapter();
            Form vibranceGui = null;

            if (adapter == GraphicsAdapter.Amd)
            {
                Func<List<ApplicationSetting>, Dictionary<string, List<ResolutionModeWrapper>>, IVibranceProxy> getProxy = (x, y) => new AmdDynamicVibranceProxy(Environment.Is64BitOperatingSystem
                    ? new AmdAdapter64()
                    : (IAmdAdapter)new AmdAdapter32(), x, y);
                vibranceGui = new VibranceGUI(getProxy, 
                    100, 
                    0,
                    300,
                    100,
                    x => x);
            }
            else if (adapter == GraphicsAdapter.Nvidia)
            {
                const string nvidiaAdapterName = "vibranceDLL.dll";
                string resourceName = $"{typeof(Program).Namespace}.NVIDIA.{nvidiaAdapterName}";
                CommonUtils.LoadUnmanagedLibraryFromResource(
                    Assembly.GetExecutingAssembly(),
                    resourceName,
                    nvidiaAdapterName);
                Marshal.PrelinkAll(typeof(NvidiaDynamicVibranceProxy));

                vibranceGui = new VibranceGUI(
                    (x, y) => new NvidiaDynamicVibranceProxy(x, y),
                    NvidiaDynamicVibranceProxy.NvapiDefaultLevel,
                    NvidiaDynamicVibranceProxy.NvapiDefaultLevel,
                    NvidiaDynamicVibranceProxy.NvapiMaxLevel,
                    NvidiaDynamicVibranceProxy.NvapiDefaultLevel,
                    x => NvidiaVibranceValueWrapper.Find(x).Percentage);
            }
            else if (adapter == GraphicsAdapter.Unknown)
            {
                if (MessageBox.Show(UiLanguage.Translate(ErrorGraphicsAdapterUnknown),
                    UiLanguage.Translate(MessageBoxCaption), MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start("https://twitter.com/juvlarN");
                }
                return;
            }
            else if (adapter == GraphicsAdapter.Ambiguous)
            {
                MessageBox.Show(UiLanguage.Translate(ErrorGraphicsAdapterAmbiguous),
                    UiLanguage.Translate(MessageBoxCaption), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (args.Contains("-minimized"))
            {
                vibranceGui.WindowState = FormWindowState.Minimized;
                ((VibranceGUI)vibranceGui).SetAllowVisible(false);
            }
            vibranceGui.Text += String.Format(" ({0}, {1})", adapter.ToString().ToUpper(), Application.ProductVersion);
            Application.Run(vibranceGui);

            GC.KeepAlive(mutex);
        }

#if SINGLE_EXE
        private static void VerifyBundle()
        {
            try
            {
                CreateBundleControls();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                Environment.ExitCode = 1;
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private static void CreateBundleControls()
        {
            Application.EnableVisualStyles();
            using (var button = new Krypton.Toolkit.KryptonButton())
            using (var grid = new Krypton.Toolkit.KryptonDataGridView())
            {
                button.CreateControl();
                grid.CreateControl();
            }
        }
#endif
    }
}
