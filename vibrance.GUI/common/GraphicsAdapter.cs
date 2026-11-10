using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using vibrance.GUI.AMD.vendor;

namespace vibrance.GUI.common
{
    public enum GraphicsAdapter
    {
        Unknown = 0,
        Nvidia = 1,
        Amd = 2,
        Ambiguous = 3
    }

    public class GraphicsAdapterHelper
    {
        private const uint DigcfPresent = 0x00000002;
        private const uint DnStarted = 0x00000008;
        private const uint DnHasProblem = 0x00000400;
        private const int ErrorNoMoreItems = 259;
        private const int MaxDeviceIdLength = 200;
        private static readonly Guid DisplayClass = new Guid("4d36e968-e325-11ce-bfc1-08002be10318");

        [StructLayout(LayoutKind.Sequential)]
        private struct DeviceInfoData
        {
            public uint Size;
            public Guid ClassGuid;
            public uint DevInst;
            public IntPtr Reserved;
        }

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr SetupDiGetClassDevs(ref Guid classGuid, string enumerator, IntPtr parent, uint flags);

        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern bool SetupDiEnumDeviceInfo(IntPtr deviceInfoSet, uint index, ref DeviceInfoData deviceInfo);

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SetupDiGetDeviceInstanceId(IntPtr deviceInfoSet, ref DeviceInfoData deviceInfo,
            StringBuilder instanceId, int instanceIdSize, IntPtr requiredSize);

        [DllImport("setupapi.dll")]
        private static extern bool SetupDiDestroyDeviceInfoList(IntPtr deviceInfoSet);

        [DllImport("cfgmgr32.dll")]
        private static extern uint CM_Get_DevNode_Status(out uint status, out uint problem, uint devInst, uint flags);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        private static extern bool FreeLibrary(IntPtr module);

        public static GraphicsAdapter GetAdapter()
        {
            var adapters = GetStartedAdapters();
            bool amd = adapters.Contains(GraphicsAdapter.Amd) && IsAmdDriverAvailable();
            bool nvidia = adapters.Contains(GraphicsAdapter.Nvidia) && IsNvidiaDriverAvailable();

            if (amd && nvidia)
                return GraphicsAdapter.Ambiguous;
            if (amd)
                return GraphicsAdapter.Amd;
            if (nvidia)
                return GraphicsAdapter.Nvidia;
            return GraphicsAdapter.Unknown;
        }

        private static HashSet<GraphicsAdapter> GetStartedAdapters()
        {
            var adapters = new HashSet<GraphicsAdapter>();
            Guid classGuid = DisplayClass;
            IntPtr deviceInfoSet = SetupDiGetClassDevs(ref classGuid, "PCI", IntPtr.Zero, DigcfPresent);
            if (deviceInfoSet == new IntPtr(-1))
            {
                Trace.TraceWarning("Could not enumerate display adapters. Win32 error: {0}.", Marshal.GetLastWin32Error());
                return adapters;
            }

            try
            {
                var device = new DeviceInfoData { Size = (uint)Marshal.SizeOf(typeof(DeviceInfoData)) };
                var instanceId = new StringBuilder(MaxDeviceIdLength);
                for (uint index = 0; SetupDiEnumDeviceInfo(deviceInfoSet, index, ref device); index++)
                {
                    uint status, problem;
                    if (CM_Get_DevNode_Status(out status, out problem, device.DevInst, 0) != 0 ||
                        (status & (DnStarted | DnHasProblem)) != DnStarted ||
                        !SetupDiGetDeviceInstanceId(deviceInfoSet, ref device, instanceId, instanceId.Capacity, IntPtr.Zero))
                    {
                        continue;
                    }

                    string id = instanceId.ToString();
                    if (id.StartsWith(@"PCI\VEN_1002&", StringComparison.OrdinalIgnoreCase))
                        adapters.Add(GraphicsAdapter.Amd);
                    else if (id.StartsWith(@"PCI\VEN_10DE&", StringComparison.OrdinalIgnoreCase))
                        adapters.Add(GraphicsAdapter.Nvidia);
                }

                int error = Marshal.GetLastWin32Error();
                if (error != ErrorNoMoreItems)
                    Trace.TraceWarning("Display adapter enumeration ended early. Win32 error: {0}.", error);
            }
            finally
            {
                SetupDiDestroyDeviceInfoList(deviceInfoSet);
            }
            return adapters;
        }

        private static bool IsAmdDriverAvailable()
        {
            try
            {
                IAmdAdapter adapter = Environment.Is64BitOperatingSystem ? (IAmdAdapter)new AmdAdapter64() : new AmdAdapter32();
                return adapter.IsAvailable();
            }
            catch (Exception ex) when (ex is DllNotFoundException || ex is EntryPointNotFoundException || ex is BadImageFormatException)
            {
                Trace.TraceWarning("Could not load the AMD display driver: {0}", ex.Message);
                return false;
            }
        }

        private static bool IsNvidiaDriverAvailable()
        {
            IntPtr module = LoadLibrary(Path.Combine(Environment.SystemDirectory, "nvapi.dll"));
            if (module == IntPtr.Zero)
                return false;

            FreeLibrary(module);
            return true;
        }
    }
}
