// Author: Deci | Project: NWave.Server | Name: NWaveYouTube.cs
// Date: 2025/04/08 @ 13:04:16

using System.Text.Json.Serialization;

namespace NWave.Server.Types;

public class NWaveYouTube
{

	[JPO(0)]
	public string Url { get; init; }

	[JPO(1)]
	public string Path { get; init; }

	public NWaveYouTube() { }

	[JsonConstructor]
	public NWaveYouTube(string url, string path)
	{
		Url = url;
		Path = path;
	}

}