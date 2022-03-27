using System;
using System.Collections;
using System.Collections.Generic;
using LibVLCSharp.Shared;
using LibVLCSharp.WPF;

namespace WpfTeslaCamViewer;

public class PlayerGroup : IEnumerable<MediaPlayer>
{
    private const int Front = 0;
    private const int Left = 1;
    private const int Right = 2;
    private const int Back = 3;
    private readonly VideoView viewBack;
    private readonly VideoView viewFront;
    private readonly VideoView viewLeft;
    private readonly VideoView viewRight;

    private readonly List<MediaPlayer> players;

    public PlayerGroup(VideoView viewFront, VideoView viewLeft, VideoView viewRight, VideoView viewBack)
    {
        this.viewFront = viewFront;
        this.viewLeft = viewLeft;
        this.viewRight = viewRight;
        this.viewBack = viewBack;
        players = new List<MediaPlayer>(4);
        IsInitialized = false;
    }

    public MediaPlayer PlayerFront { get; set; }
    public MediaPlayer PlayerLeft { get; set; }
    public MediaPlayer PlayerRight { get; set; }
    public MediaPlayer PlayerBack { get; set; }
    public bool IsInitialized { get; set; }

    public IEnumerator<MediaPlayer> GetEnumerator()
    {
        return players.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public PlayerGroup Initialize(Func<MediaPlayer> getMediaPlayerFunc)
    {
        viewFront.MediaPlayer = getMediaPlayerFunc.Invoke();
        viewLeft.MediaPlayer = getMediaPlayerFunc.Invoke();
        viewRight.MediaPlayer = getMediaPlayerFunc.Invoke();
        viewBack.MediaPlayer = getMediaPlayerFunc.Invoke();

        PlayerFront = viewFront.MediaPlayer;
        PlayerBack = viewBack.MediaPlayer;
        PlayerLeft = viewLeft.MediaPlayer;
        PlayerRight = viewRight.MediaPlayer;

        PlayerBack.Mute = true;
        PlayerFront.Mute = true;
        PlayerRight.Mute = true;
        PlayerLeft.Mute = true;

        players.Insert(Front, PlayerFront);
        players.Insert(Left, PlayerLeft);
        players.Insert(Right, PlayerRight);
        players.Insert(Back, PlayerBack);

        IsInitialized = true;

        return this;
    }
}