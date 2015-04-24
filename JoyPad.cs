/**
 *  JoyPad.cs
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
using System.Windows.Forms;
using Microsoft.DirectX.DirectInput;
using Common;

namespace JoyToAny
{
    static class JoyPad
    {
        /// <summary>
        /// 
        /// </summary>
        private static System.Threading.Timer timer = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interval"></param>
        internal static void Start(int interval)
        {
            Stop();
            timer = new System.Threading.Timer(DI_Loop, null, interval * 2, interval);
        }

        /// <summary>
        /// 
        /// </summary>
        internal static void Stop()
        {
            if (timer != null)
                timer.Dispose();
            timer = null;
        }

        /// <summary>
        /// 接続した DirectInput デバイス
        /// </summary>
        private static Device DI_joypad = null;

        /// <summary>
        /// デバイス名
        /// </summary>
        public static string DeviceName = "";

        /// <summary>
        /// 軸の数
        /// </summary>
        public static int NumAxis = 0;

        /// <summary>
        /// PoVの数
        /// </summary>
        public static int NumPovs = 0;

        /// <summary>
        /// ボタンの数
        /// </summary>
        public static int NumBtns = 0;

        /// <summary>
        /// ボタンの押され状態
        /// 0..15 ボタン１～15
        /// 16..19 Pov0 (↑→↓←)
        /// 20..23 Pov1 (↑→↓←)
        /// 24..27 X, Y, Rx, Ry
        /// 28, 29 L1, R1
        /// 30, 31 L2, R2
        /// </summary>
        public static bool[] ButtonPress = new bool[32];

        /// <summary>
        /// 入力イベントを全てをスキップ
        /// </summary>
        public static bool AllStop = false;

        /// <summary>
        /// 画像キャプチャ指示
        /// </summary>
        public static bool SnapShot = false;

        /// <summary>
        /// 画面内での操作キャラクタの座標
        /// </summary>
        public static Point CharaPosition = new Point();

        /// <summary>
        /// キーボードイベントとして使用できるキー
        /// </summary>
        public static bool[] USE_KEYS = new bool[256];

        /// <summary>
        /// 左スティック機能割り当て（1:フリーマウス移動 2:キャラ移動）
        /// </summary>
        public static byte AnalogMapL = 0;

        /// <summary>
        /// 右スティック機能割り当て（1:フリーマウス移動 2:キャラ移動）
        /// </summary>
        public static byte AnalogMapR = 0;

        /// <summary>
        /// アナログ３機能割り当て（1:フリーマウス移動 2:キャラ移動 3:Ctrlキー）
        /// </summary>
        public static byte AnalogMapZ = 0;

        /// <summary>
        /// ボタンへの機能割り当て
        /// 0..15 ボタン１～15
        /// 16..19 Pov0 (↑→↓←)
        /// 20..23 Pov1 (↑→↓←)
        /// 24..27 reserved
        /// 28, 29 L1, R1
        /// 30, 31 L2, R2
        /// </summary>
        public static byte[] BtnMap = new byte[32] {
			0x00, 0x00, 0x00, 0x00, // A, B, X, Y
			0x00, 0x00, 0x00, 0x00, // L1, R1, Lpush, Rpush
			0x00, 0x00, 0x00, 0x00, // Back, Next, 10, 11
			0x00, 0x00, 0x00, 0x00, // 12, 13, 14, 15
			0x00, 0x00, 0x00, 0x00, // (左十字キー) ↑,→,↓,←
			0x00, 0x00, 0x00, 0x00, // (右十字キー) ↑,→,↓,←
			0x00, 0x00, 0x00, 0x00, // 予約,予約,予約,予約
			0x00, 0x00, 0x00, 0x00  // (トリガ) L1, R1, L2, R2
		};

        /// <summary>
        /// アナログ値の基準値
        /// </summary>
        private static int[] anaBase = new int[8];

        /// <summary>
        /// アナログスティックの遊び幅 (0..24000)
        /// </summary>
        public static int AnalogFree
        {
            get
            {
                return analogFree;
            }
            set
            {
                // 24000 以上の値は使えない (32768 での ZeroDiv 発生も回避)
                if (value > 24000)
                {
                    value = 24000;
                }
                analogFree = value;
                charaMoveZit = (float)CharaMoveMax / (32768 - (float)analogFree);
                freeMoveZit = (float)FreeMoveMax / (32768 - (float)analogFree);
            }
        }
        private static int analogFree = 10000;

        /// <summary>
        /// 左スティック入力する最大変化値
        /// </summary>
        public static int CharaMoveMax
        {
            get
            {
                return charaMoveMax;
            }
            set
            {
                charaMoveMax = value;
                charaMoveZit = (float)charaMoveMax / (32768 - (float)AnalogFree);
            }
        }
        private static int charaMoveMax = 100;

        /// <summary>
        /// 右スティックで入力する最大変化値
        /// </summary>
        public static int FreeMoveMax
        {
            get
            {
                return freeMoveMax;
            }
            set
            {
                freeMoveMax = value;
                freeMoveZit = (float)freeMoveMax / (32768 - (float)AnalogFree);
            }
        }
        private static int freeMoveMax = 10;

        /// <summary>
        /// 左スティックのボリューム感度
        /// </summary>
        private static float charaMoveZit = (float)CharaMoveMax / (32768 - (float)AnalogFree);

        /// <summary>
        /// 右スティックのボリューム感度
        /// </summary>
        private static float freeMoveZit = (float)FreeMoveMax / (32768 - (float)AnalogFree);

        /// <summary>
        /// 初期化
        /// </summary>
        static JoyPad()
        {
            USE_KEYS = new bool[256];
        }

        /// <summary>
        /// 接続されているジョイパッド名リストを取得
        /// </summary>
        /// <returns></returns>
        internal static string[] DI_GetDevice()
        {
            DeviceList devList = Manager.GetDevices(DeviceClass.GameControl, EnumDevicesFlags.AttachedOnly);
            string[] devName = new string[devList.Count];

            int i = 0;
            foreach (DeviceInstance dev in devList)
            {
                Device d = new Device(dev.InstanceGuid);
                devName[i++] = d.DeviceInformation.ProductName;
            }
            return devName;
        }

        /// <summary>
        /// コントロールパネルを開く
        /// </summary>
        /// <param name="name"></param>
        internal static void DI_OpenSetting(string name)
        {
            // ジョイパッド一覧を取得
            Device d = null;
            DeviceList devList = Manager.GetDevices(DeviceClass.GameControl, EnumDevicesFlags.AttachedOnly);
            foreach (DeviceInstance dev in devList)
            {
                d = new Microsoft.DirectX.DirectInput.Device(dev.InstanceGuid);
                if (d.DeviceInformation.ProductName == name)
                {
                    // コントロールパネルを開く
                    d.RunControlPanel();
                    break;
                }
            }
        }

        /// <summary>
        /// 指定されたジョイパッドを接続
        /// </summary>
        /// <param name="name"></param>
        internal static bool DI_Connect(System.Windows.Forms.Form self, string name)
        {
            // ジョイパッド一覧を取得
            Device d = null;
            DeviceList devList = Manager.GetDevices(DeviceClass.GameControl, EnumDevicesFlags.AttachedOnly);
            foreach (DeviceInstance dev in devList)
            {
                d = new Microsoft.DirectX.DirectInput.Device(dev.InstanceGuid);
                if (d.DeviceInformation.ProductName == name)
                {
                    d.SetCooperativeLevel(self, CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive);
                    break;
                }
            }

            if (d == null)
            {
                return false;
            }

            // 占有権を取る
            d.SetDataFormat(DeviceDataFormat.Joystick);
            d.Acquire();
            DI_joypad = d;

            // デバイス情報を取得する
            DeviceName = d.DeviceInformation.ProductName;
            NumAxis = d.Caps.NumberAxes;
            NumPovs = d.Caps.NumberPointOfViews;
            NumBtns = d.Caps.NumberButtons;

            return true;
        }

        /// <summary>
        /// 初期化済みフラグ (アナログ基準値取得)
        /// </summary>
        private static bool isInited = false;

        /// <summary>
        /// 現在の並列処理数
        /// </summary>
        private static bool nowProcessing = false;

        /// <summary>
        /// キーの状態の記憶
        /// </summary>
        private static bool[] lastState = new bool[256];
        public static bool[] KeyState = new bool[256];
        private static bool isRunning = false;

        /// <summary>
        /// ジョイパッド監視ループ
        /// </summary>
        /// <param name="sender"></param>
        internal static void DI_Loop(object sender)
        {
            // 重複処理防止のためのチェック
            if (nowProcessing == true)
            {
                //System.Diagnostics.Debug.WriteLine(string.Format("{0}: Skip DI_LOOP", DateTime.Now.ToString("HH:mm:ss")));
                return;
            }
            nowProcessing = true;
            try
            {
                // コントローラの状態をポーリングで取得
                // TODO: 例外処理 https://msdn.microsoft.com/ja-jp/library/cc351898.aspx
                DI_joypad.Poll();
                JoystickState state = DI_joypad.CurrentJoystickState;

                // ボタン状態
                // 仮想キーコード (VK_* / System.Windows.Forms.Keys) のマップにいくつか独自定義
                bool[] dState = new bool[256];

                // ボタン状態チェック（GetButtons() で byte[128] が取れるので境界注意）
                byte[] buttons = state.GetButtons();
                for (int i = 0; i < BtnMap.Length; i++)
                {
                    if (buttons[i] > 0)
                    {
                        dState[BtnMap[i]] = true;
                    }
                }

                // 16ボタン
                for (int i = 0; i < 15; i++)
                {
                    ButtonPress[i] = buttons[i] > 0;
                }
                for (int i = 16; i < 32; i++)
                {
                    ButtonPress[i] = false;
                }


                // POV値（十字キー）の取得
                int[] pov = state.GetPointOfView();
                if (pov.Length >= 1)
                {
                    switch (pov[0])
                    {
                        case 0: // 上
                            ButtonPress[16] = true;
                            dState[BtnMap[16]] = true;
                            break;
                        case 9000: // 右
                            ButtonPress[17] = true;
                            dState[BtnMap[17]] = true;
                            break;
                        case 18000: // 下
                            ButtonPress[18] = true;
                            dState[BtnMap[18]] = true;
                            break;
                        case 27000: // 左
                            ButtonPress[19] = true;
                            dState[BtnMap[19]] = true;
                            break;
                        default:
                            // case -1: 何も押していない
                            // 斜めは処理しない
                            break;
                    }
                }
                if (pov.Length >= 2)
                {
                    switch (pov[1])
                    {
                        case -1: // 何も押してない
                            break;
                        case 0: // 上
                            ButtonPress[20] = true;
                            dState[BtnMap[20]] = true;
                            break;
                        case 9000: // 右
                            ButtonPress[21] = true;
                            dState[BtnMap[21]] = true;
                            break;
                        case 18000: // 下
                            ButtonPress[22] = true;
                            dState[BtnMap[22]] = true;
                            break;
                        case 27000: // 左
                            ButtonPress[23] = true;
                            dState[BtnMap[23]] = true;
                            break;
                        default:
                            // case -1: 何も押していない
                            // 斜めは処理しない
                            break;
                    }
                }

                // トラブル対策
                // TODO: コンストラクタに戻したい
                if (isInited == false)
                {
                    isInited = true;
                    // デバイス固有情報で初期化する
                    anaBase[0] = state.X;
                    anaBase[1] = state.Y;
                    anaBase[2] = state.Z;
                    anaBase[3] = state.Rx;
                    anaBase[4] = state.Ry;
                    anaBase[5] = state.Rz;
                }

                // アナログ変位取得
                int Lx = state.X - anaBase[0];
                int Ly = state.Y - anaBase[1];
                int Lz = state.Z - anaBase[2];
                int Rx = state.Rx - anaBase[3];
                int Ry = state.Ry - anaBase[4];
                int Rz = state.Rz - anaBase[5];

                // アナログトリガ（デジタル信号扱いする場合があるので先に処理）
                if (Math.Abs(Lz) > AnalogFree)
                {
                    ButtonPress[28] = Lz > 0;
                    ButtonPress[29] = Lz < 0;
                    dState[BtnMap[28 + (Lz > 0 ? 0 : 1)]] = true;
                }
                if (Math.Abs(Rz) > AnalogFree)
                {
                    ButtonPress[30] = Rz > 0;
                    ButtonPress[31] = Rz < 0;
                    dState[BtnMap[30 + (Lz > 0 ? 0 : 1)]] = true;
                }

                if (Math.Abs(Lx) > AnalogFree)
                {
                    ButtonPress[24] = true;
                }
                if (Math.Abs(Ly) > AnalogFree)
                {
                    ButtonPress[25] = true;
                }
                if (Math.Abs(Rx) > AnalogFree)
                {
                    ButtonPress[26] = true;
                }
                if (Math.Abs(Ry) > AnalogFree)
                {
                    ButtonPress[27] = true;
                }

                // 入力イベントを全て止める
                if (AllStop == true)
                {
                    return;
                }

                // アナログ状態チェック
                int[,] aState = new int[4, 3];

                // 右アナログ
                if (Rx * Rx + Ry * Ry > AnalogFree * AnalogFree)
                {
                    if (AnalogMapR == 1 || AnalogMapR == 2)
                    {
                        double rad = Math.Atan2(Ry, Rx);
                        aState[AnalogMapR, 0] = (int)((Rx - (Math.Cos(rad) * AnalogFree)) * (AnalogMapR == 2 ? freeMoveZit : charaMoveZit));
                        aState[AnalogMapR, 1] = (int)((Ry - (Math.Sin(rad) * AnalogFree)) * (AnalogMapR == 2 ? freeMoveZit : charaMoveZit));
                        aState[AnalogMapR, 2] = AnalogMapR;
                    }
                }

                // 左アナログ（右アナログと同じ処理の場合は上書きする）
                if (Lx * Lx + Ly * Ly > AnalogFree * AnalogFree)
                {
                    if (AnalogMapL == 1 || AnalogMapL == 2)
                    {
                        double rad = Math.Atan2(Ly, Lx);
                        aState[AnalogMapL, 0] = (int)((Lx - (Math.Cos(rad) * AnalogFree)) * (AnalogMapL == 2 ? freeMoveZit : charaMoveZit));
                        aState[AnalogMapL, 1] = (int)((Ly - (Math.Sin(rad) * AnalogFree)) * (AnalogMapL == 2 ? freeMoveZit : charaMoveZit));
                        aState[AnalogMapL, 2] = AnalogMapL;
                    }
                }

                // アナログ３（右アナログと同じ処理の場合は上書きする）
                if (Lz * Lz + Rz * Rz > AnalogFree * AnalogFree)
                {
                    if (AnalogMapZ == 1 || AnalogMapZ == 2)
                    {
                        double rad = Math.Atan2(Lz, Rz);
                        aState[AnalogMapZ, 0] = (int)((Lz - (Math.Cos(rad) * AnalogFree)) * (AnalogMapZ == 2 ? freeMoveZit : charaMoveZit));
                        aState[AnalogMapZ, 1] = (int)((Rz - (Math.Sin(rad) * AnalogFree)) * (AnalogMapZ == 2 ? freeMoveZit : charaMoveZit));
                        aState[AnalogMapZ, 2] = AnalogMapZ;
                    }
                }

                // キーボード状態チェック
                if (ImageMap.curSceneName == "@フィールド")
                {
                    if (KeyState[(int)Keys.NumPad8]) { dState[38] = true; }
                    if (KeyState[(int)Keys.NumPad6]) { dState[39] = true; }
                    if (KeyState[(int)Keys.NumPad2]) { dState[40] = true; }
                    if (KeyState[(int)Keys.NumPad4]) { dState[37] = true; }
                    if (KeyState[(int)Keys.NumPad7]) { dState[61] = true; } // ←
                    if (KeyState[(int)Keys.NumPad9]) { dState[58] = true; } // ↑
                    if (KeyState[(int)Keys.NumPad3]) { dState[60] = true; } // ↓
                }
                else
                {
                    if (KeyState[(int)Keys.NumPad8]) { dState[58] = true; } // ↑
                    if (KeyState[(int)Keys.NumPad6]) { dState[59] = true; } // →
                    if (KeyState[(int)Keys.NumPad2]) { dState[60] = true; } // ↓
                    if (KeyState[(int)Keys.NumPad4]) { dState[61] = true; } // ←
                }
                if (KeyState[(int)Keys.NumPad0]) { dState[1] = true; } // 左クリック
                if (KeyState[(int)Keys.NumPad5]) { dState[2] = true; } // 右クリック
                if (KeyState[(int)Keys.Divide]) { dState[14] = true; } // 左タブ
                if (KeyState[(int)Keys.Multiply]) { dState[15] = true; } // 右タブ

                if (KeyState[(int)Keys.Decimal]) { dState[17] = true; } // Ctrl
                if (KeyState[(int)Keys.NumPad1]) { dState[17] = true; } // Ctrl

                // 十字キー上下左右 (1:上 / 2:右 / 3:下 / 4:左)
                if (!(lastState[0x3A] || lastState[0x3B] || lastState[0x3C] || lastState[0x3D]))
                {
                    aState[3, 0] = dState[0x3A] ? 1 : dState[0x3B] ? 2 : dState[0x3C] ? 3 : dState[0x3D] ? 4 : 0;
                }
                aState[3, 2] = 8;

                // マウスカーソル移動系の最終値を決定
                for (int i = 1; i < 4; i++)
                {
                    if ((aState[i, 2] != 0) && ((aState[i, 0] != 0) || (aState[i, 1] != 0)))
                    {
                        aState[0, 0] = aState[i, 0];
                        aState[0, 1] = aState[i, 1];
                        aState[0, 2] = aState[i, 2];
                        break;
                    }
                }

                // 現在のカーソル位置を取得
                Point absoluteCurPos;
                w32.GetCursorPos(out absoluteCurPos);

                // ウィンドウ枠内にカーソルがあるか
                bool isMouseInWindow = ScreenCapture.WindowPos.Contains(absoluteCurPos);

                // curPos(クライアント座標) と cur(Windows座標)
                Point clientCurPos = new Point(absoluteCurPos.X - ScreenCapture.WindowPos.Left, absoluteCurPos.Y - ScreenCapture.WindowPos.Top);
                Point targetCurPos = clientCurPos;

                // 移動先確定
                switch (aState[0, 2])
                {
                    case 1:
                        // カーソル移動 (左ボタン押し下げ無効)
                        targetCurPos.X = absoluteCurPos.X + aState[0, 0];
                        targetCurPos.Y = absoluteCurPos.Y + aState[0, 1];
                        break;
                    case 2:
                        // キャラ移動 (座標指定＋左ボタン押し下げ)はフィールドのみ
                        if (ImageMap.curSceneName == "@フィールド")
                        {
                            targetCurPos.X = ScreenCapture.WindowPos.Left + CharaPosition.X + aState[0, 0];
                            targetCurPos.Y = ScreenCapture.WindowPos.Top + CharaPosition.Y + aState[0, 1];
                            isRunning = true;
                        }
                        break;
                    case 8:
                        // 十字キー (左ボタン押し下げ無効)

                        //// 押されなかったことにして次のインターバルに回す
                        //if (bmp == null)
                        //{
                        //    dState[0x3A] = dState[0x3B] = dState[0x3C] = dState[0x3D] = false;
                        //    break;
                        //}
                        targetCurPos = ImageMap.MoveCursor(clientCurPos, aState[0, 0]);

                        // マイナスの値の場合はホイール信号が必要（正負反転）
                        if (targetCurPos.X < 0)
                        {
                            dState[0x0A] = true;
                            targetCurPos.X *= -1; // これ以降、マウスを移動してはいけない
                        }
                        else if (targetCurPos.Y < 0)
                        {
                            dState[0x0B] = true;
                            targetCurPos.Y *= -1; // これ以降、マウスを移動してはいけない
                        }
                        else
                        {
                            targetCurPos.X += ScreenCapture.WindowPos.Left;
                            targetCurPos.Y += ScreenCapture.WindowPos.Top;
                        }
                        break;
                    default:
                        break;
                }

                // 移動中（左ボタン押し下げ）の終了判定
                if (isRunning == true)
                {
                    isRunning = (aState[0, 2] == 2);
                    dState[1] = isRunning;
                }

                // ウィンドウがアクティブでない場合はフリーカーソル移動のみ許可
                bool isWindowActive = ScreenCapture.IsWindowActive;
                if (isWindowActive == false)
                {
                    if (aState[0, 2] == 1)
                    {
                        // ホイール時は入らない (aState[0, 2] == 8)
                        if (targetCurPos != clientCurPos)
                        {
                            WinSendInput.MouseMove(targetCurPos.X, targetCurPos.Y);
                        }
                    }
                    return;
                }

                // この時点でウィンドウがアクティブであることが保障される

                // 移動するべきであればカーソルを移動（ホイールの場合は動かさない）
                bool isMoved = false;
                if (targetCurPos != clientCurPos)
                {
                    // ホイール時は処理してはいけない
                    if (dState[0x0A] == false && dState[0x0B] == false)
                    {
                        WinSendInput.MouseMove(targetCurPos.X, targetCurPos.Y);
                        isMoved = true;
                    }
                }

                // ウィンドウの外にカーソルがある場合は全ての処理を行わない
                if (isMouseInWindow == false)
                {
                    // 0x07 画面キャプチャは取りたい？
                    if (dState[0x07] == true && lastState[0x07] == false)
                    {
                        SnapShot = true;
                        lastState[7] = true;
                    }
                    return;
                }

                // この時点でウィンドウがアクティブでマウスがウィンドウ枠内に存在することが保障される

                for (int i = 0; i < dState.Length; i++)
                {
                    if (lastState[i] != dState[i])
                    {
                        switch (i)
                        {
                            case 0x01: // マウス左ボタン
                            case 0x02: // マウス右ボタン
                            case 0x04: // マウス中ボタン
                            case 0x05: // マウス XBUTTON1
                            case 0x06: // マウス XBUTTON2
                            case 0x0A: // ホイール上 (独自定義)
                            case 0x0B: // ホイール下 (独自定義)
                                if (dState[i] == true)
                                {
                                    WinSendInput.MouseEvent(i, true, (i == 0x0A ? targetCurPos.X : 0));
                                }
                                else
                                {
                                    WinSendInput.MouseEvent(i, false, (i == 0x0B ? targetCurPos.Y : 0));
                                }
                                break;
                            case 0x07: // 画面キャプチャ命令
                                if (dState[i] == true)
                                {
                                    SnapShot = true;
                                }
                                break;
                            case 0x0E: // 左タブ (独自定義)
                            case 0x0F: // 右タブ (独自定義)
                                if (dState[i] == true)
                                {
                                    if (isWindowActive && isMoved == false)
                                    {
                                        // タブ切り替える場所に移動してクリックして戻ってくる
                                        Point tabPos = ImageMap.TabSelect(clientCurPos, i == 0x0F);

                                        if (clientCurPos != tabPos)
                                        {
                                            if (tabPos.X > 0)
                                            {
                                                WinSendInput.MouseMove(ScreenCapture.WindowPos.Left + tabPos.X, ScreenCapture.WindowPos.Top + tabPos.Y);
                                                WinSendInput.MouseEvent(1, true, 0);
                                                WinSendInput.MouseEvent(1, false, 0);
                                            }
                                            else
                                            {
                                                tabPos.X *= -1;
                                                WinSendInput.MouseMove(ScreenCapture.WindowPos.Left + tabPos.X, ScreenCapture.WindowPos.Top + tabPos.Y);
                                                WinSendInput.MouseEvent(1, true, 0);
                                                WinSendInput.MouseEvent(1, false, 0);
                                                // タブエリア外から飛んできた場合は戻る
                                                System.Threading.Thread.Sleep(200);
                                                WinSendInput.MouseMove(ScreenCapture.WindowPos.Left + clientCurPos.X, ScreenCapture.WindowPos.Top + clientCurPos.Y);
                                            }
                                        }
                                    }
                                }
                                break;
                            default:
                                if (USE_KEYS[i] == true)
                                {
                                    if (dState[i] == true)
                                    {
                                        WinSendInput.DirectKey(i, true);
                                    }
                                    else
                                    {
                                        WinSendInput.DirectKey(i, false);
                                        lastState[i] = false;
                                    }
                                }
                                break;
                        }
                    }
                    lastState[i] = dState[i];
                }

            }
            catch (Exception)
            {
                //System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
            finally
            {
                // 排他フラグ下げる
                nowProcessing = false;
            }
        }
    }

    internal static class WinSendInput
    {
        #region Win32API SendInput

        /// <summary>0x0:マウスイベント</summary>
        private const int INPUT_MOUSE = 0;
        /// <summary>0x1:キーボードイベント</summary>
        private const int INPUT_KEYBOARD = 1;
        /// <summary>0x2:ハードウェアイベント</summary>
        private const int INPUT_HARDWARE = 2;

        /// <summary>0x01:マウスを移動する</summary>
        private const int MOUSEEVENTF_MOVE = 0x1;
        /// <summary>0x2:左ボタンを押す</summary>
        private const int MOUSEEVENTF_LEFTDOWN = 0x2;
        /// <summary>0x4:左ボタンを離す</summary>
        private const int MOUSEEVENTF_LEFTUP = 0x4;
        /// <summary>0x08:右ボタンを押す</summary>
        private const int MOUSEEVENTF_RIGHTDOWN = 0x8;
        /// <summary>0x10:右ボタンを離す</summary>
        private const int MOUSEEVENTF_RIGHTUP = 0x10;
        /// <summary>0x20:中央ボタンを押す</summary>
        private const int MOUSEEVENTF_MIDDLEDOWN = 0x20;
        /// <summary>0x40:中央ボタンを離す</summary>
        private const int MOUSEEVENTF_MIDDLEUP = 0x40;
        /// <summary>0x800:ホイールを回転する</summary>
        private const int MOUSEEVENTF_WHEEL = 0x800;
        /// <summary>0x8000:絶対座標指定</summary>
        private const int MOUSEEVENTF_ABSOLUTE = 0x8000;

        /// <summary>0x0:キーを押す</summary>
        private const int KEYEVENTF_KEYDOWN = 0x0;
        /// <summary>0x2:キーを離す</summary>
        private const int KEYEVENTF_KEYUP = 0x2;
        /// <summary>0x1:拡張コード</summary>
        private const int KEYEVENTF_EXTENDEDKEY = 0x1;

        /// <summary>ホイール回転値 (固定:1)</summary>
        private const int WHEEL_DELTA = 64;

        #endregion

        /// <summary>
        /// マウスイベント共通
        /// </summary>
        /// <param name="?"></param>
        internal static void MouseMove(int x, int y)
        {
#if SENDINPUTMOVE
			// SendInput でマウス移動
			// マウス操作実行用のデータ
			const int num = 1;
			INPUT[] inp = new INPUT[num];

			inp[0].type = INPUT_MOUSE;
			inp[0].mi.dwFlags = MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE;
			inp[0].mi.dx = x * (65535 / Screen.PrimaryScreen.Bounds.Width);
			inp[0].mi.dy = y * (65535 / Screen.PrimaryScreen.Bounds.Height);
			inp[0].mi.mouseData = 0;
			inp[0].mi.dwExtraInfo = 0;
			inp[0].mi.time = 0;

			// マウス操作実行
			SendInput(num, ref inp[0], Marshal.SizeOf(inp[0]));
#else
            // SetCursorPos でマウス移動
            w32.SetCursorPos(x, y);
#endif
            return;
        }

        /// <summary>
        /// マウスイベント共通
        /// </summary>
        /// <param name="btn"></param>
        /// <param name="UpDown"></param>
        internal static void MouseEvent(int btn, bool UpDown, int delta)
        {
            // マウス操作実行用のデータ
            const int num = 1;
            w32.INPUT[] inp = new w32.INPUT[num];

            inp[0].type = INPUT_MOUSE;
            inp[0].mi.dwFlags = 0;
            inp[0].mi.mouseData = 0;
            inp[0].mi.dx = 0;
            inp[0].mi.dy = 0;
            inp[0].mi.mouseData = 0;
            inp[0].mi.dwExtraInfo = 0;
            inp[0].mi.time = 0;

            switch (btn)
            {
                case 1:
                    inp[0].mi.dwFlags = UpDown ? MOUSEEVENTF_LEFTDOWN : MOUSEEVENTF_LEFTUP;
                    break;
                case 2:
                    inp[0].mi.dwFlags = UpDown ? MOUSEEVENTF_RIGHTDOWN : MOUSEEVENTF_RIGHTUP;
                    break;
                case 3:
                    inp[0].mi.dwFlags = UpDown ? MOUSEEVENTF_MIDDLEDOWN : MOUSEEVENTF_MIDDLEUP;
                    break;
                case 0x0A:
                    inp[0].mi.dwFlags = MOUSEEVENTF_WHEEL;
                    inp[0].mi.mouseData = WHEEL_DELTA * 1;
                    break;
                case 0x0B:
                    inp[0].mi.dwFlags = MOUSEEVENTF_WHEEL;
                    inp[0].mi.mouseData = WHEEL_DELTA * -1;
                    break;
                default:
                    return;
            }

            // マウス操作実行
            w32.SendInput(num, ref inp[0], System.Runtime.InteropServices.Marshal.SizeOf(inp[0]));

            return;
        }

        /// <summary>
        /// キーコードを指定して処理
        /// </summary>
        /// <param name="UpDown"></param>
        /// <param name="keyCode"></param>
        internal static void DirectKey(int keyCode, bool UpDown)
        {
            // 対応できるのは byte 範囲の仮想キーコードのみ
            if (keyCode < 0 || keyCode > 0xFF)
                return;

            const int num = 1;
            w32.INPUT[] inp = new w32.INPUT[num];
            inp[0].type = INPUT_KEYBOARD;
            inp[0].ki.wVk = (short)keyCode;
            inp[0].ki.wScan = (short)w32.MapVirtualKey(keyCode, 0);
            inp[0].ki.dwFlags = KEYEVENTF_EXTENDEDKEY | (UpDown ? KEYEVENTF_KEYDOWN : KEYEVENTF_KEYUP);
            inp[0].ki.dwExtraInfo = 0;
            inp[0].ki.time = 0;
            w32.SendInput(num, ref inp[0], System.Runtime.InteropServices.Marshal.SizeOf(inp[0]));
            return;
        }
    }
}

