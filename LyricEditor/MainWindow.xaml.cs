using LyricEditor.Lyric;
using LyricEditor.UserControls;
using LyricEditor.Utils;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Forms = System.Windows.Forms;

namespace LyricEditor
{
    public enum LrcPanelType
    {
        LrcLinePanel,
        LrcTextPanel
    }

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            LrcLinePanel = (LrcLineView)LrcPanelContainer.Content;
            LrcTextPanel = new LrcTextView();

            Timer = new DispatcherTimer();
            Timer.Tick += new EventHandler(Timer_Tick);
            Timer.Interval = new TimeSpan(0, 0, 0, 0, 20);
            Timer.Start();
        }

        #region 成员变量

        private LrcPanelType CurrentLrcPanel = LrcPanelType.LrcLinePanel;

        /// <summary>
        /// 表示播放器是否正在播放
        /// </summary>
        private bool isPlaying = false;

        private LrcLineView LrcLinePanel;
        private LrcTextView LrcTextPanel;

        public TimeSpan ShortTimeShift { get; private set; } = new TimeSpan(0, 0, 2);
        public TimeSpan LongTimeShift { get; private set; } = new TimeSpan(0, 0, 5);

        private string fileName;

        #endregion

        #region 计时器

        DispatcherTimer Timer;

        /// <summary>
        /// 每个计时器时刻，更新时间轴上的全部信息
        /// </summary>
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!IsMediaAvailable)
                return;

            var current = MediaPlayer.Position;
            CurrentTimeText.Text = $"{current.Minutes:00}:{current.Seconds:00}";

            TimeBackground.Value =
                MediaPlayer.Position.TotalSeconds
                / MediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            CurrentLrcText.Text = LrcManager.Instance.GetNearestLrc(MediaPlayer.Position);
        }

        #endregion

        #region 媒体播放器

        private bool IsMediaAvailable
        {
            get
            {
                if (MediaPlayer.Source is null)
                    return false;
                else
                    return MediaPlayer.HasAudio && MediaPlayer.NaturalDuration.HasTimeSpan;
            }
        }

        private void Play()
        {
            if (!IsMediaAvailable)
                return;

            MediaPlayer.Play();

            PlayButton.Tag = true;

            isPlaying = true;
        }

        private void Pause()
        {
            if (!IsMediaAvailable)
                return;

            MediaPlayer.Pause();

            PlayButton.Tag = false;

            isPlaying = false;
        }

        private void Stop()
        {
            if (!IsMediaAvailable)
                return;

            MediaPlayer.Stop();

            PlayButton.Tag = false;

            isPlaying = false;
        }

        #endregion

        #region 内部方法

        private void UpdateLrcView()
        {
            LrcLinePanel.UpdateLrcPanel();
            LrcTextPanel.Text = LrcManager.Instance.ToString();
        }

        private void ImportMedia(string filename)
        {
            try
            {
                MediaPlayer.Source = new Uri(filename);
                MediaPlayer.Stop();
                var title = TagLibHelper.GetTitle(filename);
                if (string.IsNullOrWhiteSpace(title))
                    title = Path.GetFileNameWithoutExtension(filename);
                Title = $"歌词编辑器 {title}";
                Cover.Source = TagLibHelper.GetAlbumArt(filename);
            }
            catch
            {
                Cover.Source = ResourceHelper.GetIcon("disc.png");
            }
        }

        #endregion

        #region 菜单按钮

        /// <summary>
        /// 界面读取，用于初始化
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            #region 读取配置

            // 退出时自动缓存
            AutoSaveTemp.IsChecked = bool.Parse(ConfigurationManager.AppSettings["AutoSaveTemp"]);
            // 导出 UTF-8
            ExportUTF8.IsChecked = bool.Parse(ConfigurationManager.AppSettings["ExportUTF8"]);
            // 时间取近似值
            LrcLinePanel.ApproxTime =
                LrcLine.IsShort =
                ApproxTime.IsChecked =
                    bool.Parse(ConfigurationManager.AppSettings["ApproxTime"]);
            // 时间偏差（改变 Text 会触发 TextChanged 事件，下同）
            TimeOffset.Text = ConfigurationManager.AppSettings["TimeOffset"];
            // 快进快退
            ShortShift.Text = ConfigurationManager.AppSettings["ShortTimeShift"];
            LongShift.Text = ConfigurationManager.AppSettings["LongTimeShift"];

            #endregion

            // 打开缓存文件
            if (AutoSaveTemp.IsChecked && File.Exists(FileHelper.TempFileName))
            {
                LrcManager.Instance.LoadFromFile(FileHelper.TempFileName);
                UpdateLrcView();
            }
        }

        /// <summary>
        /// 程序退出，关闭计时器，修改配置文件
        /// </summary>
        private void Window_Closed(object sender, EventArgs e)
        {
            Timer.Stop();

            // 保存配置文件
            Configuration cfa = ConfigurationManager.OpenExeConfiguration(
                ConfigurationUserLevel.None
            );

            cfa.AppSettings.Settings["AutoSaveTemp"].Value = AutoSaveTemp.IsChecked.ToString();
            cfa.AppSettings.Settings["ExportUTF8"].Value = ExportUTF8.IsChecked.ToString();
            cfa.AppSettings.Settings["ApproxTime"].Value = LrcLinePanel.ApproxTime.ToString();
            cfa.AppSettings.Settings["TimeOffset"].Value = (
                -LrcLinePanel.TimeOffset.TotalMilliseconds
            ).ToString();
            cfa.AppSettings.Settings["ShortTimeShift"].Value =
                ShortTimeShift.TotalSeconds.ToString();
            cfa.AppSettings.Settings["LongTimeShift"].Value = LongTimeShift.TotalSeconds.ToString();

            cfa.Save();

            // 保存缓存
            if (AutoSaveTemp.IsChecked)
            {
                Encoding encoding = ExportUTF8.IsChecked ? Encoding.UTF8 : Encoding.Default;
                File.WriteAllText(FileHelper.TempFileName, LrcManager.Instance.ToString(), encoding);
            }
            else if (File.Exists(FileHelper.TempFileName))
            {
                File.Delete(FileHelper.TempFileName);
            }
        }

        /// <summary>
        /// 导入音频文件
        /// </summary>
        private void ImportMedia_Click(object sender, RoutedEventArgs e)
        {
            Forms.OpenFileDialog ofd = new Forms.OpenFileDialog();
            ofd.Filter = "媒体文件|*.mp3;*.wav;*.3gp;*.mp4;*.avi;*.wmv;*.wma;*.aac|所有文件|*.*";

            if (ofd.ShowDialog() == Forms.DialogResult.OK)
            {
                ImportMedia(ofd.FileName);
                fileName = ofd.FileName;
            }
        }

        /// <summary>
        /// 导入歌词文件
        /// </summary>
        private void ImportLyric_Click(object sender, RoutedEventArgs e)
        {
            Forms.OpenFileDialog ofd = new Forms.OpenFileDialog();
            ofd.Filter = "歌词文件|*.lrc;*.txt|所有文件|*.*";

            if (ofd.ShowDialog() == Forms.DialogResult.OK)
            {
                LrcManager.Instance.LoadFromFile(ofd.FileName);
                UpdateLrcView();
                fileName = ofd.FileName;
            }
        }

        /// <summary>
        /// 将歌词保存为文本文件
        /// </summary>
        private void ExportLyric_Click(object sender, RoutedEventArgs e)
        {
            Forms.SaveFileDialog ofd = new Forms.SaveFileDialog();
            ofd.Filter = "歌词文件|*.lrc|文本文件|*.txt|所有文件|*.*";

            if (!string.IsNullOrEmpty(fileName))
            {
                ofd.FileName = Path.GetFileNameWithoutExtension(fileName);
            }

            if (ofd.ShowDialog() == Forms.DialogResult.OK)
            {
                Encoding encoding = ExportUTF8.IsChecked ? Encoding.UTF8 : Encoding.Default;
                File.WriteAllText(ofd.FileName, LrcManager.Instance.ToString(), encoding);
            }
        }

        /// <summary>
        /// 从剪贴板粘贴歌词文本
        /// </summary>
        private void ImportLyricFromClipboard_Click(object sender, RoutedEventArgs e)
        {
            LrcManager.Instance.LoadFromText(Clipboard.GetText());
            UpdateLrcView();
        }

        /// <summary>
        /// 将歌词文本复制到剪贴板
        /// </summary>
        private void ExportLyricToClipboard_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(LrcManager.Instance.ToString());
        }

        /// <summary>
        /// 配置选项发生变化
        /// </summary>
        private void Settings_Checked(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;
            switch (item.Name)
            {
                case "ApproxTime":
                    LrcLinePanel.ApproxTime = item.IsChecked;
                    LrcLine.IsShort = item.IsChecked;
                    if (LrcPanelContainer.Content is LrcLineView view)
                    {
                        view.LrcLinePanel.Items.Refresh();
                    }
                    break;
            }
        }

        /// <summary>
        /// 打开媒体文件后，更新时间轴上的总时间
        /// </summary>
        private void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            var totalTime = MediaPlayer.NaturalDuration.TimeSpan;
            TotalTimeText.Text = $"{totalTime.Minutes:00}:{totalTime.Seconds:00}";
            CurrentTimeText.Text = "00:00";
            Pause();
        }

        /// <summary>
        /// 处理窗口的文件拖入事件
        /// </summary>
        public void Window_Drop(object sender, DragEventArgs e)
        {
            string[] filePath = ((string[])e.Data.GetData(DataFormats.FileDrop));

            foreach (var file in filePath)
            {
                string ext = Path.GetExtension(file).ToLower();
                if (FileHelper.MediaExtensions.Contains(ext))
                {
                    ImportMedia(file);
                    fileName = file;
                }
                else if (FileHelper.LyricExtensions.Contains(ext))
                {
                    LrcManager.Instance.LoadFromFile(file);
                    UpdateLrcView();
                    fileName = file;
                }
            }
        }

        public void Window_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Link;
            else
                e.Effects = DragDropEffects.None;
        }

        /// <summary>
        /// 关闭按钮
        /// </summary>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TimeOffset_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (LrcLinePanel is null)
                return;
            if (int.TryParse(TimeOffset.Text, out int offset))
            {
                LrcLinePanel.TimeOffset = new TimeSpan(0, 0, 0, 0, -offset);
            }
        }

        private void TimeShift_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (LrcLinePanel is null)
                return;

            TextBox box = sender as TextBox;
            if (int.TryParse(box.Text, out int value))
            {
                switch (box.Name)
                {
                    case "ShortShift":
                        ShortTimeShift = new TimeSpan(0, 0, value);
                        break;

                    case "LongShift":
                        LongTimeShift = new TimeSpan(0, 0, value);
                        break;
                }
            }
        }

        /// <summary>
        /// 重置所有歌词行的时间
        /// </summary>
        private void ResetAllTime_Click(object sender, RoutedEventArgs e)
        {
            LrcLinePanel.ResetAllTime();
        }

        /// <summary>
        /// 平移所有歌词行的时间
        /// </summary>
        private void ShiftAllTime_Click(object sender, RoutedEventArgs e)
        {
            string str = InputBox.Show(this, "输入", "请输入时间偏移量(s)：");
            if (double.TryParse(str, out double offset))
            {
                LrcLinePanel.ShiftAllTime(new TimeSpan(0, 0, 0, 0, (int)(offset * 1000)));
            }
        }

        #endregion

        #region 工具按钮

        /// <summary>
        /// 切换面板
        /// </summary>
        private void SwitchLrcPanel_Click(object sender, RoutedEventArgs e)
        {
            switch (CurrentLrcPanel)
            {
                // 切换回纯文本
                case LrcPanelType.LrcLinePanel:
                    UpdateLrcView();
                    LrcPanelContainer.Content = LrcTextPanel;
                    CurrentLrcPanel = LrcPanelType.LrcTextPanel;
                    // 切换到文本编辑模式时，按钮旋转180度，且相关按钮不可用
                    ((Image)((Button)sender).Content).LayoutTransform = new RotateTransform(180);
                    ToolsForLrcLineOnly.Visibility = Visibility.Collapsed;
                    FlagButton.Visibility = Visibility.Hidden;
                    ClearAllTime.IsEnabled = true;
                    SortTime.IsEnabled = false;
                    break;

                // 切换回歌词行
                case LrcPanelType.LrcTextPanel:
                    // 在回到歌词行模式前，要检查当前文本能否进行正确转换
                    if (!LrcManager.Instance.LoadFromText(LrcTextPanel.Text))
                        return;
                    UpdateLrcView();
                    LrcPanelContainer.Content = LrcLinePanel;
                    CurrentLrcPanel = LrcPanelType.LrcLinePanel;
                    // 切换到文本编辑模式时，按钮旋转角度复原，且相关按钮可用
                    ((Image)((Button)sender).Content).LayoutTransform = new RotateTransform(0);
                    ToolsForLrcLineOnly.Visibility = Visibility.Visible;
                    FlagButton.Visibility = Visibility.Visible;
                    ClearAllTime.IsEnabled = false;
                    SortTime.IsEnabled = true;
                    break;
            }
        }

        /// <summary>
        /// 播放按钮
        /// </summary>
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isPlaying)
            {
                Play();
            }
            else
            {
                Pause();
            }
        }

        /// <summary>
        /// 停止按钮
        /// </summary>
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        /// <summary>
        /// 快进快退
        /// </summary>
        private void TimeShift_Click(object sender, RoutedEventArgs e)
        {
            if (!IsMediaAvailable)
                return;

            switch (((Button)sender).Name)
            {
                case "ShortShiftLeft":
                    MediaPlayer.Position -= ShortTimeShift;
                    break;
                case "ShortShiftRight":
                    MediaPlayer.Position += ShortTimeShift;
                    break;
                case "LongShiftLeft":
                    MediaPlayer.Position -= LongTimeShift;
                    break;
                case "LongShiftRight":
                    MediaPlayer.Position += LongTimeShift;
                    break;
            }
        }

        /// <summary>
        /// 时间轴点击
        /// </summary>
        private void TimeClickBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsMediaAvailable)
                return;

            double current = e.GetPosition(TimeClickBar).X;
            double percent = current / TimeClickBar.ActualWidth;
            TimeBackground.Value = percent;

            MediaPlayer.Position = new TimeSpan(
                0,
                0,
                0,
                0,
                (int)(MediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds * percent)
            );
        }

        /// <summary>
        /// 撤销
        /// </summary>
        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            switch (CurrentLrcPanel)
            {
                case LrcPanelType.LrcLinePanel:
                    LrcLinePanel.Undo();
                    break;

                case LrcPanelType.LrcTextPanel:
                    LrcTextPanel.LrcTextPanel.Undo();
                    break;
            }
        }

        /// <summary>
        /// 重做
        /// </summary>
        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            switch (CurrentLrcPanel)
            {
                case LrcPanelType.LrcLinePanel:
                    LrcLinePanel.Redo();
                    break;

                case LrcPanelType.LrcTextPanel:
                    LrcTextPanel.LrcTextPanel.Redo();
                    break;
            }
        }

        /// <summary>
        /// 将媒体播放位置应用到当前歌词行
        /// </summary>
        private void SetTime_Click(object sender, RoutedEventArgs e)
        {
            if (!IsMediaAvailable)
                return;
            if (CurrentLrcPanel != LrcPanelType.LrcLinePanel)
                return;

            LrcLinePanel.SetCurrentLineTime(MediaPlayer.Position);
        }

        /// <summary>
        /// 添加新行
        /// </summary>
        private void AddNewLine_Click(object sender, RoutedEventArgs e)
        {
            LrcLinePanel.AddNewLine(MediaPlayer.Position);
        }

        /// <summary>
        /// 删除行
        /// </summary>
        private void DeleteLine_Click(object sender, RoutedEventArgs e)
        {
            LrcLinePanel.DeleteLine();
        }

        /// <summary>
        /// 上移一行
        /// </summary>
        private void MoveUp_Click(object sender, RoutedEventArgs e)
        {
            LrcLinePanel.MoveUp();
        }

        /// <summary>
        /// 下移一行
        /// </summary>
        private void MoveDown_Click(object sender, RoutedEventArgs e)
        {
            LrcLinePanel.MoveDown();
        }

        /// <summary>
        /// 清空所有时间标记
        /// </summary>
        private void ClearAllTime_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentLrcPanel != LrcPanelType.LrcTextPanel)
                return;
            LrcTextPanel.ClearAllTime();
        }

        /// <summary>
        /// 强制排序
        /// </summary>
        private void SortTime_Click(object sender, RoutedEventArgs e)
        {
            LrcManager.Instance.Sort();
            LrcLinePanel.UpdateLrcPanel();
        }

        /// <summary>
        /// 清空全部内容
        /// </summary>
        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            switch (CurrentLrcPanel)
            {
                case LrcPanelType.LrcLinePanel:
                    LrcManager.Instance.Clear();
                    LrcLinePanel.UpdateLrcPanel();
                    break;

                case LrcPanelType.LrcTextPanel:
                    LrcTextPanel.Clear();
                    break;
            }
        }

        /// <summary>
        /// 软件信息
        /// </summary>
        private void Info_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show(Properties.Resources.Info, "相关信息", MessageBoxButton.OKCancel);
            if (res == MessageBoxResult.OK)
                Process.Start("https://zhuanlan.zhihu.com/p/32588196");
        }

        #endregion

        #region 快捷键

        private void SetTimeShortcut_Executed(object sender, ExecutedRoutedEventArgs e) =>
            SetTime_Click(this, e);

        private void HelpShortcut_Executed(object sender, ExecutedRoutedEventArgs e) =>
            Info_Click(this, e);

        private void PlayShortcut_Executed(object sender, ExecutedRoutedEventArgs e) =>
            PlayButton_Click(this, e);

        private void UndoShortcut_Executed(object sender, ExecutedRoutedEventArgs e) =>
            Undo_Click(this, e);

        private void RedoShortcut_Executed(object sender, ExecutedRoutedEventArgs e) =>
            Redo_Click(this, e);

        private void InsertShortcut_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (CurrentLrcPanel == LrcPanelType.LrcLinePanel)
            {
                AddNewLine_Click(this, null);
            }
        }

        #endregion
    }
}
