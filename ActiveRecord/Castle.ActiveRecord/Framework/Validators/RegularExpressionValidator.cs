// Copyright 2004-2007 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.ActiveRecord.Framework.Validators
{
	using System;
	using System.Text.RegularExpressions;


	[Serializable]
	public class RegularExpressionValidator : AbstractValidator
	{
		private readonly Regex _regexRule;

		public RegularExpressionValidator(String expression) : this(expression, RegexOptions.Compiled)
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="RegularExpressionValidator"/> class.
		/// </summary>
		/// <param name="expression">The expression.</param>
		/// <param name="options">The regular expression options.</param>
		public RegularExpressionValidator(String expression, RegexOptions options)
		{
			_regexRule = new Regex(expression, options);
		}

		public override bool Perform(object instance, object fieldValue)
		{
			if (fieldValue != null)
			{
				return _regexRule.IsMatch( fieldValue.ToString() );
			}

			return true;
		}

		protected override string BuildErrorMessage()
		{
			return String.Format("Field {0} is not a valid entry for the expected pattern.", Property.Name);
		}
	}
}
