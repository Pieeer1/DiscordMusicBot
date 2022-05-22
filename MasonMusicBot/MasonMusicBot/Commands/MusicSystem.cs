namespace MasonMusicBot.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using YoutubeExplode;
    using YoutubeExplode.Common;
    using YoutubeExplode.Converter;
    using YoutubeExplode.Search;
    using YoutubeExplode.Videos.Streams;
    using NAudio;
    using NAudio.Wave;
    using System.Diagnostics;

    internal class MusicSystem
    {

        public async Task PlayVideo(string search)
        {

            var youtube = new YoutubeClient();

            var videos = await youtube.Search.GetVideosAsync(search);

            var streamUrl = videos[0].Url;

            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(streamUrl);

            var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

            await youtube.Videos.DownloadAsync(streamUrl, "temp.mp4");






        }

    }
}
