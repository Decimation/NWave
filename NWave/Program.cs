#define WINDOWS
using System.Management;
using NAudio;
using NAudio.Wave;
using NAudio.Wave;
using NAudio.CoreAudioApi;
namespace NWave
{
	internal class Program
	{
		static void Main(string[] args)
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
				var c=WaveOut.GetCapabilities(a);
				Console.WriteLine($"{c.ProductName} {c.Channels}");
			}

			int    deviceIndex   = 0;                     // Change this to the index of your desired audio device
			string audioFilePath = @"H:\Other Music\Audio resources\NMicspam\ow.wav"; // Change this to the path of your audio file
			
			var waveOut = new WaveOutEvent();
			waveOut.DeviceNumber = deviceIndex; // Set the desired audio device
			
			using (var audioFileReader = new AudioFileReader(audioFilePath)) {
				waveOut.Init(audioFileReader);
				waveOut.Play();
				
				while (waveOut.PlaybackState == PlaybackState.Playing) {
					System.Threading.Thread.Sleep(100);
				}
			}
		}
	}
}