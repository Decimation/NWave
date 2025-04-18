﻿#define WINDOWS
using System.Management;
using System.Net;
using NAudio;
using NAudio.Wave;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using NWave.Lib;

namespace NWave;

internal class Program
{
	static async Task Main(string[] args)
	{
		var x = await YouTubeSoundItem.FromAudioUrlAsync("https://youtu.be/s9SqpD00T98?si=CtuOkLCp-8h5XK1a");
		Console.WriteLine(x);


		var b = SoundUtility.GetWindowsDevices();
		Console.WriteLine(b);
		
		foreach (ManagementBaseObject o in SoundUtility.WindowsSoundDevices.Value) {
			Console.WriteLine(o.Properties);

			foreach (PropertyData propertyData in o.Properties) {
				Console.WriteLine($"{propertyData.Name} {propertyData.Value}");
			}
		}

		Console.WriteLine();

		foreach (var device in SoundUtility.GetWaveOutDevices()) {
			Console.WriteLine($"{device.Key} {device.Value.ProductName}");
		}
	}

	static void Test1()
	{
		/*var objSearcher = new ManagementObjectSearcher(
			"SELECT * FROM Win32_SoundDevice");

		var objCollection = objSearcher.Get();
		foreach (var d in objCollection)
		{
			Console.WriteLine("=====DEVICE====");
			foreach (var p in d.Properties)
			{
				Console.WriteLine($"{p.Name}:{p.Value}");
			}
		}*/

		for (int a = 0; a < WaveOut.DeviceCount; a++) {
			var c = WaveOut.GetCapabilities(a);
			Console.WriteLine($"{c.ProductName} {c.Channels}");
		}

		int deviceIndex = 0; // Change this to the index of your desired audio device

		string audioFilePath =
			@"H:\Other Music\Audio resources\NMicspam\ow.wav"; // Change this to the path of your audio file

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