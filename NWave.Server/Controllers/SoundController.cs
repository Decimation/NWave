using System.Net;
using Microsoft.AspNetCore.Mvc;
using NAudio.Wave;

namespace NWave.Server.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class SoundController : ControllerBase
	{
		private static readonly string[] Summaries = new[]
		{
			"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
		};

		private readonly ILogger<SoundController> _logger;

		public SoundController(ILogger<SoundController> logger)
		{
			_logger = logger;
		}

		[HttpGet(Name = "PlayAudio")]
		public HttpResponseMessage Get()
		{

			return new HttpResponseMessage(HttpStatusCode.OK) { Content = { } };
		}
	}
}