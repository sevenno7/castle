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

namespace Castle.MonoRail.Framework
{
	using System;

	/// <summary>
	/// Declares that for the specific method (action)
	/// no filter should be applied.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple=true), Serializable]
	public class SkipFilterAttribute : Attribute
	{
		private Type _filterType;

		/// <summary>
		/// Constructs a SkipFilterAttribute which skips all filters.
		/// </summary>
		public SkipFilterAttribute()
		{
		}

		/// <summary>
		/// Constructs a SkipFilterAttribute associating 
		/// the filter type that should be skipped.
		/// </summary>
		/// <param name="filterType"></param>
		public SkipFilterAttribute(Type filterType)
		{
			_filterType = filterType;
		}

		public Type FilterType
		{
			get { return _filterType; }
		}

		public bool BlanketSkip
		{
			get { return _filterType == null; }
		}
	}
}