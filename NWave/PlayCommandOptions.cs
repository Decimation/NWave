// Author: Deci | Project: NWave | Name: PlayCommandOptions.cs
// Date: 2025/07/09 @ 19:07:59

#nullable disable
using System.ComponentModel;
using NWave.Lib;
using NWave.Lib.Model;
using Spectre.Console.Cli;

namespace NWave;

public class PlayCommandOptions : CommandSettings
{

	[DefaultValue(BaseSoundItem.DEFAULT_DEVICE_INDEX)]
	[CommandArgument(0, "[deviceId]")]
	public int DeviceId { get; set; }

	[NN]
	[CommandOption("--source")]
	public string Source { get; set; }

}