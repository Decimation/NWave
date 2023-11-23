// Read S NWave.Lib DynamicSoundItem.cs
// 2023-11-23 @ 2:55 PM

#nullable disable
using Flurl;
using NAudio.Wave;

namespace NWave.Lib;

public class DynamicSoundItem : BaseSoundItem
{

	public override float Volume
	{
		get => VOL_INVALID;
		set => throw new NotSupportedException();
	}

	public override bool SupportsVolume => false;

	public Url Url { get; }

	public DynamicSoundItem(string url, string fullName, int idx) : base(fullName, idx)
	{
		Url      = url;
		Out      = new WaveOut()
		{
			DeviceNumber = idx

		};
		Provider = new MediaFoundationReader(url);
		Out.Init(Provider);
	}

}