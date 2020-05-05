using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore;

namespace WebAPI.Model.AWS
{
	//spark-submit --class org.apache.spark.deploy.dotnet.DotnetRunner --packages io.delta:delta-core_2.11:0.6.0,org.apache.hadoop:hadoop-aws:2.7.7   --master local C:\Count4U\trunk\WebAPI.Model.AWS\src\WebAPI.Model.AWS\bin\Debug\netcoreapp3.1\microsoft-spark-2.4.x-0.10.0.jar debug   
	public class Program
	{
		public static void Main(string[] args)
		{
			BuildWebHost(args).Run();
		}

		public static IWebHost BuildWebHost(string[] args) =>
		WebHost.CreateDefaultBuilder(args)
		 .UseUrls(	"http://0.0.0.0:52025")
		 .UseStartup<StartupWebAPIAWS>() 
		 .Build();
	}
}
