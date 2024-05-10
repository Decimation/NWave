// $User.Name $File.ProjectName $File.FileName
// $File.CreatedYear-$File.CreatedMonth-$File.CreatedDay @ $File.CreatedHour:$File.CreatedMinute

global using CBN = JetBrains.Annotations.CanBeNullAttribute;
using System.Text;
using CliWrap;
using Flurl;
using JetBrains.Annotations;

#nullable disable
namespace NWave.Lib;

public static class SoundUtility
{

	public static async Task<string[]> RunYtdlpAsync(string cmd, CancellationToken c = default)
	{
		var stderr = new StringBuilder();
		var stdout = new StringBuilder();

		var task = CliWrap.Cli.Wrap("yt-dlp")
			.WithArguments(cmd)
			.WithStandardErrorPipe(PipeTarget.ToStringBuilder(stderr))
			.WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdout))
			.WithValidation(CommandResultValidation.None)
			.ExecuteAsync(c);

		var res = await task;

		if (res.ExitCode == 0) {
			return (stdout.ToString()).Split('\n');
		}

		return null;
	}

	public static async Task<YtdlpAudioFile> GetYtdlpAudioFileAsync(string u, string p, CancellationToken c = default)
	{
		p = Path.TrimEndingDirectorySeparator(p);

		var x = await RunYtdlpAsync($"--print after_move:filepath,id,title -x \"{u}\" --audio-format wav -P \"{p}\"",
		                            c);

		return new YtdlpAudioFile()
		{
			Url   = u,
			Audio = null,
			Id    = x[1],
			Path  = x[0],
			Title = x[2]
		};
	}

	public static async Task<YtdlpAudioFile> GetYtdlpAudioUrlAsync(string u, CancellationToken c = default)
	{
		var urls  = await RunYtdlpAsync($"--get-url {u} --print id,title -x", c);
		var urls1 = urls;

		return new YtdlpAudioFile()
		{
			Url   = u,
			Id    = urls1[0],
			Title = urls1[1],
			Audio = urls1[2]
		};
	}

}

public sealed class YtdlpAudioFile
{

	public Url Url { get; init; }

	public string Id { get; init; }

	public string Title { get; init; }

	[CBN]
	public Url Audio { get; init; }

	public Url Video { get; init; }

	[CBN]
	public string Path { get; init; }

	public YtdlpAudioFile() { }

}