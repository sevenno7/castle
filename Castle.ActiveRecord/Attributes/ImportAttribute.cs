// Copyright 2004-2006 Castle Project - http://www.castleproject.org/
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

namespace Castle.ActiveRecord
{
	using System;

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true), Serializable]
	public class ImportAttribute : Attribute
	{
		private readonly Type type;
		private String rename;

		public ImportAttribute(Type type, string rename)
		{
			this.type = type;
			this.rename = rename;
		}

		public Type Type
		{
			get { return this.type; }
		}

		public String Rename
		{
			get { return rename; }
			set { rename = value; }
		}
	}
}