// Read S NWave.UI SoundStatus.cs
// 2023-09-28 @ 9:05 PM

#nullable disable
using NAudio.Wave;

namespace NWave.Lib;

public class FixedSoundItem : BaseSoundItem
{

	public FixedSoundItem(string fileName, int idx = SoundLibrary.DEFAULT_DEVICE_INDEX, int? id = null) 
		: base(fileName, idx, id)
	{
		Out = new WaveOut()
		{
			DeviceNumber = DeviceIndex
		};

		Out.PlaybackStopped += OnHandler;

		Provider = new AudioFileReader(FileName);

		Out.Init(Provider);
	}

	public override bool SupportsVolume => true;

	public override float Volume
	{
		get => ((AudioFileReader) Provider).Volume;
		set => ((AudioFileReader) Provider).Volume = SoundUtility.ClampVolume(value);
	}

	public override string ToString()
	{
		return $"{base.ToString()} | {Volume}";
	}

	public override void Dispose()
	{
		base.Dispose();

	}

}