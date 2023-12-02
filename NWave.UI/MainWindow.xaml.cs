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
using System.Reflection;
using System.Windows.Forms;
using Control = System.Windows.Controls.Control;

// ReSharper disable InconsistentNaming

namespace NWave.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{

	private const string SERVER = $"https://localhost:7182";

	private const string SOUNDS = @"H:\Other Music\Audio resources\NMicspam\";

	public enum AddOperation
	{

		YT_URL,
		YT_FILE,
		DIRECTORY,
		FILE

	}

	public static readonly AddOperation[] Item_Ops = Enum.GetValues<AddOperation>();

	public MainWindow()
	{
		DataContext = this;
		InitializeComponent();

		IEnumerable<string> files = EnumFiles(SOUNDS);

		Sounds = new ObservableCollection<BaseSoundItem>(files.Select(x => new FixedSoundItem(x, DEVICE_INDEX)));
		Lv_Sounds.ItemsSource    = Sounds;
		
		Cb_InputType.ItemsSource = Item_Ops;

		m_bg = new DispatcherTimer(DispatcherPriority.Normal)
		{
			Interval  = TimeSpan.FromMilliseconds(300),
			IsEnabled = true,

		};

		m_bg.Tick += Ticker;

	}

	private static IEnumerable<string> EnumFiles(string dir)
	{
		var files = Directory.EnumerateFiles(dir, searchPattern: "*.*", new EnumerationOptions()
		{
			RecurseSubdirectories = true
		});
		return files;
	}

	private readonly SemaphoreSlim m_semaphore = new SemaphoreSlim(1);

	private async void Ticker(object? sender, EventArgs args)
	{
		if (await m_semaphore.WaitAsync(TimeSpan.Zero)) {
			return;
		}
		
		var items = Sounds.Where(x => x.Status.IsIndeterminate());

		foreach (var item in items) {
			// Debug.WriteLine($"updating {item}");
			
			item.UpdateProperties();
		}

		m_semaphore.Release();

	}

	private readonly DispatcherTimer m_bg;

	private WindowInteropHelper m_h;

	private HwndSource m_wnd;

	[MNNW(true, nameof(Selected))]
	public bool HasSelected => Selected != null;

	public ObservableCollection<BaseSoundItem> Sounds { get; private set; }

	public IEnumerable<BaseSoundItem> Selected => Lv_Sounds.SelectedItems.Cast<BaseSoundItem>();

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

	private const int DEVICE_INDEX = 3;

	public const int HOOK_ID = 9000;

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
								foreach (var item in Selected) {
									item.PlayPause();

								}
							});
						}

						if (vkey == (uint) VirtualKey.UP) {
							//handle global hot key here...
							Debug.Print($"{vkey}!!");

							int index = Lv_Sounds.SelectedIndex - 1;

							if (index >= 0) {
								Lv_Sounds.SelectedIndex = index;

							}
						}

						if (vkey == (uint) VirtualKey.DOWN) {
							//handle global hot key here...
							Debug.Print($"{vkey}!!");
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

		Native.RegisterHotKey(m_h.Handle, HOOK_ID, HotKeyModifiers.MOD_CONTROL | HotKeyModifiers.MOD_SHIFT,
		                      (uint) VirtualKey.UP);

		Native.RegisterHotKey(m_h.Handle, HOOK_ID, HotKeyModifiers.MOD_CONTROL | HotKeyModifiers.MOD_SHIFT,
		                      (uint) VirtualKey.DOWN);
	}

	private void OnClosing(object? sender, CancelEventArgs e)
	{
		Native.UnregisterHotKey(m_h.Handle, HOOK_ID);
		Debug.Print("Unregistered");
	}

	#region

	void AddSound(BaseSoundItem soundItem)
	{
		Sounds.Add(soundItem);
		Lv_Sounds.ScrollIntoView(soundItem);
		Tb_Url.Text = null;
	}

	#endregion

}

public static class ControlExtensions
{
	public static void DoubleBuffering(this Control control, bool enable)
	{
		var method = typeof(Control).GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic);
		method.Invoke(control, new object[] { ControlStyles.OptimizedDoubleBuffer, enable });
	}
}