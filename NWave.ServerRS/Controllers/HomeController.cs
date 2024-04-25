using Microsoft.AspNetCore.Mvc;
using NWave.ServerRS.Models;
using System.Diagnostics;
using NWave.Lib;

namespace NWave.ServerRS.Controllers
{
	public class HomeController : Controller
	{

		private readonly ILogger<HomeController> _logger;

		public HomeController(ILogger<HomeController> logger)
		{
			_logger = logger;
			snd.InitDirectory(SOUNDS, DEVICE_INDEX);
		}

		public const int DEVICE_INDEX = 3;

		public const string SOUNDS = @"H:\Other Music\Audio resources\NMicspam\";

		private static readonly SoundLibrary snd = new SoundLibrary();

		public IActionResult Index()
		{
			return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}

		public IActionResult List()
		{
			var select = snd.Sounds.Keys.Select(x => new SoundModel() { Sound = x, Name = x.Name });
			return View(select);
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

	}
}