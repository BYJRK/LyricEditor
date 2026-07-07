using System;
using NAudio.Wave;
using SoundTouch.Net.NAudioSupport;

namespace LyricEditor.Utils;

/// <summary>
/// 基于 NAudio + SoundTouch 的音频播放器，替代 MediaElement。
/// 播放链：AudioFileReader → SoundTouchWaveStream（变速不变调）→ WaveOutEvent。
/// </summary>
public sealed class AudioPlayer : IDisposable
{
    private AudioFileReader _reader;
    private SoundTouchWaveStream _soundTouch;
    private WaveOutEvent _waveOut;

    private float _volume = 0.7f;
    private double _tempo = 1.0;

    /// <summary>
    /// 播放自然到达末尾时触发（手动停止不触发）
    /// </summary>
    public event EventHandler PlaybackEnded;

    /// <summary>
    /// 是否已成功加载音频文件
    /// </summary>
    public bool IsLoaded => _reader != null;

    /// <summary>
    /// 音频总时长
    /// </summary>
    public TimeSpan TotalTime => _reader?.TotalTime ?? TimeSpan.Zero;

    /// <summary>
    /// 当前播放位置，设置时自动钳制在有效范围内并清空变速缓冲
    /// </summary>
    public TimeSpan Position
    {
        get => _reader?.CurrentTime ?? TimeSpan.Zero;
        set
        {
            if (!IsLoaded)
                return;

            if (value < TimeSpan.Zero)
                value = TimeSpan.Zero;
            else if (value > _reader.TotalTime)
                value = _reader.TotalTime;

            _reader.CurrentTime = value;
            // 清空 SoundTouch 内部缓冲，避免跳转后播放残留音频
            _soundTouch.Flush();
        }
    }

    /// <summary>
    /// 音量（0.0 ~ 1.0）
    /// </summary>
    public float Volume
    {
        get => _volume;
        set
        {
            _volume = value;
            if (_reader != null)
                _reader.Volume = value;
        }
    }

    /// <summary>
    /// 播放速度（1.0 为原速），变速不变调
    /// </summary>
    public double Tempo
    {
        get => _tempo;
        set
        {
            _tempo = value;
            if (_soundTouch != null)
                _soundTouch.Tempo = value;
        }
    }

    /// <summary>
    /// 加载音频文件，加载完成后即可读取 <see cref="TotalTime"/>
    /// </summary>
    public void Load(string filename)
    {
        Close();

        try
        {
            _reader = new AudioFileReader(filename) { Volume = _volume };
            _soundTouch = new SoundTouchWaveStream(_reader) { Tempo = _tempo };
            _waveOut = new WaveOutEvent { DesiredLatency = 100 };
            _waveOut.Init(_soundTouch);
            _waveOut.PlaybackStopped += OnPlaybackStopped;
        }
        catch
        {
            Close();
            throw;
        }
    }

    public void Play()
    {
        if (!IsLoaded)
            return;
        _waveOut.Play();
    }

    public void Pause()
    {
        if (!IsLoaded)
            return;
        if (_waveOut.PlaybackState == PlaybackState.Playing)
            _waveOut.Pause();
    }

    /// <summary>
    /// 停止播放并将进度归零
    /// </summary>
    public void Stop()
    {
        if (!IsLoaded)
            return;
        _waveOut.Stop();
        Position = TimeSpan.Zero;
    }

    private void OnPlaybackStopped(object sender, StoppedEventArgs e)
    {
        // 只有自然播放到末尾才复位并通知外部；手动 Stop 也会进入此回调，需要区分
        if (IsLoaded && _reader.Position >= _reader.Length)
        {
            Position = TimeSpan.Zero;
            PlaybackEnded?.Invoke(this, EventArgs.Empty);
        }
    }

    private void Close()
    {
        if (_waveOut != null)
        {
            _waveOut.PlaybackStopped -= OnPlaybackStopped;
            _waveOut.Dispose();
            _waveOut = null;
        }
        // SoundTouchWaveStream 释放时会连带释放内部的 AudioFileReader
        _soundTouch?.Dispose();
        _soundTouch = null;
        _reader = null;
    }

    public void Dispose() => Close();
}
