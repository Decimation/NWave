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

	public static async Task Main(string[] args)
	{

		Routes.Init();

		var builder = WebApplication.CreateBuilder(args);

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
		
		// _app.UseCors();

		_app.Map("/exception", () =>
		{
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
			b.AddConsole().AddDebug().AddTraceSource("TRACE");
		});

		_logger = loggerFactory.CreateLogger<Program>();

		// Configure the HTTP request pipeline.
		if (_app.Environment.IsDevelopment())
		{
			// _app.UseSwagger();
			// _app.UseSwaggerUI();
		}

		_app.MapPost("/Play", Routes.PlayAsync);
		_app.MapPost("/Stop", (Routes.StopAsync));
		_app.MapPost("/Pause", (Routes.PauseAsync));
		_app.MapGet("/Status", (Routes.StatusAsync));
		_app.MapGet("/List", (Routes.ListAsync));
		_app.MapPost("/Add", Routes.AddAsync);
		_app.MapPost("/Remove", Routes.RemoveAsync);
		_app.MapPost("/Update", Routes.UpdateAsync);
		_app.MapPost("/AddYouTubeFile", Routes.AddYouTubeAudioFileAsync);
		_app.MapPost("/AddYouTubeUrl", Routes.AddYouTubeAudioUrlAsync);

		_logger.LogDebug("dbg");

		await _app.RunAsync();
	}

	public static WebApplication _app;

	public static ILogger<Program> _logger;

}
