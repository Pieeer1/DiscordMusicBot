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

        private int counter = 0;

        private VoiceTransmitSink transmit { get; set; }

        private Stream pcm { get; set; }

        private VoiceNextConnection connection { get; set; }

        private List<string> queue = new List<string>();

        [Command("ping")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("Pong").ConfigureAwait(false);
        }

        [Command("play")]
        public async Task Play(CommandContext ctx, [RemainingText] string url)
        {
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

                        pcm = ffmpeg.StandardOutput.BaseStream;


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
            queue.RemoveAt(0);
            transmit.Dispose();
            connection.Dispose();
            pcm.Dispose();
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
                transmit.Dispose();
                connection.Dispose();
                pcm.Dispose();
                if (queue.Count > 0)
                {
                    await Play(ctx, queue[0]);
                }
                else
                {
                    await ctx.Channel.SendMessageAsync($"No more songs to skip").ConfigureAwait(false);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
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
                playing = false;
                queue.RemoveAt(0);
                transmit.Dispose();
                connection.Dispose();
                pcm.Dispose();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
