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

	public const int DEFAULT_DEVICE_INDEX = -1;

	public int Count { get; private set; }

	public ConcurrentDictionary<BaseSoundItem, object> Sounds { get; } = new();

	public string RootDir { get; private set; }

	public int DeviceIndex { get; private set; } = DEFAULT_DEVICE_INDEX;

	public bool InitDirectory(string dir, int di = DEFAULT_DEVICE_INDEX)
	{
		RootDir     = dir;
		DeviceIndex = (di);

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

	public async Task<bool> AddYtdlpAudioUrlAsync(string u, int? di = null)
	{
		di ??= DeviceIndex;

		var yt = (await SoundUtility.GetYtdlpAudioUrlAsync(u));
		var y  = yt.Audio;

		if (!String.IsNullOrWhiteSpace(y)) {
			return TryAdd(new DynamicSoundItem(y, yt.Title, di.Value));
		}

		return false;
	}

	public async Task<bool> AddYtdlpAudioFileAsync(string u, int? di = null)
	{
		di ??= DeviceIndex;
		var yt = await SoundUtility.GetYtdlpAudioFileAsync(u, RootDir);
		var y  = yt.Path;

		if (!String.IsNullOrWhiteSpace(y)) {
			return TryAdd(new FixedSoundItem(y, di.Value));
		}

		return false;
	}

	public IEnumerable<BaseSoundItem> FindSoundsByNames(IEnumerable<string> rnames)
	{
		var s1s = rnames as string[] ?? rnames.ToArray();

		if (s1s.Length == 0) {

			return [];

			// return Sounds.Keys.AsEnumerable();
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

	[CBN]
	public BaseSoundItem FindSoundByName(string rname)
	{
		var kv = Sounds.FirstOrDefault(x =>
		{
			return x.Key.Name.Contains(
				rname, StringComparison.InvariantCultureIgnoreCase);
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
		foreach ((BaseSoundItem key, var _) in Sounds) {
			if (!key.IsDisposed) {
				key.Dispose();

			}
		}

		Sounds.Clear();
	}

}