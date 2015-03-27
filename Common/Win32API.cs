using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Common
{
    public static class w32
    {

        #region GetWindowRect / GetClientRect

        [DllImport("user32.dll")]
        internal extern static int GetWindowRect(IntPtr hwnd, ref  RECT lpRect);

        [DllImport("user32.dll")]
        internal static extern bool GetClientRect(IntPtr hWnd, ref RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        #endregion

        #region PrintWindow / BitBlt

        [DllImport("User32.dll")]
        internal extern static bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);

        public enum TernaryRasterOperations : uint
        {
            SRCCOPY = 0x00CC0020,
            SRCPAINT = 0x00EE0086,
            SRCAND = 0x008800C6,
            SRCINVERT = 0x00660046,
            SRCERASE = 0x00440328,
            NOTSRCCOPY = 0x00330008,
            NOTSRCERASE = 0x001100A6,
            MERGECOPY = 0x00C000CA,
            MERGEPAINT = 0x00BB0226,
            PATCOPY = 0x00F00021,
            PATPAINT = 0x00FB0A09,
            PATINVERT = 0x005A0049,
            DSTINVERT = 0x00550009,
            BLACKNESS = 0x00000042,
            WHITENESS = 0x00FF0062,
            CAPTUREBLT = 0x40000000
        }

        [DllImport("gdi32.dll")]
        public static extern int BitBlt(IntPtr hDestDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, TernaryRasterOperations dwRop);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern Int32 ReleaseDC(IntPtr hwnd, IntPtr hdc);

        #endregion

        #region GetForegroundWindow

        [DllImport("user32.dll")]
        internal static extern IntPtr GetForegroundWindow();

        #endregion

        #region GetCursorPos / SetCursorPos

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(out Point lpPoint);

        [DllImport("USER32.dll", CallingConvention = CallingConvention.StdCall)]
        internal static extern void SetCursorPos(int X, int Y);

        #endregion

        #region PostMessage / SendMessage

        //[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        //internal static extern bool PostMessage(IntPtr hWnd, uint Msg, uint wParam, uint lParam);

        //[DllImport("user32.dll", EntryPoint = "SendMessageA")]
        //internal extern static int SendMessage(int hwnd, int msg, int wParam, int lParam);

        #endregion

        #region memcmp

        [DllImport("msvcrt.dll", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern int memcmp(byte[] b1, byte[] b2, UIntPtr count);

        public static bool memcmp(byte[] a, byte[] b, uint length)
        {
            return memcmp(a, b, new UIntPtr(length)) == 0;
        }

        #endregion

        #region GetKeyState / GetKeyboardState / SetKeyboardState

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        internal static extern int GetKeyState(int nVirtKey);

        [DllImport("user32.dll")]
        internal extern static int GetKeyboardState(byte[] lpKeyState);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        internal static extern bool SetKeyboardState(byte[] lpKeyState);

        #endregion

        #region SetWindowsHookEx / UnhookWindowsHookEx / DllImport

#if USEHOTKEY

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        internal struct KBDLLHOOKSTRUCT
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        [System.Runtime.InteropServices.UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.Cdecl)]
        internal delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, ref KBDLLHOOKSTRUCT lParam);

        //[System.Runtime.InteropServices.DllImport("kernel32.dll")]
        //internal static extern IntPtr GetModuleHandle(string lpModuleName);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        internal static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, int dwThreadId);
        
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        internal static extern IntPtr CallNextHookEx(IntPtr hHook, int nCode, IntPtr wParam, ref KBDLLHOOKSTRUCT lParam);
        
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        internal static extern bool UnhookWindowsHookEx(IntPtr hHook);

#endif

        #endregion

        #region SendInput

        // キー操作、マウス操作をシミュレート(擬似的に操作する)
        [DllImport("user32.dll")]
        internal extern static void SendInput(int nInputs, ref INPUT pInputs, int cbsize);

        // 仮想キーコードをスキャンコードに変換
        [DllImport("user32.dll", EntryPoint = "MapVirtualKeyA")]
        internal extern static int MapVirtualKey(int wCode, int wMapType);

        // マウスイベント(mouse_eventの引数と同様のデータ)
        [StructLayout(LayoutKind.Sequential)]
        internal struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public int mouseData;
            public int dwFlags;
            public int time;
            public int dwExtraInfo;
        };

        // キーボードイベント(keybd_eventの引数と同様のデータ)
        [StructLayout(LayoutKind.Sequential)]
        internal struct KEYBDINPUT
        {
            public short wVk;
            public short wScan;
            public int dwFlags;
            public int time;
            public int dwExtraInfo;
        };

        // ハードウェアイベント
        [StructLayout(LayoutKind.Sequential)]
        internal struct HARDWAREINPUT
        {
            public int uMsg;
            public short wParamL;
            public short wParamH;
        };

        // 各種イベント(SendInputの引数データ)
        [StructLayout(LayoutKind.Explicit)]
        internal struct INPUT
        {
            [FieldOffset(0)]
            public int type;
            [FieldOffset(4)]
            public MOUSEINPUT mi;
            [FieldOffset(4)]
            public KEYBDINPUT ki;
            [FieldOffset(4)]
            public HARDWAREINPUT hi;
        };

        #endregion


    }
}
