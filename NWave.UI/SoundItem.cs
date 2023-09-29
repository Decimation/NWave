// Read S NWave.UI SoundStatus.cs
// 2023-09-28 @ 9:05 PM

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using NAudio.Wave;

namespace NWave.UI;

public class SoundItem : INotifyPropertyChanged, IDisposable
{
	public string Name { get; }

	public string FullName { get; }

	private PlaybackStatus m_status;

	public AudioFileReader FileReader { get; }

	public PlaybackStatus Status
	{
		get => m_status;
		set
		{
			if (value == m_status) return;
			m_status = value;
			OnPropertyChanged();
		}
	}

	public WaveOutEvent Out         { get; set; }
	public int          DeviceIndex { get; }

	public SoundItem(string fullName, int idx)
	{
		FullName    = fullName;
		Name        = Path.GetFileName(FullName);
		Status      = PlaybackStatus.None;
		DeviceIndex = idx;

		Out = new WaveOutEvent()
		{
			DeviceNumber = DeviceIndex
		};
		Out.PlaybackStopped += OnHandler;
		FileReader          =  new AudioFileReader(FullName);

		Out.Init(FileReader);
	}

	private void OnHandler(object? sender, StoppedEventArgs args)
	{
		Status = PlaybackStatus.Stopped;

	}

	public void Play()
	{
		if (Status == PlaybackStatus.Stopped) {
			FileReader.Position = 0;
		}

		Status = PlaybackStatus.Playing;
		Out.Play();
	}

	public void Stop()
	{
		Out.Stop();
		// Dispose();
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
	{
		if (EqualityComparer<T>.Default.Equals(field, value)) return false;
		field = value;
		OnPropertyChanged(propertyName);
		return true;
	}

	public void Dispose()
	{
		Out.PlaybackStopped -= OnHandler;
		Out.Dispose();
		FileReader.Dispose();
	}
}

public enum PlaybackStatus
{
	None,
	Playing,
	Stopped,
}