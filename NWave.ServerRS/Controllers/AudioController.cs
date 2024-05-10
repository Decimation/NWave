// Author: Deci | Project: NWave.ServerRS | Name: AudioController.cs
// Date: 2024/05/10 @ 09:05:44

using Microsoft.AspNetCore.Mvc;
using NAudio.Wave;

namespace NWave.ServerRS.Controllers;

[Route("[controller]")]
[ApiController]
public class AudioController : ControllerBase
{
	[HttpPost("play")]
	public IActionResult PlayAudio([FromBody] string audioFilePath)
	{
		try
		{
			using (var audioFile = new AudioFileReader(audioFilePath))
				using (var outputDevice = new WaveOutEvent())
				{
					outputDevice.Init(audioFile);
					outputDevice.Play();
					while (outputDevice.PlaybackState == PlaybackState.Playing)
					{
						System.Threading.Thread.Sleep(1000);
					}
				}
			return Ok();
		}
		catch (Exception ex)
		{
			return BadRequest($"Error: {ex.Message}");
		}
	}
}