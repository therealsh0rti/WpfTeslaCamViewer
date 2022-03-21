using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfTeslaCamViewer
{
    internal class TeslaCamPlayer
    {
        LibVLC vlc;
        LibVLCSharp.Shared.MediaPlayer playerFront, playerLeft, playerRight, playerBack;
        List<FileInfo>? filesFront, filesLeft, filesRight, filesBack;
        int indexFront, indexLeft, indexRight, indexBack;

        Action<string>? DebugInfoAction;

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

            this.playerFront.Stopped += PlayerFront_Stopped;
            this.playerLeft.Stopped += PlayerLeft_Stopped;
            this.playerRight.Stopped += PlayerRight_Stopped;
            this.playerBack.Stopped += PlayerBack_Stopped;

            this.indexFront = -1;
            this.indexLeft = -1;
            this.indexRight = -1;
            this.indexBack = -1;

            UpdateDebugInfo();
        }

        private void PlayerFront_Stopped(object? sender, EventArgs? e)
        {
            indexFront += 1;
            if (filesFront != null && indexFront < filesFront.Count && indexFront >= 0)
                playerFront.Play(new Media(this.vlc, new Uri(filesFront[indexFront].FullName)));
            UpdateDebugInfo();
        }

        private void PlayerLeft_Stopped(object? sender, EventArgs? e)
        {
            indexLeft += 1;
            if (filesLeft != null && indexLeft < filesLeft.Count && indexLeft >= 0)
                playerLeft.Play(new Media(this.vlc, new Uri(filesLeft[indexLeft].FullName)));
            UpdateDebugInfo();
        }

        private void PlayerRight_Stopped(object? sender, EventArgs? e)
        {
            indexRight += 1;
            if (filesRight != null && indexRight < filesRight.Count && indexRight >= 0)
                playerRight.Play(new Media(this.vlc, new Uri(filesRight[indexRight].FullName)));
            UpdateDebugInfo();
        }

        private void PlayerBack_Stopped(object? sender, EventArgs? e)
        {
            indexBack += 1;
            if (filesBack != null && indexBack < filesBack.Count && indexBack >= 0)
                playerBack.Play(new Media(this.vlc, new Uri(filesBack[indexBack].FullName)));
            UpdateDebugInfo();
        }

        public bool Play(string Path)
        {
            this.indexFront = -1;
            this.indexLeft = -1;
            this.indexRight = -1;
            this.indexBack = -1;

            if (!String.IsNullOrWhiteSpace(Path) && Directory.Exists(Path))
            {
                DirectoryInfo dir = new(Path);
                var allVideos = dir.GetFiles().Where(file => file.Extension == ".mp4");
                this.filesFront = allVideos.Where(file => file.Name.Contains("front")).OrderBy(file => file.Name).ToList();
                this.filesLeft = allVideos.Where(file => file.Name.Contains("left")).OrderBy(file => file.Name).ToList();
                this.filesRight = allVideos.Where(file => file.Name.Contains("right")).OrderBy(file => file.Name).ToList();
                this.filesBack = allVideos.Where(file => file.Name.Contains("back")).OrderBy(file => file.Name).ToList();

                if (filesFront.Count == 0 && filesLeft.Count == 0 && filesRight.Count == 0 && filesBack.Count == 0)
                    return false;
                else
                {
                    PlayerFront_Stopped(null, null);
                    PlayerLeft_Stopped(null, null);
                    PlayerRight_Stopped(null, null);
                    PlayerBack_Stopped(null, null);

                    return true;
                }
            }
            else
                return false;
        }

        protected void UpdateDebugInfo()
        {
            if (DebugInfoAction != null)
            {
                StringBuilder sb = new();

                sb.Append("Front: ");
                sb.Append(indexFront.ToString());
                sb.Append("/");
                sb.AppendLine(filesFront?.Count.ToString() ?? "0");

                sb.Append("Left: ");
                sb.Append(indexLeft.ToString());
                sb.Append("/");
                sb.AppendLine(filesLeft?.Count.ToString() ?? "0");

                sb.Append("Right: ");
                sb.Append(indexRight.ToString());
                sb.Append("/");
                sb.AppendLine(filesRight?.Count.ToString() ?? "0");

                sb.Append("Back: ");
                sb.Append(indexBack.ToString());
                sb.Append("/");
                sb.AppendLine(filesBack?.Count.ToString() ?? "0");

                this.DebugInfoAction(sb.ToString());
            }
        }

        public void SetDebugInfoAction(Action<string> action)
        {
            this.DebugInfoAction = action;
        }
    }
}
