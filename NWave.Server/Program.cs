using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using CliWrap;
using Flurl;
using Kantan.Text;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Logging.Console;
using NAudio.Wave;
using Novus.FileTypes;
using NWave.Lib;
using static System.Net.Mime.MediaTypeNames;

namespace NWave.Server;

public sealed class Program
{
	private const int    DEVICE_INDEX = 3;
	private const string SOUNDS       = @"H:\Other Music\Audio resources\NMicspam\";

	private static WebApplication   _app;
	private static ILogger<Program> _logger;

	private static readonly SoundLibrary _sndlib;

	static Program()
	{
		_sndlib = new SoundLibrary();

	}

	public static async Task Main(string[] args)
	{
		_sndlib.InitDirectory(SOUNDS, DEVICE_INDEX);

		var builder = WebApplication.CreateBuilder(args);

		// Add services to the container.

		// builder.Services.AddControllers();
		// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
		// builder.Services.AddEndpointsApiExplorer();
		// builder.Services.AddSwaggerGen();
		// builder.Services.AddRazorPages();

		_app = builder.Build();

		// _app.UseAuthentication();
		/*
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
		*/

		_app.UseExceptionHandler(exceptionHandlerApp =>
		{
			exceptionHandlerApp.Run(async context =>
			{

				await Results.Problem().ExecuteAsync(context);
			});
		});

		_app.Map("/exception", () =>
		{
			throw new InvalidOperationException("Sample Exception");
		});
		;
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
		_app.MapPost("/Add", AddAsync);
		_app.MapPost("/Remove", RemoveAsync);
		_app.MapPost("/AddYouTube", AddYouTubeAudioAsync);

		_logger.LogDebug("dbg");

		await _app.RunAsync();
	}

	#region

	/// <summary>
	/// <list type="bullet">
	/// <item>Body: multiple [absolute file path]</item>
	/// </list>
	/// </summary>
	private static async Task AddAsync(HttpContext ctx)
	{
		var bodyEntries = await ctx.ReadBodyEntriesAsync();
		ctx.Response.ContentType = Text.Plain;

		foreach (var s in bodyEntries) {
			if (!File.Exists(s)) {
				await ctx.Response.WriteAsync($"{s}: not found\n", ServerUtil.Encoding);
				continue;
			}

			var si = new SoundItem(s, DEVICE_INDEX);

			var b = _sndlib.TryAdd(si);
			await ctx.Response.WriteAsync($"{si}: {b}\n", ServerUtil.Encoding);
			_logger.LogInformation("Added {Sound}: {OK}", si, b);
		}

		await ctx.Response.CompleteAsync();
	}

	/// <summary>
	/// <list type="bullet">
	/// <item>Body: single [regex pattern]</item>
	/// <item>Body: multiple [name contains]</item>
	/// </list>
	/// </summary>
	private static async Task RemoveAsync(HttpContext ctx)
	{
		ctx.Response.ContentType = Text.Plain;

		var snds = await GetSoundsByHeader(ctx, MODE_SIMPLE);

		foreach (SoundItem snd in snds) {

			var b = _sndlib.TryRemove(snd);
			await ctx.Response.WriteAsync($"{snd}: {b}\n", ServerUtil.Encoding);
			_logger.LogInformation("Removed {Sound}: {OK}", snd, b);

		}

		await ctx.Response.CompleteAsync();
	}

	/// <summary>
	/// <list type="bullet">
	/// <item>Body: single [regex pattern]</item>
	/// <item>Body: multiple [name contains]</item>
	/// </list>
	/// </summary>
	private static async Task PlayAsync(HttpContext context)
	{
		// string audioFilePath = @"H:\Other Music\Audio resources\NMicspam\ow.wav"; // Change this to the path of your audio file
		var snds = await GetSoundsByHeader(context, MODE_SIMPLE);

		context.Response.ContentType = Text.Plain;

		foreach (var si in snds) {
			ThreadPool.QueueUserWorkItem((x) =>
			{
				_logger.LogInformation("Playing {Audio}", si);
				si.Play();
			});

			await context.Response.WriteAsync($"{si} playing\n", ServerUtil.Encoding);

		}

		await context.Response.CompleteAsync();

	}

	/// <summary>
	/// <list type="bullet">
	/// <item>Body: single [regex pattern]</item>
	/// <item>Body: multiple [name contains]</item>
	/// </list>
	/// </summary>
	private static async Task StopAsync(HttpContext context)
	{
		var snds = await GetSoundsByHeader(context, MODE_SIMPLE);

		context.Response.ContentType = Text.Plain;

		foreach (var si in snds) {
			si.Stop();
			await context.Response.WriteAsync($"{si}\n", ServerUtil.Encoding);
			_logger.LogInformation("Stopped {Sound}", si);

		}

		await context.Response.CompleteAsync();

	}

	private static async Task AddYouTubeAudioAsync(HttpContext context)
	{
		context.Response.ContentType = Text.Plain;
		var b    = await context.ReadBodyTextAsync();
		var ok = await _sndlib.AddYouTubeAudioAsync(b, DEVICE_INDEX);
		await context.Response.WriteAsync($"{b} : {ok}", ServerUtil.Encoding);

		await context.Response.CompleteAsync();

	}

	/// <summary>
	/// <list type="bullet">
	/// <item>Body: none</item>
	/// </list>
	/// </summary>
	private static async Task StatusAsync(HttpContext context)
	{

		IEnumerable<SoundItem> snds = await GetSoundsByHeader(context);
		snds = snds.Where(s => s.Status.IsIndeterminate()).ToArray();

		context.Response.ContentType = Text.Plain;

		if (!snds.Any()) {
			await context.Response.WriteAsync("---");
		}

		foreach (var si in snds) {
			await context.Response.WriteAsync($"{si}\n", ServerUtil.Encoding);
		}

		await context.Response.CompleteAsync();
	}

	/// <summary>
	/// <list type="bullet">
	/// <item>Body: none</item>
	/// </list>
	/// </summary>
	private static async Task ListAsync(HttpContext ctx)
	{
		ctx.Response.ContentType = Text.Plain;

		foreach ((var key, var _) in _sndlib.Sounds) {
			await ctx.Response.WriteAsync($"{key.Name}\n", ServerUtil.Encoding);

		}

		await ctx.Response.CompleteAsync();
	}

	public const string MODE_SIMPLE = "Simple";
	public const string MODE_REGEX  = "Regex";
	public const string HDR_MODE    = "Mode";

	private static async Task<IEnumerable<SoundItem>> GetSoundsByHeader(HttpContext context, string moded = MODE_SIMPLE)
	{
		var bodyEntries = await context.ReadBodyEntriesAsync();

		IEnumerable<SoundItem> snds;

		var b = context.Request.Headers.TryGetValue(HDR_MODE, out var e);

		var mode = b ? e[0] : moded;

		if (bodyEntries.Length == 0) {
			mode = MODE_SIMPLE;
		}

		switch (mode) {
			case MODE_REGEX:
				snds = _sndlib.FindByPattern(bodyEntries[0]);
				break;
			case MODE_SIMPLE:
				snds = _sndlib.FindSoundsByNames(bodyEntries);
				break;
			default:
				goto case MODE_SIMPLE;
		}

		return snds;
	}

	#endregion

	private const string ALL = "*";
}