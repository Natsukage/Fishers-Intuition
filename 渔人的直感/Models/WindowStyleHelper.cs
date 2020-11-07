using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace 渔人的直感.Models
{
    /// <summary>
    ///     设置Window Style令窗体实现鼠标穿透、隐藏等。
    /// </summary>
    internal class WindowStyleHelper
    {
        public static int ExStyle
        {
            get => (int) GetWindowLong(new WindowInteropHelper(MainWindow.CurrentMainWindow).Handle, -20); //GetWindowLongFields.GWL_EXSTYLE = -20
            set => SetWindowLong(new WindowInteropHelper(MainWindow.CurrentMainWindow).Handle, -20, (IntPtr) value);
        }

        #region Window styles

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        private static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            var error = 0;
            var result = IntPtr.Zero;
            // Win32 SetWindowLong doesn't clear error on success
            SetLastError(0);

            if (IntPtr.Size == 4)
            {
                // use SetWindowLong
                var tempResult = IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
                error = Marshal.GetLastWin32Error();
                result = new IntPtr(tempResult);
            }
            else
            {
                // use SetWindowLongPtr
                result = IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
                error = Marshal.GetLastWin32Error();
            }

            if (result == IntPtr.Zero && error != 0) throw new Win32Exception(error);

            return result;
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern int IntSetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private static int IntPtrToInt32(IntPtr intPtr)
        {
            return unchecked((int) intPtr.ToInt64());
        }

        [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
        public static extern void SetLastError(int dwErrorCode);

        #endregion
    }
}