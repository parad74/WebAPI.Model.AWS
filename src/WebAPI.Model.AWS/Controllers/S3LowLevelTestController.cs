using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Spark.Extensions.Delta.Tables;
using Microsoft.Spark.Sql;
using Microsoft.Spark.Sql.Types;
using WebAPI.Model.AWS.Constant;
using static WebAPI.Model.AWS.Urls;

namespace WebAPI.Model.AWS
{
	[ApiController]
	[Produces("application/json")]
	[Consumes("application/json")]
	public class S3LowLevelTestController : ControllerBase
	{
		private readonly ILogger<S3LowLevelTestController> _logger;
		private IAmazonS3 _s3Client;
		private IAWSSettings _awsSettings;
		string _bucketName = "test4eaet1";
		string _defaultFolder = "my-folder";
		public S3LowLevelTestController(
			 IAWSSettings awsSettings
			, ILogger<S3LowLevelTestController> logger)
		{
			this._logger = logger;
			this._awsSettings = awsSettings ??
				   throw new ArgumentNullException(nameof(awsSettings));
			//RegionEndpoint endpoint = RegionEndpoint.GetBySystemName(this._awsSettings.Region);		  //don't work
			this._s3Client = new AmazonS3Client(this._awsSettings.AccessKey, this._awsSettings.SecretKey, RegionEndpoint.USEast1);
		}

		[HttpPost(WebApiS3LowLevelTest.CreateFolder)]
		public async Task<string> CreateFolder([FromRoute] string folderPath)
		{
			string result = String.Empty;
			try
			{
				folderPath = folderPath.Trim() + "/";

				PutObjectRequest request = new PutObjectRequest()
				{
					BucketName = _bucketName,
					Key = folderPath  // <- in S3 key as path  
				};
				await this._s3Client.PutObjectAsync(request);

				result = string.Format($"{ this._bucketName}/{folderPath} ");
			}
			catch (Exception ex)
			{
				result = ex.Message;
			}
			return result;
		}


		[HttpPost(WebApiS3LowLevelTest.CopyFileToFolder)]
		public async Task<string> CopyFileToFolder([FromRoute] string fileName)
		{
			string result = String.Empty;
			try
			{
				FileInfo file = new FileInfo(fileName);
				string path = this._defaultFolder + "/" + file.Name;
				PutObjectRequest request = new PutObjectRequest()
				{
					InputStream = file.OpenRead(),
					BucketName = _bucketName,
					Key = path  // <- in S3 key as path  
				};
				await this._s3Client.PutObjectAsync(request);

				result = string.Format($"{ this._bucketName}/{path} ");

				//https://{bucket-name}.amazonaws.com/{folder}/{file-name}
				// https://test4eaet1.s3.amazonaws.com/my-folder/ExportReport.ini
			}
			catch (Exception ex)
			{
				result = ex.Message;
			}
			return result;
		}

		[HttpPost(WebApiS3LowLevelTest.ListFileInFolder)]
		public async Task<List<string>> ListFileInFolder([FromRoute] string folderPath)
		{
			List<string> result = new List<string>();
			try
			{
				folderPath = folderPath.Trim() + "/";

				ListObjectsRequest request = new ListObjectsRequest()
				{
					BucketName = _bucketName,
					Prefix = folderPath  // <- in S3 key as path  
				};
				var response = await this._s3Client.ListObjectsAsync(request);

				foreach (S3Object obj in response.S3Objects)
				{
					result.Add($"{ this._bucketName}/{obj.Key}");
				}

			}
			catch (Exception ex)
			{
				result.Add(ex.Message);
			}
			return result;
		}


		[HttpPost(WebApiS3LowLevelTest.DeleteFile)]
		public async Task<string> DeleteFile([FromRoute] string fileName)
		{
			string result = String.Empty;
			string path = this._defaultFolder + "/" + fileName;
			try
			{
				DeleteObjectRequest request = new DeleteObjectRequest()
				{
					BucketName = _bucketName,
					Key = path  // <- in S3 key as path  
				};
				DeleteObjectResponse rsponse = await this._s3Client.DeleteObjectAsync(request);

				result = string.Format($"{ this._bucketName}/{path} ");
			}
			catch (Exception ex)
			{
				result = ex.Message;
			}
			return result;
		}

		[HttpPost(WebApiS3LowLevelTest.SparkTest)]
		public string SparkTest([FromServices] IAWSSettings awsSettings)
		{
			string result = "ok";
			try
			{
				SparkSession spark = SparkSession
					  .Builder()
					  .AppName("itur")
					  .GetOrCreate();

				var mySchema = new Microsoft.Spark.Sql.Types.StructType(new[]
					{
					new StructField("IturCode", new Microsoft.Spark.Sql.Types.StringType()),
					new StructField("IturERP", new Microsoft.Spark.Sql.Types.StringType()) ,
					new StructField("QuantityEdit", new Microsoft.Spark.Sql.Types.StringType()),
					new StructField("PartialQuantity", new Microsoft.Spark.Sql.Types.StringType())
				});

				string assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				string iturInputPath = Path.Combine(assemblyLocation, "data", "itur.csv");

				DataFrame df = spark.Read()
			   .Format("csv")
			   .Schema(mySchema)
			   .Option("delimiter", ",")
			   .Option("header", true)
			   //.Option("dateFormat", "dd/MM/yyyy")
			   .Load(iturInputPath);

				string dt = DateTime.Now.ToString("MMddhhmmss");
				string outputfile = Path.Combine(assemblyLocation, "outputData", $"itur_out{dt}.json");
				df.Write().Json(outputfile);

				//string toPath = $"s3n://{awsSettings.AccessKey}:{awsSettings.SecretKey}@{_bucketName}/{path}";
				//spark.Range(100).Repartition(5).Write().Mode("overwrite").Text(toPath) ;

				spark.Stop();
			}
			catch (Exception ex)
			{
				result = ex.Message;
			}
			return result;
		}

		[HttpPost(WebApiS3LowLevelTest.DeltaTest)]
		public string DeltaTest([FromServices] IAWSSettings awsSettings)
		{
			string result = String.Empty;
			try
			{
				SparkSession spark = SparkSession
					  .Builder()
					  .AppName("DeltaTest")
					  .GetOrCreate();

				string tempDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
	
				string dt = DateTime.Now.ToString("MMddhhmmss");
				string path = Path.Combine(tempDirectory, $"delta-table{dt}");

				// Write data to a Delta table.
				DataFrame data = spark.Range(0, 5);

				result += "Write data to a Delta table >> spark.Range(0, 5)" + "              ";
				foreach (var row in data.ToDF().Collect())
				{
					result += row.Values[0];
					result += " | ";
				}
				result += "              ";
				data.Write().Format("delta").Save(path);

				// Create a second iteration of the table.
				data = spark.Range(5, 10);
				result += "Create a second iteration of the table >> spark.Range(0, 5)" + "              ";
				foreach (var row in data.ToDF().Collect())
				{
					result += row.Values[0];
					result += " | ";
				}
				result += "              ";
				data.Write().Format("delta").Mode("overwrite").Save(path);

				// Load the data into a DeltaTable object.
				DeltaTable deltaTable = DeltaTable.ForPath(path);
				result += "Load the data into a DeltaTable object >> DeltaTable.ForPath" + "              ";
				foreach (var row in deltaTable.ToDF().Collect())
				{
					result += row.Values[0];
					result += " | ";
				}
				result += "              ";
				// Update every even value by adding 100 to it.
				deltaTable.Update(
					condition: Functions.Expr("id % 2 == 0"),
					set: new Dictionary<string, Column>() {
						{ "id", Functions.Expr("id + 100") }
					});

				result += "Update every even value by adding 100 to it." + "              ";
				foreach (var row in deltaTable.ToDF().Collect())
				{
					result += row.Values[0];
					result += " | ";
				}
				result += "              ";

				// Delete every even value.
				deltaTable.Delete(condition: Functions.Expr("id % 2 == 0"));
				result += "Delete every even value  id % 2 == 0" + "              ";
				foreach (var row in deltaTable.ToDF().Collect())
				{
					result += row.Values[0];
					result += " | ";
				}
				result += "              ";

				// Upsert (merge) new data.
				DataFrame newData = spark.Range(0, 20).As("newData").ToDF();
				result += "Upsert (merge) new data" + Environment.NewLine;
				foreach (var row in newData.ToDF().Collect())
				{
					result += row.Values[0];
					result += " | ";
				}
				result += "              ";

				deltaTable.As("oldData")
					.Merge(newData, "oldData.id = newData.id")
					.WhenMatched()
					.Update(
						new Dictionary<string, Column>() { { "id", Functions.Col("newData.id") } })
					.WhenNotMatched()
					.InsertExpr(new Dictionary<string, string>() { { "id", "newData.id" } })
					.Execute();


				spark.Stop();
			}
			catch (Exception ex)
			{
				result = ex.Message;
			}
			return result;
		}

	}
}
