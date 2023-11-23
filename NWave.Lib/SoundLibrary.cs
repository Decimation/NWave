using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CliWrap;
using Flurl;
using JetBrains.Annotations;

namespace NWave.Lib;

#nullable disable

public class SoundLibrary : IDisposable
{

	public ConcurrentDictionary<BaseSoundItem, object> Sounds { get; } = new();

	public string RootDir { get; private set; }

	public bool InitDirectory(string dir, int di)
	{
		RootDir = dir;
		IEnumerable<string> files = EnumerateDirectory(dir);

		var items = files.Select(x => new FixedSoundItem(x, di));

		foreach (FixedSoundItem item in items) {
			if (TryAdd(item)) { }

		}

		return true;
	}

	public static IEnumerable<string> EnumerateDirectory(string d)
	{
		var files = Directory.EnumerateFiles(d, searchPattern: "*.*", new EnumerationOptions()
		{
			RecurseSubdirectories = true
		});
		return files;
	}

	public async Task<bool> AddYouTubeAudioUrlAsync(string u, int di)
	{
		var yt = (await GetYouTubeAudioUrlAsync(u));
		var y  = yt.Audio;
		if (!string.IsNullOrWhiteSpace(y)) {
			return TryAdd(new DynamicSoundItem(y, yt.Title, di));
		}

		return false;
	}

	public async Task<bool> AddYouTubeAudioFileAsync(string u, int di)
	{
		var yt = await GetYouTubeAudioFileAsync(u, RootDir);
		var y  = yt.Path;
		if (!string.IsNullOrWhiteSpace(y)) {
			return TryAdd(new FixedSoundItem(y, di));
		}

		return false;
	}

	#region YT

	public static async Task<string[]> ytdlp_async(string cmd, CancellationToken c = default)
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

	public record YouTubeVideo
	{

		public Url Url { get; init; }

		public string Id { get; init; }

		public string Title { get; init; }

		public Url Audio { get; init; }

		public Url Video { get; init; }

		[CanBeNull]
		public string Path { get; init; }

	}

	public static async Task<YouTubeVideo> GetYouTubeAudioFileAsync(string u, string p, CancellationToken c = default)
	{
		p = Path.TrimEndingDirectorySeparator(p);

		var x = await ytdlp_async($"--print after_move:filepath,id,title -x \"{u}\" --audio-format wav -P \"{p}\"", c);

		return new YouTubeVideo()
		{
			Url = u,
			Audio = null, 
			Id = x[1], 
			Path = x[0], 
			Title = x[2]
		};
	}

	public static async Task<YouTubeVideo> GetYouTubeAudioUrlAsync(string u, CancellationToken c = default)
	{
		var urls  = await ytdlp_async($"--get-url {u} --print id,title -x", c);
		var urls1 = urls;

		return new YouTubeVideo()
		{
			Url   = u,
			Id    = urls1[0],
			Title = urls1[1],
			Audio = urls1[2]
		};
	}

	#endregion

	public IEnumerable<BaseSoundItem> FindSoundsByNames(IEnumerable<string> rnames)
	{
		var s1s = rnames as string[] ?? rnames.ToArray();

		if (s1s.Length == 0) {
			return Sounds.Keys.AsEnumerable();
		}

		var buf = new List<BaseSoundItem>(s1s.Length);

		foreach (string s1 in s1s) {

			var si = FindSoundByName(s1);

			if (si != null) {
				buf.Add(si);

			}
		}

		return buf;
	}

	public IEnumerable<BaseSoundItem> FindByPattern(string pattern)
	{
		foreach (var (a, _) in Sounds) {
			if (Regex.Match(a.Name, pattern, RegexOptions.IgnoreCase).Success) {
				yield return a;
			}
		}
	}

	public IEnumerable<BaseSoundItem> FindByPredicate(string[] rnames, Func<BaseSoundItem, bool> ff)
	{
		IEnumerable<BaseSoundItem> snds;

		if (rnames.Length != 0) {
			snds = FindSoundsByNames(rnames);
		}
		else {
			snds = Sounds.Keys.Where(ff);
		}

		return snds;
	}

	[CanBeNull]
	public BaseSoundItem FindSoundByName(string rname)
	{
		var kv = Sounds.FirstOrDefault(x =>
		{
			return x.Key.Name.Contains(rname, StringComparison.InvariantCultureIgnoreCase);
		});
		return kv.Key;
	}

	public bool TryRemove(BaseSoundItem item)
	{
		var b = Sounds.TryRemove(item, out _);

		if (b) {
			if (!item.IsDisposed) {
				item.Dispose();
			}

		}

		return b;
	}

	public bool TryAdd(BaseSoundItem s)
	{
		return Sounds.TryAdd(s, null);

	}

	public void Dispose()
	{
		foreach ((BaseSoundItem? key, var _) in Sounds) {
			if (!key.IsDisposed) {
				key.Dispose();

			}
		}

		Sounds.Clear();
	}

}