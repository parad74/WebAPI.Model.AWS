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
			this.AccessKey = @"A_K_I_A_Z_KLXKAKGCIG5FZOH";
			this.SecretKey = @"D_4_U_9_z_KHRxqfSFUdrKGmDYbdhlJDIaS+NIGxEKtKV";
		}
		public void Validate()
		{
			Validator.ValidateObject(this, new ValidationContext(this), validateAllProperties: true);
		}

	}
}


