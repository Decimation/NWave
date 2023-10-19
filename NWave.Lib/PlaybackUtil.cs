// Read S NWave.Lib PlaybackUtil.cs
// 2023-10-19 @ 12:07 AM

namespace NWave.Lib;

public static class PlaybackUtil
{
	public static bool IsPlayable(this PlaybackStatus s)
	{
		return s is PlaybackStatus.Paused or PlaybackStatus.Stopped or PlaybackStatus.None;
	}

	public static bool IsIndeterminate(this PlaybackStatus s)
	{
		return s != PlaybackStatus.None && s != PlaybackStatus.Stopped;
	}
}