#define WINDOWS
#nullable disable
global using NN = System.Diagnostics.CodeAnalysis.NotNullAttribute;
global using AC = Spectre.Console.AnsiConsole;
using System.Diagnostics.CodeAnalysis;
using System.Management;
using System.Net;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using NAudio;
using NAudio.Wave;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Cli.Help;

namespace NWave;

public static class Program
{

	public static readonly ILoggerFactory Factory = LoggerFactory.Create(b =>
	{
		//
		b.AddConsole().AddDebug().AddTraceSource("TRACE");
		b.SetMinimumLevel(LogLevel.Trace);

	});

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


	private static void Test1()
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

		for (int a = 0; a < WaveOut.DeviceCount; a++) {
			var c = WaveOut.GetCapabilities(a);
			AC.WriteLine($"{c.ProductName} {c.Channels}");
		}

		int deviceIndex = 0;

		string audioFilePath = @"H:\Other Music\Audio resources\NMicspam\ow.wav"; // Change this to the path of your audio file

		var waveOut = new WaveOutEvent();
		waveOut.DeviceNumber = deviceIndex; // Set the desired audio device

		using (var audioFileReader = new AudioFileReader(audioFilePath)) {
			waveOut.Init(audioFileReader);
			waveOut.Play();

			while (waveOut.PlaybackState == PlaybackState.Playing) {
				Thread.Sleep(100);
			}
		}

	}

}