// Copyright 2004-2005 Castle Project - http://www.castleproject.org/
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
namespace Castle.MonoRail.Views.Brail.TestSite.Controllers

import System
import System.IO
import Castle.MonoRail.Framework
import Castle.MonoRail.Views.Brail
import System.Collections
import System.Reflection

class SubViewController(Controller):
	
	def useLotsOfSubViews():
		viewEngine as BooViewEngine = ServiceProvider.GetService(typeof(IViewEngine));
		compilations as Hashtable = typeof(BooViewEngine).GetField("compilations", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(viewEngine)
		if Context.Request.QueryString["replaceSubView"] == "reset":
			compilations["""subview\listItem.boo"""] = null
			return
		if Context.Request.QueryString["replaceSubView"] != "true":
			return
		
		# we replace the type, so that in the test, we can verify
		# that Brail is using this type from the cache and not creating a new one
		compilations["""subview\listItem.boo"""] = DummySubView

	def Index():
		pass
	
	def SubViewWithPath():
		pass
		
	def SubViewWithLayout():
		LayoutName = "master"
		RenderView("index")
	
	def SubViewWithParameters():
		RenderView("CallSubViewWithParameters")


class DummySubView(BrailBase):
	def constructor(viewEngine as BooViewEngine, output as TextWriter, context as IRailsEngineContext, controller as Controller):
		super(viewEngine, output, context, controller)

	def Run():
		OutputStream.Write("dummy")