// Read S NWave.UI SoundStatus.cs
// 2023-09-28 @ 9:05 PM

#nullable disable
using NAudio.Wave;

namespace NWave.Lib.Model;

public class FixedSoundItem : BaseSoundItem
{

	public FixedSoundItem(string fullName, int idx = DEFAULT_DEVICE_INDEX) 
		: base(fullName, idx)
	{
		Out = new WaveOut()
		{
			DeviceNumber = DeviceIndex
		};

		Out.PlaybackStopped += OnHandler;

		Provider = new AudioFileReader(FullName);

		Out.Init(Provider);
	}

	public override bool SupportsVolume => true;

	public override float? Volume
	{
		get => ((AudioFileReader) Provider).Volume;
		set => ((AudioFileReader) Provider).Volume = SoundUtility.ClampVolume(value.Value);
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