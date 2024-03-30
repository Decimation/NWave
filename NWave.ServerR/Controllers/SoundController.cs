// Author: Deci | Project: NWave.ServerR | Name: SoundController.cs
// Date: 2024/3/30 @ 14:55:55

using System.Net.Mime;
using Kantan.Text;
using Microsoft.AspNetCore.Mvc;
using NWave.Lib;
using NWave.ServerR.Model;

namespace NWave.ServerR.Controllers;

[ApiController]
[Route("[controller]")]
public class SoundController : Controller
{

	private readonly ISoundItemService m_service;

	private SoundLibrary SndLib => ((SoundItemService) m_service).SndLib;

	public SoundController(ISoundItemService service)
	{

		m_service = service;
	}

	// GET
	public IActionResult Index()
	{

		return View(m_service.GetAllSoundsAsync());
	}

	[HttpGet]
	public IActionResult ButtonClicked(int id)
	{
		// Logic for handling button click
		// For demo purposes, simply return the ID as content
		return Content($"Button {id} clicked");
	}

	public const int DEVICE_INDEX = 3;

	public const string SOUNDS = @"H:\Other Music\Audio resources\NMicspam\";

	public void Init()
	{
		SndLib.InitDirectory(SOUNDS, DEVICE_INDEX);

	}

	#region

	/// <summary>
	/// <list type="bullet">
	/// <item>Body: multiple [absolute file path]</item>
	/// </list>
	/// </summary>
	[HttpPost]
	public async Task<IActionResult> AddAsync(HttpContext ctx)
	{
		var bodyEntries = await ctx.ReadBodyEntriesAsync();
		ctx.Response.ContentType = MediaTypeNames.Text.Plain;

		foreach (var s in bodyEntries) {
			if (!System.IO.File.Exists(s)) {
				await ctx.Response.WriteAsync($"{s}: not found\n", ServerUtil.Encoding);
				continue;
			}

			var si = new FixedSoundItem(s, DEVICE_INDEX);

			var b = SndLib.TryAdd(si);
			await ctx.Response.WriteAsync($"{si}: {b}\n", ServerUtil.Encoding);

			// LoggerExtensions.LogInformation(Program._logger, "Added {Sound}: {OK}", si, b);
		}

		await ctx.Response.CompleteAsync();
		return Empty;
	}

	/// <summary>
	/// <list type="bullet">
	/// <item>Body: single [regex pattern]</item>
	/// <item>Body: multiple [name contains]</item>
	/// </list>
	/// </summary>
	[HttpPost]
	public async Task<IActionResult> RemoveAsync(HttpContext ctx)
	{
		ctx.Response.ContentType = MediaTypeNames.Text.Plain;

		var snds = await GetSoundsBySelectionModeAsync(ctx, MODE_SIMPLE);

		foreach (BaseSoundItem snd in snds) {

			var b = SndLib.TryRemove(snd);
			await ctx.Response.WriteAsync($"{snd}: {b}\n", ServerUtil.Encoding);
			Program._logger.LogInformation("Removed {Sound}: {OK}", snd, b);

		}

		await ctx.Response.CompleteAsync();
		return Empty;
	}

	/// <summary>
	/// <list type="bullet">
	/// <item>Body: single [regex pattern]</item>
	/// <item>Body: multiple [name contains]</item>
	/// </list>
	/// </summary>
	[HttpPost]
	public async Task<IActionResult> PlayAsync(HttpContext context)
	{
		// string audioFilePath = @"H:\Other Music\Audio resources\NMicspam\ow.wav"; // Change this to the path of your audio file
		var snds = await GetSoundsBySelectionModeAsync(context, MODE_SIMPLE);

		context.Response.ContentType = MediaTypeNames.Text.Plain;

		foreach (var si in snds) {
			ThreadPool.QueueUserWorkItem((x) =>
			{
				Program._logger.LogInformation("Playing {Audio}", si);
				si.Play();
			});

			await context.Response.WriteAsync($"{si} playing\n", ServerUtil.Encoding);

		}

		await context.Response.CompleteAsync();
		return Empty;
	}

	/// <summary>
	/// <list type="bullet">
	/// <item>Body: single [regex pattern]</item>
	/// <item>Body: multiple [name contains]</item>
	/// </list>
	/// </summary>
	[HttpPost]
	public async Task<IActionResult> PauseAsync(HttpContext context)
	{
		var snds = await GetSoundsBySelectionModeAsync(context, MODE_SIMPLE);

		context.Response.ContentType = MediaTypeNames.Text.Plain;

		foreach (var si in snds) {
			si.Pause();
			await context.Response.WriteAsync($"{si}\n", ServerUtil.Encoding);
			Program._logger.LogInformation("Paused {Sound}", si);

		}

		await context.Response.CompleteAsync();
		return Empty;

	}

	/// <summary>
	/// <list type="bullet">
	/// <item>Body: single [regex pattern]</item>
	/// <item>Body: multiple [name contains]</item>
	/// </list>
	/// </summary>
	[HttpPost]
	public async Task<IActionResult> StopAsync(HttpContext context)
	{
		var snds = await GetSoundsBySelectionModeAsync(context, MODE_SIMPLE);

		context.Response.ContentType = MediaTypeNames.Text.Plain;

		foreach (var si in snds) {
			si.Stop();
			await context.Response.WriteAsync($"{si}\n", ServerUtil.Encoding);
			Program._logger.LogInformation("Stopped {Sound}", si);

		}

		await context.Response.CompleteAsync();
		return Empty;

	}

	[HttpPost]
	public async Task<IActionResult> UpdateAsync(HttpContext context)
	{
		context.Response.ContentType = MediaTypeNames.Text.Plain;
		var snds  = await GetSoundsBySelectionModeAsync(context, MODE_SIMPLE);
		var items = snds as BaseSoundItem[] ?? Enumerable.ToArray<BaseSoundItem>(snds);
		Program._logger.LogInformation("Sounds: {Sounds}", items.QuickJoin());
		var opt   = context.Request.Headers.TryGetValue(HDR_VOL, out var sv);
		var snds2 = items.Where(s => s.SupportsVolume).ToArray();

		if (opt) {
			foreach (var si in snds2) {
				var f = float.Parse(sv[0]);

				if (f < 0f || f > 1.0f) {
					f /= 100f;
					f =  Math.Clamp(f, 0f, 1.0f);
				}

				si.Volume = f;
				await context.Response.WriteAsync($"{si}\n", ServerUtil.Encoding);
				Program._logger.LogInformation("Updated {Sound}", si);

			}

		}

		await context.Response.CompleteAsync();
		return Empty;

	}

	/// <summary>
	/// <list type="bullet">
	/// <item>Body: none</item>
	/// </list>
	/// </summary>
	[HttpGet]
	public async Task<IActionResult> StatusAsync(HttpContext context)
	{
		IEnumerable<BaseSoundItem> snds = await GetSoundsBySelectionModeAsync(context);
		snds = snds.Where(s => s.Status.IsIndeterminate()).ToArray();

		context.Response.ContentType = MediaTypeNames.Text.Plain;

		if (!snds.Any()) {
			await context.Response.WriteAsync("---");
		}

		foreach (var si in snds) {
			await context.Response.WriteAsync($"{si}\n", ServerUtil.Encoding);
		}

		await context.Response.CompleteAsync();
		return Empty;
	}

	/// <summary>
	/// <list type="bullet">
	/// <item>Body: none</item>
	/// </list>
	/// </summary>
	[HttpGet]
	public async Task<IActionResult> ListAsync(HttpContext ctx)
	{
		ctx.Response.ContentType = MediaTypeNames.Text.Plain;

		foreach ((var key, var _) in SndLib.Sounds) {
			await ctx.Response.WriteAsync($"{key.Name}\n", ServerUtil.Encoding);

		}

		await ctx.Response.CompleteAsync();
		return Empty;
	}

	[HttpPost]
	public async Task<IActionResult> AddYouTubeAudioUrlAsync(HttpContext context)
	{
		context.Response.ContentType = MediaTypeNames.Text.Plain;
		var b  = await context.ReadBodyTextAsync();
		var ok = await SndLib.AddYtdlpAudioUrlAsync(b, DEVICE_INDEX);
		await context.Response.WriteAsync($"{b} : {ok}", ServerUtil.Encoding);

		await context.Response.CompleteAsync();

		return Empty;
	}

	[HttpPost]
	public async Task<IActionResult> AddYouTubeAudioFileAsync(HttpContext context)
	{
		context.Response.ContentType = MediaTypeNames.Text.Plain;
		var b  = await context.ReadBodyTextAsync();
		var ok = await SndLib.AddYtdlpAudioFileAsync(b, DEVICE_INDEX);
		await context.Response.WriteAsync($"{b} : {ok}", ServerUtil.Encoding);

		await context.Response.CompleteAsync();
		return Empty;
	}

	#endregion

	public const string MODE_SIMPLE = "Simple";

	public const string MODE_REGEX = "Regex";

	public const string HDR_MODE = "Mode";

	public const string HDR_VOL = "Vol";

	private async Task<IEnumerable<BaseSoundItem>> GetSoundsBySelectionModeAsync(HttpContext context,
		string selMode = MODE_SIMPLE)
	{
		var bodyEntries = await context.ReadBodyEntriesAsync();

		IEnumerable<BaseSoundItem> snds;

		var b = context.Request.Headers.TryGetValue(HDR_MODE, out var e);

		var mode = b ? e[0] : selMode;

		if (bodyEntries.Length == 0) {
			mode = MODE_SIMPLE;
		}

		switch (mode) {
			case MODE_REGEX:
				snds = SndLib.FindByPattern(bodyEntries[0]);
				break;

			case MODE_SIMPLE:
				snds = SndLib.FindSoundsByNames(bodyEntries);
				break;

			default:
				goto case MODE_SIMPLE;
		}

		return snds;
	}

	private const string ALL = "*";

}