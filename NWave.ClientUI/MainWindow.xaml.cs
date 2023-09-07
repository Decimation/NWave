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
using Path = System.IO.Path;

namespace NWave.ClientUI
{
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

			Sounds = new ObservableCollection<SoundStatus>(soundFIles.Select(x => new SoundStatus(x)));
			Sounds.Insert(0, new SoundStatus("*"));

			Lv_Sounds.ItemsSource = Sounds;

			m_client = new FlurlClient(SERVER);

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

			var snd = Sounds.FirstOrDefault(x => x.Status == Status.Playing);

			if (snd == null) {
				return;
			}

			var res = await m_client.Request("/IsPlaying")
				          .WithHeaders(new { s = snd.FullName })
				          .GetAsync();

			var s = await res.GetStringAsync();

			if (s == "False") {
				snd.Status = Status.Stopped;
			}

			m_semaphore.Release();

		}

		private readonly DispatcherTimer m_bg;
		private readonly FlurlClient     m_client;

		private SoundStatus m_selected;

		public SoundStatus Selected
		{
			get => m_selected;
			set
			{
				if (Equals(value, m_selected)) return;
				m_selected = value;
				OnPropertyChanged();
			}
		}

		public ObservableCollection<SoundStatus> Sounds { get; }

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
			Application.Current.Dispatcher.Invoke(() =>
			{
				var res = m_client.Request("/Play")
					.WithHeaders(new { s = Selected.FullName })
					.GetAsync();
				Selected.Status = Status.Playing;
			});
			e.Handled = true;
		}

		private void Btn_Stop_Click(object sender, RoutedEventArgs e)
		{
			Dispatcher.Invoke(() =>
			{
				var res = m_client.Request("/Stop")
					.WithHeaders(new { s = Selected.FullName })
					.GetAsync();
				Selected.Status = Status.Stopped;
			});
			e.Handled = true;
		}
	}

	public class SoundStatus : INotifyPropertyChanged
	{
		public string Name { get; }

		public string FullName { get; set; }

		private Status m_status;

		public Status Status
		{
			get => m_status;
			set
			{
				if (value == m_status) return;
				m_status = value;
				OnPropertyChanged();
			}
		}

		public SoundStatus(string fullName)
		{
			FullName = fullName;
			Name     = Path.GetFileName(FullName);
			Status   = Status.None;
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
	}

	public enum Status
	{
		None,
		Playing,
		Stopped,
	}
}