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

namespace Castle.DynamicProxy
{
	using System;
	using System.Reflection;
	using System.Collections;

	/// <summary>
	/// Summary description for GeneratorContext.
	/// </summary>
	public sealed class GeneratorContext : DictionaryBase
	{
		private IList m_skipInterfaces = new ArrayList();
		private IList m_skipMethods = new ArrayList();
		private ArrayList m_mixins = new ArrayList();

		public GeneratorContext()
		{
		}

		public bool HasMixins
		{
			get { return m_mixins.Count != 0; }
		}

		public void AddMixinInstance(object instance)
		{
			m_mixins.Add(instance);
		}

		public object[] MixinsAsArray()
		{
			return m_mixins.ToArray();
		}

		public bool ShouldSkip(Type interfaceType)
		{
			return m_skipInterfaces.Contains(interfaceType);
		}

		public bool ShouldSkip(MethodInfo method)
		{
			return m_skipMethods.Contains(method);
		}

		public void AddInterfaceToSkip(Type interfaceType)
		{
			m_skipInterfaces.Add(interfaceType);
		}

		public void AddMethodToSkip(MethodInfo method)
		{
			m_skipMethods.Add(method);
		}

		public object this[String key]
		{
			get { return Dictionary[key]; }
			set { Dictionary[key] = value; }
		}
	}
}