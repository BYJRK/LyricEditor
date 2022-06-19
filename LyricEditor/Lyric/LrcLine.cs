using System;

namespace LyricEditor.Lyric
{
    public class LrcLine : IComparable<LrcLine>
    {
        // https://en.wikipedia.org/wiki/LRC_(file_format)

        public TimeSpan? LrcTime { get; set; }

        public static bool IsShort = false;

        public string LrcTimeText
        {
            get
            {
                return LrcTime.HasValue ? LrcHelper.ToShortString(LrcTime.Value, IsShort) : string.Empty;
            }
            set
            {
                if (LrcHelper.TryParseTimeSpan(value, out TimeSpan ts))
                    LrcTime = ts;
                else
                    LrcTime = null;
            }
        }

        public string LrcText { get; set; }

        public LrcLine(double time, string text)
        {
            LrcTime = new TimeSpan(0, 0, 0, 0, (int)(time * 1000));
            LrcText = text;
        }
        public LrcLine(TimeSpan? time, string text)
        {
            LrcTime = time;
            LrcText = text;
        }
        public LrcLine(TimeSpan? time)
            : this(time, string.Empty)
        {

        }
        public LrcLine(LrcLine lrcLine)
        {
            LrcTime = lrcLine.LrcTime;
            LrcText = lrcLine.LrcText;
        }
        public LrcLine(string line)
            : this(Parse(line))
        {

        }
        public LrcLine()
        {
            LrcTime = null;
            LrcText = string.Empty;
        }

        /// <summary>
        /// 将单行的歌词文本进行解析
        /// </summary>
        public static LrcLine Parse(string line)
        {
            // 歌曲信息|[al:album]      | Time = null, Content = Info
            // 空白行  |                | Time = null, Content = empty
            // 正常歌词|[00:00.000]Info | Time = time, Content = content
            // 空白歌词|[00:00.000]     | Time = time, Content = empty
            // 多行歌词|[00:00.000][00:01.000]Info

            // 判断是否为空白行
            if (string.IsNullOrWhiteSpace(line))
            {
                return Empty;
            }
            // 这里不考虑多行歌词的情况
            if (CheckMultiLine(line)) throw new FormatException();

            // 此时只能为正常歌词
            var slices = line.TrimStart().TrimStart('[').Split(']');
            if (slices.Length != 2) throw new FormatException();

            // 如果方括号中的内容无法转化为时间，则认为是歌曲信息
            if (!LrcHelper.TryParseTimeSpan(slices[0], out TimeSpan time))
            {
                return new LrcLine(null, slices[0]);
            }

            // 正常歌词和空白歌词不需要进行额外区分
            return new LrcLine(time, slices[1]);

        }
        public static bool TryParse(string line, out LrcLine lrcLine)
        {
            try
            {
                lrcLine = Parse(line);
                return true;
            }
            catch
            {
                lrcLine = Empty;
                return false;
            }
        }

        public static readonly LrcLine Empty = new LrcLine();

        /// <summary>
        /// 判断是否为多行歌词（通过检查是否有超过一个左方括号）
        /// </summary>
        public static bool CheckMultiLine(string line)
        {
            // 指的是从左侧第二个字符开始找，如果仍然能够找到“[”，则认为包含超过一个时间框
            if (line.TrimStart().IndexOf('[', 1) != -1) return true;
            else return false;
        }
        /// <summary>
        /// 将歌词行以形如 [mm:ss.mss]歌词文本 输出
        /// </summary>
        public override string ToString()
        {
            // 歌曲信息|[al:album]      | Time = null, Content = Info
            // 空白行  |                | Time = null, Content = empty
            // 正常歌词|[00:00.000]Info | Time = time, Content = content
            // 空白歌词|[00:00.000]     | Time = time, Content = empty

            // 正常歌词或空白歌词
            if (LrcTime.HasValue)
            {
                return $"[{LrcHelper.ToShortString(LrcTime.Value, IsShort)}]{LrcText}";
            }
            // 歌曲信息
            else if (!string.IsNullOrWhiteSpace(LrcText))
            {
                return $"[{LrcText}]";
            }
            // 空白行
            else
            {
                return string.Empty;
            }
        }
        public int CompareTo(LrcLine other)
        {
            // 保证歌曲信息永远在最开头
            if (!LrcTime.HasValue) return -1;
            if (!other.LrcTime.HasValue) return 1;
            // 正常的歌词按照时间顺序进行排列
            return LrcTime.Value.CompareTo(other.LrcTime.Value);
        }
    }
}
