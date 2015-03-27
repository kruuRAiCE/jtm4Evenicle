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
using System.Collections.Generic;
using System.Drawing;
using Common;

namespace JoyToAny
{
    static class ImageMap
    {
        #region シーン情報構造体

        /// <summary>
        /// (画像解析)チェックポイント
        /// </summary>
        public struct Box
        {
            /// 判定矩形メモ
            /// </summary>
            public string Label;

            /// <summary>
            /// 矩形
            /// </summary>
            public Point Point;

            /// <summary>
            /// 否定要素
            /// </summary>
            public bool Not;

            /// <summary>
            /// 必須要素
            /// </summary>
            public bool Must;

            /// <summary>
            /// 特殊状態
            /// </summary>
            public bool Selected;

            /// <summary>
            /// 判定矩形のCRC32値
            /// </summary>
            public string CRC32;
            /// <summary>
        }

        /// <summary>
        /// クリック座標
        /// </summary>
        public class MatchPoint
        {
            /// <summary>
            /// 座標名
            /// </summary>
            public string Label;

            /// <summary>
            /// 座標
            /// </summary>
            public Point point;

            /// <summary>
            /// 座標上と判定する範囲
            /// </summary>
            public Rectangle matchArea;

            /// <summary>
            /// 判定座標リスト
            /// </summary>
            public List<Box> checkPoint = new List<Box>();
        }

        /// <summary>
        /// 座標グループ
        /// </summary>
        public class MatchBox
        {
            /// <summary>
            /// 座標グループ名
            /// </summary>
            public string Label;

            /// <summary>
            /// 座標グループ領域
            /// </summary>
            public Rectangle Rect;

            /// <summary>
            /// 配置タイプ true=横並び false=縦並び
            /// </summary>
            public bool IsYoko;

            /// <summary>
            /// 座標リスト
            /// </summary>
            public List<MatchPoint> ChildBox = new List<MatchPoint>();

            /// <summary>
            /// タブ処理を行うか？ しない場合は負数
            /// (画面内に１つだけ許容される)
            /// </summary>
            public int TabPos = -1;

            /// <summary>
            /// 終端でホイール回すか？
            /// </summary>
            public int WheelArea = 0;
        }

        /// <summary>
        /// シーン情報
        /// </summary>
        public class SceneInfo
        {
            /// <summary>
            /// シーン名
            /// </summary>
            public string Label;

            /// <summary>
            /// 座標リスト
            /// </summary>
            public List<Box> CheckPoint = new List<Box>();

            /// <summary>
            /// 判定座標
            /// </summary>
            public List<MatchBox> ChildBox = new List<MatchBox>();

            /// <summary>
            /// 不明な時に継続するか
            /// </summary>
            public bool MissType;
        }

        #endregion

        /// <summary>
        /// 現在のシーン
        /// </summary>
        public static SceneInfo curScene = null;

        /// <summary>
        /// 現在のシーン名
        /// </summary>
        public static string curSceneName = "";

        /// <summary>
        /// シーン判定情報リスト
        /// </summary>
        public static List<SceneInfo> SceneList = new List<SceneInfo>();

        /// <summary>
        /// 座標グループリスト (解析用)
        /// </summary>
        public static List<MatchBox> GroupList = new List<MatchBox>();

        /// <summary>
        /// タブ切り替えボタン
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="next"></param>
        public static Point TabSelect(Point pos, bool next)
        {
            SceneInfo si = curScene;
            if (si == null)
            {
                return pos;
            }

            Point tabPos = pos;

            // タブ移動
            foreach (MatchBox mb in si.ChildBox)
            {
                // タブ移動するグループがある
                if (mb.TabPos < 0)
                {
                    continue;
                }

                ImageAnalyze ia = lastImageData;
                int selectedIndex = -1;

                // TODO: 現在のタブ状態の取得 (mp.point)
                for (int i = 0; i < mb.ChildBox.Count; i++)
                {
                    MatchPoint mp = mb.ChildBox[i];

                    foreach (Box box in mp.checkPoint)
                    {
                        if (box.Selected)
                        {
                            if (ia.CheckMatch(box.Point, box.CRC32) == true)
                            {
                                selectedIndex = i;
                                break;
                            }
                        }
                    }
                    if (selectedIndex != -1)
                    {
                        break;
                    }
                }

                // TODO: そこから移動先の座標を取得して返す
                if (selectedIndex >= 0)
                {
                    if (next == true)
                    {
                        if (selectedIndex < mb.ChildBox.Count - 1)
                        {
                            tabPos = mb.ChildBox[selectedIndex + 1].point;
                        }
                    }
                    else
                    {
                        if (selectedIndex > 0)
                        {
                            tabPos = mb.ChildBox[selectedIndex - 1].point;
                        }
                    }

                    if (mb.Rect.Contains(pos) == false)
                    {
                        tabPos.X *= -1;
                    }
                }
                else
                {
                    tabPos = mb.ChildBox[0].point;
                }
            }
            return tabPos;
        }

        /// <summary>
        /// 最短距離の計算
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        private static int calcMinDist(Point p, MatchPoint mp)
        {
            Point[] target = new Point[5];
            target[0] = mp.point;
            target[1] = new Point(mp.matchArea.Left, mp.matchArea.Top);
            target[2] = new Point(mp.matchArea.Left, mp.matchArea.Bottom);
            target[3] = new Point(mp.matchArea.Right, mp.matchArea.Top);
            target[4] = new Point(mp.matchArea.Right, mp.matchArea.Bottom);

            double dist = double.MaxValue;
            for (int i = 0; i < target.Length; i++)
            {
                dist = Math.Min(dist, Math.Pow(Math.Pow(p.X - target[i].X, 2) + Math.Pow(p.Y - target[i].Y, 2), 0.5));
            }
            return (int)dist;
        }

        /// <summary>
        /// 十字キー操作
        /// </summary>
        /// <param name="pos">現在のカーソル座標</param>
        /// <param name="Allow">1:上 2:右 3:下 4:左</param>
        /// <returns>移動先座標</returns>
        public static Point MoveCursor(Point pos, int Allow)
        {
            Point targetPos = pos;
            Allow--;

            SceneInfo si = curScene;
            if (si == null)
            {
                return pos;
            }

            ImageAnalyze ia = lastImageData;

            // 現在のカーソル位置をどうするか？
            MatchBox curGroup = null;
            MatchPoint curPoint = null;

            foreach (MatchBox mb in si.ChildBox)
            {
                if (mb.Rect.Contains(pos) == true)
                {
                    curGroup = mb;
                    if (curGroup != null)
                    {
                        foreach (MatchPoint mp in mb.ChildBox)
                        {
                            if (mp.matchArea.Contains(pos))
                            {
                                curPoint = mp;
                                break;
                            }
                        }
                    }
                }
            }

            // 8x8 矩形のバイト列を取得
            byte[] ary2 = new byte[8 * 4 * 8];

            // 判定用
            int minDist = int.MaxValue;
            int wheelOK = 0;

            if (Allow % 2 == 0)
            {
                if (curGroup != null && curGroup.IsYoko == false)
                {
                    // BOXグループ内移動
                    MatchPoint destPos = null;

                    foreach (MatchPoint mp in curGroup.ChildBox)
                    {
                        if (mp != curPoint)
                        {
                            int dist = (pos.Y - mp.point.Y) * (Allow == 0 ? 1 : -1);
                            if (dist > 0)
                            {
                                foreach (Box box in mp.checkPoint)
                                {
                                    if (ia.CheckMatch(box.Point, box.CRC32))
                                    {
                                        if (box.Not == false)
                                        {
                                            if (dist < minDist)
                                            {
                                                destPos = mp;
                                                minDist = dist;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (Box box in mp.checkPoint)
                            {
                                if (box.Selected == true)
                                {
                                    wheelOK = curGroup.WheelArea;
                                }
                            }
                        }
                    }


                    if (minDist != int.MaxValue)
                    {
                        // 移動先あり
                        targetPos = destPos.point;
                    }
                    else if (wheelOK != 0)
                    {
                        // 移動限界だがホイールOK
                        if (Allow == 0)
                        {
                            targetPos.X *= wheelOK * -1;
                        }
                        else
                        {
                            targetPos.Y *= wheelOK * -1;
                        }
                    }
                    else
                    {
                        // 移動先がなくホイールも回せない
                        wheelOK = 0;
                    }
                }

                if (minDist == int.MaxValue && wheelOK == 0)
                {
                    // BOX移動
                    MatchBox destBox = null;

                    foreach (MatchBox mb in si.ChildBox)
                    {
                        if (mb != curGroup)
                        {
                            if (Allow == 0)
                            {
                                int dist = (mb.Rect.Bottom - pos.Y) * -1;
                                if (dist > 0 && dist < minDist)
                                {
                                    bool isHit = false;
                                    foreach (MatchPoint mp in mb.ChildBox)
                                    {
                                        if (ia.CheckMatch(mp.checkPoint[0].Point, mp.checkPoint[0].CRC32))
                                        {
                                            isHit = true;
                                            break;
                                        }

                                    }
                                    if (isHit == true)
                                    {
                                        destBox = mb;
                                        minDist = dist;
                                    }
                                }
                            }
                            else
                            {
                                int distY = (mb.Rect.Top - pos.Y);
                                if (distY > 0 && distY < minDist)
                                {
                                    bool isHit = false;
                                    foreach (MatchPoint mp in mb.ChildBox)
                                    {
                                        if (ia.CheckMatch(mp.checkPoint[0].Point, mp.checkPoint[0].CRC32))
                                        {
                                            isHit = true;
                                            break;
                                        }

                                    }
                                    if (isHit == true)
                                    {
                                        destBox = mb;
                                        minDist = distY;
                                    }
                                }
                            }
                        }
                    }

                    if (destBox != null)
                    {
                        minDist = int.MaxValue;
                        MatchPoint destPos = null;
                        foreach (MatchPoint mp in destBox.ChildBox)
                        {
                            if (destBox.Rect.Contains(mp.point) == true)
                            {
                                int dist = calcMinDist(pos, mp);
                                if (dist < minDist)
                                {
                                    bool isHit = false;
                                    foreach (Box box in mp.checkPoint)
                                    {
                                        if (ia.CheckMatch(box.Point, box.CRC32))
                                        {
                                            if (box.Not == false)
                                            {
                                                isHit = true;
                                                break;
                                            }
                                        }

                                    }
                                    if (isHit == true)
                                    {
                                        destPos = mp;
                                        minDist = dist;
                                    }
                                    if (minDist != int.MaxValue)
                                    {
                                        targetPos = destPos.point;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (curGroup == null)
                        {
                            minDist = int.MaxValue;
                            MatchPoint destPosY = null;
                            foreach (MatchBox mb in si.ChildBox)
                            {
                                foreach (MatchPoint mp in mb.ChildBox)
                                {
                                    if (mb.Rect.Contains(mp.point) == true)
                                    {
                                        int dist = (pos.Y - mp.point.Y) * (Allow == 0 ? 1 : -1);
                                        if (dist > 0)
                                        {
                                            dist = (int)(Math.Pow(pos.Y - mp.point.Y, 2) + Math.Pow(pos.X - mp.point.X, 2));
                                            if (dist < minDist)
                                            {
                                                if (ia.CheckMatch(mp.checkPoint[0].Point, mp.checkPoint[0].CRC32))
                                                {
                                                    destPosY = mp;
                                                    minDist = dist;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if (minDist != int.MaxValue)
                            {
                                targetPos = destPosY.point;
                            }
                        }
                    }
                }
            }
            else
            {
                if (curGroup != null && curGroup.IsYoko == true)
                {
                    // BOXグループ内移動
                    MatchPoint destPos = null;

                    foreach (MatchPoint mp in curGroup.ChildBox)
                    {
                        if (mp != curPoint)
                        {
                            int dist = (pos.X - mp.point.X) * (Allow == 1 ? -1 : 1);
                            if (dist > 0)
                            {
                                if (dist < minDist)
                                {
                                    foreach (Box box in mp.checkPoint)
                                    {
                                        if (ia.CheckMatch(box.Point, box.CRC32))
                                        {
                                            if (box.Not == false)
                                            {
                                                if (dist < minDist)
                                                {
                                                    destPos = mp;
                                                    minDist = dist;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (Box box in mp.checkPoint)
                            {
                                if (box.Selected == true)
                                {
                                    wheelOK = curGroup.WheelArea;
                                }
                            }
                        }
                    }

                    if (minDist != int.MaxValue)
                    {
                        // 移動先あり
                        targetPos = destPos.point;
                    }
                    else if (wheelOK != 0)
                    {
                        // 移動限界だがホイールOK
                        if (Allow == 3)
                        {
                            targetPos.X *= wheelOK * -1;
                        }
                        else
                        {
                            targetPos.Y *= wheelOK * -1;
                        }
                    }
                    else
                    {
                        // 移動先がなくホイールも回せない
                        wheelOK = 0;
                    }

                }

                if (minDist == int.MaxValue && wheelOK == 0)
                {
                    // BOX移動
                    MatchBox destBox = null;

                    foreach (MatchBox mb in si.ChildBox)
                    {
                        if (mb != curGroup)
                        {
                            if (Allow == 1)
                            {
                                int dist = (mb.Rect.Left - pos.X);
                                if (dist > 0 && dist < minDist)
                                {
                                    bool isHit = false;
                                    foreach (MatchPoint mp in mb.ChildBox)
                                    {
                                        if (ia.CheckMatch(mp.checkPoint[0].Point, mp.checkPoint[0].CRC32))
                                        {
                                            isHit = true;
                                            break;
                                        }

                                    }
                                    if (isHit == true)
                                    {
                                        destBox = mb;
                                        minDist = dist;
                                    }
                                }
                            }
                            else
                            {
                                int distX = (mb.Rect.Right - pos.X) * -1;
                                if (distX > 0 && distX < minDist)
                                {
                                    bool isHit = false;
                                    foreach (MatchPoint mp in mb.ChildBox)
                                    {
                                        if (ia.CheckMatch(mp.checkPoint[0].Point, mp.checkPoint[0].CRC32))
                                        {
                                            isHit = true;
                                            break;
                                        }

                                    }
                                    if (isHit == true)
                                    {
                                        destBox = mb;
                                        minDist = distX;
                                    }
                                }
                            }
                        }
                    }

                    if (destBox != null)
                    {
                        minDist = int.MaxValue;
                        MatchPoint destPos = null;
                        foreach (MatchPoint mp in destBox.ChildBox)
                        {
                            if (destBox.Rect.Contains(mp.point) == true)
                            {
                                int dist = calcMinDist(pos, mp);
                                if (dist < minDist)
                                {
                                    bool isHit = false;
                                    foreach (Box box in mp.checkPoint)
                                    {
                                        if (ia.CheckMatch(box.Point, box.CRC32))
                                        {
                                            if (box.Not == false)
                                            {
                                                isHit = true;
                                                break;
                                            }
                                        }

                                    }
                                    if (isHit == true)
                                    {
                                        destPos = mp;
                                        minDist = dist;
                                    }
                                    if (minDist != int.MaxValue)
                                    {
                                        targetPos = destPos.point;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (curGroup == null)
                        {
                            minDist = int.MaxValue;
                            MatchPoint destPosX = null;
                            foreach (MatchBox mb in si.ChildBox)
                            {
                                foreach (MatchPoint mp in mb.ChildBox)
                                {
                                    if (mb.Rect.Contains(mp.point) == true)
                                    {
                                        int dist = (pos.X - mp.point.X) * (Allow == 1 ? -1 : 1);
                                        if (dist > 0)
                                        {
                                            dist = (int)(Math.Pow(pos.Y - mp.point.Y, 2) + Math.Pow(pos.X - mp.point.X, 2));
                                            if (dist < minDist)
                                            {
                                                if (ia.CheckMatch(mp.checkPoint[0].Point, mp.checkPoint[0].CRC32))
                                                {
                                                    destPosX = mp;
                                                    minDist = dist;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if (minDist != int.MaxValue)
                            {
                                targetPos = destPosX.point;
                            }
                        }
                    }
                }
            }

            return targetPos;
        }

        private static ImageAnalyze lastImageData;

        /// <summary>
        /// 画面の解析
        /// </summary>
        internal static string CheckScene(ImageAnalyze ia)
        {
            // バイト列取得
            if (ia == null)
            {
                curScene = null;
                return null;
            }

            lastImageData = ia;

            if (ia.Width != 1280 || ia.Height != 720)
            {
                curScene = null;
                curSceneName = "1280x720じゃない";
                return curSceneName;
            }

            string sceneName = "";

            SceneInfo si = curScene;
            if (si != null)
            {
                // 最後に判定されたものと高い確率で同じはず

                int matchCnt = 0;
                foreach (Box b in si.CheckPoint)
                {
                    if (ia.CheckMatch(b.Point, b.CRC32))
                    {
                        if (b.Not == true)
                        {
                            matchCnt = 0;
                            break;
                        }
                        matchCnt++;
                    }
                    else
                    {
                        if (b.Must == true)
                        {
                            matchCnt = 0;
                            break;
                        }
                    }
                }
                if (matchCnt > 0)
                {
                    sceneName = si.Label;
                }
            }

            if (sceneName == "")
            {
                List<SceneInfo> sceneList = SceneList;
                foreach (SceneInfo k in sceneList)
                {
                    int matchCnt = 0;
                    foreach (Box b in k.CheckPoint)
                    {
                        if (ia.CheckMatch(b.Point, b.CRC32))
                        {
                            if (b.Not == true)
                            {
                                matchCnt = 0;
                                break;
                            }

                            matchCnt++;
                            if (sceneName != "")
                            {
                            }
                            break;
                        }
                        else
                        {
                            if (b.Must == true)
                            {
                                matchCnt = 0;
                                break;
                            }
                        }
                    }
                    if (matchCnt > 0)
                    {
                        curScene = k;
                        if (sceneName != "")
                        {
                            // 複数のシーンがダブった
                        }
                        sceneName += k.Label;
                    }
                }

                if (sceneName == "")
                {
                    // 継続属性確認
                    if (curScene != null && curScene.MissType == true)
                    {
                        sceneName = curScene.Label;
                    }
                    else
                    {
                        curScene = null;
                        sceneName = "不明";
                    }
                }
            }

            curSceneName = sceneName;
            return sceneName;
        }

        /// <summary>
        /// 座標ファイルをロードする
        /// </summary>
        /// <param name="filename">ファイル名</param>
        /// <param name="checkOnly">true の場合は内容チェックのみ</param>
        /// <returns></returns>
        public static string LoadImageMapTxt(string filename, bool checkOnly)
        {
            if (System.IO.File.Exists(filename) == false)
            {
                return string.Format("設定ファイルが見つかりません\n[{0}]", filename);
            }

            Dictionary<string, string> sceneCheck = new Dictionary<string, string>();
            Dictionary<string, MatchBox> boxCheck = new Dictionary<string, MatchBox>();

            SceneInfo lastScene = null;
            MatchBox lastBox = null;

            System.Text.StringBuilder errMsg = new System.Text.StringBuilder();

            List<SceneInfo> sceneList = new List<SceneInfo>();
            List<MatchBox> groupList = new List<MatchBox>();
            try
            {
                // SJISでファイルを開く
                using (System.IO.StreamReader sr = new System.IO.StreamReader(filename, System.Text.Encoding.GetEncoding(932)))
                {
                    int lineNo = 0;
                    while (sr.Peek() >= 0)
                    {
                        lineNo++;

                        string line = sr.ReadLine().TrimEnd();
                        if (line.Length == 0)
                        {
                            // 空行
                            if (checkOnly)
                            {
                                errMsg.AppendLine(string.Format("{0:000}: {1}", lineNo, ""));
                            }
                            continue;
                        }

                        string c = line.Substring(0, 1);
                        string[] items = line.Split(',');

                        if (c == "\t")
                        {
                            // 項目リスト
                            if (lastScene != null)
                            {
                                int x, y;

                                if (items.Length != 4)
                                {
                                    if (checkOnly)
                                    {
                                        errMsg.AppendLine(string.Format("{0:000}: {1}", lineNo, ""));
                                    }
                                    continue;
                                }
                                if (int.TryParse(items[0].Trim(), out x) == false)
                                {
                                    if (checkOnly)
                                    {
                                        errMsg.AppendLine(string.Format("{0:000}: {1}", lineNo, ""));
                                    }
                                    continue;
                                }
                                if (int.TryParse(items[1].Trim(), out y) == false)
                                {
                                    if (checkOnly)
                                    {
                                        errMsg.AppendLine(string.Format("{0:000}: {1}", lineNo, ""));
                                    }
                                    continue;
                                }

                                // x,y,crc,label のはず
                                Box b = new Box();
                                b.Point = new Point(x, y);
                                b.CRC32 = items[2].Trim();
                                b.Label = items[3].Trim();
                                if (b.CRC32.Length > 1)
                                {
                                    if (b.CRC32[0] == '!')
                                    {
                                        b.CRC32 = b.CRC32.Substring(1);
                                        b.Not = true;
                                    }
                                    else if (b.CRC32[0] == '&')
                                    {
                                        b.CRC32 = b.CRC32.Substring(1);
                                        b.Must = true;
                                    }
                                }
                                lastScene.CheckPoint.Add(b);

                                if (checkOnly)
                                {
                                    errMsg.AppendLine(string.Format("{0:000}: {1}", lineNo, ""));
                                }
                            }
                            else if (lastBox != null)
                            {
                                int x, y, l, t, w, h;
                                if ((items.Length < 10) || (items.Length % 3 != 1))
                                {
                                    if (checkOnly)
                                    {
                                        errMsg.AppendLine(string.Format("{0:000}: {1}", lineNo, ""));
                                    }
                                    continue;
                                }
                                if (int.TryParse(items[1].Trim(), out x) == false)
                                {
                                    if (checkOnly)
                                    {
                                        errMsg.AppendLine(string.Format("{0:000}: {1}", lineNo, ""));
                                    }
                                    continue;
                                }
                                if (int.TryParse(items[2].Trim(), out y) == false)
                                {
                                    if (checkOnly)
                                    {
                                        errMsg.AppendLine(string.Format("{0:000}: {1}", lineNo, ""));
                                    }
                                    continue;
                                }
                                if (int.TryParse(items[3].Trim(), out l) == false)
                                {
                                    if (checkOnly)
                                    {
                                        errMsg.AppendLine(string.Format("{0:000}: {1}", lineNo, ""));
                                    }
                                    continue;
                                }
                                if (int.TryParse(items[4].Trim(), out t) == false)
                                {
                                    if (checkOnly)
                                    {
                                        errMsg.AppendLine(string.Format("{0:000}: {1}", lineNo, ""));
                                    }
                                    continue;
                                }
                                if (int.TryParse(items[5].Trim(), out w) == false)
                                {
                                    if (checkOnly)
                                    {
                                        errMsg.AppendLine(string.Format("{0:000}: {1}", lineNo, ""));
                                    }
                                    continue;
                                }
                                if (int.TryParse(items[6].Trim(), out h) == false)
                                {
                                    if (checkOnly)
                                    {
                                        errMsg.AppendLine(string.Format("{0:000}: {1}", lineNo, ""));
                                    }
                                    continue;
                                }

                                // label,x,y,l,t,w,h,(x,y,crc32)+ のはず   
                                MatchPoint mp = new MatchPoint();
                                mp.Label = items[0].TrimStart();
                                mp.point = new Point(x, y);
                                mp.matchArea = new Rectangle(l, t, w - l, h - t);
                                for (int i = 7; i < items.Length; i += 3)
                                {
                                    Box b = new Box();

                                    if (int.TryParse(items[i + 0].Trim(), out x) == false)
                                    {
                                        if (checkOnly)
                                        {
                                            errMsg.AppendLine(string.Format("{0:000}: {1}", lineNo, ""));
                                        }
                                        continue;
                                    }
                                    if (int.TryParse(items[i + 1].Trim(), out y) == false)
                                    {
                                        if (checkOnly)
                                        {
                                            errMsg.AppendLine(string.Format("{0:000}: {1}", lineNo, ""));
                                        }
                                        continue;
                                    }

                                    b.Label = "";
                                    b.Point = new Point(x, y);
                                    b.CRC32 = items[i + 2].Trim();

                                    if (b.CRC32.Length > 1)
                                    {
                                        if (b.CRC32[0] == '!')
                                        {
                                            b.CRC32 = b.CRC32.Substring(1);
                                            b.Not = true; // 存在してはいけない
                                        }
                                        else if (b.CRC32[0] == '&')
                                        {
                                            b.CRC32 = b.CRC32.Substring(1);
                                            b.Must = true; // 存在しなくてはならない
                                        }
                                        else if (b.CRC32[0] == '$')
                                        {
                                            // タブ：有効な場合は「選択されている状態」として扱う
                                            // ホイール：ホイール信号が有効な状態
                                            b.CRC32 = b.CRC32.Substring(1);
                                            b.Selected = true; // 特殊な状態（条件はノーマル[OR]）
                                        }

                                    }

                                    mp.checkPoint.Add(b);

                                }
                                lastBox.ChildBox.Add(mp);

                            }
                            else
                            {
                                // エラー
                                if (checkOnly)
                                {
                                    errMsg.AppendLine(string.Format("{0:000}: {1}", lineNo, ""));
                                }
                            }
                        }

                        else if (c == "@")
                        {
                            // シーン定義 @シーン名, (#BOX名, )+
                            if (items.Length <= 1)
                            {
                                // 
                                if (checkOnly)
                                {
                                    errMsg.AppendLine(string.Format("{0:000}: {1}", lineNo, ""));
                                }
                            }

                            if (sceneCheck.ContainsKey(items[0].Trim()) == true)
                            {
                                // シーン名の重複あり
                                if (checkOnly)
                                {
                                    errMsg.AppendLine(string.Format("{0:000}: {1}", lineNo, ""));
                                }
                            }
                            else
                            {
                                // 後で処理するので仮保存
                                sceneCheck.Add(items[0].Trim(), line);

                                SceneInfo si = new SceneInfo();
                                sceneList.Add(si);

                                si.Label = items[0].Trim();

                                lastScene = si;
                                lastBox = null;

                                if (checkOnly)
                                {
                                    errMsg.AppendLine(string.Format("{0:000}: {1}", lineNo, ""));
                                }
                            }
                        }

                        else if (c == "#")
                        {
                            // BOX定義 #Label, Left,Right,Top,Bottom, IsYoko, TabFlag, WheelFlag
                            if (items.Length != 8)
                            {
                                // 
                                if (checkOnly)
                                {
                                    errMsg.AppendLine(string.Format("{0:000}: {1}", lineNo, ""));
                                }
                            }

                            if (boxCheck.ContainsKey(items[0]) == true)
                            {
                                // BOX名の重複あり
                                if (checkOnly)
                                {
                                    errMsg.AppendLine(string.Format("{0:000}: {1}", lineNo, ""));
                                }
                            }
                            else
                            {
                                int left, top, right, bottom, isYoko, flgTab, flgWheel;

                                if (int.TryParse(items[1].Trim(), out left) == false)
                                {
                                    if (checkOnly)
                                    {
                                        errMsg.AppendLine(string.Format("{0:000}: {1}", lineNo, ""));
                                    }
                                    continue;
                                }
                                if (int.TryParse(items[2].Trim(), out top) == false)
                                {
                                    if (checkOnly)
                                    {
                                        errMsg.AppendLine(string.Format("{0:000}: {1}", lineNo, ""));
                                    }
                                    continue;
                                }
                                if (int.TryParse(items[3].Trim(), out right) == false)
                                {
                                    if (checkOnly)
                                    {
                                        errMsg.AppendLine(string.Format("{0:000}: {1}", lineNo, ""));
                                    }
                                    continue;
                                }
                                if (int.TryParse(items[4].Trim(), out bottom) == false)
                                {
                                    if (checkOnly)
                                    {
                                        errMsg.AppendLine(string.Format("{0:000}: {1}", lineNo, ""));
                                    }
                                    continue;
                                }
                                if (int.TryParse(items[5].Trim(), out isYoko) == false)
                                {
                                    if (checkOnly)
                                    {
                                        errMsg.AppendLine(string.Format("{0:000}: {1}", lineNo, ""));
                                    }
                                    continue;
                                }
                                if (int.TryParse(items[6].Trim(), out flgTab) == false)
                                {
                                    if (checkOnly)
                                    {
                                        errMsg.AppendLine(string.Format("{0:000}: {1}", lineNo, ""));
                                    }
                                    continue;
                                }
                                if (int.TryParse(items[7].Trim(), out flgWheel) == false)
                                {
                                    if (checkOnly)
                                    {
                                        errMsg.AppendLine(string.Format("{0:000}: {1}", lineNo, ""));
                                    }
                                    continue;
                                }

                                MatchBox mb = new MatchBox();
                                boxCheck.Add(items[0].Trim(), mb);
                                groupList.Add(mb);

                                mb.Label = items[0].Trim();
                                mb.IsYoko = (isYoko == 1);
                                mb.Rect = new Rectangle(left, top, right - left, bottom - top);
                                mb.TabPos = (flgTab > 0) ? flgTab : -1;
                                mb.WheelArea = flgWheel;

                                lastScene = null;
                                lastBox = mb;

                                if (checkOnly)
                                {
                                    errMsg.AppendLine(string.Format("{0:000}: {1}", lineNo, ""));
                                }
                            }
                        }
                    }
                }
                // ファイルの読み込み完了

                // 項目整理
                Dictionary<string, bool> checkMatchBox = new Dictionary<string, bool>();
                foreach (SceneInfo si in sceneList)
                {
                    if (sceneCheck.ContainsKey(si.Label) == false)
                    {
                        // ロジック上おかしい
                        if (checkOnly)
                        {
                            errMsg.AppendLine(string.Format("{0}: {1}", "POST", ""));
                        }
                        throw new Exception();
                    }

                    string[] items = sceneCheck[si.Label].Split(',');
                    for (var i = 1; i < items.Length; i++)
                    {
                        if (boxCheck.ContainsKey(items[i].Trim()) == false)
                        {
                            int flg;
                            if (int.TryParse(items[i], out flg))
                            {
                                if (flg == 1)
                                {
                                    if (checkOnly)
                                    {
                                        errMsg.AppendLine(string.Format("{0}: {1}", "POST", ""));
                                    }
                                    si.MissType = true;
                                    continue;
                                }
                            }

                            // 座標グループが見つからない
                            if (checkOnly)
                            {
                                errMsg.AppendLine(string.Format("{0}: {1}", "POST", ""));
                            }
                            continue;
                        }

                        if (checkMatchBox.ContainsKey(items[i].Trim()) == false)
                        {
                            checkMatchBox.Add(items[i].Trim(), true);
                        }

                        MatchBox mb = boxCheck[items[i].Trim()];
                        si.ChildBox.Add(mb);
                    }

                    foreach (string key in sceneCheck.Keys)
                    {
                        if (checkMatchBox.ContainsKey(key) == false)
                        {
                            // 使われてない BOX がある
                            if (checkOnly)
                            {
                                errMsg.AppendLine(string.Format("{0}: {1}", "POST", ""));
                            }
                        }
                    }
                }
            }
            finally
            {

            }
            if (checkOnly == false)
            {
                GroupList = groupList;
                SceneList = sceneList;
            }
            return errMsg.ToString();
        }

    }

    internal class ImageAnalyze
    {
        private byte[] imageData;
        public int Width
        {
            get
            {
                return width;
            }
            private set
            {
                width = value;
            }
        }
        private int width = 0;
        public int Height
        {
            get
            {
                return height;
            }
            private set
            {
                height = value;
            }
        }
        private int height;
        private int depth;
        private int stride;

        public ImageAnalyze(Bitmap bmp)
        {
            imageData = Util.ImageToBytes(bmp);
            Width = bmp.Width;
            Height = bmp.Height;
            depth = 4;
            stride = Width * depth;
        }

        public bool CheckMatch(Point p, string crc32)
        {
            if (p.X > Width - 8 || p.Y > Height - 2)
            {
                return false;
            }

            byte[] ary2 = new byte[8 * 4 * 2];
            for (int i = 0; i < 2; i++)
            {
                Array.Copy(imageData, (p.Y + i) * stride + (p.X * depth), ary2, ((8 * depth) * i), 8 * depth);
            }
            string crc = Util.bytesToCrc32(ary2);

            return crc == crc32;
        }

        public string GetCrc32(int x, int y)
        {
            if (x < 0 || x > Width - 8 || y < 0 || y > Height - 2)
            {
                return "";
            }

            byte[] ary2 = new byte[8 * depth * 2];
            for (int i = 0; i < 2; i++)
            {
                Array.Copy(imageData, (y + i) * stride + (x * depth), ary2, ((8 * depth) * i), 8 * depth);
            }

            return Util.bytesToCrc32(ary2);
        }
    }
}

