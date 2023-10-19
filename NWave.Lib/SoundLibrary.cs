using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace NWave.Lib;

#nullable disable

public class SoundLibrary : IDisposable
{
	public ConcurrentDictionary<SoundItem, object> Sounds { get; } = new();

	public bool InitDirectory(string dir, int di)
	{
		IEnumerable<string> files = EnumerateDirectory(dir);

		var items = files.Select(x => new SoundItem(x, di));

		foreach (SoundItem item in items) {
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

	public IEnumerable<SoundItem> FindSoundsByNames(IEnumerable<string> rnames)
	{
		var s1s = rnames as string[] ?? rnames.ToArray();

		if (s1s.Length == 0) {
			return Sounds.Keys.AsEnumerable();
		}

		var buf = new List<SoundItem>(s1s.Length);

		foreach (string s1 in s1s) {

			var si = FindSoundByName(s1);

			if (si != null) {
				buf.Add(si);

			}
		}

		return buf;
	}

	public IEnumerable<SoundItem> FindByPattern(string pattern)
	{
		foreach (var (a, _) in Sounds) {
			if (Regex.Match(a.Name, pattern, RegexOptions.IgnoreCase).Success) {
				yield return a;
			}
		}
	}

	public IEnumerable<SoundItem> FindByPredicate(string[] rnames, Func<SoundItem, bool> ff)
	{
		IEnumerable<SoundItem> snds;

		if (rnames.Length != 0) {
			snds = FindSoundsByNames(rnames);
		}
		else {
			snds = Sounds.Keys.Where(ff);
		}

		return snds;
	}

	[CanBeNull]
	public SoundItem FindSoundByName(string rname)
	{
		var kv = Sounds.FirstOrDefault(x =>
		{
			return x.Key.Name.Contains(rname, StringComparison.InvariantCultureIgnoreCase);
		});
		return kv.Key;
	}

	public bool TryRemove(SoundItem item)
	{
		var b = Sounds.TryRemove(item, out _);

		if (b) {
			if (!item.IsDisposed) {
				item.Dispose();
			}

		}

		return b;
	}

	public bool TryAdd(SoundItem s)
	{
		return Sounds.TryAdd(s, null);

	}

	public void Dispose()
	{
		foreach ((SoundItem? key, var _) in Sounds) {
			if (!key.IsDisposed) {
				key.Dispose();

			}
		}

		Sounds.Clear();
	}
}