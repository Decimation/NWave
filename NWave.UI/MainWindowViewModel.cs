using Avalonia.Input;
using NWave.Lib.Model;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NWave.UI
{
	public class MainWindowViewModel : ReactiveObject
	{

		public ObservableCollection<SoundEntry> Items { get; } = new ObservableCollection<SoundEntry>();

		/// <inheritdoc />
		public MainWindowViewModel()
		{
		}

	}
}