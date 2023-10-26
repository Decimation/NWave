global using CMN = System.Runtime.CompilerServices.CallerMemberNameAttribute;
global using MNNW = System.Diagnostics.CodeAnalysis.MemberNotNullWhenAttribute;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Flurl.Http;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using Novus.Win32;
using Novus.Win32.Structures.User32;
using NWave.Lib;

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

		var files = Directory.EnumerateFiles(SOUNDS, searchPattern: "*.*", new EnumerationOptions()
		{
			RecurseSubdirectories = true
		});

		Sounds = new ObservableCollection<SoundItem>(files.Select(x => new SoundItem(x, DEVICE_INDEX)));

		Lv_Sounds.ItemsSource = Sounds;

		m_bg = new DispatcherTimer(DispatcherPriority.Background)
		{
			Interval  = TimeSpan.FromSeconds(0.33),
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

		foreach (SoundItem item in Sounds.Where(x=>x.Status.IsIndeterminate())) {
			// Debug.WriteLine($"updating {item}");
			item.UpdateProperties();
		}

		m_semaphore.Release();

	}

	private readonly DispatcherTimer m_bg;

	private SoundItem m_selected;

	private WindowInteropHelper m_h;

	private HwndSource m_wnd;

	public SoundItem? Selected
	{
		get => m_selected;
		set
		{
			if (Equals(value, m_selected)) return;
			m_selected = value;
			OnPropertyChanged();
		}
	}

	[MNNW(true, nameof(Selected))]
	public bool HasSelected => Lv_Sounds.SelectedIndex != -1;

	public ObservableCollection<SoundItem> Sounds { get; private set; }

	private readonly SoundLibrary m_lib;

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged([CMN] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	protected bool SetField<T>(ref T field, T value, [CMN] string? propertyName = null)
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

			Selected?.Play();
		});

		e.Handled = true;
	}

	private const int DEVICE_INDEX = 3;

	public const int HOOK_ID  = 9000;
	public const int HOOK_ID1 = 9001;
	public const int HOOK_ID2 = 9002;

	private void Btn_Stop_Click(object sender, RoutedEventArgs e)
	{
		Dispatcher.BeginInvoke(() =>
		{
			Selected?.Stop();

		});
		e.Handled = true;
	}

	private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
	{
		const int WM_HOTKEY = 0x0312;

		switch (msg) {
			case WM_HOTKEY:
				switch (wParam.ToInt32()) {
					case HOOK_ID:
						int vkey = (((int) lParam >> 16) & 0xFFFF);

						if (vkey == (uint) VirtualKey.KEY_P) {
							//handle global hot key here...
							Debug.Print($"{vkey}!!");

							Dispatcher.BeginInvoke(() =>
							{

								Selected?.PlayPause();
							});
						}

						handled = true;
						break;
					case HOOK_ID1:
						int vkey1 = (((int) lParam >> 16) & 0xFFFF);

						if (vkey1 == (uint) VirtualKey.UP) {
							//handle global hot key here...
							Debug.Print($"{vkey1}!!");
							Lv_Sounds.SelectedIndex--;
						}

						handled = true;
						break;
					case HOOK_ID2:
						int vkey2 = (((int) lParam >> 16) & 0xFFFF);

						if (vkey2 == (uint) VirtualKey.DOWN) {
							//handle global hot key here...
							Debug.Print($"{vkey2}!!");
							Lv_Sounds.SelectedIndex++;

						}

						handled = true;
						break;
				}

				break;
		}

		return IntPtr.Zero;
	}

	private void OnSourceInitialized(object? sender, EventArgs e)
	{
		m_h   = new WindowInteropHelper(this);
		m_wnd = HwndSource.FromHwnd(m_h.Handle);
		m_wnd.AddHook(HwndHook);

		Native.RegisterHotKey(m_h.Handle, HOOK_ID, HotKeyModifiers.MOD_CONTROL | HotKeyModifiers.MOD_SHIFT,
		                      (uint) VirtualKey.KEY_P);

		Native.RegisterHotKey(m_h.Handle, HOOK_ID1, HotKeyModifiers.MOD_CONTROL | HotKeyModifiers.MOD_SHIFT,
		                      (uint) VirtualKey.UP);

		Native.RegisterHotKey(m_h.Handle, HOOK_ID2, HotKeyModifiers.MOD_CONTROL | HotKeyModifiers.MOD_SHIFT,
		                      (uint) VirtualKey.DOWN);
	}

	private void OnClosing(object? sender, CancelEventArgs e)
	{
		Native.UnregisterHotKey(m_h.Handle, HOOK_ID);
		Native.UnregisterHotKey(m_h.Handle, HOOK_ID1);
		Native.UnregisterHotKey(m_h.Handle, HOOK_ID2);
		Debug.Print("Unregistered");
	}

	private void Lv_Sounds_DragOver(object sender, DragEventArgs e)
	{
		e.Handled = true;
	}

	private void Lv_Sounds_Drop(object sender, DragEventArgs e)
	{
		e.Handled = true;

	}

	private void Lv_Sounds_DragEnter(object sender, DragEventArgs e)
	{
		var f = e.GetFilesFromDrop();

		foreach (string s in f) {
			if (Sounds.All(x => x.FullName != s)) {
				Sounds.Add(new SoundItem(s, DEVICE_INDEX));
			}
		}

		e.Handled = true;
	}

	private void Lv_Sounds_PreviewDragOver(object sender, DragEventArgs e)
	{
		e.Handled = true;
	}

	private void Lv_Sounds_KeyDown(object sender, KeyEventArgs e)
	{

		switch (e.Key) {
			case Key.Delete:
				if (HasSelected) {
					Selected.Dispose();
					Sounds.Remove(Selected);

				}

				break;
		}

		e.Handled = true;
	}

	private void Btn_Pause_Click(object sender, RoutedEventArgs e)
	{
		Dispatcher.BeginInvoke(() =>
		{
			Selected?.PlayPause();

		});
		e.Handled = true;
	}

	private void Btn_StopAll_Click(object sender, RoutedEventArgs e)
	{
		Dispatcher.BeginInvoke(() =>
		{
			foreach (SoundItem item in Sounds) {
				item.Stop();
			}

		});
		e.Handled = true;

	}

	private void Btn_Clear_Click(object sender, RoutedEventArgs e)
	{
		Dispatcher.BeginInvoke(() =>
		{
			foreach (SoundItem item in Sounds) {
				item.Dispose();
			}

			Sounds.Clear();

		});
		e.Handled = true;

	}

	private SoundItem[] m_buf;

	private void Tb_Search_TextChanged(object sender, TextChangedEventArgs e)
	{
		var s = Tb_Search.Text;

		if (string.IsNullOrEmpty(s)) {
			Lv_Sounds.ItemsSource = Sounds;
		}
		else {
			
			var filteredData =
				new ObservableCollection<SoundItem>(Sounds.Where(item => item.Name.Contains(s, StringComparison.OrdinalIgnoreCase)));
			Lv_Sounds.ItemsSource = filteredData;
		}

		e.Handled = true;
	}
}