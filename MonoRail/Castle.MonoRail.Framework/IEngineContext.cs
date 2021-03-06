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

namespace Castle.MonoRail.Framework
{
	using System;
	using System.ComponentModel.Design;
	using System.Web;
	using System.Security.Principal;
	using System.Collections;
	using System.Collections.Specialized;
	using Castle.Components.Binder;

	/// <summary>
	/// Represents an abstraction between the MonoRail API
	/// and the ASP.Net API. 
	/// </summary>
	public interface IEngineContext : IServiceContainer
	{
		/// <summary>
		/// Returns an <see cref="IServerUtility"/>.
		/// </summary>
		IServerUtility Server { get; }

		/// <summary>
		/// Gets the underlying context of the API being used.
		/// </summary>
		HttpContext UnderlyingContext { get; }

		/// <summary>
		/// Gets a reference to the MonoRail services.
		/// </summary>
		/// <value>The services.</value>
		IMonoRailServices Services { get; }

		/// <summary>
		/// Access the session objects.
		/// </summary>
		IDictionary Session { get; set; }

		/// <summary>
		/// Gets the request object.
		/// </summary>
		IRequest Request { get; }

		/// <summary>
		/// Gets the response object.
		/// </summary>
		IResponse Response { get; }

		/// <summary>
		/// Gets the trace object.
		/// </summary>
		ITrace Trace { get; }

		/// <summary>
		/// Access a dictionary of volative items.
		/// </summary>
		Flash Flash { get; set; }

//		/// <summary>
//		/// Transfer the execution to another resource.
//		/// </summary>
//		/// <param name="path"></param>
//		/// <param name="preserveForm"></param>
//		void Transfer( String path, bool preserveForm );

		/// <summary>
		/// Gets or sets the current user.
		/// </summary>
		IPrincipal CurrentUser { get; set; }

		/// <summary>
		/// Gets the last exception raised during
		/// the execution of an action.
		/// </summary>
		Exception LastException { get; set; }

		/// <summary>
		/// Returns the application path.
		/// </summary>
		String ApplicationPath { get; }

		/// <summary>
		/// Returns the physical application path.
		/// </summary>
		String ApplicationPhysicalPath { get; }

		/// <summary>
		/// Returns the <see cref="UrlInfo"/> of the the current request.
		/// </summary>
		UrlInfo UrlInfo { get; }

		/// <summary>
		/// Returns the Items collection from the current HttpContext.
		/// </summary>
		IDictionary Items { get; }

		/// <summary>
		/// Gets or sets the current controller.
		/// </summary>
		/// <value>The current controller.</value>
		IController CurrentController { get; set; }

		/// <summary>
		/// Gets or sets the current controller context.
		/// </summary>
		/// <value>The current controller context.</value>
		IControllerContext CurrentControllerContext { get; set; }
	}
}
