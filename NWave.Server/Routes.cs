using System.Net.Mime;
using Kantan.Text;
using NWave.Lib;

namespace NWave.Server
{
	public static class Routes
	{

		static Routes()
		{
			Routes.Lib = new SoundLibrary();

		}

		public const int DEVICE_INDEX = 3;

		public const string SOUNDS = @"H:\Other Music\Audio resources\NMicspam\";

		public static readonly SoundLibrary Lib;

		public static void Init()
		{
			Routes.Lib.InitDirectory(Routes.SOUNDS, Routes.DEVICE_INDEX);

		}

		/// <summary>
		/// <list type="bullet">
		/// <item>Body: multiple [absolute file path]</item>
		/// </list>
		/// </summary>
		public static async Task AddAsync(HttpContext ctx)
		{
			var bodyEntries = await ctx.ReadBodyEntriesAsync();
			ctx.Response.ContentType = MediaTypeNames.Text.Plain;

			foreach (var s in bodyEntries) {
				if (!File.Exists(s)) {
					await ctx.Response.WriteAsync($"{s}: not found\n", ServerUtil.Encoding);
					continue;
				}

				var si = new FixedSoundItem(s, DEVICE_INDEX);

				var b = Lib.TryAdd(si);
				await ctx.Response.WriteAsync($"{si}: {b}\n", ServerUtil.Encoding);

				// LoggerExtensions.LogInformation(Program._logger, "Added {Sound}: {OK}", si, b);
			}

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
			ctx.Response.ContentType = MediaTypeNames.Text.Plain;

			var snds = await GetSoundsBySelectionModeAsync(ctx, MODE_SIMPLE);

			foreach (BaseSoundItem snd in snds) {

				var b = Lib.TryRemove(snd);
				await ctx.Response.WriteAsync($"{snd}: {b}\n", ServerUtil.Encoding);
				Program._logger.LogInformation("Removed {Sound}: {OK}", snd, b);

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

		}

		/// <summary>
		/// <list type="bullet">
		/// <item>Body: single [regex pattern]</item>
		/// <item>Body: multiple [name contains]</item>
		/// </list>
		/// </summary>
		public static async Task PauseAsync(HttpContext context)
		{
			var snds = await GetSoundsBySelectionModeAsync(context, MODE_SIMPLE);

			context.Response.ContentType = MediaTypeNames.Text.Plain;

			foreach (var si in snds) {
				si.Pause();
				await context.Response.WriteAsync($"{si}\n", ServerUtil.Encoding);
				Program._logger.LogInformation("Paused {Sound}", si);

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
			var snds = await GetSoundsBySelectionModeAsync(context, MODE_SIMPLE);

			context.Response.ContentType = MediaTypeNames.Text.Plain;

			foreach (var si in snds) {
				si.Stop();
				await context.Response.WriteAsync($"{si}\n", ServerUtil.Encoding);
				Program._logger.LogInformation("Stopped {Sound}", si);

			}

			await context.Response.CompleteAsync();

		}

		public static async Task UpdateAsync(HttpContext context)
		{
			context.Response.ContentType = MediaTypeNames.Text.Plain;
			var snds  = await GetSoundsBySelectionModeAsync(context, MODE_SIMPLE);
			var items = snds as BaseSoundItem[] ?? snds.ToArray<BaseSoundItem>();
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

		}

		/// <summary>
		/// <list type="bullet">
		/// <item>Body: none</item>
		/// </list>
		/// </summary>
		public static async Task StatusAsync(HttpContext context)
		{
			IEnumerable<BaseSoundItem> snds    = await GetSoundsBySelectionModeAsync(context);
			snds = snds.Where(s => s.Status.IsIndeterminate()).ToArray();

			context.Response.ContentType = MediaTypeNames.Text.Plain;

			if (!snds.Any()) {
				await context.Response.WriteAsync("---");
			}

			foreach (var si in snds) {
				await context.Response.WriteAsync($"{si}\n", ServerUtil.Encoding);
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
			ctx.Response.ContentType = MediaTypeNames.Text.Plain;

			foreach ((var key, var _) in Lib.Sounds) {
				await ctx.Response.WriteAsync($"{key.Name}\n", ServerUtil.Encoding);

			}

			await ctx.Response.CompleteAsync();
		}

		public static async Task AddYouTubeAudioUrlAsync(HttpContext context)
		{
			context.Response.ContentType = MediaTypeNames.Text.Plain;
			var b  = await context.ReadBodyTextAsync();
			var ok = await Lib.AddYtdlpAudioUrlAsync(b, DEVICE_INDEX);
			await context.Response.WriteAsync($"{b} : {ok}", ServerUtil.Encoding);

			await context.Response.CompleteAsync();

		}

		public static async Task AddYouTubeAudioFileAsync(HttpContext context)
		{
			context.Response.ContentType = MediaTypeNames.Text.Plain;
			var b  = await context.ReadBodyTextAsync();
			var ok = await Lib.AddYtdlpAudioFileAsync(b, DEVICE_INDEX);
			await context.Response.WriteAsync($"{b} : {ok}", ServerUtil.Encoding);

			await context.Response.CompleteAsync();

		}

		public const string MODE_SIMPLE = "Simple";

		public const string MODE_REGEX = "Regex";

		public const string HDR_MODE = "Mode";

		public const string HDR_VOL = "Vol";

		private static async Task<IEnumerable<BaseSoundItem>> GetSoundsBySelectionModeAsync(HttpContext context,
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
					snds = Lib.FindByPattern(bodyEntries[0]);
					break;
				case MODE_SIMPLE:
					snds = Lib.FindSoundsByNames(bodyEntries);
					break;
				default:
					goto case MODE_SIMPLE;
			}

			return snds;
		}

		private const string ALL = "*";

	}
}