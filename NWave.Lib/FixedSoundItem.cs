﻿// Read S NWave.UI SoundStatus.cs
// 2023-09-28 @ 9:05 PM

#nullable disable
using NAudio.Wave;

namespace NWave.Lib;

public class FixedSoundItem : BaseSoundItem
{

	public FixedSoundItem(string fullName, int idx) : base(fullName, idx)
	{
		Out = new WaveOut()
		{
			DeviceNumber = DeviceIndex
		};

		Out.PlaybackStopped += OnHandler;

		Provider = new AudioFileReader(FullName);

		Out.Init(Provider);
	}

	public override void Dispose()
	{
		base.Dispose();

	}

	public override bool SupportsVolume => true;

	public override float Volume
	{
		get => ((AudioFileReader) Provider).Volume;
		set => ((AudioFileReader) Provider).Volume = value;
	}

	public override string ToString()
	{
		return $"{base.ToString()} | {Volume}";
	}

}

public enum PlaybackStatus
{

	None,
	Playing,
	Paused,
	Stopped,

}