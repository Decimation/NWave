// Author: Deci | Project: NWave | Name: PlayCommand.cs
// Date: 2025/07/09 @ 19:07:55

#nullable disable
using Flurl;
using NWave.Lib;
using NWave.Lib.Model;
using Spectre.Console;
using Spectre.Console.Cli;

namespace NWave;

public class PlayCommand : AsyncCommand<PlayCommandOptions>
{

	public override async Task<int> ExecuteAsync(CommandContext context, PlayCommandOptions settings)
	{
		var sl = new SoundLibrary();

		// var ok = sl.;
		BaseSoundItem bsi = null;

		var src = settings.Source;

		AC.WriteLine($"Adding {src}");

		if (File.Exists(src)) {
			bsi = new FixedSoundItem(src, settings.DeviceId);
		}
		else if (Url.IsValid(src)) {

			await AC.Status().StartAsync("Adding YT", async (r) =>
			{
				//
				bsi = await YouTubeSoundItem.FromAudioUrlAsync(src);

				r.Refresh();
			});
		}
		else {
			return -1;
		}


		var ok = sl.TryAdd(bsi);
		var ar = new AutoResetEvent(false);

		if (ok) {
			// AC.WriteLine($"{bsi}");

			bsi.Out.PlaybackStopped += (sender, args) =>
			{
				//
				ar.Set();
			};

			await AC.Progress().StartAsync(async (p) =>
			{
				var tas = p.AddTask($"Playing {bsi.Name}", true, bsi.Length);
				bsi.Play();

				do {
					tas.Increment(bsi.Position);

					// tas.Description = $"";

				} while (!ar.WaitOne(TimeSpan.FromMilliseconds(100)));

			});
		}


		// ar.WaitOne();

		return 0;
	}

}