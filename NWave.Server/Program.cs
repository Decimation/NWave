using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging.Console;
using NAudio.Wave;
using Novus.FileTypes;
using NWave.Lib;
using static System.Net.Mime.MediaTypeNames;

namespace NWave.Server;

public class Program
{
	private const string HDR_SOUND    = "s";
	private const int    DEVICE_INDEX = 3;
	private const string SOUNDS       = @"H:\Other Music\Audio resources\NMicspam\";

	/*
	$x = iwr -Method Get -Uri "https://localhost:7182/Status" -Headers @{'s'="H:\Other Music\Drum N Bass\Darchives\darchives-scenario-(primcd003)-cd-flac-1999-dl\01-darchives-first_offensive.flac"}
	$x
	 */

	/*
	iwr -Method Get -Uri "https://localhost:7182/Play" -Headers @{'s'="H:\Other Music\Drum N Bass\Darchives\darchives-scenario-(primcd003)-cd-flac-1999-dl\01-darchives-first_offensive.flac"}
	 */

	/*
	iwr -Method Get -Uri "https://localhost:7182/Stop" -Headers @{'s'="H:\Other Music\Drum N Bass\Darchives\darchives-scenario-(primcd003)-cd-flac-1999-dl\01-darchives-first_offensive.flac"}
	 */

	private static readonly ConcurrentBag<SoundItem> Sounds;

	private static WebApplication   App;
	private static ILogger<Program> Logger;

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
		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSwaggerGen();

		App = builder.Build();

		App.UseExceptionHandler(exceptionHandlerApp =>
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

		App.UseHsts();

		using var loggerFactory = LoggerFactory.Create(b =>
		{
			b.AddConsole().AddDebug().AddTraceSource("TRACE");
		});

		Logger = loggerFactory.CreateLogger<Program>();

		// Configure the HTTP request pipeline.
		if (App.Environment.IsDevelopment()) {
			App.UseSwagger();
			App.UseSwaggerUI();
		}

		App.UseHttpsRedirection();
		// app.UseAuthorization();
		// App.MapControllers();

		App.MapPost("/Play", PlayAsync);
		App.MapPost("/Stop", (StopAsync));
		App.MapGet("/Status", (StatusAsync));
		App.MapGet("/List", (ListAsync));

		Logger.LogDebug("dbg");

		await App.RunAsync();
	}

	private static async Task ListAsync(HttpContext ctx)
	{
		foreach (SoundItem soundItem in Sounds) {
			await ctx.Response.WriteAsync($"{soundItem}");

		}

		await ctx.Response.CompleteAsync();
	}
	private static async Task StopAsync(HttpContext context)
	{
		if (!context.Request.Headers.TryGetValue(HDR_SOUND, out var sv)) {

			context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
			await context.Response.CompleteAsync();
			return;
		}

		string s = sv[0];

		var sii = Find(s);
		sii?.Stop();

		if (s == "*") {
			foreach (var si in Sounds) {
				si.Stop();
			}
		}

		Logger.LogInformation("Stopped {Sound}", s);

	}

	private static async Task StatusAsync(HttpContext context)
	{
		if (!context.Request.Headers.TryGetValue(HDR_SOUND, out var sv)) {
			var pl = Sounds.Where(x => x.Status != PlaybackStatus.None);
			context.Response.ContentType = FileType.MT_TEXT_PLAIN;

			foreach (var item in pl) {
				await context.Response.WriteAsync(text: $"{item}\n", Encoding.UTF8);

			}

			await context.Response.CompleteAsync();

			return;
		}

		string s = sv[0];
		string m = null;

		if (s == null) {
			m = "Not found";
		}
		else {
			var sn = Find(s);

			if (sn == null) {
				m = $"Not found";
			}
			else {
				m = $"{sn.Status}";

			}
		}

		await context.WriteMessageAsync($"{m}");
	}

	private static async Task PlayAsync(HttpContext context)
	{
		// string audioFilePath = @"H:\Other Music\Audio resources\NMicspam\ow.wav"; // Change this to the path of your audio file
		string ss = await context.ReadBodyAsync();

		if (!context.Request.Headers.TryGetValue(HDR_SOUND, out var s)) {
			context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
			await context.Response.CompleteAsync();
			return;
		}

		var si = Find(s);

		if (si == null) {
			context.Response.StatusCode  = 404;
			await context.WriteMessageAsync( $"{s} not found");
			return;
		}

		ThreadPool.QueueUserWorkItem((x) =>
		{
			Logger.LogInformation("Playing {Audio}", si);
			si.Play();
		});

		await context.WriteMessageAsync($"{si} playing");
	}

	public static bool ContainsLike(string source, string like)
	{
		if (string.IsNullOrEmpty(source))
			return false; // or throw exception if source == null
		else if (string.IsNullOrEmpty(like))
			return false; // or throw exception if like == null 

		return Regex.IsMatch(
			source,
			"^" + Regex.Escape(like).Replace("_", ".").Replace("%", ".*") + "$");
	}

	private static SoundItem? Find(string? s)
	{
		return Sounds.FirstOrDefault(x => x.Name.Contains(s, StringComparison.InvariantCultureIgnoreCase));
	}
}