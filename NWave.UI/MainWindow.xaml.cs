using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Flurl.Http;
using Microsoft.Extensions.Logging;
using NAudio.Wave;

namespace NWave.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{
	private const string SERVER = $"https://localhost:7182";
	private const string SOUNDS = @"H:\Other Music\Audio resources\NMicspam\";

	public MainWindow()
	{
		DataContext = this;
		InitializeComponent();

		var soundFIles = Directory.EnumerateFiles(SOUNDS, searchPattern: "*.*", new EnumerationOptions()
		{
			RecurseSubdirectories = true
		});

		Sounds = new ObservableCollection<SoundItem>(soundFIles.Select(x => new SoundItem(x, DEVICE_INDEX)));

		Lv_Sounds.ItemsSource = Sounds;

		m_bg = new DispatcherTimer(DispatcherPriority.Background)
		{
			Interval  = TimeSpan.FromSeconds(1),
			IsEnabled = true,

		};
		m_bg.Tick += Ticker;

	}

	private readonly SemaphoreSlim m_semaphore = new SemaphoreSlim(1);

	private async void Ticker(object? sender, EventArgs args)
	{
		if (await m_semaphore.WaitAsync(TimeSpan.Zero)) {
			return;
		}

		m_semaphore.Release();

	}

	private readonly DispatcherTimer m_bg;

	private SoundItem m_selected;

	public SoundItem Selected
	{
		get => m_selected;
		set
		{
			if (Equals(value, m_selected)) return;
			m_selected = value;
			OnPropertyChanged();
		}
	}

	public ObservableCollection<SoundItem> Sounds { get; }

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

	private void Btn_Play_Click(object sender, RoutedEventArgs e)
	{

		Dispatcher.BeginInvoke(() =>
		{
			Selected.Play();
		});

		e.Handled = true;
	}

	private const int DEVICE_INDEX = 3;

	private void Btn_Stop_Click(object sender, RoutedEventArgs e)
	{
		Dispatcher.BeginInvoke(() =>
		{
			Selected.Stop();

		});
		e.Handled = true;
	}
}