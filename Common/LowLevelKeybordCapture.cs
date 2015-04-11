#if USEHOTKEY

using System;

namespace Common
{
    public static class LowLevelKeybordCapture
    {
        public static event EventHandler<KeybordCaptureEventArgs> KeyDown;
        public static event EventHandler<KeybordCaptureEventArgs> KeyUp;
        public static event EventHandler<KeybordCaptureEventArgs> SysKeyUp;
        public static event EventHandler<KeybordCaptureEventArgs> SysKeyDown;

        public const int WH_KEYBOARD_LL = 13;
        public const int HC_ACTION = 0;
        public const int WM_KEYDOWN = 0x0100;
        public const int WM_KEYUP = 0x0101;
        public const int WM_SYSKEYDOWN = 0x0104;
        public const int WM_SYSKEYUP = 0x0105;

        private static IntPtr s_hook;
        public static bool IsCapture { get { return s_hook != IntPtr.Zero; } }

        private static w32.LowLevelKeyboardProc s_proc;

        static LowLevelKeybordCapture()
        {
            s_proc = new w32.LowLevelKeyboardProc(HookProc);
            IntPtr hIns = System.Runtime.InteropServices.Marshal.GetHINSTANCE(typeof(LowLevelKeybordCapture).Module);
            s_hook = w32.SetWindowsHookEx(WH_KEYBOARD_LL, s_proc, hIns, 0);
            AppDomain.CurrentDomain.DomainUnload += (sender, e) =>
            {
                if (s_hook != IntPtr.Zero)
                    w32.UnhookWindowsHookEx(s_hook);
            };
        }

        static IntPtr HookProc(int nCode, IntPtr wParam, ref w32.KBDLLHOOKSTRUCT lParam)
        {
            bool cancel = false;
            if (nCode == HC_ACTION)
            {
                KeybordCaptureEventArgs ev = new KeybordCaptureEventArgs(lParam);
                switch (wParam.ToInt32())
                {
                    case WM_KEYDOWN: if (KeyDown != null) KeyDown(null, ev); break;
                    case WM_KEYUP: if (KeyUp != null) KeyUp(null, ev); break;
                    case WM_SYSKEYDOWN: if (SysKeyDown != null) SysKeyDown(null, ev); break;
                    case WM_SYSKEYUP: if (SysKeyUp != null) SysKeyUp(null, ev); break;
                }
                cancel = ev.Cancel;
            }
            return cancel ? (IntPtr)1 : w32.CallNextHookEx(s_hook, nCode, wParam, ref lParam);
        }

        public sealed class KeybordCaptureEventArgs : EventArgs
        {
            private int m_keyCode;
            private int m_scanCode;
            private int m_flags;
            private int m_time;
            private bool m_cancel;

            internal KeybordCaptureEventArgs(w32.KBDLLHOOKSTRUCT keyData)
            {
                m_keyCode = keyData.vkCode;
                m_scanCode = keyData.scanCode;
                m_flags = keyData.flags;
                m_time = keyData.time;
                m_cancel = false;
            }

            public int KeyCode { get { return m_keyCode; } }
            public int ScanCode { get { return m_scanCode; } }
            public int Flags { get { return m_flags; } }
            public int Time { get { return m_time; } }
            public bool Cancel { get { return m_cancel; } set { m_cancel = value; } }
        }
    }
}

#endif
