// $User.Name $File.ProjectName $File.FileName
// $File.CreatedYear-$File.CreatedMonth-$File.CreatedDay @ $File.CreatedHour:$File.CreatedMinute

using System.Management;
using System.Text;
using CliWrap;
using JetBrains.Annotations;
using NAudio.Wave;

#nullable disable
namespace NWave.Lib;

public static class SoundUtility
{

	public static Lazy<ManagementObjectCollection> WindowsSoundDevices { get; private set; }

	static SoundUtility()
	{
		WindowsSoundDevices = new Lazy<ManagementObjectCollection>(GetWindowsDevices, LazyThreadSafetyMode.None);
	}

	[MURV]
	public static ManagementObjectCollection GetWindowsDevices()
	{
		using var objSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_SoundDevice");

		var objCollection = objSearcher.Get();

		return objCollection;
	}

	public static Dictionary<int, WaveOutCapabilities> GetWaveOutDevices()
	{
		var map = Enumerable.Range(0, WaveOut.DeviceCount)
			.ToDictionary(e => e, WaveOut.GetCapabilities);

		return map;
	}

	public static float ClampVolume(float f)
	{
		if (f < 0f || f > 1.0f) {
			f /= 100f;
			f =  Math.Clamp(f, 0f, 1.0f);
		}

		return f;
	}

}