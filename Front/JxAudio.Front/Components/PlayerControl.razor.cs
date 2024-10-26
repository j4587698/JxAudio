using System.Net.Http.Json;
using BootstrapBlazor.Components;
using Jx.Toolbox.Extensions;
using JxAudio.Front.Data;
using JxAudio.Front.Enums;
using JxAudio.TransVo;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Console = System.Console;

namespace JxAudio.Front.Components;

public partial class PlayerControl
{
    [Parameter] public int CurrentTime { get; set; }

    [Parameter] public int Duration { get; set; }

    [Parameter] public EventCallback<int> CurrentTimeChanged { get; set; }

    private double Percent { get; set; }
    private int _volume = 100;

    private int Volume
    {
        get => _volume;
        set
        {
            if (_volume != value)
            {
                _volume = value;
                InvokeVoidAsync("setVolume", value / 100.0);
            }
        }
    }

    private PlayStatus _playStatus = PlayStatus.Stop;
    private LoopStatus _loopStatus = LoopStatus.LoopOnce;

    private string CurrentTimeString
    {
        get => CurrentTime.ToString();
        set
        {
            CurrentTime = int.Parse(value);
            Percent = CurrentTime * 100.0 / Duration;
            CurrentTimeChanged.InvokeAsync(CurrentTime);
            InvokeVoidAsync("setCurrentTime", CurrentTime);
        }
    }

    private List<TrackVo> _tracks = new List<TrackVo>();
    private List<TrackVo>? _shuffleTrack;
    private int _playIndex = 0;
    private bool _showLrc;
    private string _lrc = "暂无歌词";

    private TrackVo? CurrentTrack
    {
        get
        {
            if (_loopStatus == LoopStatus.ShuffleOne)
            {
                return _shuffleTrack?.Count > _playIndex ? _shuffleTrack[_playIndex] : null;
            }

            return _tracks.Count > _playIndex ? _tracks[_playIndex] : null;
        }
    }

    protected override Task InvokeInitAsync() => InvokeVoidAsync("init", Interop);

    [JSInvokable]
    public void OnTimeUpdate(double currentTime)
    {
        var time = (int)currentTime;
        if (_showLrc)
        {
            GetCurrentLrc(currentTime);
        }
        if (CurrentTime != time)
        {
            CurrentTime = time;
            CurrentTimeChanged.InvokeAsync(time);
            Percent = currentTime * 100.0 / Duration;
            StateHasChanged();
        }
    }

    [JSInvokable]
    public async Task OnEnded()
    {
        _playStatus = PlayStatus.Stop;
        switch (_loopStatus)
        {
            case LoopStatus.LoopOnce:
                _playIndex = _playIndex < _tracks.Count - 1 ? _playIndex + 1 : 0;
                break;
            case LoopStatus.ShuffleOne:
                _playIndex = _playIndex < _shuffleTrack!.Count - 1 ? _playIndex + 1 : 0;
                break;
        }

        await PlayNow();
    }

    [JSInvokable]
    public void OnError(string error)
    {
        Console.WriteLine(error);
    }

    [JSInvokable]
    public void OnLoaded()
    {
        _playStatus = PlayStatus.Play;
        StateHasChanged();
    }

    [JSInvokable]
    public void OnPlaying()
    {
        _playStatus = PlayStatus.Play;
        StateHasChanged();
    }
    
    private async Task Star()
    {
        if (CurrentTrack == null)
        {
            return;
        }

        if (CurrentTrack.Star)
        {
            if (await Http.GetStringAsync("/api/Track/UnStar?id=" + CurrentTrack.Id) == "s")
            {
                CurrentTrack.Star = false;
            }
        }
        else
        {
            if (await Http.GetStringAsync("/api/Track/Star?id=" + CurrentTrack.Id) == "s")
            {
                CurrentTrack.Star = true;
            }
        }
    }
    
    private async Task Next()
    {
        switch (_loopStatus)
        {
            case LoopStatus.PlayOnce:
                return;
            case LoopStatus.LoopOnce:
                if (_playIndex < _tracks.Count - 1)
                {
                    _playIndex++;
                }
                break;
            case LoopStatus.ShuffleOne:
                if (_playIndex < _shuffleTrack?.Count - 1)
                {
                    _playIndex++;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        await PlayNow();
    }

    private async Task Preview()
    {
        if (_loopStatus == LoopStatus.PlayOnce)
        {
            return;
        }

        if (_playIndex > 0)
        {
            _playIndex--;
        }

        await PlayNow();
    }

    private async Task Play()
    {
        if (_playStatus == PlayStatus.Loading)
        {
            return;
        }

        if (_playStatus == PlayStatus.Play)
        {
            _playStatus = PlayStatus.Pause;
            await InvokeVoidAsync("playOrPause");
        }
        else if (_playStatus == PlayStatus.Pause)
        {
            _playStatus = PlayStatus.Play;
            await InvokeVoidAsync("playOrPause");
        }
        else if (_playStatus == PlayStatus.Stop)
        {
            await PlayNow();
        }
    }

    private async Task PlayNow()
    {
        if (CurrentTrack == null)
        {
            return;
        }

        if (CurrentTrack.Lrc == null && CurrentTrack.LrcId != 0)
        {
            CurrentTrack.Lrc = await Http.GetFromJsonAsync<List<LrcVo>>("/api/Lrc?id=" + CurrentTrack.LrcId);
        }
        
        _playStatus = PlayStatus.Loading;
        Duration = (int)CurrentTrack.Duration;
        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
        await InvokeVoidAsync("play", CurrentTrack.Id, CurrentTrack.MimeType);
    }

    private async Task Notify(DispatchEntry<AddTrackMessage> entry)
    {
        if (entry.Entry != null)
        {
            var message = entry.Entry;
            if (message is { Type: PlayType.Add, Tracks: not null })
            {
                _tracks = _tracks.Union(message.Tracks).ToList();
                if (_loopStatus == LoopStatus.ShuffleOne)
                {
                    _shuffleTrack = new List<TrackVo>(_tracks);
                    Shuffle();
                    ToShuffle();
                }

                if (_playStatus == PlayStatus.Stop)
                {
                    await PlayNow();
                }
            }
            else if (message is { Type: PlayType.Replace, Tracks: not null })
            {
                _tracks = message.Tracks;
                if (_loopStatus == LoopStatus.ShuffleOne)
                {
                    _shuffleTrack = new List<TrackVo>(_tracks);
                }

                _playIndex = 0;
                await PlayNow();
            }
            else if (message is {Type: PlayType.AddAndPlay, Tracks: not null})
            {
                _tracks = _tracks.Union(message.Tracks).ToList();
                if (_loopStatus == LoopStatus.ShuffleOne)
                {
                    _shuffleTrack = [.._tracks];
                    Shuffle();
                    ToShuffle();
                    _playIndex = _shuffleTrack.IndexOf(message.Tracks[0]);
                }
                else
                {
                    _playIndex = _tracks.IndexOf(message.Tracks[0]);
                }

                await PlayNow();
            }

            StateHasChanged();
        }
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        DispatchService.Subscribe(Notify);
    }

    public void Dispose()
    {
        DispatchService.UnSubscribe(Notify);
    }

    private async Task LoopChanged()
    {
        switch (_loopStatus)
        {
            case LoopStatus.PlayOnce:
                _loopStatus = LoopStatus.LoopOnce;
                await MessageService.Show(new MessageOption()
                {
                    Content = Localizer["Loop"]
                });
                break;
            case LoopStatus.LoopOnce:
                _loopStatus = LoopStatus.ShuffleOne;
                if (_tracks.Count > 0)
                {
                    Shuffle();
                    ToShuffle();
                }
                await MessageService.Show(new MessageOption()
                {
                    Content = Localizer["Shuffle"]
                });
                break;
            case LoopStatus.ShuffleOne:
                _loopStatus = LoopStatus.PlayOnce;
                if (_tracks.Count > 0)
                {
                    ToBase();
                }
                await MessageService.Show(new MessageOption()
                {
                    Content = Localizer["PlayOnce"]
                });
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async Task SelectedTrackChanged(TrackVo track)
    {
        var index = -1;
        if (_loopStatus == LoopStatus.ShuffleOne)
        {
            index = _shuffleTrack?.IndexOf(track) ?? -1;
        }
        else
        {
            index = _tracks.IndexOf(track);
        }

        if (index != -1)
        {
            _playIndex = index;
            await PlayNow();
        }
    }

    private void ToShuffle()
    {
        var track = _tracks.Count < _playIndex ? null : _tracks[_playIndex];
        if (track == null)
        {
            _playIndex = 0;
            return;
        }

        if (_shuffleTrack == null)
        {
            Shuffle();
        }

        var index = _shuffleTrack!.IndexOf(track);
        _playIndex = index == -1 ? 0 : index;
    }

    private void ToBase()
    {
        var track = _shuffleTrack?[_playIndex];
        if (track != null)
        {
            _playIndex = _tracks.IndexOf(track);
            if (_playIndex == -1)
            {
                _playIndex = 0;
            }
        }
    }

    private void Shuffle()
    {
        _shuffleTrack = [.._tracks];
        _shuffleTrack.Shuffle();
    }

    private void GetCurrentLrc(double currentTime)
    {
        var lrc = CurrentTrack?.Lrc?.Where(x => x.TimestampMs <= currentTime * 1000).MaxBy(x => x.TimestampMs);
        if (lrc != null)
        {
            _lrc = lrc.Text ?? "";
        }
        else
        {
            _lrc = Localizer["NoneLrc"];
        }
    }
    
    private async Task TrackDelete(TrackVo track)
    {
        var deletePlaying = CurrentTrack == track;
        _tracks.Remove(track);
        _shuffleTrack?.Remove(track);
        if (deletePlaying && _playStatus is not PlayStatus.Stop)
        {
            await PlayNow();
        }
    }
}