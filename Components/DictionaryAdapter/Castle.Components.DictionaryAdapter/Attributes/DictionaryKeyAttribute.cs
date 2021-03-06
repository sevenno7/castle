// Copyright 2004-2008 Castle Project - http://www.castleproject.org/
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

namespace Castle.Components.DictionaryAdapter
{
	using System;
	using System.Collections;

	/// <summary>
	/// Assignes a specific dictionary key.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class DictionaryKeyAttribute : DictionaryBehaviorAttribute, IDictionaryKeyBuilder
	{
		private readonly String key;

		/// <summary>
		/// Initializes a new instance of the <see cref="DictionaryKeyAttribute"/> class.
		/// </summary>
		/// <param name="key">The key.</param>
		public DictionaryKeyAttribute(String key)
		{
			this.key = key;
		}

		String IDictionaryKeyBuilder.GetKey(IDictionary dictionary, String key,
		                                   PropertyDescriptor property)
		{
			return this.key;
		}
	}
}