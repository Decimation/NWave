using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using CliWrap;
using Flurl;
using Kantan.Text;
using Microsoft.AspNetCore.Cors.Infrastructure;
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

	static Program() { }

	public static WebApplication App;

	public static ILogger<Program> Logger;

	public const int DEVICE_INDEX = 1;

	public const string SOUNDS = @"H:\Other Music\Audio resources\NMicspam\";

	public static async Task Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);
		builder.Configuration.AddCommandLine(args);

		// Add services to the container.

		// builder.Services.AddControllers();
		// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
		// builder.Services.AddEndpointsApiExplorer();
		// builder.Services.AddSwaggerGen();
		// builder.Services.AddRazorPages();
		/*builder.Services.AddCors(x =>
		{
			x.AddDefaultPolicy(new CorsPolicy()
			{
				Origins = {  "*" },
				Methods = {"*" },
				Headers = { "*" }
			});
		});*/

		App = builder.Build();

		string dir, s;
		int    di;

#if DEBUG
		dir = SOUNDS;
		di  = DEVICE_INDEX;
#else
		dir = App.Configuration["sourceDirectory"];
		s = App.Configuration["deviceId"];
		di = Int32.Parse(s);
#endif


		if (!Routes.Lib.InitDirectory(dir, di)) {
			throw new ArgumentException($"{dir} / {di}");
		}

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

		App.UseExceptionHandler(exceptionHandlerApp =>
		{
			exceptionHandlerApp.Run(context =>
			{
				//
				return Results.Problem().ExecuteAsync(context);
			});
		});

		// _app.UseCors();

		App.Map("/exception", () =>
		{
			//
			throw new InvalidOperationException("Sample Exception");
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
			//
			b.AddConsole().AddDebug().AddTraceSource("TRACE");
		});

		Logger = loggerFactory.CreateLogger<Program>();

		// Configure the HTTP request pipeline.
		if (App.Environment.IsDevelopment()) {
			// _app.UseSwagger();
			// _app.UseSwaggerUI();
		}

		App.MapPost("/Play", Routes.PlayAsync);
		App.MapPost("/Stop", Routes.StopAsync);
		App.MapPost("/Pause", Routes.PauseAsync);
		App.MapGet("/Status", Routes.StatusAsync);
		App.MapGet("/List", Routes.ListAsync);
		App.MapPost("/Add", Routes.AddAsync);
		App.MapPost("/Remove", Routes.RemoveAsync);
		App.MapPost("/Update", Routes.UpdateAsync);
		App.MapPost("/AddYouTubeFile", Routes.AddYouTubeAudioFileAsync);
		App.MapPost("/AddYouTubeUrl", Routes.AddYouTubeAudioUrlAsync);

		Logger.LogDebug("dbg");

		await App.RunAsync();
	}

}