﻿// Copyright 2004-2008 Castle Project - http://www.castleproject.org/
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

namespace Castle.Facilities.WcfIntegration
{
	using System;
	using System.ServiceModel;
	using System.ServiceModel.Channels;
	using Castle.Core;
	using Castle.Facilities.WcfIntegration.Internal;
	using Castle.Facilities.WcfIntegration.Rest;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Proxy;

	public class WcfClientExtension : IDisposable
	{
		private IKernel kernel;
		private WcfFacility facility;
		private WcfCommunicationDecomissionConcern decomission;
		private Binding defaultBinding;
		private TimeSpan? closeTimeout;

		public Binding DefaultBinding
		{
			get { return defaultBinding ?? facility.DefaultBinding; }
			set { defaultBinding = value; }
		}

		public TimeSpan? CloseTimeout
		{
			get { return closeTimeout ?? facility.CloseTimeout; }
			set { closeTimeout = value; }
		}

		public WcfClientExtension AddChannelBuilder<T, M>()
			where T : IClientChannelBuilder<M>
			where M : IWcfClientModel
		{
			AddChannelBuilder<T, M>(true);
			return this;
		}

		internal void Init(WcfFacility facility)
		{
			this.facility = facility;
			kernel = facility.Kernel;
			decomission = new WcfCommunicationDecomissionConcern(this);

			AddDefaultChannelBuilders();

			kernel.AddComponent<WcfManagedChannelInterceptor>();
			kernel.ComponentModelCreated += Kernel_ComponentModelCreated;
			kernel.ComponentUnregistered += Kernel_ComponentUnregistered;
		}

		private void Kernel_ComponentModelCreated(ComponentModel model)
		{
			IWcfClientModel clientModel = ResolveClientModel(model);

			if (clientModel != null)
			{
				model.CustomComponentActivator = typeof(WcfClientActivator);
				model.ExtendedProperties[WcfConstants.ClientModelKey] = clientModel;
				ProxyOptions options = ProxyUtil.ObtainProxyOptions(model, true);
				options.AddAdditionalInterfaces(typeof(IContextChannel));	
				model.LifecycleSteps.Add(LifecycleStepType.Decommission,
					WcfClientCleanUpDecomissionConcern.Instance);
				model.LifecycleSteps.Add(LifecycleStepType.Decommission, decomission);
				InstallManagedChannelInterceptor(model);

				var dependencies = new ExtensionDependencies(model, kernel)
					.Apply(new WcfEndpointExtensions(WcfExtensionScope.Clients))
					.Apply(clientModel.Extensions);

				if (clientModel.Endpoint != null)
				{
					dependencies.Apply(clientModel.Endpoint.Extensions);
				}
			}
		}

		private void Kernel_ComponentUnregistered(string key, IHandler handler)
		{
			ComponentModel model = handler.ComponentModel;
			var burden = model.ExtendedProperties[WcfConstants.ClientBurdenKey] as IWcfBurden;
			if (burden != null) burden.CleanUp();
		}

		private void AddDefaultChannelBuilders()
		{
			AddChannelBuilder<DefaultChannelBuilder, DefaultClientModel>(false);
			AddChannelBuilder<DuplexChannelBuilder, DuplexClientModel>(false);
			AddChannelBuilder<RestChannelBuilder, RestClientModel>(false);
        }

		internal void AddChannelBuilder<T, M>(bool force)
			where T : IClientChannelBuilder<M>
			where M : IWcfClientModel
		{
			if (force || !kernel.HasComponent(typeof(IClientChannelBuilder<M>)))
			{
				kernel.AddComponent<T>(typeof(IClientChannelBuilder<M>));
			}
		}

		private void InstallManagedChannelInterceptor(ComponentModel model)
		{
			model.Dependencies.Add(new DependencyModel(DependencyType.Service, null,
													   typeof(WcfManagedChannelInterceptor), false));
			model.Interceptors.Add(new InterceptorReference(typeof(WcfManagedChannelInterceptor)));
			ProxyOptions options = ProxyUtil.ObtainProxyOptions(model, true);
			options.AllowChangeTarget = true;
		}

		private IWcfClientModel ResolveClientModel(ComponentModel model)
		{

			if (model.Service.IsInterface)
			{
				foreach (var clientModel in WcfUtils.FindDependencies<IWcfClientModel>(model.CustomDependencies))
				{
					return clientModel;
				}
			}

			if (model.Configuration != null)
			{
				string endpointConfiguration =
					model.Configuration.Attributes[WcfConstants.EndpointConfiguration];

				if (!string.IsNullOrEmpty(endpointConfiguration))
				{
					return new DefaultClientModel(WcfEndpoint.FromConfiguration(endpointConfiguration));
				}
			}

			return null;
		}

		#region IDisposable Members

		public void Dispose()
		{
		}

		#endregion
	}
}
