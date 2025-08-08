// Read S NWave.Lib DynamicSoundItem.cs
// 2023-11-23 @ 2:55 PM

#nullable disable
using Flurl;
using NAudio.Wave;

namespace NWave.Lib.Model;

public class DynamicSoundItem : BaseSoundItem
{

	public override float? Volume
	{
		get => null;
		set => throw new NotSupportedException($"{Name}: {nameof(SupportsVolume)} not supported");
	}

	public override bool SupportsVolume => false;

	public DynamicSoundItem(string fullName, int idx = DEFAULT_DEVICE_INDEX) 
		: base(fullName, idx)
	{
		Out = new WaveOut()
		{
			DeviceNumber = idx
		};
		Provider = new MediaFoundationReader(fullName);
		Out.Init(Provider);
	}

}