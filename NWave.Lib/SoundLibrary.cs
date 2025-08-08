global using CMN = System.Runtime.CompilerServices.CallerMemberNameAttribute;
global using JIGN = System.Text.Json.Serialization.JsonIgnoreAttribute;
global using CBN = JetBrains.Annotations.CanBeNullAttribute;
global using MURV = JetBrains.Annotations.MustUseReturnValueAttribute;
global using NN = JetBrains.Annotations.NotNullAttribute;
global using MN = System.Diagnostics.CodeAnalysis.MaybeNullAttribute;
global using JINC = System.Text.Json.Serialization.JsonIncludeAttribute;
global using JPO = System.Text.Json.Serialization.JsonPropertyOrderAttribute;
global using MNNW = System.Diagnostics.CodeAnalysis.MemberNotNullWhenAttribute;
global using MNN = System.Diagnostics.CodeAnalysis.MemberNotNullAttribute;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CliWrap;
using Flurl;
using JetBrains.Annotations;
using NWave.Lib.Model;

namespace NWave.Lib;

#nullable disable

public class SoundLibrary : IDisposable
{

	public ConcurrentDictionary<string, BaseSoundItem> Sounds { get; } = new();

	public string RootDir { get; private set; }

	public int DeviceIndex { get; private set; } = BaseSoundItem.DEFAULT_DEVICE_INDEX;

	public const string ALL = "*";

	public bool InitDirectory(string dir, int di)
	{
		RootDir     = dir;
		DeviceIndex = (di);

		IEnumerable<string> files = EnumerateDirectory(dir);

		foreach (var file in files) {
			var fsi = new FixedSoundItem(file, di);

			if (!TryAdd(fsi)) {
				Trace.WriteLine($"Couldn't add {fsi}");
			}
		}

		return true;
	}

	[NN]
	public static IEnumerable<string> EnumerateDirectory(string dir)
	{
		var files = Directory.EnumerateFiles(dir, searchPattern: ALL, new EnumerationOptions()
		{
			RecurseSubdirectories = true
		});
		return files;
	}


	public IEnumerable<BaseSoundItem> FindByRegex(string pattern)
	{
		foreach (var (name, bsi) in Sounds) {
			if (Regex.Match(bsi.Name, pattern, RegexOptions.IgnoreCase).Success) {
				yield return bsi;
			}
		}
	}

	public IEnumerable<BaseSoundItem> FindBySimple(string rname)
	{
		foreach (var (name, bsi) in Sounds) {
			if (name.Contains(rname, StringComparison.InvariantCultureIgnoreCase)) {
				yield return bsi;
			}
		}

	}

	public delegate IEnumerable<BaseSoundItem> FindByDelegate(string name);

	public bool TryRemove(BaseSoundItem item)
	{
		var b = Sounds.TryRemove(item.Name, out _);

		if (b) {
			if (!item.IsDisposed) {
				item.Dispose();
			}

		}

		return b;
	}

	public bool TryAdd(BaseSoundItem s)
	{
		return Sounds.TryAdd(s.Name, s);
	}

	public void Dispose()
	{
		foreach (var (key, v) in Sounds) {
			if (!v.IsDisposed) {
				v.Dispose();

			}
		}

		Sounds.Clear();
	}

}