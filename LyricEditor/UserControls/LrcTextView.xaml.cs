using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;

namespace LyricEditor.UserControls
{
    /// <summary>
    /// TextEditorView.xaml 的交互逻辑
    /// </summary>
    public partial class LrcTextView : UserControl
    {
        public LrcTextView()
        {
            InitializeComponent();

            Clear();
        }

        /// <summary>
        /// 清空呈现歌词文本的文本框
        /// </summary>
        public void Clear()
        {
            LrcTextPanel.Clear();
        }
        /// <summary>
        /// 清除所有时间标记
        /// </summary>
        public void ClearAllTime()
        {
            Text = Regex.Replace(Text, @"\[\d+:\d+.\d+\]", "");
        }

        /// <summary>
        /// 获取或设置呈现歌词文本的文本框内容
        /// </summary>
        public string Text
        {
            get => LrcTextPanel.Text;
            set => LrcTextPanel.Text = value;
        }

        private void ReplaceButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 不使用正则表达式
                if (!UsingRegex.IsChecked.Value)
                {
                    Text = Regex.Replace(Text, Regex.Escape(PatternText.Text), ReplaceText.Text);
                }
                else
                {
                    Text = Regex.Replace(Text, PatternText.Text, ReplaceText.Text);
                }
            }
            catch
            {
                // 正则表达式出现问题，不报错，不产生任何效果
            }
        }
    }
}
