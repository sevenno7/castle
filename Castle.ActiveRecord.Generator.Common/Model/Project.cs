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

namespace Castle.ActiveRecord.Generator.Model
{
	using System;
	using System.Collections;
	using System.CodeDom.Compiler;
	using System.IO;

	using Castle.Model;

	using Castle.ActiveRecord.Generator.Database;
	using Castle.ActiveRecord.Generator.CodeGenerator;


	[Transient]
	public class Project
	{
		private String _name;
		private String _driver;
		private String _connectionString;
		private String _namespace;
		private CodeProviderInfo _codeProvider;		
		private DatabaseDefinition _dbDefinition;
		
		private IList _activeRecordDescriptors = new ArrayList();
		
		private IDatabaseDefinitionBuilder _definitionBuilder;
		private ICodeDomGenerator _codeGenerator;
		private ICodeProviderFactory _codeProviderFactory;


		public Project(IDatabaseDefinitionBuilder definitionBuilder, 
			ICodeProviderFactory codeProviderFactory, ICodeDomGenerator codeGenerator)
		{
			_definitionBuilder = definitionBuilder;
			_codeProviderFactory = codeProviderFactory;
			_codeGenerator = codeGenerator;
		}

		public String Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public String Driver
		{
			get { return _driver; }
			set { _driver = value; }
		}

		public String ConnectionString
		{
			get { return _connectionString; }
			set { _connectionString = value; }
		}

		public String CodeNamespace
		{
			get { return _namespace; }
			set { _namespace = value; }
		}

		public CodeProviderInfo CodeProvider
		{
			get { return _codeProvider; }
			set { _codeProvider = value; }
		}

		public bool IsValid()
		{
			return true;
		}

		public DatabaseDefinition DatabaseDefinition
		{
			get { return _dbDefinition; }
			set { _dbDefinition = value; }
		}

		public void RefreshDatabaseDefinition()
		{
			_dbDefinition = _definitionBuilder.Build( this );
		}

		public String GenerateCode(ActiveRecordDescriptor arDesc)
		{
			CodeDomProvider provider = _codeProviderFactory.GetProvider(_codeProvider);

			StringWriter writer = new StringWriter();

			ICodeGenerator generator = provider.CreateGenerator();
			generator.GenerateCodeFromType( _codeGenerator.Generate(arDesc), writer, null ); 

			return writer.GetStringBuilder().ToString();
		}
	}
}
