using Avalonia.Controls;
using Avalonia.Input;
using System.Collections.ObjectModel;
using Avalonia.Platform.Storage;
using NWave.Lib.Model;

namespace NWave.UI
{
	public partial class MainWindow : Window
	{

		public MainWindow()
		{
			InitializeComponent();

			AddHandler(DragDrop.DropEvent, (sender, args) =>
			{
				var items = DataContext as MainWindowViewModel;

				foreach (var item in args.DataTransfer.Items) {
					var getRaw = item.TryGetRaw(DataFormat.File) as IStorageItem;

					var bso = new SoundEntry(new FixedSoundItem(getRaw.Path.ToString()));
					items.Items.Add(bso);
					break;
				}
			});
		}

	}
}