using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Kantan.Text;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Logging.Console;
using NAudio.Wave;
using Novus.FileTypes;
using NWave.Lib;
using static System.Net.Mime.MediaTypeNames;

namespace NWave.Server;

public sealed class Program
{
	private const string HDR_SOUND    = "s";
	private const int    DEVICE_INDEX = 3;
	private const string SOUNDS       = @"H:\Other Music\Audio resources\NMicspam\";

	private static readonly ConcurrentBag<SoundItem> Sounds;

	private static WebApplication   _app;
	private static ILogger<Program> _logger;

	static Program()
	{
		var files = Directory.EnumerateFiles(SOUNDS, searchPattern: "*.*", new EnumerationOptions()
		{
			RecurseSubdirectories = true
		});

		Sounds = new ConcurrentBag<SoundItem>(files.Select(x => new SoundItem(x, DEVICE_INDEX)));
	}

	public static async Task Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		// Add services to the container.

		// builder.Services.AddControllers();
		// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
		// builder.Services.AddEndpointsApiExplorer();
		// builder.Services.AddSwaggerGen();
		builder.Services.AddRazorPages();

		_app = builder.Build();

		// _app.UseAuthentication();
		_app.UseExceptionHandler(exceptionHandlerApp =>
		{
			exceptionHandlerApp.Run(async context =>
			{
				context.Response.StatusCode = StatusCodes.Status500InternalServerError;

				// using static System.Net.Mime.MediaTypeNames;
				context.Response.ContentType = Text.Plain;

				await context.Response.WriteAsync("An exception was thrown.");

				var exceptionHandlerPathFeature =
					context.Features.Get<IExceptionHandlerPathFeature>();

				if (exceptionHandlerPathFeature?.Error is FileNotFoundException) {
					await context.Response.WriteAsync(" The file was not found.");
				}

				if (exceptionHandlerPathFeature?.Path == "/") {
					await context.Response.WriteAsync(" Page: Home.");
				}
			});
		});

		// _app.UseHsts();

		// _app.UseAuthorization();
		// _app.UseRouting();
		// _app.UseStaticFiles();
		// _app.UseHttpsRedirection();
		// _app.UseAuthorization();
		// App.MapControllers();

		using var loggerFactory = LoggerFactory.Create(b =>
		{
			b.AddConsole().AddDebug().AddTraceSource("TRACE");
		});

		_logger = loggerFactory.CreateLogger<Program>();

		// Configure the HTTP request pipeline.
		if (_app.Environment.IsDevelopment()) {
			// _app.UseSwagger();
			// _app.UseSwaggerUI();
		}

		_app.MapPost("/Play", PlayAsync);
		_app.MapPost("/Stop", (StopAsync));
		_app.MapGet("/Status", (StatusAsync));
		_app.MapGet("/List", (ListAsync));

		_logger.LogDebug("dbg");

		await _app.RunAsync();
	}

	#region

	static string tosb<T>(IEnumerable<T> t, Func<T, string> x)
	{
		var sb = new StringBuilder();

		foreach (T v in t) {
			sb.AppendLine(x(v));
		}

		return sb.ToString();
	}

	private static async Task ListAsync(HttpContext ctx)
	{
		ctx.Response.ContentType = Text.Plain;

		await ctx.Response.WriteAsync(tosb(Sounds, item => $"{item.Name}"), Util.Encoding);

		await ctx.Response.CompleteAsync();
	}

	private static async Task StopAsync(HttpContext context)
	{
		var                    ss = await context.ReadBodyEntriesAsync();
		IEnumerable<SoundItem> snds;

		if (ss.Length != 0) {
			snds = FindSounds(ss);

		}
		else {
			snds = Sounds.Where(s => s.Status == PlaybackStatus.Playing);
		}

		context.Response.ContentType = Text.Plain;

		foreach (var si in snds) {
			si.Stop();
			await context.Response.WriteAsync($"{si}\n", Util.Encoding);
			_logger.LogInformation("Stopped {Sound}", si);

		}

		await context.Response.CompleteAsync();

	}

	private static async Task StatusAsync(HttpContext context)
	{
		var                    ss = await context.ReadBodyEntriesAsync();
		IEnumerable<SoundItem> snds;

		if (ss.Length != 0) {
			snds = FindSounds(ss);
		}
		else {
			snds = Sounds.Where(s => s.Status != PlaybackStatus.None && s.Status != PlaybackStatus.Stopped);
		}

		context.Response.ContentType = Text.Plain;

		foreach (var si in snds) {
			await context.Response.WriteAsync($"{si}\n", Util.Encoding);
		}

		await context.Response.CompleteAsync();
	}

	private static async Task PlayAsync(HttpContext context)
	{
		// string audioFilePath = @"H:\Other Music\Audio resources\NMicspam\ow.wav"; // Change this to the path of your audio file
		var ss = await context.ReadBodyEntriesAsync();
		_logger.LogInformation("Request {Sounds}", ss.QuickJoin());
		var snds = FindSounds(ss);
		_logger.LogInformation("Found {Sounds}", snds.Count());
		context.Response.ContentType = Text.Plain;

		foreach (var si in snds) {
			ThreadPool.QueueUserWorkItem((x) =>
			{
				_logger.LogInformation("Playing {Audio}", si);
				si.Play();
			});

			await context.Response.WriteAsync($"{si} playing\n", Util.Encoding);

		}

		await context.Response.CompleteAsync();

	}

	#endregion

	private static List<SoundItem> FindSounds2(IEnumerable<string> sounds)
	{
		var ls = new List<SoundItem>();

		foreach (string s in sounds) {
			foreach (var s1 in Sounds) {
				if (s1.Name.Contains(s, StringComparison.InvariantCultureIgnoreCase)) {

					// yield return s1;

					ls.Add(s1);
				}

			}
		}

		return ls;
	}

	private const string ALL = "*";

	private static IEnumerable<SoundItem> FindSounds(IEnumerable<string> sounds)
	{

		foreach (string s1 in sounds) {

			var si = FindSound(s1);

			if (si != null) {
				yield return si;

			}
		}
	}

	private static SoundItem? FindSound(string s)
	{
		return Sounds.FirstOrDefault(x => x.Name.Contains(s, StringComparison.InvariantCultureIgnoreCase));
	}
}