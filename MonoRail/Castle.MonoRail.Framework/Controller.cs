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
	using System.IO;
	using System.Text;
	using System.Web;
	using System.Reflection;
	using System.Threading;
	using System.Collections;
	using System.Collections.Specialized;

	using Castle.Components.Common.EmailSender;
	using Castle.Core;
	using Castle.Core.Logging;
	using Castle.MonoRail.Framework.Configuration;
	using Castle.MonoRail.Framework.Helpers;
	using Castle.MonoRail.Framework.Internal;

	/// <summary>
	/// Implements the core functionality and exposes the
	/// common methods for concrete controllers.
	/// </summary>
	public abstract class Controller
	{
		#region Fields

		/// <summary>
		/// The reference to the <see cref="IViewEngine"/> instance
		/// </summary>
		internal IViewEngine viewEngine;

		/// <summary>
		/// Holds the request/context information
		/// </summary>
		internal IRailsEngineContext context;
		
		/// <summary>
		/// Logger instance. Should never be null
		/// </summary>
		private ILogger logger = NullLogger.Instance;

		/// <summary>
		/// Holds information to pass to the view
		/// </summary>
		private IDictionary bag = new HybridDictionary();

		/// <summary>
		/// Holds the filters associated with the action
		/// </summary>
		private FilterDescriptor[] filters;

		/// <summary>
		/// Reference to the <see cref="IFilterFactory"/> instance
		/// </summary>
		internal IFilterFactory filterFactory;

		/// <summary>
		/// The area name which was used to access this controller
		/// </summary>
		private String _areaName;

		/// <summary>
		/// The controller name which was used to access this controller
		/// </summary>
		private String _controllerName;

		/// <summary>
		/// The view name selected to be rendered after the execution 
		/// of the action
		/// </summary>
		private String _selectedViewName;

		/// <summary>
		/// The layout name that the view engine should use
		/// </summary>
		private String _layoutName;

		/// <summary>
		/// The original action requested
		/// </summary>
		private String _evaluatedAction;

		/// <summary>
		/// The helper instances collected
		/// </summary>
		private IDictionary helpers = null;

		/// <summary>
		/// The resources associated with this controller
		/// </summary>
		private ResourceDictionary resources = null;

		/// <summary>
		/// Reference to the <see cref="IResourceFactory"/> instance
		/// </summary>
		private IResourceFactory resourceFactory;

		internal IDictionary _dynamicActions = new HybridDictionary(true);

		internal IScaffoldingSupport scaffoldSupport;

		internal bool directRenderInvoked;

		private ControllerMetaDescriptor metaDescriptor;

		private IServiceProvider serviceProvider;

		private bool isPostBack = false;
     
		#endregion

		#region Constructors

		/// <summary>
		/// Constructs a Controller
		/// </summary>
		public Controller()
		{
			
		}

		#endregion

		#region Useful Properties

		/// <summary>
		/// This is intended to be used by MonoRail infrastructure.
		/// </summary>
		public ControllerMetaDescriptor MetaDescriptor
		{
			get { return metaDescriptor; }
		}

		public ICollection Actions
		{
			get { return metaDescriptor.Actions.Values; }
		}

		public ResourceDictionary Resources
		{
			get { return resources; }
		}

		public IDictionary Helpers
		{
			get
			{
				if (helpers == null) CreateAndInitializeHelpers();

				return helpers;
			}
		}

		/// <summary>
		/// Gets the controller's name.
		/// </summary>
		public String Name
		{
			get { return _controllerName; }
		}

		/// <summary>
		/// Gets the controller's area name.
		/// </summary>
		public String AreaName
		{
			get { return _areaName; }
		}

		/// <summary>
		/// Gets or set the layout being used.
		/// </summary>
		public String LayoutName
		{
			get { return _layoutName; }
			set { _layoutName = value; }
		}

		/// <summary>
		/// Gets the name of the action being processed.
		/// </summary>
		public String Action
		{
			get { return _evaluatedAction; }
		}

		/// <summary>
		/// Logger for the controller
		/// </summary>
		public ILogger Logger
		{
			get { return logger; }
		}

		/// <summary>
		/// Gets or sets the view which will be rendered by this action.
		/// </summary>
		public String SelectedViewName
		{
			get { return _selectedViewName; }
			set { _selectedViewName = value; }
		}

		/// <summary>
		/// Gets the property bag, which is used
		/// to pass variables to the view.
		/// </summary>
		public IDictionary PropertyBag
		{
			get { return bag; }
			set { bag = value; }
		}

		/// <summary>
		/// Gets the context of this web execution.
		/// </summary>
		public IRailsEngineContext Context
		{
			get { return context; }
		}

		/// <summary>
		/// Gets the Session dictionary.
		/// </summary>
		protected IDictionary Session
		{
			get { return context.Session; }
		}

		/// <summary>
		/// Gets a dictionary of volative items.
		/// Ideal for showing success and failures messages.
		/// </summary>
		public Flash Flash
		{
			get { return context.Flash; }
		}

		/// <summary>
		/// Gets the web context of ASP.NET API.
		/// </summary>
		protected HttpContext HttpContext
		{
			get { return context.UnderlyingContext; }
		}

		/// <summary>
		/// Gets the request object.
		/// </summary>
		public IRequest Request
		{
			get { return Context.Request; }
		}

		/// <summary>
		/// Gets the response object.
		/// </summary>
		public IResponse Response
		{
			get { return Context.Response; }
		}

		/// <summary>
		/// Shortcut to <see cref="IRequest.Params"/> 
		/// </summary>
		public NameValueCollection Params
		{
			get { return Request.Params; }
		}
		
		/// <summary>
		/// Shortcut to <see cref="IRequest.Form"/> 
		/// </summary>
		public NameValueCollection Form
		{
			get { return Request.Form; }
		}

		/// <summary>
		/// Shortcut to <see cref="IRequest.QueryString"></see>
		/// </summary>
		public NameValueCollection Query
		{
			get { return Request.QueryString; }
		}

		public IDictionary DynamicActions
		{
			get { return _dynamicActions; }
		}

		[Obsolete("Use the DynamicActions property instead")]
		public IDictionary CustomActions
		{
			get { return _dynamicActions; }
		}

		/// <summary>
		/// Shortcut to 
		/// <see cref="IResponse.IsClientConnected"/>
		/// </summary>
		protected bool IsClientConnected
		{
			get { return context.Response.IsClientConnected; }
		}

 		/// <summary>
		/// Determines if the current Action resulted from an ASP.NET PostBack.
		/// As a result, this property is only relavent when using WebForms views.
		/// It is placed on the base Controller for convenience only to avoid the
		/// need to extend the Controller or provide additional helper classes.
		/// </summary>
		protected bool IsPostBack
		{
			get { return isPostBack; }
		}

		private void DetermineIfPostBack()
		{
			NameValueCollection fields = Context.Params;
			isPostBack = (fields["__VIEWSTATE"] != null) || (fields["__EVENTTARGET"] != null);
		}

		#endregion

		#region Useful Operations

		/// <summary>
		/// Specifies the view to be processed after the action has finished its processing. 
		/// </summary>
		public void RenderView(String name)
		{
			String basePath = _controllerName;

			if (_areaName != null && _areaName.Length > 0)
			{
				basePath = Path.Combine(_areaName, _controllerName);
			}

			_selectedViewName = Path.Combine(basePath, name);
		}

		/// <summary>
		/// Specifies the view to be processed after the action has finished its processing. 
		/// </summary>
		public void RenderView(String name, bool skipLayout)
		{
			if (skipLayout) CancelLayout();

			RenderView(name);
		}

		/// <summary>
		/// Specifies the view to be processed after the action has finished its processing. 
		/// </summary>
		public void RenderView(String controller, String name)
		{
			_selectedViewName = Path.Combine(controller, name);
		}

		/// <summary>
		/// Specifies the view to be processed after the action has finished its processing. 
		/// </summary>
		public void RenderView(String controller, String name, bool skipLayout)
		{
			if (skipLayout) CancelLayout();

			RenderView(controller, name);
		}

		/// <summary>
		/// Specifies the view to be processed and results are written to System.IO.TextWriter. 
		/// </summary>
		/// <param name="output"></param>
		/// <param name="name">The name of the view to process.</param>
		public void InPlaceRenderView(TextWriter output, String name)
		{
			String basePath = _controllerName;

			if (_areaName != null && _areaName.Length > 0)
			{
				basePath = Path.Combine(_areaName, _controllerName);
			}

			viewEngine.Process(output, Context, this, Path.Combine(basePath, name));			
		}

		/// <summary>
		/// Specifies the shared view to be processed after the action has finished its
		/// processing. (A partial view shared 
		/// by others views and usually in the root folder
		/// of the view directory).
		/// </summary>
		public void RenderSharedView(String name)
		{
			_selectedViewName = name;
		}

		/// <summary>
		/// Specifies the shared view to be processed after the action has finished its
		/// processing. (A partial view shared 
		/// by others views and usually in the root folder
		/// of the view directory).
		/// </summary>
		public void RenderSharedView(String name, bool skipLayout)
		{
			if (skipLayout) CancelLayout();
			
			RenderSharedView(name);
		}

		/// <summary>
		/// Specifies the shared view to be processed and results are written to System.IO.TextWriter.
		/// (A partial view shared by others views and usually in the root folder
		/// of the view directory).
		/// </summary>
		/// <param name="output"></param>
		/// <param name="name">The name of the view to process.</param>
		public void InPlaceRenderSharedView(TextWriter output, String name)
		{
			viewEngine.Process(output, Context, this, name);
		}

		/// <summary>
		/// Cancels the view processing.
		/// </summary>
		public void CancelView()
		{
			_selectedViewName = null;
		}

		/// <summary>
		/// Cancels the layout processing.
		/// </summary>
		public void CancelLayout()
		{
			LayoutName = null;
		}

		/// <summary>
		/// Cancels the view processing and writes
		/// the specified contents to the browser
		/// </summary>
		public void RenderText(String contents)
		{
			CancelView();

			Response.Write(contents);
		}
		
		/// <summary>
		/// Cancels the view processing and writes
		/// the specified contents to the browser
		/// </summary>
		public void RenderText(String contents, params object[] args)
		{
			RenderText(String.Format(contents, args));
		}

		/// <summary>
		/// Cancels the view processing and writes
		/// the specified contents to the browser
		/// </summary>
		public void RenderText(IFormatProvider formatProvider, String contents, params object[] args)
		{
			RenderText(String.Format(formatProvider, contents, args));
		}

		/// <summary>
		/// Sends raw contents to be rendered directly by the view engine.
		/// It's up to the view engine just to apply the layout and nothing else.
		/// </summary>
		/// <param name="contents">Contents to be rendered.</param>
		public void DirectRender(String contents)
		{
			CancelView();

			if (directRenderInvoked)
			{
				throw new ControllerException("DirectRender should be called only once.");
			}

			directRenderInvoked = true;

			viewEngine.ProcessContents(context, this, contents);
		}

		/// <summary>
		/// Returns true if the specified template exists.
		/// </summary>
		/// <param name="templateName"></param>
		public bool HasTemplate(String templateName)
		{
			return viewEngine.HasTemplate(templateName);
		}

		#region RedirectToAction
		
		/// <summary> 
		/// Redirects to another action in the same controller.
		/// </summary>
		/// <param name="action">The action name</param>
		protected void RedirectToAction(String action)
		{
			RedirectToAction(action, (NameValueCollection) null);
		}

		/// <summary> 
		/// Redirects to another action in the same controller.
		/// </summary>
		protected void RedirectToAction(String action, params String[] queryStringParameters)
		{
			RedirectToAction(action, DictHelper.Create(queryStringParameters));
		}

		/// <summary> 
		/// Redirects to another action in the same controller.
		/// </summary>
		protected void RedirectToAction(String action, IDictionary queryStringParameters)
		{
			if (queryStringParameters != null)
			{
				Redirect(AreaName, Name, TransformActionName(action), queryStringParameters);
			}
			else
			{
				Redirect(AreaName, Name, TransformActionName(action));
			}
		}

		/// <summary> 
		/// Redirects to another action in the same controller.
		/// </summary>
		protected void RedirectToAction(String action, NameValueCollection queryStringParameters)
		{
			if (queryStringParameters != null)
			{
				Redirect(AreaName, Name, TransformActionName(action), queryStringParameters);
			}
			else
			{
				Redirect(AreaName, Name, TransformActionName(action));
			}
		}

		/// <summary>
		/// Gives a chance to subclasses to format the action name properly
		/// <seealso cref="WizardStepPage"/>
		/// </summary>
		/// <param name="action">Raw action name</param>
		/// <returns>Properly formatted action name</returns>
		internal virtual String TransformActionName(String action)
		{
			return action;
		}

		#endregion

		protected String CreateAbsoluteRailsUrl(String area, String controller, String action)
		{
			return UrlInfo.CreateAbsoluteRailsUrl(Context.ApplicationPath, area, controller, action, Context.UrlInfo.Extension);
		}
		
		protected String CreateAbsoluteRailsUrl(String controller, String action)
		{
			return UrlInfo.CreateAbsoluteRailsUrl(Context.ApplicationPath, controller, action, Context.UrlInfo.Extension);
		}
		
		protected String CreateAbsoluteRailsUrlForAction(String action)
		{
			return UrlInfo.CreateAbsoluteRailsUrl(Context.ApplicationPath, AreaName, Name, action, Context.UrlInfo.Extension);
		}
		
		/// <summary>
		/// Redirects to the specified URL. All other Redirects call this one.
		/// </summary>
		/// <param name="url">Target URL</param>
		public virtual void Redirect(String url)
		{
			CancelView();

			context.Response.Redirect(url);
		}

		/// <summary>
		/// Redirects to the specified URL. All other Redirects call this one.
		/// </summary>
		/// <param name="url">Target URL</param>
		/// <param name="parameters">URL parameters</param>
		public virtual void Redirect(String url, IDictionary parameters)
		{
			CancelView();
			
			if (parameters != null && parameters.Count != 0)
			{
				if (url.IndexOf('?') != -1)
				{
					url = url + '&' + ToQueryString(parameters);
				}
				else
				{
					url = url + '?' + ToQueryString(parameters);
				}
			}

			context.Response.Redirect(url);
		}

		/// <summary>
		/// Redirects to another controller and action.
		/// </summary>
		/// <param name="controller">Controller name</param>
		/// <param name="action">Action name</param>
		public void Redirect(String controller, String action)
		{
			Redirect( CreateAbsoluteRailsUrl(controller, action) );
		}

		/// <summary>
		/// Redirects to another controller and action.
		/// </summary>
		/// <param name="area">Area name</param>
		/// <param name="controller">Controller name</param>
		/// <param name="action">Action name</param>
		public void Redirect(String area, String controller, String action)
		{
			Redirect( CreateAbsoluteRailsUrl(area, controller, action) );
		}

		/// <summary>
		/// Redirects to another controller and action with the specified paramters.
		/// </summary>
		/// <param name="controller">Controller name</param>
		/// <param name="action">Action name</param>
		/// <param name="parameters">Key/value pairings</param>
		public void Redirect(String controller, String action, NameValueCollection parameters)
		{
			String querystring = ToQueryString(parameters);
			String url = CreateAbsoluteRailsUrl(controller, action);

			Redirect(url + '?' + querystring);
		}

		/// <summary>
		/// Redirects to another controller and action with the specified paramters.
		/// </summary>
		/// <param name="area">Area name</param>
		/// <param name="controller">Controller name</param>
		/// <param name="action">Action name</param>
		/// <param name="parameters">Key/value pairings</param>
		public void Redirect(String area, String controller, String action, NameValueCollection parameters)
		{
			String querystring = ToQueryString(parameters);
			String url = CreateAbsoluteRailsUrl(area, controller, action);

			Redirect(url + '?' + querystring);
		}
		
		/// <summary>
		/// Redirects to another controller and action with the specified paramters.
		/// </summary>
		/// <param name="controller">Controller name</param>
		/// <param name="action">Action name</param>
		/// <param name="parameters">Key/value pairings</param>
		public void Redirect(String controller, String action, IDictionary parameters)
		{
			String querystring = ToQueryString(parameters);
			String url = CreateAbsoluteRailsUrl(controller, action);

			Redirect(url + '?' + querystring);
		}

		/// <summary>
		/// Redirects to another controller and action with the specified paramters.
		/// </summary>
		/// <param name="area">Area name</param>
		/// <param name="controller">Controller name</param>
		/// <param name="action">Action name</param>
		/// <param name="parameters">Key/value pairings</param>
		public void Redirect(String area, String controller, String action, IDictionary parameters)
		{
			String querystring = ToQueryString(parameters);
			String url = CreateAbsoluteRailsUrl(area, controller, action);

			Redirect(url + '?' + querystring);
		}

		protected String ToQueryString(NameValueCollection parameters)
		{
			StringBuilder buffer = new StringBuilder();
			IServerUtility srv = Context.Server;
	
			foreach(String key in parameters.Keys)
			{
				buffer.Append( srv.UrlEncode(key) )
					  .Append('=')
					  .Append( srv.UrlEncode(parameters[key]) )
					  .Append('&');
			}

			if (buffer.Length > 0) // removing extra &
			{
				buffer.Length -= 1;
			} 

			return buffer.ToString();
		}

		protected String ToQueryString(IDictionary parameters)
		{
			if (parameters == null || parameters.Count == 0)
			{
				return String.Empty;
			}
			
			StringBuilder buffer = new StringBuilder();
			IServerUtility srv = Context.Server;
	
			foreach(DictionaryEntry entry in parameters)
			{
				buffer.Append( srv.UrlEncode(Convert.ToString(entry.Key)) )
					  .Append('=')
					  .Append( srv.UrlEncode(Convert.ToString(entry.Value)) )
					  .Append('&');
			}
			
			if (buffer.Length > 0)
			{
				// removes extra &
				buffer.Length -= 1;
			} 

			return buffer.ToString();
		}

		#endregion

		#region Core members

		/// <summary>
		/// Returns the current controller instance
		/// </summary>
		/// <remarks>
		/// The <see cref="Controller.Process"/> is responsible
		/// for adding the controller instance.
		/// </remarks>
		internal static Controller CurrentController
		{
			get { return (Controller) HttpContext.Current.Items["mr.controller"] ; }
		}

		internal bool ShouldCheckWhetherClientHasDisconnected
		{
			get 
			{ 
				MonoRailConfiguration conf = (MonoRailConfiguration) 
					context.GetService(typeof(MonoRailConfiguration)); 
				return conf.CheckClientIsConnected;
			}
		}

		protected internal IServiceProvider ServiceProvider
		{
			get { return serviceProvider; }
		}

		internal void InitializeFieldsFromServiceProvider(IRailsEngineContext context)
		{
			serviceProvider = context;
			
			viewEngine = (IViewEngine) serviceProvider.GetService(typeof(IViewEngine));
			filterFactory = (IFilterFactory) serviceProvider.GetService(typeof(IFilterFactory));
			resourceFactory = (IResourceFactory) serviceProvider.GetService(typeof(IResourceFactory));
			scaffoldSupport = (IScaffoldingSupport) serviceProvider.GetService(typeof(IScaffoldingSupport));

			IControllerDescriptorProvider controllerDescriptorBuilder = (IControllerDescriptorProvider)
				serviceProvider.GetService( typeof(IControllerDescriptorProvider) );

			metaDescriptor = controllerDescriptorBuilder.BuildDescriptor(this);

			ILoggerFactory loggerFactory = (ILoggerFactory) context.GetService(typeof(ILoggerFactory));
			
			if (loggerFactory != null)
			{
				logger = loggerFactory.Create(GetType().Name);
			}
			
			this.context = context;
		}

		internal void InitializeControllerState(String areaName, String controllerName, String actionName)
		{
			SetEvaluatedAction(actionName);
			_areaName = areaName;
			_controllerName = controllerName;
		}

		internal void SetEvaluatedAction(String actionName)
		{
			_evaluatedAction = actionName;
		}

		/// <summary>
		/// Method invoked by the engine to start 
		/// the controller process. 
		/// </summary>
		internal void Process(IRailsEngineContext context, 
		                      String areaName, String controllerName, String actionName)
		{        
			InitializeFieldsFromServiceProvider(context);

			InitializeControllerState(areaName, controllerName, actionName);

			HttpContext.Items["mr.controller"] = this;

#if ALLOWTEST
			HttpContext.Items["mr.flash"] = Flash;
			HttpContext.Items["mr.session"] = Session;
			HttpContext.Items["mr.propertybag"] = PropertyBag;
#endif

			if (metaDescriptor.Filters.Length != 0)
			{
				filters = CopyFilterDescriptors();
			}

			LayoutName = ObtainDefaultLayoutName();

			ProcessScaffoldIfPresent();

			ActionProviderUtil.RegisterActions(this);

			DetermineIfPostBack();

			Initialize();

			InternalSend(actionName, null);
		}

		/// <summary>
		/// Performs the specified action, which means:
		/// <br/>
		/// 1. Define the default view name<br/>
		/// 2. Run the before filters<br/>
		/// 3. Select the method related to the action name and invoke it<br/>
		/// 4. On error, execute the rescues if available<br/>
		/// 5. Run the after filters<br/>
		/// 6. Invoke the view engine<br/>
		/// </summary>
		/// <param name="action">Action name</param>
		public void Send(String action)
		{
			isPostBack = false;
			InternalSend(action, null);
		}

		/// <summary>
		/// Performs the specified action with arguments.
		/// </summary>
		/// <param name="action">Action name</param>
		/// <param name="actionArgs">Action arguments</param>
		public void Send(String action, params object[] actionArgs)
		{
			isPostBack = false;
			InternalSend(action, actionArgs);
		}
	    
		/// <summary>
		/// Performs the specified action, which means:
		/// <br/>
		/// 1. Define the default view name<br/>
		/// 2. Run the before filters<br/>
		/// 3. Select the method related to the action name and invoke it<br/>
		/// 4. On error, execute the rescues if available<br/>
		/// 5. Run the after filters<br/>
		/// 6. Invoke the view engine<br/>
		/// </summary>
		/// <param name="action">Action name</param>
		/// <param name="actionArgs">Action arguments</param>
		protected virtual void InternalSend(String action, object[] actionArgs)
		{
			// If a redirect was sent there's no point in
			// wasting processor cycles
			if (Response.WasRedirected) return;
			
			if (logger.IsDebugEnabled)
			{
				logger.Debug("InternalSend for action '{0}'", action);
			}

			bool checkWhetherClientHasDisconnected = ShouldCheckWhetherClientHasDisconnected;

			// Nothing to do if the peer disconnected
			if (checkWhetherClientHasDisconnected && !IsClientConnected) return;

			// Record the action
			SetEvaluatedAction(action);

			// Record the default view for this area/controller/action
			RenderView(action);

			// If we have an HttpContext available, store the original view name
			if (HttpContext != null)
			{
				if (!HttpContext.Items.Contains(Constants.OriginalViewKey))
				{
					HttpContext.Items[Constants.OriginalViewKey] = _selectedViewName;
				}
			}

			// Look for the target method

			MethodInfo method = SelectMethod(action, MetaDescriptor.Actions, context.Request, actionArgs);

			// If we couldn't find a method for this action, look for a dynamic action
			IDynamicAction dynAction = null;
			
			if (method == null)
			{
				dynAction = DynamicActions[action] as IDynamicAction;

				if (dynAction == null)
				{
					method = FindOutDefaultMethod(actionArgs);

					if (method == null)
					{
						throw new ControllerException(String.Format("Unable to locate action [{0}] on controller [{1}].", action, Name));
					}
				}
			}
			else
			{
				ActionMetaDescriptor actionDesc = MetaDescriptor.GetAction(method);

				// Overrides the current layout, if the action specifies one
				if (actionDesc.Layout != null)
				{
					LayoutName = actionDesc.Layout.LayoutName;
				}

				if (actionDesc.AccessibleThrough != null)
				{
					string verbName = actionDesc.AccessibleThrough.Verb.ToString();
					string requestType = Context.RequestType;

					if (String.Compare(verbName, requestType, true) != 0)
					{
						throw new ControllerException(string.Format("Access to the action [{0}] " + 
							"on controller [{1}] is not allowed by the http verb [{2}].", 
							action.ToLower(), Name.ToLower(), requestType));
					}
				}
			}

			HybridDictionary filtersToSkip = new HybridDictionary();

			bool skipFilters = ShouldSkip(method, filtersToSkip);
			bool hasError = false;
			Exception exceptionToThrow = null;

			try
			{
				// Nothing to do if the peer disconnected
				if (checkWhetherClientHasDisconnected && !IsClientConnected) return;

				// If we are supposed to run the filters...
				if (!skipFilters)
				{
					// ...run them. If they fail...
					if (!ProcessFilters(ExecuteEnum.BeforeAction, filtersToSkip))
					{
						// Record that they failed.
						hasError = true;
					}
				}

				// If we haven't failed anywhere yet...
				if (!hasError)
				{
					CreateResources(method);

					// Execute the method / dynamic action
					if (method != null)
					{
						InvokeMethod(method, actionArgs);
					}
					else
					{
						dynAction.Execute(this);
					}

					if (!skipFilters)
					{
						// ...run the AfterAction filters. If they fail...
						if (!ProcessFilters(ExecuteEnum.AfterAction, filtersToSkip))
						{
							// Record that they failed.
							hasError = true;
						}
					}
				}
			}
			catch (ThreadAbortException)
			{
				if (logger.IsErrorEnabled)
				{
					logger.Error("ThreadAbortException, process aborted");
				}

				hasError = true;
			}
			catch (Exception ex)
			{
				hasError = true;
				
				if (logger.IsErrorEnabled)
				{
					logger.Error("Exception during action process", ex);
				}

				// Try and perform the rescue
				if (!PerformRescue(method, ex))
				{
					exceptionToThrow = ex;
				}

				RaiseOnActionExceptionOnExtension();
			}

			try
			{
				// Nothing to do if the peer disconnected
				if (checkWhetherClientHasDisconnected && !IsClientConnected) return;

				// If we haven't failed anywhere and no redirect was issued
				if (!hasError && !Response.WasRedirected)
				{
					// Render the actual view then cleanup
					ProcessView();
				}

				if (exceptionToThrow != null)
				{
					throw new RailsException("Unhandled Exception while rendering view", exceptionToThrow);
				}
			}
			finally
			{
				// Run the filters if required
				if (!skipFilters)
				{
					ProcessFilters(ExecuteEnum.AfterRendering, filtersToSkip);
				}

				DisposeFilter();

				ReleaseResources();
			}
		}

		/// <summary>
		/// The following lines were added to handle _default processing
		/// if present look for and load _default action method
		/// <seealso cref="DefaultActionAttribute"/>
		/// </summary>
		private MethodInfo FindOutDefaultMethod(object[] methodArgs)
		{
			if (metaDescriptor.DefaultAction != null)
			{
				return SelectMethod(metaDescriptor.DefaultAction.DefaultAction, MetaDescriptor.Actions, context.Request, methodArgs);
			}

			return null;
		}

		protected virtual void CreateAndInitializeHelpers()
		{
			helpers = new HybridDictionary();
			
			// Custom helpers

			foreach(HelperDescriptor helper in metaDescriptor.Helpers)
			{
				object helperInstance = Activator.CreateInstance(helper.HelperType);

				IControllerAware aware = helperInstance as IControllerAware;

				if (aware != null)
				{
					aware.SetController(this);
				}

				PerformAdditionalHelperInitialization(helperInstance);

				if (helpers.Contains(helper.Name))
				{
					throw new ControllerException(String.Format("Found a duplicate helper " + 
						"attribute named '{0}' on controller '{1}'", helper.Name, Name));
				}

				helpers.Add(helper.Name, helperInstance);
			}

			CreateStandardHelpers();
		}

		private void CreateStandardHelpers()
		{
			AbstractHelper[] builtInHelpers =
				new AbstractHelper[]
					{
						new AjaxHelper(),
						new EffectsFatHelper(), new Effects2Helper(),
						new DateFormatHelper(), new HtmlHelper(),
						new ValidationHelper(), new DictHelper(),
						new PaginationHelper(), new FormHelper()
					};

			foreach(AbstractHelper helper in builtInHelpers)
			{
				helper.SetController(this);

				String helperName = helper.GetType().Name;

				if (!helpers.Contains(helperName))
				{
					helpers[helperName] = helper;
				}
				
				PerformAdditionalHelperInitialization(helper);
			}
		}

		private void PerformAdditionalHelperInitialization(object helperInstance)
		{
			IServiceEnabledComponent serviceEnabled = helperInstance as IServiceEnabledComponent;

			if (serviceEnabled != null)
			{
				serviceEnabled.Service(serviceProvider);
			}
		}


		/// <summary>
		/// Invokes the scaffold support if the controller
		/// is associated with a scaffold
		/// </summary>
		private void ProcessScaffoldIfPresent()
		{
			if (metaDescriptor.Scaffoldings.Count != 0)
			{
				if (scaffoldSupport == null)
				{
					String message = "You must enable scaffolding support on the " +
						"configuration file, or, to use the standard ActiveRecord support " +
						"copy the necessary assemblies to the bin folder.";

					throw new RailsException(message);
				}

				scaffoldSupport.Process(this);
			}
		}
		
		#endregion

		#region Action Invocation

		/// <summary>
		/// Pendent
		/// </summary>
		/// <param name="action"></param>
		/// <param name="actions"></param>
		/// <param name="request"></param>
		/// <param name="actionArgs"></param>
		/// <returns></returns>
		protected virtual MethodInfo SelectMethod(String action, IDictionary actions, 
		                                          IRequest request, params object[] actionArgs)
		{
			return actions[action] as MethodInfo;
		}

		/// <summary>
		/// Pendent
		/// </summary>
		/// <param name="method"></param>
		/// <param name="methodArgs"></param>
		private void InvokeMethod(MethodInfo method, object[] methodArgs)
		{
			InvokeMethod(method, context.Request, methodArgs);
		}

		/// <summary>
		/// Pendent
		/// </summary>
		/// <param name="method"></param>
		/// <param name="request"></param>
		/// <param name="methodArgs"></param>
		protected virtual void InvokeMethod(MethodInfo method, IRequest request, object[] methodArgs)
		{
 			method.Invoke(this, new object[0]);
		}
	    
		#endregion

		#region Resources

		protected virtual void CreateResources(MethodInfo method)
		{
			resources = new ResourceDictionary();

			Assembly typeAssembly = GetType().Assembly;

			foreach(ResourceDescriptor resource in metaDescriptor.Resources)
			{
				resources.Add(resource.Name, resourceFactory.Create(resource, typeAssembly));
			}

			if (method == null) return;

			ActionMetaDescriptor actionMeta = metaDescriptor.GetAction(method);

			foreach(ResourceDescriptor resource in actionMeta.Resources)
			{
				resources[resource.Name] = resourceFactory.Create(resource, typeAssembly);
			}
		}

		protected virtual void ReleaseResources()
		{
			if (resources == null) return;

			foreach(IResource resource in resources.Values)
			{
				resourceFactory.Release(resource);
			}
		}

		#endregion

		#region Filters

		protected internal bool ShouldSkip(MethodInfo method, IDictionary filtersToSkip)
		{
			if (method == null)
			{
				// Dynamic Action, run the filters if we have any
				return (filters == null);
			}

			if (filters == null)
			{
				// No filters, so skip 
				return true;
			}

			ActionMetaDescriptor actionMeta = metaDescriptor.GetAction(method);

			if (actionMeta.SkipFilters.Count == 0)
			{
				// Nothing against filters declared for this action
				return false;
			}

			foreach(SkipFilterAttribute skipfilter in actionMeta.SkipFilters)
			{
				// SkipAllFilters handling...
				if (skipfilter.BlanketSkip) return true;

				filtersToSkip[skipfilter.FilterType] = String.Empty;
			}

			return false;
		}

		/// <summary>
		/// Clones all Filter descriptors, in order to get a writable copy.
		/// </summary>
		protected internal FilterDescriptor[] CopyFilterDescriptors()
		{
			FilterDescriptor[] clone = (FilterDescriptor[]) metaDescriptor.Filters.Clone();

			for (int i=0; i < clone.Length; i++)
			{
				clone[i] = (FilterDescriptor) clone[i].Clone();
			}

			return clone;
		}

		private bool ProcessFilters(ExecuteEnum when, IDictionary filtersToSkip)
		{
			foreach(FilterDescriptor desc in filters)
			{
				if (filtersToSkip.Contains(desc.FilterType)) continue;

				if ((desc.When & when) != 0)
				{
					if (!ProcessFilter(when, desc))
					{
						return false;
					}
				}
			}

			return true;
		}

		private bool ProcessFilter(ExecuteEnum when, FilterDescriptor desc)
		{
			if (desc.FilterInstance == null)
			{
				desc.FilterInstance = filterFactory.Create(desc.FilterType);

				IFilterAttributeAware filterAttAware = desc.FilterInstance as IFilterAttributeAware;

				if (filterAttAware != null)
				{
					filterAttAware.Filter = desc.Attribute;
				}
			}

			return desc.FilterInstance.Perform(when, context, this);
		}

		private void DisposeFilter()
		{
			if (filters == null) return;

			foreach(FilterDescriptor desc in filters)
			{
				if (desc.FilterInstance != null)
				{
					filterFactory.Release(desc.FilterInstance);
				}
			}
		}
		#endregion

		#region Views and Layout

		protected virtual String ObtainDefaultLayoutName()
		{
			if (metaDescriptor.Layout != null)
			{
				return metaDescriptor.Layout.LayoutName;
			}
			else
			{
				String defaultLayout = String.Format("layouts/{0}", Name);
				
				if ( HasTemplate(defaultLayout) )
				{
					return Name;
				}
			}
			return null;
		}

		private void ProcessView()
		{
			if (_selectedViewName != null)
			{
				viewEngine.Process(context, this, _selectedViewName);
			}
		}

		#endregion

		#region Rescue

		protected virtual bool PerformRescue(MethodInfo method, Exception ex)
		{		
			context.LastException = (ex is TargetInvocationException) ? ex.InnerException : ex;
			
			// Dynamic action 
			if (method == null) return false;

			Type exceptionType = context.LastException.GetType();

			ActionMetaDescriptor actionMeta = metaDescriptor.GetAction(method);
			
			if (actionMeta.SkipRescue != null) return false;
			
			RescueDescriptor att = GetRescueFor(actionMeta.Rescues, exceptionType);
			
			if (att == null)
			{
				att = GetRescueFor(metaDescriptor.Rescues, exceptionType);

				if (att == null) return false;
			}
				
			try
			{
				_selectedViewName = Path.Combine("rescues", att.ViewName);
				ProcessView();
				return true;
			}
			catch (Exception exception)
			{
				// In this situation, the rescue view could not be found
				// So we're back to the default error exibition
				
				if (logger.IsFatalErrorEnabled)
				{
					logger.FatalError("Failed to process rescue view. View name " + _selectedViewName, exception);
				}
			}

			return false;
		}

		protected virtual RescueDescriptor GetRescueFor(IList rescues, Type exceptionType)
		{
			if (rescues == null || rescues.Count == 0) return null;
			
			RescueDescriptor bestCandidate = null;
			
			foreach(RescueDescriptor rescue in rescues)
			{
				if (rescue.ExceptionType == exceptionType)
				{
					return rescue;
				}
				else if (rescue.ExceptionType != null && 
					rescue.ExceptionType.IsAssignableFrom(exceptionType))
				{
					bestCandidate = rescue;
				}
			}
			
			return bestCandidate;
		}

		#endregion

		#region Lifecycle (overridables)

		protected virtual void Initialize()
		{
			
		}

		/// <summary>
		/// Invoked by the view engine to perform
		/// any logic before the view is sent to the client.
		/// </summary>
		/// <param name="view"></param>
		public virtual void PreSendView(object view)
		{
			if (view is IControllerAware)
			{
				(view as IControllerAware).SetController(this);
			}

			if (context.UnderlyingContext != null)
			{
				context.UnderlyingContext.Items[Constants.ControllerContextKey] = this;
			}
		}

		/// <summary>
		/// Invoked by the view engine to perform
		/// any logic after the view had been sent to the client.
		/// </summary>
		/// <param name="view"></param>
		public virtual void PostSendView(object view)
		{
		}

		#endregion

		#region Email operations

		/// <summary>
		/// Creates an instance of <see cref="Message"/>
		/// using the specified template for the body
		/// </summary>
		/// <param name="templateName">
		/// Name of the template to load. 
		/// Will look in Views/mail for that template file.
		/// </param>
		/// <returns>An instance of <see cref="Message"/></returns>
		public Message RenderMailMessage(String templateName)
		{
			IEmailTemplateService templateService = (IEmailTemplateService) 
				ServiceProvider.GetService(typeof(IEmailTemplateService));

			return templateService.RenderMailMessage(templateName, Context, this);
		}

		/// <summary>
		/// Attempts to deliver the Message using the server specified on the web.config.
		/// </summary>
		/// <param name="message">The instance of System.Web.Mail.MailMessage that will be sent</param>
		public void DeliverEmail(Message message)
		{
			try
			{
				IEmailSender sender = (IEmailSender) ServiceProvider.GetService( typeof(IEmailSender) );

				sender.Send(message);
			}
			catch(Exception ex)
			{
				if (logger.IsErrorEnabled)
				{
					logger.Error("Error sending e-mail", ex);
				}
				
				throw new RailsException("Error sending e-mail", ex);
			}
		}

		/// <summary>
		/// Renders and delivers the e-mail message.
		/// <seealso cref="RenderMailMessage"/>
		/// <seealso cref="DeliverEmail"/>
		/// </summary>
		/// <param name="templateName"></param>
		public void RenderEmailAndSend(String templateName)
		{
			Message message = RenderMailMessage(templateName);
			DeliverEmail(message);
		}

		#endregion

		#region Extension

		protected void RaiseOnActionExceptionOnExtension()
		{
			ExtensionManager manager = (ExtensionManager) 
				ServiceProvider.GetService( typeof(ExtensionManager) );

			manager.RaiseActionError(Context);
		}

		#endregion
	}
}