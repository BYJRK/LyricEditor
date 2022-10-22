using LyricEditor.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace LyricEditor.Lyric
{
    public class LrcManager
    {
        public static LrcManager Instance { get; } = new LrcManager();

        public List<LrcLine> LrcList { get; private set; } = new List<LrcLine>();

        public int Count => LrcList.Count;

        public void Clear()
        {
            AddHistory(-1);
            LrcList.Clear();
        }

        public void LoadFromFile(string filename)
        {
            var encoding = FileHelper.GetEncoding(filename);
            var text = File.ReadAllText(filename, encoding);
            LoadFromText(text);
        }

        public bool LoadFromText(string text)
        {
            // 不论导入成功与否，均清空当前的显示
            Clear();

            // 导入的内容为空
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var lines = Regex.Split(text, @"\r?\n");

            // 查找形如 [00:00.000] 的时间标记
            var reTimeMark = new Regex(@"\[\d+\:\d+\.\d+\]");
            // 查找形如 [al:album] 的歌词信息
            var reLrcInfo = new Regex(@"\[\w+\:.+\]");
            // 查找纯歌词文本
            var reLyric = new Regex(@"(?<=\])[^\]]+$");

            // 文本中不包含时间信息
            if (!reTimeMark.IsMatch(text))
            {
                foreach (var line in lines)
                {
                    // 即便是不包含时间信息的歌词文本，也可能出现歌词信息
                    if (reLrcInfo.IsMatch(line))
                    {
                        LrcList.Add(new LrcLine(null, line.Trim('[', ']')));
                    }
                    // 否则将会为当前歌词行添加空白的时间标记，即便当前行是空行
                    else
                        LrcList.Add(new LrcLine(0, line));
                }
            }
            // 文本中包含时间信息
            else
            {
                // 如果在解析过程中发现存在单行的多时间标记的情况，会在最后进行排序
                bool multiLrc = false;

                int lineNumber = 1;
                try
                {
                    foreach (var line in lines)
                    {
                        // 在确认文本中包含时间标记的情况下，会忽略所有空行
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            lineNumber++;
                            continue;
                        }

                        var matches = reTimeMark.Matches(line);
                        // 出现了类似 [00:00.000][00:01.000] 的包含多个时间信息的歌词行
                        if (matches.Count > 1)
                        {
                            var lrc = reLyric.Match(line).ToString();
                            foreach (var match in matches)
                            {
                                LrcList.Add(
                                    new LrcLine(
                                        LrcHelper.ParseTimeSpan(match.ToString().Trim('[', ']')),
                                        lrc
                                    )
                                );
                            }

                            multiLrc = true;
                        }
                        // 常规的单行歌词 [00:00.000]
                        else if (matches.Count == 1)
                        {
                            LrcList.Add(LrcLine.Parse(line));
                        }
                        // 说明这是一个歌词信息行
                        else if (reLrcInfo.IsMatch(line))
                        {
                            LrcList.Add(
                                new LrcLine(null, reLrcInfo.Match(line).ToString().Trim('[', ']'))
                            );
                        }
                        // 说明正常的歌词里面出现了一个不是空行，却没有时间标记的内容，则添加空时间标记
                        else
                        {
                            LrcList.Add(new LrcLine(TimeSpan.Zero, line));
                        }
                        lineNumber++;
                    }
                    // 如果出现单行出现多个歌词信息的情况，所以进行排序
                    if (multiLrc)
                        LrcList = LrcList.OrderBy(x => x.LrcTime).ToList();
                }
                catch (Exception e)
                {
                    MessageBox.Show($"歌词文本第{lineNumber}行存在格式问题，请在检查后重试。");
                    LrcList.Clear();
                    return false;
                }
            }
            return true;
        }

        public void Sort()
        {
            AddHistory(-1);
            LrcList = LrcList.OrderBy(x => x.LrcTime).ToList();
        }

        public LrcManager()
        {
            // 创建一个空的历史纪录，用来撤销到导入歌词前
            AddHistory(-1);
        }

        #region 历史记录

        /// <summary>
        /// 存储历史记录（用的是最笨的方法真是抱歉）
        /// </summary>
        private List<History> HistoryList = new List<History>();

        /// <summary>
        /// 历史记录指针
        /// </summary>
        private int HistoryPointer = 0;

        /// <summary>
        /// 最大历史记录数量
        /// </summary>
        public int MaxHistoryCount { get; set; } = 20;

        /// <summary>
        /// 历史记录的数量
        /// </summary>
        private int HistoryCount => HistoryList.Count;

        private bool CanUndo => HistoryPointer > 0;
        private bool CanRedo => HistoryPointer < HistoryCount - 1;

        /// <summary>
        /// 将当前的歌词列表添加到历史纪录中，并移动历史记录指针
        /// </summary>
        public void AddHistory(int index = -1)
        {
            // 如果可以重做，说明历史记录中存在需要被清理的内容
            if (CanRedo)
                HistoryList.RemoveRange(HistoryPointer + 1, HistoryCount - 1 - HistoryPointer);
            HistoryList.Add(new History(LrcList, index));
            HistoryPointer++;
        }

        public void AddHistory(ListView list)
        {
            AddHistory(list.SelectedIndex);
        }

        /// <summary>
        /// 撤销
        /// </summary>
        public void Undo(ListView list)
        {
            // 返回 -2 的意思是撤销失败，因为 ListView 的 SelectedIndex 最小为 -1
            if (!CanUndo)
                return;
            var h = HistoryList[--HistoryPointer];
            LrcList = h.LrcList;
            UpdateLrcList(list, h.SelectedIndex);
        }

        /// <summary>
        /// 重做
        /// </summary>
        public void Redo(ListView list)
        {
            if (!CanRedo)
                return;
            var h = HistoryList[++HistoryPointer];
            LrcList = h.LrcList;
            UpdateLrcList(list, h.SelectedIndex);
        }

        #endregion

        /// <summary>
        /// 重设所有时间
        /// </summary>
        public void ResetAllTime(ListView list)
        {
            AddHistory(list);
            foreach (var line in LrcList)
            {
                if (line.LrcTime is null)
                    continue;
                line.LrcTime = TimeSpan.Zero;
            }
            UpdateLrcList(list);
        }

        /// <summary>
        /// 整体时间平移
        /// </summary>
        public void ShiftAllTime(ListView list, TimeSpan offset)
        {
            AddHistory(list);
            foreach (var line in LrcList)
            {
                if (line.LrcTime is null)
                    continue;
                line.LrcTime += offset;
                if (line.LrcTime < TimeSpan.Zero)
                    line.LrcTime = TimeSpan.Zero;
            }
            UpdateLrcList(list);
        }

        /// <summary>
        /// 添加新行
        /// </summary>
        public void AddNewLine(ListView list, TimeSpan time)
        {
            int index = list.SelectedIndex;
            AddHistory(index);
            LrcList.Insert(index + 1, new LrcLine(time));
            UpdateLrcList(list, index + 1);
        }

        /// <summary>
        /// 删除行
        /// </summary>
        public void DeleteLine(ListView list)
        {
            int index = list.SelectedIndex;
            if (index < 0)
                return;
            AddHistory(index);
            LrcList.RemoveAt(index);
            // 如果删除的是最后一行，则选中上一行；否则保持不变
            if (index >= Count)
                index--;
            UpdateLrcList(list, index);
        }

        /// <summary>
        /// 上移一行
        /// </summary>
        public void MoveUp(ListView list)
        {
            int index = list.SelectedIndex;
            // 未选中，或选择最上面一行
            if (index <= 0)
                return;
            AddHistory(index);
            var temp = LrcList[index];
            LrcList[index] = LrcList[index - 1];
            LrcList[index - 1] = temp;
            UpdateLrcList(list, index - 1);
        }

        /// <summary>
        /// 下移一行
        /// </summary>
        public void MoveDown(ListView list)
        {
            int index = list.SelectedIndex;
            // 未选中，或选择最下面一行
            if (index < 0 || index == LrcList.Count - 1)
                return;
            AddHistory(index);
            var temp = LrcList[index];
            LrcList[index] = LrcList[index + 1];
            LrcList[index + 1] = temp;
            UpdateLrcList(list, index + 1);
        }

        /// <summary>
        /// 根据歌词列表更新列表项的显示
        /// </summary>
        public void UpdateLrcList(ListView list)
        {
            list.Items.Clear();

            foreach (var line in LrcList)
            {
                list.Items.Add(line);
            }

            list.Items.Refresh();
        }

        /// <summary>
        /// 根据歌词列表更新列表项的显示，并选中指定一项
        /// </summary>
        public void UpdateLrcList(ListView list, int index)
        {
            UpdateLrcList(list);
            list.SelectedIndex = index;
            list.ScrollIntoView(list.SelectedItem);
        }

        /// <summary>
        /// 获取当前时间对应的歌词
        /// </summary>
        public string GetNearestLrc(TimeSpan time)
        {
            var line = LrcList
                .Where(x => x.LrcTime != null && x.LrcTime <= time)
                .OrderByDescending(x => x.LrcTime)
                .FirstOrDefault();

            return line != null ? line.LrcText : string.Empty;
        }

        /// <summary>
        /// 返回能够用于写 lrc 文件的文本
        /// </summary>
        public override string ToString()
        {
            return string.Join(Environment.NewLine, LrcList.Select(x => x.ToString()));
        }
    }

    public class History
    {
        public List<LrcLine> LrcList { get; private set; }
        public int SelectedIndex { get; private set; }

        public History(List<LrcLine> list, int index)
        {
            LrcList = new List<LrcLine>(list);
            SelectedIndex = index;
        }
    }
}
