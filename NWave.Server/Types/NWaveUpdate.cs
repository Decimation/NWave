// Author: Deci | Project: NWave.Server | Name: NWaveUpdate.cs
// Date: 2025/04/08 @ 13:04:12

namespace NWave.Server.Types;

public class NWaveUpdate : NWaveRequest
{

	public string Field { get; init; }

	public object Value { get; init; }

	public NWaveUpdate() { }

}