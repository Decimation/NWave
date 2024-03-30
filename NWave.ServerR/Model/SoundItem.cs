// Author: Deci | Project: NWave.ServerR | Name: SoundItem.cs
// Date: 2024/3/30 @ 15:27:48

// Author: Deci | Project: NWave.ServerR | Name: SoundItem.cs
// Date: 2024/3/30 @ 15:27:48

using NWave.Lib;

namespace NWave.ServerR.Model;

public class SoundItem
{

	public string Name => Item?.Name;

	public int ID { get; set; }

	public BaseSoundItem Item { get; set; }

	public SoundItem() { }

}