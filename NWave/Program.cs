#define WINDOWS
#nullable disable
global using NN = System.Diagnostics.CodeAnalysis.NotNullAttribute;
global using AC = Spectre.Console.AnsiConsole;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Management;
using System.Net;
using Flurl;
using JetBrains.Annotations;
using NAudio;
using NAudio.Wave;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using NWave.Lib;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Cli.Help;

namespace NWave;

public static class Program
{

	public class PlayCommandOptions : CommandSettings
	{

		[DefaultValue(SoundLibrary.DEFAULT_DEVICE_INDEX)]
		[CommandArgument(0, "[deviceId]")]
		public int DeviceId { get; set; }

		[NN]
		[CommandOption("--source")]
		public string Source { get; set; }

	}

	public class PlayCommand : AsyncCommand<PlayCommandOptions>
	{

		public override async Task<int> ExecuteAsync(CommandContext context, PlayCommandOptions settings)
		{
			var sl = new SoundLibrary();

			// var ok = sl.;
			BaseSoundItem bsi = null;

			var src = settings.Source;

			AC.WriteLine($"Adding {src}");

			if (File.Exists(src))
			{
				bsi = new FixedSoundItem(src, settings.DeviceId);
			}
			else if (Url.IsValid(src))
			{

				await AC.Status().StartAsync("Adding YT", async (r) =>
				{
					//
					bsi = await YouTubeSoundItem.FromAudioUrlAsync(src);
					r.Refresh();
				});
			}
			else
			{
				return -1;
			}

			
			var ok = sl.TryAdd(bsi);

			if (ok)
			{
				AC.WriteLine($"{bsi}");
				bsi.Play();
			}

			var ar = new AutoResetEvent(false);

			bsi.Out.PlaybackStopped += (sender, args) =>
			{
				ar.Set();
			};

			ar.WaitOne();

			return 0;
		}

		public static async Task<int> Main(string[] args)
		{
			var ca = new CommandApp<PlayCommand>();

			/*var b = SoundUtility.GetWindowsDevices();

			foreach (ManagementBaseObject o in SoundUtility.WindowsSoundDevices.Value)
			{
				foreach (PropertyData propertyData in o.Properties)
				{
					AC.WriteLine($"{propertyData.Name} {propertyData.Value}");
				}
			}

			AC.WriteLine();

			foreach (var device in SoundUtility.GetWaveOutDevices())
			{
				AC.WriteLine($"{device.Key} {device.Value.ProductName}");
			}*/

			var i = await ca.RunAsync(args);
			return i;
		}

		static void Test1()
		{
			/*var objSearcher = new ManagementObjectSearcher(
				"SELECT * FROM Win32_SoundDevice");

			var objCollection = objSearcher.Get();
			foreach (var d in objCollection)
			{
				AC.WriteLine("=====DEVICE====");
				foreach (var p in d.Properties)
				{
					AC.WriteLine($"{p.Name}:{p.Value}");
				}
			}*/

			for (int a = 0; a < WaveOut.DeviceCount; a++)
			{
				var c = WaveOut.GetCapabilities(a);
				AC.WriteLine($"{c.ProductName} {c.Channels}");
			}

			int deviceIndex = 0; // Change this to the index of your desired audio device

			string audioFilePath =
				@"H:\Other Music\Audio resources\NMicspam\ow.wav"; // Change this to the path of your audio file

			var waveOut = new WaveOutEvent();
			waveOut.DeviceNumber = deviceIndex; // Set the desired audio device

			using (var audioFileReader = new AudioFileReader(audioFilePath))
			{
				waveOut.Init(audioFileReader);
				waveOut.Play();

				while (waveOut.PlaybackState == PlaybackState.Playing)
				{
					Thread.Sleep(100);
				}
			}

		}

	}

}