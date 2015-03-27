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
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Common;


namespace JoyToAny
{
    public partial class JoyToMouse : Form
    {
#if WITHMANAGER
        /// <summary></summary>
        frmImageMapEditor frmEdit;
#endif

        /// <summary></summary>
        frmErrorForm frmError;

        private void frmJoyPadConfig_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible == true && chkWithStop.Checked == true)
            {
                JoyPad.AllStop = true;
            }
            else
            {
                JoyPad.AllStop = false;
            }
        }

        private void chkWithStop_CheckedChanged(object sender, EventArgs e)
        {
            JoyPad.AllStop = chkWithStop.Checked;
        }

        public ComboBox[] buttons;
        public ComboBox[] ana_xys;
        public Label[] button_l;
        public Label[] ana_xy_l;

        // 
        // 0: 何もしない
        // 1: 左クリック
        // 2: 右クリック
        // 3: 中クリック
        // 4: ↑キー
        // 5: →キー
        // 6: ↓キー
        // 7: ←キー
        // 8: 左タブ
        // 9: 右タブ
        // 10: Ctrl
        // 91: 画面PNG保存
        // 

        public class MyComboBoxItem
        {
            public string DisplayValue
            {
                get
                {
                    return displayValue;
                }
                private set
                {
                    displayValue = value;
                }
            }
            private string displayValue = "";
            public byte Value
            {
                get
                {
                    return value;
                }
                private set
                {
                    this.value = value;
                }
            }
            private byte value = 0;

            public MyComboBoxItem(string key, byte value)
            {
                this.DisplayValue = key;
                this.Value = value;
            }
        }

        List<MyComboBoxItem> DigitalOptions = new List<MyComboBoxItem>();
        List<MyComboBoxItem> AnalogOptions = new List<MyComboBoxItem>();
        List<MyComboBoxItem> CaptureMethodOptions = new List<MyComboBoxItem>();

        private string configPath;

        private void loadConfig()
        {
            bool isSuccess = false;
            if (File.Exists(configPath) == true)
            {
                isSuccess = true;
                try
                {
                    // SJISでファイルを開く
                    using (System.IO.StreamReader sr = new System.IO.StreamReader(configPath, System.Text.Encoding.GetEncoding(932)))
                    {
                        int lineNo = 0;
                        byte num;
                        while (sr.Peek() >= 0)
                        {
                            lineNo++;

                            if (byte.TryParse(sr.ReadLine().Trim(), out num))
                            {
                                if (lineNo <= 32)
                                {
                                    JoyToAny.Properties.Settings.Default["cmb" + (lineNo).ToString()] = num;
                                }
                                else
                                {
                                    switch (lineNo)
                                    {
                                        case 33:
                                            JoyToAny.Properties.Settings.Default["cmbXY1"] = num;
                                            break;
                                        case 34:
                                            JoyToAny.Properties.Settings.Default["cmbXY2"] = num;
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("ジョイパッド設定ファイルの読み込みに失敗しました\nデフォルトでロードします");
                    isSuccess = false;
                }
                if (isSuccess == false)
                {
                    try
                    {
                        File.Delete(configPath);
                    }
                    catch
                    {

                    }
                }
            }
            int j1;
            for (int i1 = 0; i1 < JoyPad.ButtonPress.Length; i1++)
            {
                j1 = i1 + 1;
                buttons[i1].SelectedValue = (byte)JoyToAny.Properties.Settings.Default["cmb" + j1.ToString()];
                if (buttons[i1].SelectedValue == null)
                {
                    buttons[i1].SelectedIndex = 0;
                }
            }
            int j2;
            for (int i2 = 0; i2 < ana_xys.Length; i2++)
            {
                j2 = i2 + 1;
                ana_xys[i2].SelectedValue = (byte)JoyToAny.Properties.Settings.Default["cmbXY" + j2.ToString()];
                if (ana_xys[i2].SelectedValue == null)
                {
                    ana_xys[i2].SelectedIndex = 0;
                }

            }

            cmbCaptureMethod.SelectedIndex = 0;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(configPath))
                {
                    for (int i = 0; i < buttons.Length; i++)
                    {
                        sw.WriteLine(buttons[i].SelectedValue.ToString());
                    }
                    for (int i = 0; i < ana_xys.Length; i++)
                    {
                        sw.WriteLine(ana_xys[i].SelectedValue.ToString());
                    }
                }
            }
            catch
            {
                MessageBox.Show("ジョイパッド設定ファイルが書き出せませんでした");
                return;
            }

            MessageBox.Show(string.Format("保存しました\n{0}", configPath));
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("初期値に戻します", "確認", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                JoyToAny.Properties.Settings.Default.Reset();
                int j1;
                for (int i1 = 0; i1 < JoyPad.ButtonPress.Length; i1++)
                {
                    j1 = i1 + 1;
                    buttons[i1].SelectedValue = (byte)JoyToAny.Properties.Settings.Default["cmb" + j1.ToString()];
                    if (buttons[i1].SelectedValue == null)
                    {
                        buttons[i1].SelectedIndex = 0;
                    }
                }
                int j2;
                for (int i2 = 0; i2 < ana_xys.Length; i2++)
                {
                    j2 = i2 + 1;
                    ana_xys[i2].SelectedValue = (byte)JoyToAny.Properties.Settings.Default["cmbXY" + j2.ToString()];
                    if (ana_xys[i2].SelectedValue == null)
                    {
                        ana_xys[i2].SelectedIndex = 0;
                    }
                }
            }
        }

        public JoyToMouse()
        {
            InitializeComponent();

#if USEHOTKEY
            LowLevelHotKey.KeyDown += new EventHandler<LowLevelHotKey.KeybordCaptureEventArgs>(GlobalKeybordCapture_KeyDown);
#endif

#if WITHMANAGER
            frmEdit = new frmImageMapEditor();
#endif

            lblSysInfo.Text = string.Format("");

            frmError = new frmErrorForm("");

            configPath = string.Format(@"{0}\PadConfig.txt", Program.ExecutePath);

            buttons = new ComboBox[] { cmb1, cmb2, cmb3, cmb4, cmb5, cmb6, cmb7, cmb8, cmb9, cmb10, cmb11, cmb12, cmb13, cmb14, cmb15, cmb16, cmb17, cmb18, cmb19, cmb20, cmb21, cmb22, cmb23, cmb24, cmb25, cmb26, cmb27, cmb28, cmb29, cmb30, cmb31, cmb32 };
            ana_xys = new ComboBox[] { cmbXY1, cmbXY2 };

            button_l = new Label[] { label1, label2, label3, label4, label5, label6, label7, label8, label9, label10, label11, label12, label13, label14, label15, label16, label17, label18, label19, label20, label21, label22, label23, label24, label25, label26, label27, label28, label29, label30, label31, label32 };
            ana_xy_l = new Label[] { labelXY1, labelXY2 };

            DigitalOptions.Add(new MyComboBoxItem("何もしない", 0));
            DigitalOptions.Add(new MyComboBoxItem("左クリック", 1));
            DigitalOptions.Add(new MyComboBoxItem("右クリック", 2));
            DigitalOptions.Add(new MyComboBoxItem("[A] AUTOのオンオフ", 65));
            DigitalOptions.Add(new MyComboBoxItem("[S] SKIPのオンオフ", 83));
            DigitalOptions.Add(new MyComboBoxItem("[ESC] コンフィグ画面", 27));
            DigitalOptions.Add(new MyComboBoxItem("[Home] バックシーン", 36));
            DigitalOptions.Add(new MyComboBoxItem("[PageUp] バックログ", 33));
            DigitalOptions.Add(new MyComboBoxItem("[Ctrl] スキップ", 17));
            DigitalOptions.Add(new MyComboBoxItem("ホイール↑", 10));
            DigitalOptions.Add(new MyComboBoxItem("ホイール↓", 11));
            DigitalOptions.Add(new MyComboBoxItem("左タブ", 14));
            DigitalOptions.Add(new MyComboBoxItem("右タブ", 15));
            DigitalOptions.Add(new MyComboBoxItem("↑ 選択移動", 58));
            DigitalOptions.Add(new MyComboBoxItem("→ 選択移動", 59));
            DigitalOptions.Add(new MyComboBoxItem("↓ 選択移動", 60));
            DigitalOptions.Add(new MyComboBoxItem("← 選択移動", 61));
            DigitalOptions.Add(new MyComboBoxItem("画面キャプチャ", 7));

            AnalogOptions.Add(new MyComboBoxItem("何もしない", 0));
            AnalogOptions.Add(new MyComboBoxItem("マウス移動", 1));
            AnalogOptions.Add(new MyComboBoxItem("アスタ移動", 2));

            CaptureMethodOptions.Add(new MyComboBoxItem("BitBlt", 2));
            CaptureMethodOptions.Add(new MyComboBoxItem("PrintWindow", 1));
        }


        private void timerUpdate_Tick(object sender, EventArgs e)
        {
            if (this.Visible == false)
                return;

            if (this.Visible == true)
            {
                bool Lana = false;
                bool Rana = false;
                for (int i = 0; i < JoyPad.ButtonPress.Length; i++)
                {
                    switch (i)
                    {
                        case 24:
                        case 25:
                            if (JoyPad.ButtonPress[i])
                            {
                                Lana = true;
                            }
                            break;
                        case 26:
                        case 27:
                            if (JoyPad.ButtonPress[i])
                            {
                                Rana = true;
                            }
                            break;
                        default:
                            if (JoyPad.ButtonPress[i])
                            {
                                if (button_l[i].BackColor != Color.Red)
                                {
                                    button_l[i].BorderStyle = BorderStyle.FixedSingle;
                                    button_l[i].BackColor = Color.Red;
                                }
                            }
                            else
                            {
                                if (button_l[i].BackColor != SystemColors.Control)
                                {
                                    button_l[i].BackColor = SystemColors.Control;
                                }
                            }
                            break;
                    }
                }
                if (Lana)
                {
                    if (ana_xy_l[0].BackColor != Color.Red)
                    {
                        ana_xy_l[0].BorderStyle = BorderStyle.FixedSingle;
                        ana_xy_l[0].BackColor = Color.Red;
                    }
                }
                else
                {
                    if (ana_xy_l[0].BackColor != SystemColors.Control)
                    {
                        ana_xy_l[0].BackColor = SystemColors.Control;
                    }
                }
                if (Rana)
                {
                    if (ana_xy_l[1].BackColor != Color.Red)
                    {
                        ana_xy_l[1].BorderStyle = BorderStyle.FixedSingle;
                        ana_xy_l[1].BackColor = Color.Red;
                    }
                }
                else
                {
                    if (ana_xy_l[1].BackColor != SystemColors.Control)
                    {
                        ana_xy_l[1].BackColor = SystemColors.Control;
                    }
                }

            }
        }

        private DateTime lastKown = DateTime.MaxValue;
        private int snapCount = 0;

        /// <summary>
        /// 画面解析タイマー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timerCapture_Tick(object sender, EventArgs e)
        {
            // 同時実行はひとつだけ
            if (timer1_sync == true)
            {
                //System.Diagnostics.Debug.WriteLine(string.Format("{0}: Skip Main_LOOP", DateTime.Now.ToString("HH:mm:ss")));
                return;
            }
            timer1_sync = true;
            try
            {
                // チェック
                Bitmap bmp = ScreenCapture.GetBitmap();
                if (bmp != null)
                {
                    ImageAnalyze ia = new ImageAnalyze(bmp);

                    DateTime now = DateTime.Now;

                    txtPosName.Text = ImageMap.CheckScene(ia);

                    // 画損を保存する条件を判定
                    bool nextSnap = JoyPad.SnapShot == true;

                    if (chkAutoSave.Checked == true)
                    {
                        if (txtPosName.Text == "不明")
                        {
                            TimeSpan span = now - lastKown;
                            if (span.TotalSeconds > 5f * (snapCount + 1))
                            {
                                if (snapCount < 3)
                                {
                                    nextSnap = true;
                                }
                                lastKown = now;
                            }
                        }
                        else
                        {
                            lastKown = now;
                            snapCount = 0;
                        }
                    }


                    bool freeNG = false;
                    try
                    {
#if WITHMANAGER
                        if (frmEdit != null && nextSnap == true)
                        {
                            if (nextSnap)
                            {
                                frmEdit.saveNextSnap = true;
                                JoyPad.SnapShot = false;
                                snapCount++;
                            }
                            // 処理が渡った場合は true が返る
                            freeNG = frmEdit.SetImage(bmp, ImageMap.curScene);
                        }
#else
                        // 画像キャプチャ指示

                        // TODO: フラグのセットをイベントハンドラによるコールバックにするべき
                        if (JoyPad.SnapShot == true)
                        {
                            // ドライブの空き容量確認 (32MB以下なら保存しない)
                            DriveInfo di = new DriveInfo(Path.GetPathRoot(Program.ImageFileDirectory));
                            if (di.TotalFreeSpace <= 32 * 1024 * 1024)
                            {
                                return;
                            }
                            // ファイル名生成
                            string fileNameRule = "yyyy年MM月dd日HH時mm分ss秒";
                            string saveFile = string.Format(@"{0}\{1}.png", Program.ImageFileDirectory, DateTime.Now.ToString(fileNameRule));
                            // 既に同名のファイルがあれば何もしない
                            if (File.Exists(saveFile))
                            {
                                return;
                            }
                            try
                            {
                                // PNG形式で保存
                                bmp.Save(saveFile, System.Drawing.Imaging.ImageFormat.Png);
                            }
                            catch
                            {
                            }
                        }
#endif
                    }
                    finally
                    {
                        // 必ずフラグ下ろす
                        JoyPad.SnapShot = false;

                        // キャプチャ画像を委譲してなければ後始末
                        if (freeNG == false)
                        {
                            bmp.Dispose();
                        }
                    }
                }
            }
            finally
            {
                timer1_sync = false;
            }

        }
        private bool timer1_sync = false;

        private void cmb_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (sender is ComboBox)
            {
                ComboBox cmb = (ComboBox)sender;
                byte selectedValue = (byte)cmb.SelectedValue;
                if (cmb.Tag is int)
                {
                    int id = (int)cmb.Tag;
                    if (id < 0 || id >= 34)
                    {
                        return;
                    }
                    switch (id)
                    {
                        case 32:
                            JoyPad.AnalogMapL = selectedValue;
                            break;
                        case 33:
                            JoyPad.AnalogMapR = selectedValue;
                            break;
                        default:
                            if (id < JoyPad.BtnMap.Length)
                            {
                                JoyPad.BtnMap[id] = selectedValue;
                            }
                            break;
                    }
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSelect_Click(object sender, EventArgs e)
        {
            JoyPad.DI_Connect(this, ((string)cmbSelect.SelectedItem));
            JoyPad.Start(10);
        }

        /// <summary>
        /// 設定ウィンドウをキャプチャ保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSnapShot_Click(object sender, EventArgs e)
        {
            w32.RECT w32Rect = new w32.RECT();
            w32.GetWindowRect(this.Handle, ref w32Rect);
            Rectangle rect = new Rectangle(w32Rect.Left, w32Rect.Top, w32Rect.Right - w32Rect.Left, w32Rect.Bottom - w32Rect.Top);

            using (Bitmap bmp = new Bitmap(rect.Width, rect.Height))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    IntPtr hDC = g.GetHdc();
                    w32.PrintWindow(this.Handle, hDC, 0);
                    g.ReleaseHdc(hDC);
                }
                string saveFile = string.Format(@"{0}\ジョイパッド設定画面キャプチャ.png", Program.ExecutePath);
                if (System.IO.File.Exists(saveFile))
                {
                    try
                    {
                        System.IO.File.Delete(saveFile);
                    }
                    catch
                    {
                        MessageBox.Show("キャプチャ画像が保存できませんでした");
                        return;
                    }
                }
                try
                {
                    // PNG形式で保存
                    bmp.Save(saveFile, System.Drawing.Imaging.ImageFormat.Png);
                    MessageBox.Show(string.Format("この画面のキャプチャを保存しました\n{0}", saveFile));
                }
                catch
                {
                    MessageBox.Show("キャプチャ画像が保存できませんでした");
                }

            }
        }

        /// <summary>
        ///  コントロールパネルを開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnJoyPadCpl_Click(object sender, EventArgs e)
        {
            JoyPad.DI_OpenSetting((string)cmbSelect.SelectedItem);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbCaptureInterval_SelectedIndexChanged(object sender, EventArgs e)
        {
            int interval = int.Parse((string)cmbCaptureInterval.SelectedItem);
            ScreenCapture.Start("", "EvenicleTrial", interval);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmJoyPadConfig_Load(object sender, EventArgs e)
        {
            JoyPad.USE_KEYS[0x01] = true; // LButton
            JoyPad.USE_KEYS[0x02] = true; // RButton
            JoyPad.USE_KEYS[0x0E] = true; // 左タブ
            JoyPad.USE_KEYS[0x0F] = true; // 右タブ
            JoyPad.USE_KEYS[0x07] = true; // キャプチャ
            JoyPad.USE_KEYS[0x0A] = true; // ホイール↑
            JoyPad.USE_KEYS[0x0B] = true; // ホイール↓
            JoyPad.USE_KEYS[0x11] = true; // Ctrl
            JoyPad.USE_KEYS[(byte)Keys.A] = true;
            JoyPad.USE_KEYS[(byte)Keys.S] = true;
            JoyPad.USE_KEYS[(byte)Keys.Escape] = true;
            JoyPad.USE_KEYS[(byte)Keys.Home] = true;
            JoyPad.USE_KEYS[(byte)Keys.PageUp] = true;

            JoyPad.CharaMoveMax = 10;
            JoyPad.FreeMoveMax = 100;
            JoyPad.AnalogFree = 10000;
            JoyPad.CharaPosition.X = 475;
            JoyPad.CharaPosition.Y = 340;

            for (int i = 0; i < JoyPad.ButtonPress.Length; i++)
            {
                buttons[i].Tag = i;
                buttons[i].DataSource = DigitalOptions.ToArray();
                buttons[i].DisplayMember = "DisplayValue";
                buttons[i].ValueMember = "Value";
                buttons[i].SelectedIndexChanged += new System.EventHandler(cmb_SelectedIndexChanged);
            }
            for (int i = 0; i < ana_xys.Length; i++)
            {
                ana_xys[i].Tag = 32 + i;
                ana_xys[i].DataSource = AnalogOptions.ToArray();
                ana_xys[i].DisplayMember = "DisplayValue";
                ana_xys[i].ValueMember = "Value";
                ana_xys[i].SelectedIndexChanged += new System.EventHandler(cmb_SelectedIndexChanged);
            }

            cmbCaptureMethod.DataSource = CaptureMethodOptions.ToArray();
            cmbCaptureMethod.DisplayMember = "DisplayValue";
            cmbCaptureMethod.ValueMember = "Value";
            cmbCaptureMethod.SelectedIndexChanged += new System.EventHandler(cmbCaptureMethod_SelectedIndexChanged);

            loadConfig();

            // ジョイスティック一覧を取得してドロップダウンにセット
            cmbSelect.Items.Clear();
            foreach (string name in JoyPad.DI_GetDevice())
            {
                cmbSelect.Items.Add(name);

            }
            // ジョイスティックが一つだけなら自動接続
            if (cmbSelect.Items.Count > 0)
            {
                cmbSelect.SelectedIndex = 0;
                if (cmbSelect.Items.Count == 1)
                {
                    btnSelect_Click(btnSelect, new EventArgs());
                }
            }

            // 自動キャプチャ開始
            cmbCaptureInterval.SelectedIndex = 1;

        }

        /// <summary>
        /// 座標管理画面表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void label35_DoubleClick(object sender, EventArgs e)
        {
#if WITHMANAGER
            if (frmEdit != null)
            {
                frmEdit.Show();
            }
#endif
        }

        /// <summary>
        /// 常に最前面に表示の切り替え
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkAlwaysOnTop_CheckedChanged(object sender, EventArgs e)
        {
            this.TopMost = chkAlwaysOnTop.Checked;
        }

        /// <summary>
        /// トレイアイコンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (this.Visible == false)
            {
                this.Show();
            }
        }

        /// <summary>
        /// 最小化＆トレイアイコン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmJoyToMouse_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                //this.Hide();
                notifyIcon1.Visible = true;
            }
        }

        private void cmbCaptureMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbCaptureMethod.SelectedValue is MyComboBoxItem)
            {
                ScreenCapture.CaptureMethod = ((MyComboBoxItem)cmbCaptureMethod.SelectedValue).Value;
            }
        }

#if USEHOTKEY

        private byte[] KeyState = new Byte[256];

        /// <summary>
        /// グローバルホットキーを自前で行うハンドラ
        /// ※デバッグ時に VisualStudioホスティングプロセス ではフックされないので注意
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GlobalKeybordCapture_KeyDown(object sender, LowLevelHotKey.KeybordCaptureEventArgs e)
        {
            w32.GetKeyboardState(KeyState);

            //byte v1 = KeyState[(int)Keys.LControlKey]; // 取り逃し有り
            //int v2 = w32.GetKeyState((int)Keys.LControlKey); // 取り逃し無し！
            //Trace.WriteLine(string.Format(@"VK_LControl={0}, VK_RControl={1}, VK_LShift={2}, VK_RShift={3}", KeyState[(int)Keys.LControlKey] & 128, KeyState[(int)Keys.RControlKey] & 128, KeyState[(int)Keys.LShiftKey] & 128, KeyState[(int)Keys.RShiftKey] & 128));
            //Trace.WriteLine(string.Format(@"VK_LControl={0}, VK_RControl={1}, VK_LShift={2}, VK_RShift={3}", w32.GetKeyState((int)Keys.LControlKey) & 128, w32.GetKeyState((int)Keys.RControlKey) & 128, w32.GetKeyState((int)Keys.LShiftKey) & 128, w32.GetKeyState((int)Keys.RShiftKey) & 128));

            switch (e.KeyCode)
            {
                case (int)Keys.Snapshot:
                    // PrintScreen
                    Trace.WriteLine("HotKey[ PrintScreen ]");
                    // 本来の PrintScreen をキャンセル
                    e.Cancel = true;
                    break;
                case (int)Keys.T:
                    if ((w32.GetKeyState((int)Keys.LControlKey) & 128) == 128 && (w32.GetKeyState((int)Keys.LMenu) & 128) == 128)
                    {
                        Trace.WriteLine("HotKey[ Ctrl + Alt + T ]");
                        // Ctrl + Alt + T
                    }
                    break;
                case (int)Keys.Space:
                    if ((w32.GetKeyState((int)Keys.LWin) & 128) == 128)
                    {
                        Trace.WriteLine("HotKey[ Win + Space ]");
                        // Win + Space
                    }
                    break;
            }
        }

#endif


    }
}
