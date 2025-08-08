// Author: Deci | Project: NWave.Lib | Name: BaseSoundItem.cs
// Date: 2023/11/23 @ 14:11:27

#nullable disable

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using NAudio.Wave;

namespace NWave.Lib.Model;

public abstract class BaseSoundItem : INotifyPropertyChanged, IDisposable
{

	protected PlaybackStatus m_status;

	public string Name { get; }

	public string FullName { get; }

	public int DeviceIndex { get; }

	public PlaybackStatus Status
	{
		get => m_status;
		protected set => SetField(ref m_status, value);
	}

	[JIGN]
	public bool IsDisposed { get; protected set; }

	[JIGN]
	public IWavePlayer Out { get; protected set; }

	[JIGN]
	public WaveStream Provider { get; protected set; }

	public abstract float? Volume { get; set; }

	public virtual long Length => Provider.Length;

	public virtual long Position
	{
		get => Provider.Position;
		set => Provider.Position = value;
	}

	public double PlaybackProgress => Math.Round(Position / (double) Length, 4);

	[JIGN]
	[MemberNotNullWhen(true, nameof(Volume))]
	public abstract bool SupportsVolume { get; }

	protected BaseSoundItem(string fullName, int idx = DEFAULT_DEVICE_INDEX)
	{
		/*if (!File.Exists(fullName)) {
			throw new FileNotFoundException();
		}*/

		FullName    = fullName;
		Name        = Path.GetFileName(FullName);
		Status      = PlaybackStatus.None;
		DeviceIndex = idx;

	}

	protected void OnHandler([CBN] object sender, StoppedEventArgs args)
	{
		Status = PlaybackStatus.Stopped;

	}

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
		// CheckDisposed();
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

	protected virtual void OnPropertyChanged([CBN] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	protected bool SetField<T>(ref T field, T value, [CBN] [CMN] string propertyName = null)
	{
		if (EqualityComparer<T>.Default.Equals(field, value))
			return false;

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

	public override string ToString()
	{
		return $"{Name} | {Status} | {DeviceIndex} | {Position} | {Length}";
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

	public virtual event PropertyChangedEventHandler PropertyChanged;

	public const int DEFAULT_DEVICE_INDEX = -1;

}

public enum PlaybackStatus
{

	None = 0,
	Playing,
	Paused,
	Stopped,

}