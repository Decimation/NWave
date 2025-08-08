// Author: Deci | Project: NWave.Lib | Name: YtdlpAudioFile.cs
// Date: 2025/02/14 @ 01:02:08

#nullable disable
using CliWrap;
using Flurl;
using System.IO;
using System.Security.Policy;
using System.Text;
using Url = Flurl.Url;

namespace NWave.Lib.Model;

public sealed class YouTubeSoundItem : DynamicSoundItem
{

	public const string YT_DLP_EXE = "yt-dlp";

	public string Title { get; init; }

	[CBN]
	public Url Audio { get; init; }

	public Url Video { get; init; }

	[CBN]
	public string FullPath { get; init; }

	public Url Url { get; }

	public string VideoId { get; init; }

	public YouTubeSoundItem(Url u, string fullName, int idx = DEFAULT_DEVICE_INDEX)
		: base(fullName, idx)
	{
		Url = u;
	}


	public static async Task<YouTubeSoundItem> FromAudioFileAsync(Url url, string path, CancellationToken c = default)
	{
		path = Path.TrimEndingDirectorySeparator(path);

		var stderr = new StringBuilder();
		var stdout = new StringBuilder();

		using var cmd = Cli.Wrap(YT_DLP_EXE)
			.WithArguments(["--x", url])
			.WithArguments(["--print", "after_move:filepath,id,title"])
			.WithArguments(["--audio-format", "wav"])
			.WithArguments(["-P", path])
			.WithStandardErrorPipe(PipeTarget.ToStringBuilder(stderr))
			.WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdout))
			.WithValidation(CommandResultValidation.None)
			.ExecuteAsync(c);

		var res = await cmd;

		if (res.ExitCode != 0) {
			var stdOutRes = stdout.ToString().Split('\n');

			return new YouTubeSoundItem(url, path)
			{
				Audio    = null,
				VideoId  = stdOutRes[1],
				FullPath = stdOutRes[0],
				Title    = stdOutRes[2]
			};
		}

		return null;

	}

	public static async Task<YouTubeSoundItem> FromAudioUrlAsync(Url url, CancellationToken c = default)
	{
		var stderr = new StringBuilder();
		var stdout = new StringBuilder();

		using var cmd = Cli.Wrap(YT_DLP_EXE)
			.WithArguments(["--get-url", url])
			.WithArguments(["--print", "id,title"])
			.WithArguments("-x")
			.WithStandardErrorPipe(PipeTarget.ToStringBuilder(stderr))
			.WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdout))
			.WithValidation(CommandResultValidation.None)
			.ExecuteAsync(c);

		var res = await cmd;

		if (res.ExitCode != 0) {
			var urls    = stdout.ToString().Split('\n');
			var audio   = urls[2];
			var videoId = urls[0];
			var title   = urls[1];

			return new YouTubeSoundItem(url, audio)
			{
				Audio    = audio,
				FullPath = null,
				VideoId  = videoId,
				Title    = title,
			};

		}

		return null;
	}

}