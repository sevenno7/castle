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


namespace Castle.DynamicProxy.Tests
{
	using System;
	using System.Collections.Generic;
	using Castle.DynamicProxy.Tests.GenInterfaces;
	using Castle.DynamicProxy.Tests.Interceptors;
	using NUnit.Framework;

	[TestFixture]
	public class GenericInterfaceProxyTestCase : BasePEVerifyTestCase
	{
		private ProxyGenerator generator;
		private LogInvocationInterceptor logger;

		[SetUp]
		public void Init()
		{
			generator = new ProxyGenerator();
			logger = new LogInvocationInterceptor();
		}

		[Test]
		public void ProxyWithGenericArgument()
		{
			GenInterface<int> proxy = 
				generator.CreateInterfaceProxyWithTarget<GenInterface<int>>(
					new GenInterfaceImpl<int>(), logger);

			Assert.IsNotNull(proxy);

			Assert.AreEqual(1, proxy.DoSomething(1));

			Assert.AreEqual("DoSomething ", logger.LogContents);
		}

		[Test]
		public void ProxyWithGenericArgumentAndGenericMethod()
		{
			GenInterfaceWithGenMethods<int> proxy =
				generator.CreateInterfaceProxyWithTarget<GenInterfaceWithGenMethods<int>>(
					new GenInterfaceWithGenMethodsImpl<int>(), logger);

			Assert.IsNotNull(proxy);

			proxy.DoSomething<long>(10L, 1);

			Assert.AreEqual("DoSomething ", logger.LogContents);
		}

		[Test]
		public void ProxyWithGenericArgumentAndGenericMethodAndGenericReturn()
		{
			GenInterfaceWithGenMethodsAndGenReturn<int> proxy =
				generator.CreateInterfaceProxyWithTarget<GenInterfaceWithGenMethodsAndGenReturn<int>>(
					new GenInterfaceWithGenMethodsAndGenReturnImpl<int>(), logger);

			Assert.IsNotNull(proxy);

			Assert.AreEqual(10L, proxy.DoSomething<long>(10L, 1));

			Assert.AreEqual("DoSomething ", logger.LogContents);
		}
		
		[Test]
		public void ProxyWithGenInterfaceWithGenericTypes()
		{
			GenInterfaceWithGenericTypes proxy =
				generator.CreateInterfaceProxyWithTarget<GenInterfaceWithGenericTypes>(
					new GenInterfaceWithGenericTypesImpl(), logger);

			Assert.IsNotNull(proxy);

			Assert.IsNotNull(proxy.Find(""));
			Assert.IsNotNull(proxy.Find<String>(""));
			
			proxy.Populate<String>(new List<String>());

			Assert.AreEqual("Find Find Populate ", logger.LogContents);

		}

		[Test]
		[Ignore("This fails with: Could not find a matching method on System.Collections.Generic.List`1. Method CopyTo")]
		public void ProxyWithGenericTypeThatInheritFromGenericType()
		{
			IList<int> list = generator.CreateInterfaceProxyWithTarget<IList<int>>(new List<int>(), logger);
			list.Add(1);
			list.Add(2);
			list.Remove(1);
			
			Assert.AreEqual("Add Add Remove", logger.LogContents);
		}
	}
}