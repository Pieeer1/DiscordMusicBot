using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using Discord.Audio;
using DSharpPlus.VoiceNext;
using System.Diagnostics;
using DSharpPlus.Entities;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Common;
using YoutubeExplode.Videos.Streams;

namespace MasonMusicBot.Commands
{
    internal class BaseCommands : BaseCommandModule
    {
        private static bool playing = false;
        private static AudioOutStream dstream = null;
        private static IAudioClient client = null;
        private string url { get; set; }

        private int counter = 0;

        private VoiceTransmitSink transmit { get; set; }

        private VoiceNextConnection connection { get; set; }

        private List<string> queue = new List<string>();
        private List<string> skip = new List<string>();

        [Command("ping")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("Pong").ConfigureAwait(false);
        }

        [Command("play")]
        public async Task Play(CommandContext ctx, [RemainingText] string url)
        {
            if (skip != null)
            {
                queue = skip;
            }
            queue.Add(url);
            queue = queue.Distinct().ToList();
            if (playing == true)
            {
                return;
            }
            while(queue.Count>0)
            {


                try
                {
                    if (counter == 0)
                    {
                        ctx.Client.UseVoiceNext();
                        counter++;
                    }


                    DiscordChannel channel = ctx.Member.VoiceState?.Channel;

                    using (connection = await channel.ConnectAsync())
                    {

                        var youtube = new YoutubeClient();
                        playing = true;
                        var videos = await youtube.Search.GetVideosAsync(queue[0]);

                        await ctx.Channel.SendMessageAsync($"Playing " + videos[0].Title + " " + videos[0].Duration).ConfigureAwait(false);

                        var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videos[0].Id);

                        var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

                        var stream = await youtube.Videos.Streams.GetAsync(streamInfo);

                        // Download the stream to a file
                        await youtube.Videos.Streams.DownloadAsync(streamInfo, $"video.{streamInfo.Container}");


                        transmit = connection.GetTransmitSink();

                        var filePath = $"video.{streamInfo.Container}";
                        var ffmpeg = Process.Start(new ProcessStartInfo
                        {
                            FileName = "ffmpeg",
                            Arguments = $@"-i ""{filePath}"" -ac 2 -f s16le -ar 48000 pipe:1",
                            RedirectStandardOutput = true,
                            UseShellExecute = false
                        });

                        Stream pcm = ffmpeg.StandardOutput.BaseStream;



                        await pcm.CopyToAsync(transmit);
                        queue.RemoveAt(0);

                        playing = false;

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            transmit.Dispose();
            await transmit.FlushAsync();
        }
        [Command("queue")]
        public async Task ShowQueue(CommandContext ctx)
        {
            string reString = string.Empty;
            foreach (var item in queue)
            {
                reString = reString + item.ToString() + "\n";
            }
            await ctx.Channel.SendMessageAsync(reString).ConfigureAwait(false);
        }

        [Command("skip")]
        public async Task Skip(CommandContext ctx)
        {
            try
            {
                playing = false;
                queue.RemoveAt(0);
                skip = queue;
                queue.Clear();
                connection.Disconnect();
                await Play(ctx, skip[0]);

            }
            catch
            {

            }
        }


        [Command("pause")]
        public async Task Pause(CommandContext ctx)
        {
            if (playing == true)
            {

                await ctx.Channel.SendMessageAsync("Pausing Song").ConfigureAwait(false);
                transmit.Pause();
            }
        }
        [Command("resume")]
        public async Task Resume(CommandContext ctx)
        {
            if (playing == true)
            {

                await ctx.Channel.SendMessageAsync("Continuing Song").ConfigureAwait(false);
                await transmit.ResumeAsync();
            }
        }
        [Command("stop")]
        public async Task Stop(CommandContext ctx)
        {
            try
            {
                await ctx.Channel.SendMessageAsync("Stopping Song").ConfigureAwait(false);
                transmit.Dispose();
                await transmit.FlushAsync();
                queue.Clear();
                playing = false;
            }
            catch
            { 
            
            }
        }
    }
}
