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

using Castle.Igloo.Configuration;
using Castle.Igloo.Navigation;
using Castle.Igloo.UI;

namespace Castle.Igloo.Mock
{
	/// <summary>
	/// Represent a mock View Manager to use in unit test.
	/// </summary>
	public class MockViewManager : IViewManager
    {

        #region IViewManager Members

        /// <summary>
        /// Gets the view id.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public string GetView(string path)
        {
            return ConfigUtil.Settings.GetView(path);
        }
	    
        /// <summary>
        /// Activates a specified view.
        /// </summary>
        /// <param name="navigator">The navigator.</param>
        public void ActivateView(INavigator navigator)
		{
			// Do nothing
		}


        /// <summary>
        /// Gets the next view.
        /// </summary>
        /// <param name="navigationState">The navigation context.</param>
        /// <returns>The view id</returns>
        public string GetNextView(NavigationState navigationState)
        {
            return ConfigUtil.Settings.GetNextView(navigationState.CurrentView, navigationState.Action);
        }

        #endregion
    }
}
