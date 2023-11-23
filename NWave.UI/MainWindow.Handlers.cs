// Read S NWave.UI MainWindow.Handlers.cs
// 2023-11-23 @ 4:26 PM

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NWave.Lib;

namespace NWave.UI;

public partial class MainWindow
{

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
				Sounds.Add(new FixedSoundItem(s, DEVICE_INDEX));
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
					var sel = Selected.ToArray();

					foreach (var item in sel) {
						item.Dispose();

						Sounds.Remove(item);
					}
				}

				break;
		}

		e.Handled = true;
	}

	private BaseSoundItem[] m_buf;

	private void Tb_Search_TextChanged(object sender, TextChangedEventArgs e)
	{
		var s = Tb_Search.Text;

		if (string.IsNullOrEmpty(s)) {
			Lv_Sounds.ItemsSource = Sounds;
		}
		else {

			var filteredData =
				new ObservableCollection<BaseSoundItem>(
					Sounds.Where(item => item.Name.Contains(s, StringComparison.OrdinalIgnoreCase)));
			Lv_Sounds.ItemsSource = filteredData;
		}

		e.Handled = true;
	}

	#region

	private void Btn_Play_Click(object sender, RoutedEventArgs e)
	{

		Dispatcher.BeginInvoke(() =>
		{
			foreach (var item in Selected) {
				item.Play();

			}
		});

		e.Handled = true;
	}

	private void Btn_Pause_Click(object sender, RoutedEventArgs e)
	{
		Dispatcher.BeginInvoke(() =>
		{
			foreach (var item in Selected) {
				item.Pause();
			}

		});
		e.Handled = true;
	}

	private void Btn_Stop_Click(object sender, RoutedEventArgs e)
	{
		Dispatcher.BeginInvoke(() =>
		{
			foreach (var item in Selected) {
				item.Stop();
				item.UpdateProperties();
			}

		});
		e.Handled = true;
	}

	private void Btn_StopAll_Click(object sender, RoutedEventArgs e)
	{
		Dispatcher.BeginInvoke(() =>
		{
			foreach (BaseSoundItem item in Sounds) {
				item.Stop();
			}

		});
		e.Handled = true;

	}

	private void Btn_Clear_Click(object sender, RoutedEventArgs e)
	{
		Dispatcher.BeginInvoke(() =>
		{
			foreach (BaseSoundItem item in Sounds) {
				item.Dispose();
			}

			Sounds.Clear();

		});
		e.Handled = true;

	}

	private void Btn_Add_Click(object sender, RoutedEventArgs e)
	{
		var sel = (AddOperation) Cb_InputType.SelectionBoxItem;

		var t = Tb_Url.Text.Trim('\"');

		switch (sel) {

			case AddOperation.YT_URL:
				Dispatcher.InvokeAsync(async () =>
				{
					var yt        = await SoundLibrary.GetYouTubeAudioUrlAsync(t);
					var soundItem = new DynamicSoundItem(yt.Audio, yt.Title, DEVICE_INDEX);
					AddSound(soundItem);
				});
				break;
			case AddOperation.YT_FILE:
				Dispatcher.InvokeAsync(async () =>
				{
					var yt        = await SoundLibrary.GetYouTubeAudioFileAsync(t, SOUNDS);
					var soundItem = new FixedSoundItem(yt.Path, DEVICE_INDEX);
					AddSound(soundItem);
				});
				break;
			case AddOperation.DIRECTORY:
				Dispatcher.InvokeAsync(async () =>
				{
					foreach (var v in EnumFiles(t)) {
						var soundItem = new FixedSoundItem(v, DEVICE_INDEX);
						AddSound(soundItem);
					}
				});

				break;

			case AddOperation.FILE:
				Dispatcher.InvokeAsync(async () =>
				{
					var soundItem = new FixedSoundItem(t, DEVICE_INDEX);
					AddSound(soundItem);
				});
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}

		e.Handled = true;
	}

	private void Btn_Update_Click(object sender, RoutedEventArgs e)
	{
		var s = Tb_Volume.Text;

		float? f = null;

		if (!string.IsNullOrWhiteSpace(s)) {
			f = float.Parse(s);
		}

		Dispatcher.InvokeAsync(async () =>
		{
			foreach (var item in Selected) {
				if (item.SupportsVolume && f.HasValue) {
					item.Volume = f.Value;
				}

				item.UpdateProperties();
			}
		});
		e.Handled = true;
	}

	private void Btn_Remove_Click(object sender, RoutedEventArgs e)
	{
		Dispatcher.InvokeAsync(async () =>
		{
			var sel = Selected.ToArray();
			foreach (var item in sel) {
				item.Dispose();
				Sounds.Remove(item);
			}
		});
		e.Handled = true;
	}

	#endregion

}