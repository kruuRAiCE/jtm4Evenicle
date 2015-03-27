using System;
using System.Windows.Forms;

namespace JoyToAny
{
    static class Program
    {
        /// <summary>
        /// アプリ実行ディレクトリ
        /// </summary>
        internal static string ExecutePath;

        /// <summary>
        /// キャプチャ画像を保存するディレクトリ
        /// </summary>
        internal static string ImageFileDirectory;

        /// <summary>
        /// イメージマップファイルのパス
        /// </summary>
        internal static string ImageMapFilePath;

        /// <summary>
        /// 
        /// </summary>
        [STAThread]
        static void Main()
        {
            // 各種定数の設定
            ExecutePath = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
            ImageFileDirectory = string.Format(@"{0}\Screenshot", ExecutePath);
            ImageMapFilePath = string.Format(@"{0}\ImageMap.txt", ExecutePath);

            // 複数起動チェック
            if (System.Diagnostics.Process.GetProcessesByName(System.Diagnostics.Process.GetCurrentProcess().ProcessName).Length > 1)
            {
                MessageBox.Show("既に起動しています");
                return;
            }

            // ImageMap ファイルを読み込み
            string errLog = ImageMap.LoadImageMapTxt(ImageMapFilePath, false);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Form frm = (errLog != "") ? (Form)new frmErrorForm(errLog) : (Form)new JoyToMouse();
            Application.Run(frm);
        }
    }
}
