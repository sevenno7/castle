#region Apache Notice
/*****************************************************************************
 * 
 * Castle.Igloo
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 ********************************************************************************/
#endregion

using System.Collections.Generic;
using System.Reflection;
using Castle.Core;
using Castle.Igloo.Util;
using Castle.MicroKernel;
using Castle.MicroKernel.ModelBuilder;
using Castle.Igloo.Attributes;
using Castle.Igloo.Interceptors;

namespace Castle.Igloo.Controllers
{
    /// <summary>
    /// Ensures LifestyleManager on a <see cref="IController"/> component +
    /// attachs interceptors
    /// </summary>
    public class ControllerInspector : IContributeComponentModelConstruction
    {
        /// <summary>
        /// No navigation members token
        /// </summary>
        public const string NO_NAVIGATION = "_NO_NAVIGATION_MEMBERS_";
        
        /// <summary>
        /// Binding token
        /// </summary>
        private BindingFlags BINDING_FLAGS_SET
            = BindingFlags.Public
            | BindingFlags.SetProperty
            | BindingFlags.Instance
            | BindingFlags.SetField
            ;
        
        /// <summary>
        /// Usually the implementation will look in the configuration property
        /// of the model or the service interface, or the implementation looking for
        /// something.
        /// </summary>
        /// <param name="kernel">The kernel instance</param>
        /// <param name="model">The component model</param>
        public void ProcessModel(IKernel kernel, ComponentModel model)
        {
            if (!typeof(IController).IsAssignableFrom(model.Implementation))
            {
                return;
            }

            RetrieveNoNavigationMethod(model);
            
            // Add the navigation interceptor
            model.Interceptors.AddLast(new InterceptorReference(typeof(NavigationInterceptor)));
        }

        private void RetrieveNoNavigationMethod(ComponentModel model)
        {
            IDictionary<string, NoNavigationAttribute> noNavigationMembers = new Dictionary<string, NoNavigationAttribute>();
            bool markedAllWithNoNavigation = false;
            NoNavigationAttribute noNavigationAttribute;
            
            // First, checks on Class
            noNavigationAttribute = AttributeUtil.GetNoNavigationAttribute(model.Implementation.GetType());
            if (noNavigationAttribute != null)
            {
                markedAllWithNoNavigation = true;
            }

            // Cheks on Method
            if (!markedAllWithNoNavigation)
            {
                MethodInfo[] methods = model.Implementation.GetMethods(BINDING_FLAGS_SET);
                for (int i = 0; i < methods.Length; i++)
                {
                    noNavigationAttribute = AttributeUtil.GetNoNavigationAttribute(methods[i]);
                    if (noNavigationAttribute != null)
                    {
                        noNavigationMembers.Add(methods[i].Name, noNavigationAttribute);
                    }
                }
            }
            else
            {
                // Marks all method with non navigation
                MethodInfo[] methods = model.Implementation.GetMethods(BINDING_FLAGS_SET);
                for (int i = 0; i < methods.Length; i++)
                {
                    NoNavigationAttribute attribute = new NoNavigationAttribute();
                    noNavigationMembers.Add(methods[i].Name, attribute);
                }
            }

            model.ExtendedProperties[NO_NAVIGATION] = noNavigationMembers;
        }
    }
}
