// Author: Deci | Project: NWave.ServerR | Name: ISoundItemService.cs
// Date: 2024/3/30 @ 15:12:39

using System.Collections.Concurrent;
using NWave.Lib;
using NWave.ServerR.Controllers;
using NWave.ServerR.Model;

namespace NWave.ServerR;

public interface ISoundItemService
{

	IEnumerable<SoundItem> GetAllSoundsAsync();

}

public class SoundItemService : ISoundItemService
{

	public SoundLibrary SndLib { get; }

	public IEnumerable<SoundItem> GetAllSoundsAsync()
	{
		var cd = SndLib.Sounds;

		foreach (KeyValuePair<BaseSoundItem, object> o in cd) {
			yield return new SoundItem() { Item = o.Key };
		}
	}

}