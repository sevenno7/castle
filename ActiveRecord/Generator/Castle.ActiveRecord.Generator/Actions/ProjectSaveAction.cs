using System.Runtime.Serialization.Formatters.Binary;
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

namespace Castle.ActiveRecord.Generator.Actions
{
	using System;
	using System.IO;
	using System.Windows.Forms;
	using System.Runtime.Serialization;


	public class ProjectSaveAction : AbstractAction
	{
		private MenuItem _item;

		public ProjectSaveAction()
		{
		}

		public override void Install(IWorkspace workspace, object parentMenu, object parentGroup)
		{
			base.Install(workspace, parentMenu, parentGroup);

			_item = new MenuItem("&Save");
			_item.Click += new EventHandler(OnSave);

			(parentMenu as MenuItem).MenuItems.Add(_item);
		}

		private void OnSave(object sender, EventArgs e)
		{
			try
			{
				using(FileStream fs = new FileStream(@"C:\project1.ar", FileMode.Create, FileAccess.Write, FileShare.Write))
				{
					BinaryFormatter formatter = new BinaryFormatter();

					formatter.Serialize( fs, Model.CurrentProject );
				}
			}
			catch(Exception ex)
			{
				MessageBox.Show(Workspace.ActiveWindow, ex.Message, "Error saving project");
			}
		}
	}
}
