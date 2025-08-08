global using CMN = System.Runtime.CompilerServices.CallerMemberNameAttribute;
global using JIGN = System.Text.Json.Serialization.JsonIgnoreAttribute;
global using CBN = JetBrains.Annotations.CanBeNullAttribute;
global using MURV = JetBrains.Annotations.MustUseReturnValueAttribute;
global using NN = JetBrains.Annotations.NotNullAttribute;
global using MN = System.Diagnostics.CodeAnalysis.MaybeNullAttribute;
global using JINC = System.Text.Json.Serialization.JsonIncludeAttribute;
global using JPO = System.Text.Json.Serialization.JsonPropertyOrderAttribute;
using System.Drawing.Design;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text.Json;
using JetBrains.Annotations;
using Kantan.Text;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualBasic;
using NWave.Lib;
using NWave.Server.Types;
using NWave.Lib.Model;

namespace NWave.Server;

public static class Routes
{

	static Routes()
	{
		Lib = new SoundLibrary();
	}


	public static readonly SoundLibrary Lib;


#region

	/// <summary>
	/// <list type="bullet">
	/// <item>Body: multiple [absolute file path]</item>
	/// </list>
	/// </summary>
	public static async Task AddAsync(HttpContext ctx)
	{
		string[] strings;

		strings = await ctx.TryReadBodyAsync();

		var newSi = new List<FixedSoundItem>();

		foreach (var s in strings) {
			if (!File.Exists(s)) {
				// await ctx.Response.WriteAsync($"{s}: not found\n", ServerUtil.Encoding);
				continue;
			}

			var si = new FixedSoundItem(s, Lib.DeviceIndex);

			if (Lib.TryAdd(si)) {
				newSi.Add(si);
			}
		}

		await ctx.Response.WriteAsJsonAsync(newSi);

		await ctx.Response.CompleteAsync();
	}

	/// <summary>
	/// <list type="bullet">
	/// <item>Body: single [regex pattern]</item>
	/// <item>Body: multiple [name contains]</item>
	/// </list>
	/// </summary>
	public static async Task RemoveAsync(HttpContext ctx)
	{
		var snds = await GetSoundsBySelectionModeAsync(ctx);

		foreach (BaseSoundItem snd in snds) {

			var b = Lib.TryRemove(snd);

			if (b) {
				await ctx.Response.WriteAsJsonAsync(snd);
			}

			Program.Logger.LogInformation("Removed {Sound}: {OK}", snd, b);

		}

		await ctx.Response.CompleteAsync();
	}

	/// <summary>
	/// <list type="bullet">
	/// <item>Body: single [regex pattern]</item>
	/// <item>Body: multiple [name contains]</item>
	/// </list>
	/// </summary>
	public static async Task PlayAsync(HttpContext context)
	{
		var snds = await GetSoundsBySelectionModeAsync(context);

		foreach (var si in snds) {
			var ok = ThreadPool.QueueUserWorkItem((x) =>
			{
				si.Play();
				Program.Logger.LogInformation("Playing {Audio} {Token}", si, x);
			});

			await context.Response.WriteAsJsonAsync(si);
		}

		await context.Response.CompleteAsync();

	}

	/// <summary>
	/// <list type="bullet">
	/// <item>Body: single [regex pattern]</item>
	/// <item>Body: multiple [name contains]</item>
	/// </list>
	/// </summary>
	public static async Task PauseAsync(HttpContext context)
	{
		var snds = await GetSoundsBySelectionModeAsync(context);

		foreach (var si in snds) {
			si.Pause();
			await context.Response.WriteAsJsonAsync(si);
			Program.Logger.LogInformation("Paused {Sound}", si);
		}

		await context.Response.CompleteAsync();
	}

	/// <summary>
	/// <list type="bullet">
	/// <item>Body: single [regex pattern]</item>
	/// <item>Body: multiple [name contains]</item>
	/// </list>
	/// </summary>
	public static async Task StopAsync(HttpContext context)
	{
		var snds = await GetSoundsBySelectionModeAsync(context);

		foreach (var si in snds) {
			si.Stop();
			await context.Response.WriteAsJsonAsync(si);
			Program.Logger.LogInformation("Stopped {Sound}", si);
		}

		await context.Response.CompleteAsync();
	}

	public static async Task UpdateAsync(HttpContext context)
	{
		var update = await context.Request.ReadFromJsonAsync<NWaveUpdate>();

		// var bod = context.TryReadBodyAsync2();

		var snds = GetSoundsBySelectionModeAsync(update.Names, GetHeaderMode(context));

		foreach (var si in snds) {

			// todo: map of update funcs

			switch (update.Field) {
				case nameof(BaseSoundItem.Volume):
					var f = Single.Parse(update.Value.ToString());
					si.Volume = f;
					break;

			}


			await context.Response.WriteAsJsonAsync(si);
			Program.Logger.LogInformation("Updated {Sound}", si);
		}


		await context.Response.CompleteAsync();
	}

	/// <summary>
	/// <list type="bullet">
	/// <item>Body: none</item>
	/// </list>
	/// </summary>
	public static async Task StatusAsync(HttpContext context)
	{
		IEnumerable<BaseSoundItem> snds = await GetSoundsBySelectionModeAsync(context);

		foreach (var si in snds) {
			if (si.Status.IsIndeterminate()) {
				await context.Response.WriteAsJsonAsync(si);
			}
		}

		await context.Response.CompleteAsync();
	}

	/// <summary>
	/// <list type="bullet">
	/// <item>Body: none</item>
	/// </list>
	/// </summary>
	public static async Task ListAsync(HttpContext ctx)
	{
		await ctx.Response.WriteAsJsonAsync(Lib.Sounds.Values);

		await ctx.Response.CompleteAsync();
	}

	public static async Task AddYouTubeAudioUrlAsync(HttpContext context)
	{
		var body = await context.Request.ReadFromJsonAsync<NWaveYouTube>();

		if (body != null) {
			var item = await YouTubeSoundItem.FromAudioUrlAsync(body.Url);
			await context.Response.WriteAsJsonAsync(item);
		}

		await context.Response.CompleteAsync();
	}

	public static async Task AddYouTubeAudioFileAsync(HttpContext context)
	{
		var body = await context.Request.ReadFromJsonAsync<NWaveYouTube>();

		if (body != null) {
			var item = await YouTubeSoundItem.FromAudioFileAsync(body.Url, body.Path);
			await context.Response.WriteAsJsonAsync(item);
		}

		await context.Response.CompleteAsync();
	}

#endregion

#region

	public const string MODE_SIMPLE = "Simple";

	public const string MODE_REGEX = "Regex";

	public const string HDR_MODE = "Mode";

	public const string HDR_VOL = "Vol";

#endregion

#region

	private static async Task<IEnumerable<BaseSoundItem>> GetSoundsBySelectionModeAsync(HttpContext context)
	{
		if (context.Request.ContentLength.GetValueOrDefault() <= 0) {
			return Lib.Sounds.Values;
		}

		var bodyEntries = await context.TryReadBodyAsync();

		string mode = GetHeaderMode(context);

		return GetSoundsBySelectionModeAsync(bodyEntries, mode);
	}

	private static string GetHeaderMode(HttpContext context)
	{
		return context.Request.Headers.TryGetValue(HDR_MODE, out var hdrMode)
			       ? (hdrMode[0] ?? MODE_SIMPLE)
			       : MODE_SIMPLE;
	}

	private static IEnumerable<BaseSoundItem> GetSoundsBySelectionModeAsync(string[] bodyEntries, string mode)
	{
		var snds = new List<BaseSoundItem>();

		SoundLibrary.FindByDelegate fn = mode switch
		{
			MODE_REGEX       => Lib.FindByRegex,
			MODE_SIMPLE or _ => Lib.FindBySimple
		};

		foreach (var bodyEntry in bodyEntries) {
			var snds1 = (fn(bodyEntry));
			snds.AddRange(snds1);
		}

		return snds;
	}

#endregion

}