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

	public DynamicSoundItem(string fileName, int idx = SoundLibrary.DEFAULT_DEVICE_INDEX, int? id = null) 
		: base(fileName, idx, id)
	{
		Out = new WaveOut()
		{
			DeviceNumber = idx
		};
		Provider = new MediaFoundationReader(fileName);
		Out.Init(Provider);
	}

}