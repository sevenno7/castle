// Copyright 2004 DigitalCraftsmen - http://www.digitalcraftsmen.com.br/
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

namespace Castle.MicroKernel.SubSystems.Configuration
{
	using System;
	using System.Collections;
	using System.Collections.Specialized;

	using Castle.Model.Configuration;

	/// <summary>
	/// Summary description for DefaultConfigurationStore.
	/// </summary>
	public class DefaultConfigurationStore : IConfigurationStore
	{
		private IDictionary _facilities;
		private IDictionary _components;

		public DefaultConfigurationStore()
		{
			_facilities = new HybridDictionary();
			_components = new HybridDictionary();
		}

		#region IConfigurationStore Members

		public void AddFacilityConfiguration(String key, IConfiguration config)
		{
			_facilities[key] = config;
		}

		public void AddComponentConfiguration(String key, IConfiguration config)
		{
			_components[key] = config;
		}

		public IConfiguration GetFacilityConfiguration(String key)
		{
			return _facilities[key] as IConfiguration;
		}

		public IConfiguration GetComponentConfiguration(String key)
		{
			return _components[key] as IConfiguration;
		}

		#endregion

		#region ISubSystem Members

		public void Init(IKernel kernel)
		{
		}

		public void Terminate()
		{
		}

		#endregion
	}
}
