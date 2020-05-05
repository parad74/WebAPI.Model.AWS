using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Service.Filter;
using WebAPI.Model.AWS.Constant;

namespace WebAPI.Model.AWS
{
	public class StartupWebAPIAWS
	{
		public StartupWebAPIAWS(IConfiguration configuration)
		{
			this.Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services)
		{
		services.AddTransient<IStartupFilter, SettingValidationStartupFilter>();  
 		services.AddMvc().AddNewtonsoftJson();
	
			services.AddLogging();
			services.AddLogging(config =>
		{
			// clear out default configuration
			config.ClearProviders();

			config.AddConfiguration(this.Configuration.GetSection("Logging"));
			config.AddDebug();
			config.AddEventSourceLogger();

			if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ==
			Microsoft.Extensions.Hosting.Environments.Development)
			{
				config.AddConsole();
			}
		});

			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo
				{
					Version = "v1",
					Title = "WebAPI.Model.AWS",
					Description = "AWS S3 SPARK - WEB API "
				});
				//// Set the comments path for the Swagger JSON and UI.
				var xmlFile = $"{Assembly.GetEntryAssembly().GetName().Name}.xml";
				var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
				c.IncludeXmlComments(xmlPath);
				c.CustomSchemaIds(i => i.FullName);
			});

			services.AddOptions();  
			services.AddSingleton<IConfiguration>(this.Configuration);
			services.Configure<AWSSettings>(Configuration.GetSection("Count4USettings"));
			// Explicitly register the settings object so IOptions not required (optional)
			services.AddSingleton(resolver =>
				resolver.GetRequiredService<IOptions<AWSSettings>>().Value);            
																							
			services.AddSingleton<IValidatable>(resolver =>
				resolver.GetRequiredService<IOptions<AWSSettings>>().Value);

			services.AddSingleton<IAWSSettings>(resolver =>
				resolver.GetRequiredService<IOptions<AWSSettings>>().Value);

			services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
            });
			services.AddOptions();

			services.AddControllers()
				.AddControllersAsServices(); // Add the controllers to DI

			this.MapAWSDependencyInjection(services);
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env , ILoggerFactory loggerFactory/*, Microsoft.AspNetCore.Hosting.IApplicationLifetime applicationLifetime*/)
		{
			app.UseResponseCompression();
			Microsoft.Extensions.Logging.ILogger logger = loggerFactory.CreateLogger<StartupWebAPIAWS>();
			if (env.IsDevelopment())
			{
				logger.LogInformation("Is Development enviroment");
				app.UseDeveloperExceptionPage();
			}

			app.UseSwagger();
			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "ASP.NET Core 3.1 web API v1");
			});


			app.UseRouting();
			app.UseCors(policy =>
			policy.AllowAnyOrigin() //WithOrigins("http://localhost:5000", "http://localhost:27515"
			.AllowAnyMethod()
			.AllowAnyHeader()
			.WithExposedHeaders());//.AllowCredentials());

			app.UseAuthorization();
			app.UseStatusCodePagesWithReExecute("/error/{0}");      
			app.UseExceptionHandler("/error/500");

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}

		private void MapAWSDependencyInjection(IServiceCollection services)
		{
			 // services.AddAWSService<IAmazonS3>();
		}
	}
}


