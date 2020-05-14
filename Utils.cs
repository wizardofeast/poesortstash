using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PoE.SortStash
{
    public static class Utils
    {

        #region "Win32"

        [DllImport("user32.dll")]
        private static extern IntPtr GetClipboardData(uint uFormat);
        [DllImport("user32.dll")]
        private static extern bool IsClipboardFormatAvailable(uint format);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool OpenClipboard(IntPtr hWndNewOwner);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool CloseClipboard();
        [DllImport("kernel32.dll")]
        private static extern IntPtr GlobalLock(IntPtr hMem);
        [DllImport("kernel32.dll")]
        private static extern bool GlobalUnlock(IntPtr hMem);
        [DllImport("kernel32.dll")]
        private static extern UIntPtr GlobalSize(IntPtr hMem);

        private const uint CF_UNICODETEXT = 13;

        [DllImport("user32.dll")]
        private static extern bool EmptyClipboard();


        public static bool TryEmptyClipboard()
        {
            try
            {
                if (!OpenClipboard(IntPtr.Zero))
                    return false;

                EmptyClipboard();
                CloseClipboard();
                return true;

            }
            catch
            {
                return false;
            }

        }

        //Write the clipboard to a text file without having to first convert it to a string.
        //This avoids OutOfMemoryException for large clipboards and is faster than other methods
        public static bool TryReadClipBoardText(out string result)
        {
            result = null;
            try
            {
                if (!IsClipboardFormatAvailable(CF_UNICODETEXT) || !OpenClipboard(IntPtr.Zero))
                    return false;
            }
            catch
            {
                return false;
            }

            try
            {
                var hGlobal = GetClipboardData(CF_UNICODETEXT);
                if (hGlobal == IntPtr.Zero)
                    return false;

                var lpwcstr = GlobalLock(hGlobal);
                if (lpwcstr == IntPtr.Zero)
                    return false;

                try
                {
                    long length = (long)GlobalSize(lpwcstr);
                    Stream stream;
                    unsafe
                    {
                        stream = new UnmanagedMemoryStream((byte*)lpwcstr, length);
                    }

                    const int bufSize = 4096;
                    var buffer = new char[bufSize];

                    //Clipboard text is in Encoding.Unicode == UTF-16LE
                    using (var sr = new StreamReader(stream, Encoding.Unicode))
                    {
                        result = sr.ReadToEnd();
                        return true;
                    }

                }
                finally
                {
                    GlobalUnlock(lpwcstr);
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                try
                {
                    CloseClipboard();
                }
                catch
                {
                    //ignore
                }
            }

        }



        [StructLayout(LayoutKind.Sequential)]
        private struct Rectangle
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }


        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hwnd, out Rectangle lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetForegroundWindow(IntPtr hwnd);


        public static Screen GetScreen(string processName)
        {
            var processes = Process.GetProcessesByName(processName);
            var whnd = processes[0].MainWindowHandle;

            if (GetWindowRect(whnd, out Rectangle wsize))
            {
                SetForegroundWindow(whnd);
                return new Screen(whnd,wsize.Top, wsize.Left, wsize.Right - wsize.Left, wsize.Bottom - wsize.Top);
            }

            return null;
        }

        #endregion

        public static int Compare(this string[] @this,string[] other)
        {
            Array.Sort<string>(@this);
            Array.Sort<string>(other);

            if (@this.SequenceEqual(other))
                return 0;
            else
                return @this.Length.CompareTo(other.Length);
        }    

    }
}
