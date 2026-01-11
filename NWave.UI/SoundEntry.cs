// Author: Deci | Project: NWave.UI | Name: SoundEntry.cs
// Date: 2025/10/08 @ 23:10:34

using System;
using NWave.Lib.Model;
using ReactiveUI;

namespace NWave.UI;

public class SoundEntry : ReactiveObject
{

	public BaseSoundItem Item { get; }

	public TimeOnly Start { get; set; }


	public TimeOnly End { get; set; }

	public SoundEntry(BaseSoundItem item)
	{
		Item = item;
	}

}