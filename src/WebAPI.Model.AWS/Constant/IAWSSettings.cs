using System.ComponentModel.DataAnnotations;

namespace WebAPI.Model.AWS.Constant
{
	public interface IAWSSettings
	{
		string Region { get; set; }
		string AccessKey { get; set; }
		string SecretKey { get; set; }
	}

	public class AWSSettings : IValidatable, IAWSSettings
	{
		[Required]
		public string Region { get; set; }
		[Required]
		public string AccessKey { get; set; }
		[Required]
		public string SecretKey { get; set; }

		public AWSSettings()
		{
			this.Region = @"eu-east-1";
			this.AccessKey = @"AKIAZKLXKAKGCIG5FZOH";
			this.SecretKey = @"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";
		}
		public void Validate()
		{
			Validator.ValidateObject(this, new ValidationContext(this), validateAllProperties: true);
		}

	}
}


