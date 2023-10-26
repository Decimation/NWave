// Read S NWave.UI SoundStatus.cs
// 2023-09-28 @ 9:05 PM

#nullable disable
using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using NAudio.Wave;

namespace NWave.Lib;

public class SoundItem : INotifyPropertyChanged, IDisposable
{
	public string Name { get; }

	public string FullName { get; }

	private PlaybackStatus m_status;

	public AudioFileReader Provider { get; private set; }

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

	public IWavePlayer Out { get; private set; }

	public int DeviceIndex { get; }

	public bool IsDisposed { get; private set; }

	public double PlaybackProgress => Math.Round((Provider.Position / (double) Provider.Length), 4);

	public SoundItem(string fullName, int idx)
	{
		if (!File.Exists(fullName)) {
			throw new FileNotFoundException();
		}

		FullName    = fullName;
		Name        = Path.GetFileName(FullName);
		Status      = PlaybackStatus.None;
		DeviceIndex = idx;

		Out = new WaveOutEvent()
		{
			DeviceNumber = DeviceIndex
		};
		Out.PlaybackStopped += OnHandler;

		Provider = new AudioFileReader(FullName);

		Out.Init(Provider);

	}

	private void OnHandler([CanBeNull] object sender, StoppedEventArgs args)
	{
		Status = PlaybackStatus.Stopped;

	}

	public void UpdateProperties()
	{
		OnPropertyChanged(nameof(PlaybackProgress));
	}

	public void PlayPause()
	{
		if (Status == PlaybackStatus.Playing) {
			Pause();
		}
		else if (Status.IsPlayable()) {
			Play();
		}
	}

	public void Pause()
	{
		CheckDisposed();
		Status = PlaybackStatus.Paused;
		Out.Pause();
	}

	public void Play()
	{
		CheckDisposed();

		if (Status == PlaybackStatus.Stopped) {
			Provider.Position = 0;
		}

		Status = PlaybackStatus.Playing;
		Out.Play();
	}

	public void Stop()
	{
		CheckDisposed();
		Out.Stop();
		// Dispose();
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged([CanBeNull] [CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	protected bool SetField<T>(ref T field, T value, [CanBeNull] [CallerMemberName] string propertyName = null)
	{
		if (EqualityComparer<T>.Default.Equals(field, value)) return false;
		field = value;
		OnPropertyChanged(propertyName);
		return true;
	}

	private void CheckDisposed()
	{
		if (IsDisposed) {
			throw new ObjectDisposedException($"{Name}", "Disposed");

		}
	}

	public void Dispose()
	{
		Stop();
		Out.PlaybackStopped -= OnHandler;
		Out.Dispose();
		Out = null;
		Provider.Dispose();
		Provider   = null;
		IsDisposed = true;

	}

	public override string ToString()
	{
		return $"{Name} | {Status} | {DeviceIndex} || {Provider.Volume} | {Provider.Position} | {Provider.Length}";
	}
}

public enum PlaybackStatus
{
	None,
	Playing,
	Paused,
	Stopped,
}