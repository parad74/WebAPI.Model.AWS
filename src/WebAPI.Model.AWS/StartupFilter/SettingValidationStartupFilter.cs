using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using WebAPI.Model.AWS.Constant;

namespace Service.Filter
{
	public class SettingValidationStartupFilter : IStartupFilter
	{
		readonly IEnumerable<IValidatable> _validatableObjects;
		public SettingValidationStartupFilter(IEnumerable<IValidatable> validatableObjects)
		{
			this._validatableObjects = validatableObjects;
		}

		public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
		{
			foreach (var validatableObject in this._validatableObjects)
			{
				validatableObject.Validate();
			}

			return next;
		}
	}
}   
