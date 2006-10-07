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

namespace Castle.ActiveRecord.Framework.Internal
{
	using System;
	using System.Collections;

	[Serializable]
	public class ActiveRecordModel : IModelNode
	{
		protected internal static IDictionary type2Model = Hashtable.Synchronized(new Hashtable());

		protected internal static bool isDebug = false;

		private readonly Type type;

		private bool isJoinedSubClassBase;
		private bool isDiscriminatorBase;
		private bool isDiscriminatorSubClass;
		private bool isJoinedSubClass;
		private bool isNestedType;
		
		private ActiveRecordAttribute arAtt;
		private ActiveRecordModel parent;
		private PrimaryKeyModel primaryKey;
		private CompositeKeyModel compositeKey;
		private KeyModel key;
		private TimestampModel timestamp;
		private VersionModel version;
		
		private IList imports = new ArrayList();
		private IList hasManyToAny = new ArrayList();
		private IList anys = new ArrayList();
		private IList properties = new ArrayList();
		private IList fields = new ArrayList();
		private IList classes = new ArrayList();
		private IList joinedclasses = new ArrayList();
		private IList components = new ArrayList();
		private IList belongsTo = new ArrayList();
		private IList hasMany = new ArrayList();
		private IList hasAndBelongsToMany = new ArrayList();
		private IList oneToOne = new ArrayList();
		private IList collectionIds = new ArrayList();
		private IList hilos = new ArrayList();
		private IList notMappedProperties = new ArrayList();
		private IList validators = new ArrayList();

		/// <summary>
		/// Initializes a new instance of the <see cref="ActiveRecordModel"/> class.
		/// </summary>
		/// <param name="type">The type.</param>
		public ActiveRecordModel(Type type)
		{
			this.type = type;
		}

		/// <summary>
		/// Gets or sets the parent model
		/// </summary>
		/// <value>The parent.</value>
		public ActiveRecordModel Parent
		{
			get { return parent; }
			set { parent = value; }
		}

		public Type Type
		{
			get { return type; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether this instance is joined sub class base.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is joined sub class base; otherwise, <c>false</c>.
		/// </value>
		public bool IsJoinedSubClassBase
		{
			get { return isJoinedSubClassBase; }
			set { isJoinedSubClassBase = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether this instance is discriminator base.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is discriminator base; otherwise, <c>false</c>.
		/// </value>
		public bool IsDiscriminatorBase
		{
			get { return isDiscriminatorBase; }
			set { isDiscriminatorBase = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether this instance is discriminator sub class.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is discriminator sub class; otherwise, <c>false</c>.
		/// </value>
		public bool IsDiscriminatorSubClass
		{
			get { return isDiscriminatorSubClass; }
			set { isDiscriminatorSubClass = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether this instance is joined sub class.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is joined sub class; otherwise, <c>false</c>.
		/// </value>
		public bool IsJoinedSubClass
		{
			get { return isJoinedSubClass; }
			set { isJoinedSubClass = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether this instance is nested type.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is nested type; otherwise, <c>false</c>.
		/// </value>
		public bool IsNestedType
		{
			get { return isNestedType; }
			set { isNestedType = value; }
		}

		/// <summary>
		/// Gets or sets the active record attribute
		/// </summary>
		/// <value>The active record att.</value>
		public ActiveRecordAttribute ActiveRecordAtt
		{
			get { return arAtt; }
			set { arAtt = value; }
		}

		/// <summary>
		/// Used only by joined subclasses
		/// </summary>
		public KeyModel Key
		{
			get { return key; }
			set { key = value; }
		}

		/// <summary>
		/// Gets or sets the timestamp model
		/// </summary>
		/// <value>The timestamp.</value>
		public TimestampModel Timestamp
		{
			get { return timestamp; }
			set { timestamp = value; }
		}

		/// <summary>
		/// Gets or sets the version model
		/// </summary>
		/// <value>The version.</value>
		public VersionModel Version
		{
			get { return version; }
			set { version = value; }
		}

		/// <summary>
		/// Gets all the imports
		/// </summary>
		/// <value>The imports.</value>
		public IList Imports
		{
			get { return imports; }
		}

		/// <summary>
		/// Gets all the properties
		/// </summary>
		/// <value>The properties.</value>
		public IList Properties
		{
			get { return properties; }
		}

		/// <summary>
		/// Gets all the fields
		/// </summary>
		/// <value>The fields.</value>
		public IList Fields
		{
			get { return fields; }
		}

		public IList HasManyToAny
		{
			get { return hasManyToAny; }
		}

		public IList Anys
		{
			get { return anys; }
		}

		public IList Classes
		{
			get { return classes; }
		}

		public IList JoinedClasses
		{
			get { return joinedclasses; }
		}

		public IList Components
		{
			get { return components; }
		}

		public IList BelongsTo
		{
			get { return belongsTo; }
		}

		public IList HasMany
		{
			get { return hasMany; }
		}

		public IList HasAndBelongsToMany
		{
			get { return hasAndBelongsToMany; }
		}

		public IList OneToOnes
		{
			get { return oneToOne; }
		}

		public IList CollectionIDs
		{
			get { return collectionIds; }
		}

		/// <summary>
		/// For unique Primary keys
		/// </summary>
		public PrimaryKeyModel PrimaryKey
		{
			get { return primaryKey; }
			set { primaryKey = value; }
		}

		/// <summary>
		/// For Composite Primary keys
		/// </summary>
		public CompositeKeyModel CompositeKey
		{
			get { return compositeKey; }
			set { compositeKey = value; }
		}

		public IList Hilos
		{
			get { return hilos; }
		}

		public IList NotMappedProperties
		{
			get { return notMappedProperties; }
		}

		public IList Validators
		{
			get { return validators; }
		}

		/// <summary>
		/// Gets a value indicating whether to use auto import
		/// </summary>
		/// <value><c>true</c> if should use auto import; otherwise, <c>false</c>.</value>
		public bool UseAutoImport
		{
			get 
			{ 
				if (arAtt != null)
				{
					return arAtt.UseAutoImport;
				}
				
				return true;
			}
		}

		/// <summary>
		/// Internally used
		/// </summary>
		/// <param name="arType"></param>
		/// <param name="model"></param>
		internal static void Register(Type arType, Framework.Internal.ActiveRecordModel model)
		{
			type2Model[arType] = model;
		}

		/// <summary>
		/// Gets the <see cref="Framework.Internal.ActiveRecordModel"/> for a given ActiveRecord class.
		/// </summary>
		public static Framework.Internal.ActiveRecordModel GetModel(Type arType)
		{
			return (Framework.Internal.ActiveRecordModel) type2Model[arType];
		}
		
		/// <summary>
		/// Gets an array containing the <see cref="Framework.Internal.ActiveRecordModel"/> for every registered ActiveRecord class.
		/// </summary>
		public static Framework.Internal.ActiveRecordModel[] GetModels()
		{
			Framework.Internal.ActiveRecordModel[] modelArray = new Framework.Internal.ActiveRecordModel[type2Model.Values.Count];
			
			type2Model.Values.CopyTo(modelArray, 0);
			
			return modelArray;
		}

		#region IVisitable Members

		/// <summary>
		/// Accepts the specified visitor and call the relevant IVisitor.Visit***() method
		/// </summary>
		/// <param name="visitor">The visitor.</param>
		public void Accept(IVisitor visitor)
		{
			visitor.VisitModel(this);
		}

		#endregion
	}
}
