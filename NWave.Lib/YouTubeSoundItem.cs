// Author: Deci | Project: NWave.Lib | Name: YtdlpAudioFile.cs
// Date: 2025/02/14 @ 01:02:08

#nullable disable
using System.Text;
using CliWrap;
using Flurl;

namespace NWave.Lib;

public sealed class YouTubeSoundItem : DynamicSoundItem
{

	public const string YT_DLP_EXE = "yt-dlp";

	public string Title { get; init; }

	[CBN]
	public Url Audio { get; init; }

	public Url Video { get; init; }

	[CBN]
	public string Path { get; init; }

	public Url Url { get; }

	public string VideoId { get; init; }

	public YouTubeSoundItem(Url u, string fileName, int idx = SoundLibrary.DEFAULT_DEVICE_INDEX, int? id = null)
		: base(fileName, idx, id)
	{
		Url = u;
	}

	public static async Task<string[]> RunYtdlpAsync(string cmd, CancellationToken c = default)
	{
		var stderr = new StringBuilder();
		var stdout = new StringBuilder();

		var task = Cli.Wrap(YT_DLP_EXE)
			.WithArguments(cmd)
			.WithStandardErrorPipe(PipeTarget.ToStringBuilder(stderr))
			.WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdout))
			.WithValidation(CommandResultValidation.None)
			.ExecuteAsync(c);

		var res = await task;

		if (res.ExitCode == 0) {
			return stdout.ToString().Split(Environment.NewLine);
		}

		return null;
	}

	public static async Task<YouTubeSoundItem> FromAudioFileAsync(string url, string path, CancellationToken c = default)
	{
		path = System.IO.Path.TrimEndingDirectorySeparator(path);

		var x = await RunYtdlpAsync(
			        $"--print after_move:filepath,id,title -x \"{url}\" --audio-format wav -P \"{path}\"", c);

		return new YouTubeSoundItem(url, path)
		{
			Audio   = null,
			VideoId = x[1],
			Path    = x[0],
			Title   = x[2]
		};
	}

	public static async Task<YouTubeSoundItem> FromAudioUrlAsync(string url, CancellationToken c = default)
	{
		var urls = await RunYtdlpAsync($"--get-url {url} --print id,title -x", c);

		return new YouTubeSoundItem(url, url)
		{
			Audio   = urls[2],
			Path = null,
			VideoId = urls[0],
			Title   = urls[1],
		};
	}

}