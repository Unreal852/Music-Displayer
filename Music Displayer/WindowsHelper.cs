using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Music_Displayer
{
    public static class WindowsHelper
    {
        [DllImport("user32.dll")]
        public static extern bool EnumWindows(Win32Callback enumProc, IntPtr lParam);

        [DllImport("User32", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr windowHandle, StringBuilder stringBuilder, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowTextLength(IntPtr hwnd);

        public static bool EnumWindow(IntPtr handle, IntPtr pointer)
        {
            (GCHandle.FromIntPtr(pointer).Target as List<IntPtr>).Add(handle);
            return true;
        }

        public static List<IntPtr> GetAllWindows()
        {
            Win32Callback enumProc = new Win32Callback(EnumWindow);
            List<IntPtr> numList = new List<IntPtr>();
            GCHandle gcHandle = GCHandle.Alloc((object)numList);
            try
            {
                EnumWindows(enumProc, GCHandle.ToIntPtr(gcHandle));
            }
            finally
            {
                if (gcHandle.IsAllocated)
                    gcHandle.Free();
            }
            return numList;
        }

        public static string GetTitle(IntPtr handle)
        {
            StringBuilder stringBuilder = new StringBuilder(GetWindowTextLength(handle) + 1);
            GetWindowText(handle, stringBuilder, stringBuilder.Capacity);
            return stringBuilder.ToString();
        }

        public delegate bool Win32Callback(IntPtr hwnd, IntPtr lParam);
    }
}
