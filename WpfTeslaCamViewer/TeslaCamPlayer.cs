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
        }

        private void PlayerFront_Stopped(object? sender, EventArgs? e)
        {
            indexFront += 1;
            if (filesFront != null && indexFront < filesFront.Count() && indexFront >= 0)
                playerFront.Play(new Media(this.vlc, new Uri(filesFront[indexFront].FullName)));
        }

        private void PlayerLeft_Stopped(object? sender, EventArgs? e)
        {
            indexLeft += 1;
            if (filesLeft != null && indexLeft < filesLeft.Count() && indexLeft >= 0)
                playerLeft.Play(new Media(this.vlc, new Uri(filesLeft[indexLeft].FullName)));
        }

        private void PlayerRight_Stopped(object? sender, EventArgs? e)
        {
            indexRight += 1;
            if (filesRight != null && indexRight < filesRight.Count() && indexRight >= 0)
                playerRight.Play(new Media(this.vlc, new Uri(filesRight[indexRight].FullName)));
        }

        private void PlayerBack_Stopped(object? sender, EventArgs? e)
        {
            indexBack += 1;
            if (filesBack != null && indexBack < filesBack.Count() && indexBack >= 0)
                playerBack.Play(new Media(this.vlc, new Uri(filesBack[indexBack].FullName)));
        }

        public void Play(string Path)
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

                PlayerFront_Stopped(null, null);
                PlayerLeft_Stopped(null, null);
                PlayerRight_Stopped(null, null);
                PlayerBack_Stopped(null, null);
            }
            else
                throw new Exception("Invalid path");
        }
    }
}
