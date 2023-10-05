// Read S NWave.UI SoundStatus.cs
// 2023-09-28 @ 9:05 PM

using System.ComponentModel;
using System.Runtime.CompilerServices;
using NAudio.Wave;

namespace NWave.Lib;

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

	public WaveOutEvent Out { get; set; }

	public int DeviceIndex { get; }

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

	public void PlayPause()
	{
		if (Status == PlaybackStatus.Playing) {
			Pause();
		}
		else if (Status == PlaybackStatus.Paused || Status == PlaybackStatus.Stopped || Status == PlaybackStatus.None) {
			Play();
		}
	}

	public void Pause()
	{
		Status = PlaybackStatus.Paused;
		Out.Pause();
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
		Stop();
		Out.PlaybackStopped -= OnHandler;
		Out.Dispose();
		FileReader.Dispose();
	}

	public override string ToString()
	{
		return $"{Name} : {Status}";
	}
}

public enum PlaybackStatus
{
	None,
	Playing,
	Paused,
	Stopped,
}