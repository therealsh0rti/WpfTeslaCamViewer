using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace WpfTeslaCamViewer
{
    internal class TeslaCamPlayer
    {
        LibVLC vlc;
        MediaPlayer playerFront, playerLeft, playerRight, playerBack;

        public TeslaCamPlayer(LibVLC vlc, MediaPlayer playerFront, MediaPlayer playerLeft, MediaPlayer playerRight, MediaPlayer playerBack, 
            Action playNextAction)
        {
            this.vlc = vlc;

            this.playerFront = playerFront;
            this.playerLeft = playerLeft;
            this.playerRight = playerRight;
            this.playerBack = playerBack;

            this.playerFront.Mute = true;
            this.playerLeft.Mute = true;
            this.playerRight.Mute = true;
            this.playerBack.Mute = true;

            this.playerFront.EndReached += (_, _) => playNextAction();
        }

        public bool Play(string Path)
        {
            if (!string.IsNullOrWhiteSpace(Path) && File.Exists($"{Path}-front.mp4"))
            {
                ThreadPool.QueueUserWorkItem(_ => playerFront.Play(new Media(this.vlc, new Uri($"{Path}-front.mp4"))));
                ThreadPool.QueueUserWorkItem(_ => playerBack.Play(new Media(this.vlc, new Uri($"{Path}-back.mp4"))));
                ThreadPool.QueueUserWorkItem(_ => playerLeft.Play(new Media(this.vlc, new Uri($"{Path}-left_repeater.mp4"))));
                ThreadPool.QueueUserWorkItem(_ => playerRight.Play(new Media(this.vlc, new Uri($"{Path}-right_repeater.mp4"))));
                return true;
                
            }
            return false;
        }

        public void Stop()
        {
            ThreadPool.QueueUserWorkItem(_ => playerFront.Stop());
            ThreadPool.QueueUserWorkItem(_ => playerBack.Stop());
            ThreadPool.QueueUserWorkItem(_ => playerLeft.Stop());
            ThreadPool.QueueUserWorkItem(_ => playerRight.Stop());
        }

        public string GetDebugInfo()
        {
            StringBuilder sb = new();

            sb.Append("Front: ");
            sb.Append(Math.Round(playerFront.Position * playerFront.Length / 1000, 1));
            sb.Append("s / ");
            sb.Append(playerFront.Length / 1000);
            sb.AppendLine("s");

            sb.Append("Left: ");
            sb.Append(Math.Round(playerLeft.Position * playerLeft.Length / 1000, 1));
            sb.Append("s / ");
            sb.Append(playerLeft.Length / 1000);
            sb.AppendLine("s");

            sb.Append("Right: ");
            sb.Append(Math.Round(playerRight.Position * playerRight.Length / 1000, 1));
            sb.Append("s / ");
            sb.Append(playerRight.Length / 1000);
            sb.AppendLine("s");

            sb.Append("Back: ");
            sb.Append(Math.Round(playerBack.Position * playerBack.Length / 1000, 1));
            sb.Append("s / ");
            sb.Append(playerBack.Length / 1000);
            sb.AppendLine("s");

            return sb.ToString();
        }

        public float GetCurrentVideoPosition()
        {
            return playerFront.Position;
        }

        public void SetPosition(float pos)
        {
            playerFront.Position = pos;
            playerLeft.Position = pos;
            playerRight.Position = pos;
            playerBack.Position = pos;
        }
    }
}
