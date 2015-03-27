/**
 *  ImageMap.cs
 *
 * Copyright (C) 2015 by RAiCE. ◆QC1PJykf7Q
 *
 * This file is released under the GPL v2.
 * see <http://www.gnu.org/licenses/>
 *
 * Date: Sun Mar 22 00:00:00 2015 -0900
 *
**/
using System;
using System.Drawing;
using System.Diagnostics;
using Common;

namespace JoyToAny
{
    internal static class ScreenCapture
    {
        /// <summary>
        /// 同時に並列処理を許可する数
        /// </summary>
        public static int asyncCaptureNum = 1;

        /// <summary>
        /// 現在の並列処理数
        /// </summary>
        private static int nowCapturingNum = 0;

        /// <summary>
        /// コンストラクタ（CPUコア数を元に並列処理数を設定）
        /// </summary>
        static ScreenCapture()
        {
            asyncCaptureNum = Environment.ProcessorCount - 1;
            if (asyncCaptureNum < 1)
            {
                asyncCaptureNum = 1;
            }
        }

        public static string FindClass
        {
            get
            {
                return findClass;
            }
            private set
            {
                findClass = value;
            }
        }
        private static string findClass = null;
        public static string FindTitle
        {
            get
            {
                return findTitle;
            }
            private set
            {
                findTitle = value;
            }
        }
        private static string findTitle = "";

        public static int ProcessId
        {
            get
            {
                return processId;
            }
            private set
            {
                processId = value;
            }
        }
        private static int processId = 0;
        public static IntPtr WindowHandle
        {
            get
            {
                return windowHandle;
            }
            private set
            {
                windowHandle = value;
            }
        }
        private static IntPtr windowHandle = IntPtr.Zero;
        public static Rectangle WindowPos
        {
            get
            {
                return windowPos;
            }
            private set
            {
                windowPos = value;
            }
        }
        private static Rectangle windowPos;

        #region スレッド処理 (Start / Stop)

        /// <summary>
        /// 更新タイマ（System.Threading.Timer）
        /// </summary>
        private static System.Threading.Timer timer = null;

        /// <summary>
        /// キャプチャ開始（処理間隔を変更する場合も再実行するだけでよい）
        /// </summary>
        /// <param name="className"></param>
        /// <param name="title"></param>
        /// <param name="interval"></param>
        internal static void Start(string className, string title, int interval)
        {
            FindClass = className;
            FindTitle = title;

            Stop();
            timer = new System.Threading.Timer(IntervalCapture, null, interval * 2, interval);
        }

        /// <summary>
        /// キャプチャ停止
        /// </summary>
        internal static void Stop()
        {
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }
        }

        #endregion

        /// <summary>
        /// ウィンドウのアクティブ状態を取得
        /// </summary>
        internal static bool IsWindowActive
        {
            get
            {
                return WindowHandle != IntPtr.Zero && w32.GetForegroundWindow() == WindowHandle;
            }
        }

        /// <summary>
        /// ウィンドウのフルスクリーン状態を取得
        /// </summary>
        public static bool IsFullScreen
        {
            get
            {
                return isFullScreen;
            }
            private set
            {
                isFullScreen = value;
            }
        }
        private static bool isFullScreen = false;

        /// <summary>
        /// 最新のキャプチャイメージ（要排他制御）
        /// </summary>
        private static Bitmap lastBitmap = null;

        /// <summary>
        /// 空っぽの Bitmap （排他制御の lock に使う）
        /// </summary>
        private static Bitmap BitmapNull = new Bitmap(1, 1);

        /// <summary>
        /// 安全に最新の Bitmap を取得する
        /// </summary>
        /// <returns></returns>
        public static Bitmap GetBitmap()
        {
            Bitmap bmp = null;
            lock (BitmapNull)
            {
                if (lastBitmap != BitmapNull)
                {
                    bmp = lastBitmap;
                    lastBitmap = BitmapNull;
                }
            }
            return bmp;
        }


        public static int CaptureMethod { set; get; }

        /// <summary>
        /// 対象のウィンドウをキャプチャ（タイマから実行される）
        /// </summary>
        internal static void IntervalCapture(object sender)
        {
            // 同時実行数をチェック
            // asyncCaptureNum = 0 の場合はキャプチャ処理されない (スレッドを生かしたまま止められる)
            if (nowCapturingNum >= asyncCaptureNum)
            {
                //System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString("HH:mm:ss"));
                return;
            }

            Stopwatch sw1 = new Stopwatch();
            Stopwatch sw2 = new Stopwatch();
            sw1.Start();

            nowCapturingNum++;
            try
            {
                IntPtr hWnd = IntPtr.Zero;
                int pid = 0;

                if (WindowHandle == IntPtr.Zero)
                {
                    // TODO: w32.FindWindow に変更するべき
                    foreach (Process process in Process.GetProcesses())
                    {
                        IntPtr windowHandle = process.MainWindowHandle;

                        if (windowHandle != IntPtr.Zero)
                        {
                            if (process.MainWindowTitle.IndexOf(FindTitle) == 0)
                            {
                                pid = process.Id;
                                hWnd = windowHandle;
                                break;
                            }
                        }
                    }

                    // クラス変数に記録
                    ProcessId = pid;
                    WindowHandle = hWnd;
                }

                if (WindowHandle == IntPtr.Zero)
                {
                    return;
                }


                // フレーム枠、タイトルバー、メニューバーを含まない LEFT:0 TOP:0 の矩形サイズが取得できる
                w32.RECT cliRect = new w32.RECT();
                w32.GetClientRect(WindowHandle, ref cliRect);

                // スクリーン座標上のウィンドウ位置が取得できる
                w32.RECT w32Rect = new w32.RECT();
                w32.GetWindowRect(WindowHandle, ref w32Rect);
                Rectangle rect = new Rectangle(w32Rect.Left, w32Rect.Top, w32Rect.Right - w32Rect.Left, w32Rect.Bottom - w32Rect.Top);

                Rectangle PrimaryScreenBounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
                if (PrimaryScreenBounds.Equals(rect))
                {
                    IsFullScreen = true;
                }
                else if (rect.Left < -9999)
                {
                    IsFullScreen = true;
                    rect = PrimaryScreenBounds;
                }
                else
                {
                    IsFullScreen = false;
                }

                Bitmap bmp = new Bitmap(rect.Width, rect.Height);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    IntPtr hDC = g.GetHdc();
                    IntPtr disDC = IntPtr.Zero;
                    try
                    {
                        sw2.Start();
                        switch (CaptureMethod)
                        {
                            case 1:
                                // PrintWindow
                                w32.PrintWindow(WindowHandle, hDC, 0);
                                break;
                            case 2:
                                // BitBlt 全画面キャプチャ
                                disDC = w32.GetDC(IntPtr.Zero);
                                sw2.Start();
                                w32.BitBlt(hDC, 0, 0, rect.Width, rect.Height, disDC, rect.Left, rect.Top, w32.TernaryRasterOperations.SRCCOPY);
                                sw2.Stop();
                                w32.ReleaseDC(IntPtr.Zero, disDC);
                                break;
                            case 3:
                                // BitBlt ターゲットウィンドウキャプチャ
                                disDC = w32.GetDC(WindowHandle);
                                sw2.Start();
                                w32.BitBlt(hDC, 0, 0, rect.Width, rect.Height, disDC, 0, 0, w32.TernaryRasterOperations.SRCCOPY);
                                sw2.Stop();
                                w32.ReleaseDC(WindowHandle, disDC);
                                break;
                            default:
                                break;
                        }
                        sw2.Stop();
                    }
                    finally
                    {
                        g.ReleaseHdc(hDC);
                    }
                }

                // 全画面でない時はウィンドウ枠、タイトル、メニューバーをトリミング
                if (IsFullScreen == false)
                {
                    // TODO: トリミング座標を ClientRect の情報を使うように修正

                    int CaptionHeight = System.Windows.Forms.SystemInformation.CaptionHeight;
                    Size BorderSize = System.Windows.Forms.SystemInformation.FrameBorderSize;
                    int MenuHeight = System.Windows.Forms.SystemInformation.MenuHeight;

                    // 座標の修正
                    rect.X += BorderSize.Width;
                    rect.Y += BorderSize.Height + CaptionHeight + MenuHeight;
                    rect.Width -= BorderSize.Width * 2;
                    rect.Height -= BorderSize.Height * 2 + CaptionHeight + MenuHeight;

                    // トリミング座標
                    Rectangle clipRect = new Rectangle();
                    clipRect.X = BorderSize.Width;
                    clipRect.Y = BorderSize.Height + CaptionHeight + MenuHeight;
                    clipRect.Width = rect.Width;
                    clipRect.Height = rect.Height;

                    // トリミングした Bitmap に差し替え
                    Bitmap tmp = bmp.Clone(clipRect, bmp.PixelFormat);
                    bmp.Dispose();
                    bmp = tmp;
                }
                WindowPos = rect;

                sw2.Stop();
                Bitmap oldBmp = null;
                lock (BitmapNull)
                {
                    if (lastBitmap != BitmapNull)
                    {
                        oldBmp = lastBitmap;
                    }
                    lastBitmap = bmp;
                }
                if (oldBmp != null)
                {
                    oldBmp.Dispose();
                }
            }
            catch (Exception)
            {
                //System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
            finally
            {
                nowCapturingNum--;
            }
            sw1.Stop();

            System.Diagnostics.Debug.WriteLine(string.Format("IntervalCapture: {0}ms {1}ms", sw1.ElapsedMilliseconds, sw2.ElapsedMilliseconds));

        }

    }
}
