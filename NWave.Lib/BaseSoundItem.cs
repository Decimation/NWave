// Read S NWave.Lib BaseSoundItem.cs
// 2023-11-23 @ 2:55 PM

#nullable disable
using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using NAudio.Wave;

namespace NWave.Lib;

public abstract class BaseSoundItem : INotifyPropertyChanged, IDisposable
{
	public    int?            Id { get; }

	protected PlaybackStatus m_status;

	public string Name { get; }

	public string FullName { get; }

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

	public int DeviceIndex { get; }

	public bool IsDisposed { get; protected set; }

	public IWavePlayer Out { get; protected set; }

	public WaveStream Provider { get; protected set; }

	protected void OnHandler([CanBeNull] object sender, StoppedEventArgs args)
	{
		Status = PlaybackStatus.Stopped;

	}

	public const float VOL_INVALID = float.NaN;

	public abstract float Volume { get; set; }

	public virtual long Length => Provider.Length;

	public virtual long Position
	{
		get => Provider.Position;
		set => Provider.Position = value;
	}

	public double PlaybackProgress => Math.Round((Position / (double) Length), 4);

	public void UpdateProperties()
	{
		OnPropertyChanged(nameof(PlaybackProgress));
		OnPropertyChanged(nameof(Status));
		OnPropertyChanged(nameof(Volume));
	}

	public virtual void PlayPause()
	{
		if (Status == PlaybackStatus.Playing) {
			Pause();
		}
		else if (Status.IsPlayable()) {
			Play();
		}
	}

	public virtual void Pause()
	{
		CheckDisposed();
		Status = PlaybackStatus.Paused;
		Out.Pause();
	}

	public virtual void Reset()
	{
		Position = 0;
	}

	public virtual void Play()
	{
		CheckDisposed();

		if (Status == PlaybackStatus.Stopped) {
			Reset();
		}

		Status = PlaybackStatus.Playing;
		Out.Play();
	}

	public virtual void Stop()
	{
		CheckDisposed();
		Status = PlaybackStatus.Stopped;
		Out.Stop();
		// Dispose();
	}

	public virtual event PropertyChangedEventHandler PropertyChanged;

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

	protected virtual void CheckDisposed()
	{
		if (IsDisposed) {
			throw new ObjectDisposedException($"{Name}", "Disposed");

		}
	}

	public virtual void Dispose()
	{
		Out.PlaybackStopped -= OnHandler;
		Out.Dispose();
		Out = null;

		if (Provider is IDisposable d) {
			d.Dispose();

		}

		Provider = null;

		IsDisposed = true;

	}

	protected BaseSoundItem(string fullName, int idx, int? id = null)
	{
		/*if (!File.Exists(fullName)) {
			throw new FileNotFoundException();
		}*/

		FullName    = fullName;
		Name        = Path.GetFileName(FullName);
		Status      = PlaybackStatus.None;
		DeviceIndex = idx;
		Id          = id ?? Name.GetHashCode();
	}
	public override string ToString()
	{
		return $"{Name} | {Status} | {DeviceIndex} | {Position} | {Length}";
	}

	public abstract bool SupportsVolume { get; }
}