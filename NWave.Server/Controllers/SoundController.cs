using System.Net;
using Microsoft.AspNetCore.Mvc;
using NAudio.Wave;

namespace NWave.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class SoundController : ControllerBase
{

	private readonly ILogger<SoundController> _logger;

	public SoundController(ILogger<SoundController> logger)
	{
		_logger = logger;
	}

	[HttpGet(Name = "PlayAudio")]
	public HttpResponseMessage Get()
	{
		// todo
		return new HttpResponseMessage(HttpStatusCode.OK) { Content = { } };
	}
}