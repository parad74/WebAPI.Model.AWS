namespace WebAPI.Model.AWS
{
	public class Urls
	{
		public static class WebApiS3LowLevelTest
		{
			public const string CreateFolder = @"api/s3lowleveltest/CreateFolder{folderPath}";
			public const string CopyFileToFolder = @"api/s3lowleveltest/CopyFileToFolder{fileName}";
			public const string ListFileInFolder = @"api/s3lowleveltest/ListFileInFolder{folderPath}";
			public const string DeleteFile = @"api/s3lowleveltest/DeleteFile{fileName}";
			public const string SparkTest = @"api/s3lowleveltest/SparkTest";
			public const string DeltaTest = @"api/s3lowleveltest/DeltaTest";
			
			
 		}

	}
}
