﻿// Copyright 2004-2007 Castle Project - http://www.castleproject.org/
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
	using System.Collections;
	using System.Web;
	using System.Web.SessionState;
	using Adapters;

	/// <summary>
	/// Pendent
	/// </summary>
	public abstract class BaseHttpHandler : IHttpHandler
	{
		private readonly IController controller;
		private readonly IControllerContext controllerContext;
		private readonly IEngineContext engineContext;
		private readonly bool sessionless;

		/// <summary>
		/// Initializes a new instance of the <see cref="BaseHttpHandler"/> class.
		/// </summary>
		/// <param name="engineContext">The engine context.</param>
		/// <param name="controller">The controller.</param>
		/// <param name="controllerContext">The controller context.</param>
		/// <param name="sessionless">if set to <c>true</c> then we wont have a session to work.</param>
		protected BaseHttpHandler(IEngineContext engineContext,
								  IController controller, IControllerContext controllerContext, bool sessionless)
		{
			this.controller = controller;
			this.controllerContext = controllerContext;
			this.engineContext = engineContext;
			this.sessionless = sessionless;
		}

		#region IHttpHandler

		/// <summary>
		/// Pendent
		/// </summary>
		/// <param name="context"></param>
		public void ProcessRequest(HttpContext context)
		{
			Process(context);
		}

		/// <summary>
		/// Pendent
		/// </summary>
		public bool IsReusable
		{
			get { return false; }
		}

		#endregion

		/// <summary>
		/// Performs the base work of MonoRail. Extracts
		/// the information from the URL, obtain the controller
		/// that matches this information and dispatch the execution
		/// to it.
		/// </summary>
		/// <param name="context">The context.</param>
		public virtual void Process(HttpContext context)
		{
			if (!sessionless)
			{
				// Now we have a session
				engineContext.Session = ResolveSession(context);
			}

			// items added to be used by the test context
			context.Items["mr.controller"] = controller;
			context.Items["mr.flash"] = engineContext.Flash;
			context.Items["mr.propertybag"] = controllerContext.PropertyBag;
			context.Items["mr.session"] = context.Session;

			try
			{
				controller.Process(engineContext, controllerContext);
			}
			catch(Exception ex)
			{
				throw new MonoRailException("Error processing action " +
				                            controllerContext.Action + " on controller " + controllerContext.Name, ex);
			}
			finally
			{
				if (!sessionless)
				{
					PersistFlashItems();
				}

				ReleaseController(controller);
			}
		}

		/// <summary>
		/// Resolves the session.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns></returns>
		protected virtual IDictionary ResolveSession(HttpContext context)
		{
			object session;

			if (context.Items["AspSession"] != null)
			{
				// Windows and Testing
				session = context.Items["AspSession"];
			}
			else
			{
				// Mono
				session = context.Session;
			}

			if (session is HttpSessionState)
			{
				return new SessionAdapter(session as HttpSessionState);
			}
			else
			{
				return (IDictionary)session;
			}	
		}

		private void ReleaseController(IController controller)
		{
			engineContext.Services.ControllerFactory.Release(controller);
		}

		private void PersistFlashItems()
		{
			Flash currentFlash = engineContext.Flash;

			if (currentFlash == null) return;

			currentFlash.Sweep();

			if (currentFlash.HasItemsToKeep)
			{
				engineContext.Session[Flash.FlashKey] = currentFlash;
			}
			else if (engineContext.Session.Contains(Flash.FlashKey))
			{
				engineContext.Session.Remove(Flash.FlashKey);
			}
		}
	}
}