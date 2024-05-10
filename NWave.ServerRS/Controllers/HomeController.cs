using Microsoft.AspNetCore.Mvc;
using NWave.ServerRS.Models;
using System.Diagnostics;
using NWave.Lib;

namespace NWave.ServerRS.Controllers;

public class HomeController : Controller
{

	private readonly ILogger<HomeController> _logger;

	public HomeController(ILogger<HomeController> logger)
	{
		_logger = logger;
		snd.InitDirectory(SOUNDS, DEVICE_INDEX);
		snds = new List<SoundModel>(snd.Sounds.Keys.Select(x => new SoundModel() { Name = x.Name, Sound = x }));

	}

	public const int DEVICE_INDEX = 3;

	public const string SOUNDS = @"H:\Other Music\Audio resources\NMicspam\";

	private static readonly SoundLibrary snd = new SoundLibrary();

	private readonly List<SoundModel> snds;

	public IActionResult Index()
	{
		return View();
	}

	public IActionResult Privacy()
	{
		return View();
	}

	public IActionResult Play()
	{

		return Ok();
	}

	public IActionResult List()
	{
		return View(snds);
	}

	[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
	public IActionResult Error()
	{
		return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
	}

}