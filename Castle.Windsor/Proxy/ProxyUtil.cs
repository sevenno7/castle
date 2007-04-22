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

namespace Castle.Windsor.Proxy
{
	using Castle.Core;

	/// <summary>
	/// Helper support for proxy configuration.
	/// </summary>
	public abstract class ProxyUtil
	{
		/// <summary>
		/// Obtains the <see cref="ProxyOptions"/> associated with the <see cref="ComponentModel"/>.
		/// </summary>
		/// <param name="model">The component model.</param>
		/// <param name="createOnDemand">true if the options should be created if not present.</param>
		/// <returns>The associated proxy options for the component model.</returns>
		public static ProxyOptions ObtainProxyOptions(ComponentModel model, bool createOnDemand)
		{
			ProxyOptions options = model.ExtendedProperties[ProxyConstants.ProxyOptionsKey]
			                       as ProxyOptions;

			if (options == null && createOnDemand)
			{
				options = new ProxyOptions();
				model.ExtendedProperties[ProxyConstants.ProxyOptionsKey] = options;
			}

			return options;
		}
	}
}