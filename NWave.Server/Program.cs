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
		ctx.Response.ContentType = FileType.MT_TEXT_PLAIN;
		foreach (SoundItem soundItem in Sounds) {
			await ctx.Response.WriteAsync($"{soundItem}\n");

		}

		await ctx.Response.CompleteAsync();
	}

	private static async Task StopAsync(HttpContext context)
	{
		var ss   = await GetBody(context);
		var snds = Find(ss);

		context.Response.ContentType = FileType.MT_TEXT_PLAIN;

		foreach (var si in snds)
		{
			si.Stop();
			await context.Response.WriteAsync($"{si}\n");
			Logger.LogInformation("Stopped {Sound}", si);

		}
		await context.Response.CompleteAsync();

	}

	private static async Task StatusAsync(HttpContext context)
	{
		var ss   = await GetBody(context);
		var snds = Find(ss);

		context.Response.ContentType = FileType.MT_TEXT_PLAIN;

		foreach (var si in snds)
		{

			await context.Response.WriteAsync($"{si}\n");

		}

		await context.Response.CompleteAsync();
	}

	private static async Task PlayAsync(HttpContext context)
	{
		// string audioFilePath = @"H:\Other Music\Audio resources\NMicspam\ow.wav"; // Change this to the path of your audio file
		var ss   = await GetBody(context);
		var snds = Find(ss);

		context.Response.ContentType = FileType.MT_TEXT_PLAIN;

		foreach (var si in snds) {
			ThreadPool.QueueUserWorkItem((x) =>
			{
				Logger.LogInformation("Playing {Audio}", si);
				si.Play();
			});

			await context.Response.WriteAsync($"{si} playing\n");

		}

		await context.Response.CompleteAsync();

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

	private static async Task<string[]> GetBody(HttpContext c)
	{
		var b = await c.ReadBodyAsync();

		if (b == null) {
			return Array.Empty<string>();
		}

		return b.Split(',');
	}

	private static IEnumerable<SoundItem> Find(IEnumerable<string> s)
	{
		foreach (string s1 in s) {

			var si = Find(s1);

			if (si != null) {
				yield return si;

			}
		}
	}

	private static SoundItem? Find(string? s)
	{
		return Sounds.FirstOrDefault(x => x.Name.Contains(s, StringComparison.InvariantCultureIgnoreCase));
	}
}