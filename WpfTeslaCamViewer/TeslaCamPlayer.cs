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
        LibVLCSharp.Shared.MediaPlayer playerFront, playerLeft, playerRight, playerBack;
        List<FileInfo>? filesFront, filesLeft, filesRight, filesBack;
        int indexFront, indexLeft, indexRight, indexBack;

        public TeslaCamPlayer(LibVLC vlc, MediaPlayer playerFront, MediaPlayer playerLeft, MediaPlayer playerRight, MediaPlayer playerBack)
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

            this.playerFront.EndReached += PlayerFront_PlayNext;
            this.playerLeft.EndReached += PlayerLeft_PlayNext;
            this.playerRight.EndReached += PlayerRight_PlayNext;
            this.playerBack.EndReached += PlayerBack_PlayNext;

            this.indexFront = -1;
            this.indexLeft = -1;
            this.indexRight = -1;
            this.indexBack = -1;
        }

        private void PlayerFront_PlayNext(object? sender, EventArgs? e)
        {
            indexFront += 1;
            if (filesFront != null && indexFront < filesFront.Count && indexFront >= 0)
                ThreadPool.QueueUserWorkItem(_ => playerFront.Play(new Media(this.vlc, new Uri(filesFront[indexFront].FullName))));
            else
                ThreadPool.QueueUserWorkItem(_ => playerFront.Stop());
        }

        private void PlayerLeft_PlayNext(object? sender, EventArgs? e)
        {
            indexLeft += 1;
            if (filesLeft != null && indexLeft < filesLeft.Count && indexLeft >= 0)
                ThreadPool.QueueUserWorkItem(_ => playerLeft.Play(new Media(this.vlc, new Uri(filesLeft[indexLeft].FullName))));
            else
                ThreadPool.QueueUserWorkItem(_ => playerLeft.Stop());
        }

        private void PlayerRight_PlayNext(object? sender, EventArgs? e)
        {
            indexRight += 1;
            if (filesRight != null && indexRight < filesRight.Count && indexRight >= 0)
                ThreadPool.QueueUserWorkItem(_ => playerRight.Play(new Media(this.vlc, new Uri(filesRight[indexRight].FullName))));
            else
                ThreadPool.QueueUserWorkItem(_ => playerRight.Stop());
        }

        private void PlayerBack_PlayNext(object? sender, EventArgs? e)
        {
            indexBack += 1;
            if (filesBack != null && indexBack < filesBack.Count && indexBack >= 0)
                ThreadPool.QueueUserWorkItem(_ => playerBack.Play(new Media(this.vlc, new Uri(filesBack[indexBack].FullName))));
            else
                ThreadPool.QueueUserWorkItem(_ => playerBack.Stop());
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

        public string GetDebugInfo()
        {
            StringBuilder sb = new();

            sb.Append("Front: ");
            sb.Append(indexFront + 1);
            sb.Append('/');
            sb.Append(filesFront?.Count.ToString() ?? "0");
            sb.Append(' ');
            sb.Append(Math.Round(playerFront.Position * playerFront.Length / 1000, 1));
            sb.Append("s / ");
            sb.Append(playerFront.Length / 1000);
            sb.AppendLine("s");

            sb.Append("Left: ");
            sb.Append(indexLeft + 1);
            sb.Append('/');
            sb.Append(filesLeft?.Count.ToString() ?? "0");
            sb.Append(' ');
            sb.Append(Math.Round(playerLeft.Position * playerLeft.Length / 1000, 1));
            sb.Append("s / ");
            sb.Append(playerLeft.Length / 1000);
            sb.AppendLine("s");

            sb.Append("Right: ");
            sb.Append(indexRight + 1);
            sb.Append('/');
            sb.Append(filesRight?.Count.ToString() ?? "0");
            sb.Append(' ');
            sb.Append(Math.Round(playerRight.Position * playerRight.Length / 1000, 1));
            sb.Append("s / ");
            sb.Append(playerRight.Length / 1000);
            sb.AppendLine("s");

            sb.Append("Back: ");
            sb.Append(indexBack + 1);
            sb.Append('/');
            sb.Append(filesBack?.Count.ToString() ?? "0");
            sb.Append(' ');
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

        public void SkipBack()
        {
            //lol
            this.indexFront -= 2;
            this.indexLeft -= 2;
            this.indexRight -= 2;
            this.indexBack -= 2;
            SkipForward();
        }

        public void SkipForward()
        {
            PlayerFront_PlayNext(null, null);
            PlayerLeft_PlayNext(null, null);
            PlayerRight_PlayNext(null, null);
            PlayerBack_PlayNext(null, null);
        }
    }
}
