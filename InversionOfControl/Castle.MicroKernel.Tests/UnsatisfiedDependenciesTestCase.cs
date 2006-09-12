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

namespace Castle.MicroKernel.Tests
{
	using System;

	using NUnit.Framework;

	using Castle.Core.Configuration;

	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.Resolvers;
	using Castle.MicroKernel.Tests.ClassComponents;

	[TestFixture]
	public class UnsatisfiedDependenciesTestCase
	{
		private IKernel kernel;

		[SetUp]
		public void Init()
		{
			kernel = new DefaultKernel();
		}

		[TearDown]
		public void Dispose()
		{
			kernel.Dispose();
		}

		[Test]
		[ExpectedException( typeof(HandlerException) )]
		public void UnsatisfiedService()
		{
			kernel.AddComponent("key", typeof(CommonServiceUser));
			object instance = kernel["key"];
		}

		[Test]
		[ExpectedException( typeof(DependencyResolverException), "Could not resolve non-optional dependency for 'key' (Castle.MicroKernel.Tests.ClassComponents.CustomerImpl2). Parameter 'name' type 'System.String'" )]
		public void UnsatisfiedConfigValues()
		{
			MutableConfiguration config = new MutableConfiguration("component");
			
			MutableConfiguration parameters = (MutableConfiguration ) 
				config.Children.Add( new MutableConfiguration("parameters") );

			parameters.Children.Add( new MutableConfiguration("name", "hammett") );

			kernel.ConfigurationStore.AddComponentConfiguration("customer", config);

			kernel.AddComponent("key", typeof(CustomerImpl2));
			object instance = kernel["key"];
		}

		[Test]
		[ExpectedException( typeof(HandlerException), "Can't create component 'key' as it has dependencies to be satisfied. \r\nkey is waiting for the following dependencies: \r\n\r\nKeys (components with specific keys)\r\n- common2 which was not registered. \r\n" )]
		public void UnsatisfiedOverride()
		{
			MutableConfiguration config = new MutableConfiguration("component");
			
			MutableConfiguration parameters = (MutableConfiguration ) 
				config.Children.Add( new MutableConfiguration("parameters") );

			parameters.Children.Add( new MutableConfiguration("common", "${common2}") );

			kernel.ConfigurationStore.AddComponentConfiguration("key", config);

			kernel.AddComponent("common1", typeof(ICommon), typeof(CommonImpl1));
			kernel.AddComponent("key", typeof(CommonServiceUser));
			object instance = kernel["key"];
		}

		[Test]
		[ExpectedException( typeof(HandlerException), "Can't create component 'key' as it has dependencies to be satisfied. \r\nkey is waiting for the following dependencies: \r\n\r\nKeys (components with specific keys)\r\n- common2 which was not registered. \r\n" )]
		public void OverrideIsForcedDependency()
		{
			MutableConfiguration config = new MutableConfiguration("component");
			
			MutableConfiguration parameters = (MutableConfiguration ) 
				config.Children.Add( new MutableConfiguration("parameters") );

			parameters.Children.Add( new MutableConfiguration("common", "${common2}") );

			kernel.ConfigurationStore.AddComponentConfiguration("key", config);

			kernel.AddComponent("common1", typeof(ICommon), typeof(CommonImpl1));
			kernel.AddComponent("key", typeof(CommonServiceUser3));
			object instance = kernel["key"];
		}

		[Test]
		public void SatisfiedOverride()
		{
			MutableConfiguration config = new MutableConfiguration("component");
			
			MutableConfiguration parameters = (MutableConfiguration ) 
				config.Children.Add( new MutableConfiguration("parameters") );

			parameters.Children.Add( new MutableConfiguration("common", "${common2}") );

			kernel.ConfigurationStore.AddComponentConfiguration("key", config);

			kernel.AddComponent("common1", typeof(ICommon), typeof(CommonImpl1));
			kernel.AddComponent("common2", typeof(ICommon), typeof(CommonImpl2));
			kernel.AddComponent("key", typeof(CommonServiceUser));
			CommonServiceUser instance = (CommonServiceUser) kernel["key"];
			
			Assert.IsNotNull(instance);
			Assert.IsNotNull(instance.CommonService);
			Assert.AreEqual("CommonImpl2", instance.CommonService.GetType().Name);
		}
	}
}