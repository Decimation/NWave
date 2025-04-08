// Author: Deci | Project: NWave.Server | Name: NWaveRequest.cs
// Date: 2025/04/08 @ 13:04:53

namespace NWave.Server.Types;

public class NWaveRequest
{

	[JINC]
	[JPO(0)]
	public string[] Names { get; }

	public NWaveRequest() { }

	public NWaveRequest(string[] names)
	{
		Names = names;
	}

}