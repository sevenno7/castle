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

namespace Castle.MonoRail.Framework.Helpers
{
	using System;
	using System.Collections;
	using System.Collections.Specialized;
	using System.IO;
	using System.Reflection;
	using System.Text;
	using System.Web.UI;

	using Castle.Components.Binder;

	public enum RequestContext
	{
		All,
		PropertyBag,
		Flash,
		Session,
		Request, 
		Params
	}

	/// <summary>
	/// Currently being evaluated
	/// </summary>
	/// <remarks>TODO: Make sure it generates XHTML compliant content</remarks>
	public class FormHelper : AbstractHelper
	{
		protected static readonly BindingFlags PropertyFlags = BindingFlags.GetProperty|BindingFlags.Public|BindingFlags.Instance|BindingFlags.IgnoreCase;
		protected static readonly BindingFlags FieldFlags = BindingFlags.GetField|BindingFlags.Public|BindingFlags.Instance|BindingFlags.IgnoreCase;

		#region TextFieldValue

		public String TextFieldValue(String target, object value)
		{
			return TextFieldValue(target, value, null);
		}

		public String TextFieldValue(String target, object value, IDictionary attributes)
		{
			return CreateInputElement("text", target, value, attributes);
		}

		#endregion

		#region TextField

		public String TextField(String target)
		{
			return TextField(target, null);
		}

		public String TextField(String target, IDictionary attributes)
		{
			object value = ObtainValue(target);

			return CreateInputElement("text", target, value, attributes);
		}

		#endregion

		#region TextArea

		public String TextArea(String target)
		{
			return TextArea(target, null);
		}

		public String TextArea(String target, IDictionary attributes)
		{
			object value = ObtainValue(target);

			value = value == null ? "" : HtmlEncode(value.ToString());

			String id = CreateHtmlId(target);

			return String.Format("<textarea id=\"{0}\" name=\"{1}\" {2}>{3}</textarea>", 
				id, target, GetAttributes(attributes), value);
		}

		#endregion

		#region PasswordField

		public String PasswordField(String target)
		{
			return PasswordField(target, null);
		}

		public String PasswordField(String target, IDictionary attributes)
		{
			object value = ObtainValue(target);

			return CreateInputElement("password", target, value, attributes);
		}

		#endregion

		#region TextFieldFormat

		public String TextFieldFormat(String target, String formatString)
		{
			return TextFieldFormat(target, formatString, null);
		}

		public String TextFieldFormat(String target, String formatString, IDictionary attributes)
		{
			object value = ObtainValue(target);

			if (value != null)
			{
				IFormattable formattable = value as IFormattable;

				if (formattable != null)
				{
					value = formattable.ToString(formatString, null);
				}
			}

			return CreateInputElement("text", target, value, attributes);
		}

		#endregion

		#region LabelFor

		public String LabelFor(String target, String label)
		{
			return LabelFor(target, label, null);
		}
		
		public String LabelFor(String target, String label, IDictionary attributes)
		{
			String id = CreateHtmlId(attributes, target);

			StringBuilder sb = new StringBuilder();
			StringWriter sbWriter = new StringWriter(sb);
			HtmlTextWriter writer = new HtmlTextWriter(sbWriter);

			writer.WriteBeginTag("label");
			writer.WriteAttribute("for", id);
			writer.Write(GetAttributes(attributes)); 
			writer.Write(HtmlTextWriter.TagRightChar);
			writer.Write(label);
			writer.WriteEndTag("label");

			return sbWriter.ToString();
		}

		#endregion

		#region HiddenField

		public String HiddenField(String target)
		{
			object value = ObtainValue(target);

			return CreateInputElement("hidden", target, value, null);
		}
		
		public String HiddenField(String target, IDictionary attributes)
		{
			object value = ObtainValue(target);
			
			String id = CreateHtmlId(attributes, target);
			
			value = value != null ? value : String.Empty;

			return CreateInputElement("hidden", id, target, value.ToString(), attributes);
		}

		#endregion

		#region CheckboxList
		
		public CheckboxList CreateCheckboxList(String target, IEnumerable dataSource)
		{
			return CreateCheckboxList(target, dataSource, null);
		}

		public CheckboxList CreateCheckboxList(String target, IEnumerable dataSource, IDictionary attributes)
		{
			object value = ObtainValue(target);
			
			return new CheckboxList(this, target, value, dataSource, attributes);
		}
		
		internal String CheckboxItem(int index, String target, String suffix, SetItem item, IDictionary attributes)
		{
			if (item.IsSelected)
			{
				AddChecked(attributes);
			}
			else
			{
				RemoveChecked(attributes);
			}

			target = String.Format("{0}[{1}]", target, index);
			
			String elementId = CreateHtmlId(attributes, target, false);
			
			String computedTarget = target;
			
			if (suffix != null && suffix != String.Empty)
			{
				computedTarget += "." + suffix;
			}

			return CreateInputElement("checkbox", elementId, computedTarget, item.Value, attributes);
		}

		public sealed class CheckboxList : IEnumerable, IEnumerator
		{
			private readonly FormHelper helper;
			private readonly String target;
			private readonly IDictionary attributes;
			private readonly OperationState operationState;
			private readonly IEnumerator enumerator;
			private bool hasMovedNext, hasItem;
			private int index = -1;

			public CheckboxList(FormHelper helper, String target,
								object initialSelectionSet, IEnumerable dataSource, IDictionary attributes)
			{
				if (dataSource == null) throw new ArgumentNullException("dataSource");

				this.helper = helper;
				this.target = target;
				this.attributes = attributes == null ? new HybridDictionary(true) : attributes;
				
				operationState = SetOperation.IterateOnDataSource(initialSelectionSet, dataSource, attributes);
				enumerator = operationState.GetEnumerator();
			}
			
			public String Item()
			{
				if (!hasMovedNext)
				{
					throw new InvalidOperationException("Before rendering a checkbox item, you must use MoveNext");
				}
				
				if (!hasItem)
				{
					// Nothing to render
					return String.Empty;
				}
								
				return helper.CheckboxItem(index, target, operationState.TargetSuffix, CurrentSetItem, attributes);
			}

			public IEnumerator GetEnumerator()
			{
				return this;
			}

			public bool MoveNext()
			{
				hasMovedNext = true;
				hasItem = enumerator.MoveNext();
				
				if (hasItem) index++;
				
				return hasItem;
			}

			public void Reset()
			{
				index = -1;
				enumerator.Reset();
			}

			public object Current
			{
				get { return CurrentSetItem.Item; }
			}
			
			public SetItem CurrentSetItem
			{
				get { return enumerator.Current as SetItem; }
			}
		}

		#endregion
		
		#region CheckboxField

		public String CheckboxField(String target)
		{
			return CheckboxField(target, null);
		}

		/// <summary>
		/// Document the entries trueValue and falseValue
		/// </summary>
		/// <param name="target"></param>
		/// <param name="attributes"></param>
		/// <returns></returns>
		public String CheckboxField(String target, IDictionary attributes)
		{
			object value = ObtainValue(target);
			
			String trueValue = ObtainEntryAndRemove(attributes, "trueValue", "true");
			
			bool isChecked;

			if (trueValue != "true")
			{
				isChecked = AreEqual(value, trueValue);
			}
			else
			{
				isChecked = ((value != null && value is bool && ((bool)value)) || 
					(!(value is bool) && (value != null)));
			}

			if (isChecked)
			{
				if (attributes == null)
				{
					attributes = new HybridDictionary(true);
				}

				AddChecked(attributes);
			}

			String id = CreateHtmlId(attributes, target);
			String hiddenElementId = id + "H";
			String hiddenElementValue = ObtainEntryAndRemove(attributes, "falseValue", "false");

			String result = CreateInputElement("checkbox", id, target, trueValue, attributes);
			
			result += CreateInputElement("hidden", hiddenElementId, target, hiddenElementValue, null);
			
			return result;
		}

		#endregion

		#region RadioField

		public String RadioField(String target, object valueToSend)
		{
			return RadioField(target, valueToSend, null);
		}

		public String RadioField(String target, object valueToSend, IDictionary attributes)
		{
			object value = ObtainValue(target);

			bool isChecked = AreEqual(valueToSend, value);

			if (isChecked)
			{
				if (attributes == null)
				{
					attributes = new HybridDictionary(true);
				}

				AddChecked(attributes);
			}

			return CreateInputElement("radio", target, valueToSend, attributes);
		}

		#endregion

		#region Select

		public String Select(String target, IEnumerable dataSource)
		{
			return Select(target, dataSource, null);
		}

		public String Select(String target, IEnumerable dataSource, IDictionary attributes)
		{
			object selectedValue = ObtainValue(target);

			return Select(target, selectedValue, dataSource, attributes);
		}
		
		/// <summary>
		/// Creates a <c>select</c> elements and its <c>option</c>s. If the <c>dataSource</c>
		/// elements are complex objects, use the params <c>value</c> and <c>text</c> to make
		/// the helper use the specified properties to extract the <c>option</c> value and text.
		/// <para>
		/// You can also specify the attribute <c>firstoption</c> to force the first option be
		/// something like 'please select'
		/// </para>
		/// </summary>
		/// <param name="target"></param>
		/// <param name="selectedValue"></param>
		/// <param name="dataSource"></param>
		/// <param name="attributes"></param>
		/// <returns></returns>
		public String Select(String target, object selectedValue, IEnumerable dataSource, IDictionary attributes)
		{
			String id = CreateHtmlId(target);

			StringBuilder sb = new StringBuilder();
			StringWriter sbWriter = new StringWriter(sb);
			HtmlTextWriter writer = new HtmlTextWriter(sbWriter);

			String firstOption = null; 
			String name = target;

			if (attributes != null)
			{
				firstOption = ObtainEntryAndRemove(attributes, "firstoption");
				
				if (attributes.Contains("name"))
				{
					name = (String) attributes["name"];
					attributes.Remove("name");
				}

				if (attributes.Contains("id"))
				{
					id = (String) attributes["id"];
					attributes.Remove("id");
				}
			}

			OperationState state = SetOperation.IterateOnDataSource(selectedValue, dataSource, attributes);

			writer.WriteBeginTag("select");
			writer.WriteAttribute("id", id);
			writer.WriteAttribute("name", name);
			writer.Write(" ");
			writer.Write(GetAttributes(attributes));
			writer.Write(HtmlTextWriter.TagRightChar);
			writer.WriteLine();

			if (firstOption != null)
			{
				writer.WriteBeginTag("option");
				writer.WriteAttribute("value", "0");
				writer.Write(HtmlTextWriter.TagRightChar);
				writer.Write(firstOption);
				writer.WriteEndTag("option");
				writer.WriteLine();
			}
			
			foreach(SetItem item in state)
			{
				writer.WriteBeginTag("option");
				
				if (item.IsSelected)
				{
					writer.Write(" selected=\"selected\"");
				}

				writer.WriteAttribute("value", item.Value);
				writer.Write(HtmlTextWriter.TagRightChar);
				writer.Write(item.Text);
				writer.WriteEndTag("option");
				writer.WriteLine();
			}
			
			writer.WriteEndTag("select");

			return sbWriter.ToString();
		}

		#endregion

		#region protected members

		protected String CreateInputElement(String type, String target, Object value, IDictionary attributes)
		{
			if (value == null)
			{
				value = ObtainEntryAndRemove(attributes, "defaultValue");
			}

			value = value == null ? "" : value;

			string id = CreateHtmlId(attributes, target);

			return CreateInputElement(type, id, target, value.ToString(), attributes);
		}

		protected string CreateInputElement(String type, String id, String target, String value, IDictionary attributes)
		{
			if (Controller.Context != null) // We have a context
			{
				value = HtmlEncode(value);
			}
			
			return String.Format("<input type=\"{0}\" id=\"{1}\" name=\"{2}\" value=\"{3}\" {4}/>", 
			                     type, id, target, value, GetAttributes(attributes));
		}

		protected object ObtainValue(String target)
		{
			return ObtainValue(RequestContext.All, target);
		}

		protected object ObtainValue(RequestContext context, String target)
		{
			String[] pieces = target.Split(new char[] {'.'});

			String root = pieces[0];

			int index;

			bool isIndexed = CheckForExistenceAndExtractIndex(ref root, out index);

			Object rootInstance = ObtainRootInstance(context, root);

			if (rootInstance == null)
			{
				return null;
			}

			if (isIndexed)
			{
				AssertIsValidArray(rootInstance, root, index);
			}
	
			if (!isIndexed && pieces.Length == 1)
			{
				return rootInstance;
			}
			else if (isIndexed)
			{
				rootInstance = GetArrayElement(rootInstance, index);
			}

			return QueryPropertyRecursive(rootInstance, pieces, 1);
		}

		protected object QueryPropertyRecursive(object rootInstance, string[] propertyPath, int piece)
		{
			String property = propertyPath[piece]; int index;

			Type instanceType = rootInstance.GetType();

			bool isIndexed = CheckForExistenceAndExtractIndex(ref property, out index);

			PropertyInfo propertyInfo = instanceType.GetProperty(property, PropertyFlags);

			object instance;

			if (propertyInfo == null)
			{
				FieldInfo fieldInfo = instanceType.GetField(property, FieldFlags);

				if (fieldInfo == null)
				{
					throw new BindingException("No public property or field '{0}' found on type '{1}'", 
						property, instanceType.FullName);
				}

				instance = fieldInfo.GetValue(rootInstance);
			}
			else
			{
				if (!propertyInfo.CanRead)
				{
					throw new BindingException("Property '{0}' for type '{1}' can not be read", 
						propertyInfo.Name, instanceType.FullName);
				}
				
				if (propertyInfo.GetIndexParameters().Length != 0)
				{
					throw new BindingException("Property '{0}' for type '{1}' has indexes, which is not supported", 
						propertyInfo.Name, instanceType.FullName);
				}

				instance = propertyInfo.GetValue(rootInstance, null);
			}

			if (isIndexed && instance != null)
			{
				AssertIsValidArray(instance, property, index);

				instance = GetArrayElement(instance, index);
			}

			if (instance == null || piece + 1 == propertyPath.Length)
			{
				return instance;
			}

			return QueryPropertyRecursive(instance, propertyPath, piece + 1);
		}

		protected object ObtainRootInstance(RequestContext context, String target)
		{
			object rootInstance = null;

			if (context == RequestContext.All || context == RequestContext.PropertyBag)
			{
				rootInstance = Controller.PropertyBag[target];
			}
			if (rootInstance == null && (context == RequestContext.All || context == RequestContext.Flash))
			{
				rootInstance = Controller.Context.Flash[target];
			}
			if (rootInstance == null && (context == RequestContext.All || context == RequestContext.Session))
			{
				rootInstance = Controller.Context.Session[target];
			}
			if (rootInstance == null && (context == RequestContext.All || context == RequestContext.Params))
			{
				rootInstance = Controller.Params[target];
			}
			if (rootInstance == null && (context == RequestContext.All || context == RequestContext.Request))
			{
				rootInstance = Controller.Context.UnderlyingContext.Items[target];
			}

			return rootInstance;
		}

		protected static string CreateHtmlId(IDictionary attributes, String target)
		{
			return CreateHtmlId(attributes, target, true);
		}
		
		protected static string CreateHtmlId(IDictionary attributes, String target, bool removeEntry)
		{
			String id;
			
			if (removeEntry)
			{
				id = ObtainEntryAndRemove(attributes, "id");
			}
			else
			{
				id = ObtainEntry(attributes, "id");
			}

			if (id == null)
			{
				id = CreateHtmlId(target);
			}
			
			return id;
		}

		protected internal static String ObtainEntry(IDictionary attributes, String key)
		{
			if (attributes != null && attributes.Contains(key))
			{
				return (String) attributes[key];
			}
			
			return null;
		}

		protected internal static String ObtainEntryAndRemove(IDictionary attributes, String key, String defaultValue)
		{
			String value = ObtainEntryAndRemove(attributes, key);
			
			return value != null ? value : defaultValue;
		}
		
		protected internal static String ObtainEntryAndRemove(IDictionary attributes, String key)
		{
			String value = null;
			
			if (attributes != null && attributes.Contains(key))
			{
				value = (String) attributes[key];

				attributes.Remove(key);
			}
			
			return value;
		}

		#endregion

		#region private helpers

		private void AssertIsValidArray(object instance, string property, int index)
		{
			Type instanceType = instance.GetType();

			IList list = instance as IList;

			bool validList = false;

#if DOTNET2
			if (list == null && instanceType.IsGenericType)
			{
				Type[] genArgs = instanceType.GetGenericArguments();

				Type genList = typeof(System.Collections.Generic.IList<>).MakeGenericType(genArgs);
				Type genTypeDef = instanceType.GetGenericTypeDefinition().MakeGenericType(genArgs);

				validList = genList.IsAssignableFrom(genTypeDef);
			}
#endif
			
			if (!validList && list == null)
			{
				throw new RailsException("The property {0} is being accessed as " + 
					"an indexed property but does not seem to implement IList. " + 
					"In fact the type is {1}", property, instanceType.FullName);
			}

			if (index < 0)
			{
				throw new RailsException("The specified index '{0}' is outside the bounds " + 
					"of the array. Property {1}", index, property);
			}
		}

		private object GetArrayElement(object instance, int index)
		{
			IList list = instance as IList;

#if DOTNET2
			if (list == null && instance != null && instance.GetType().IsGenericType)
			{
				Type instanceType = instance.GetType();

				Type[] genArguments = instanceType.GetGenericArguments();

				Type genType = instanceType.GetGenericTypeDefinition().MakeGenericType(genArguments);
				
				// I'm not going to retest for IList implementation as 
				// if we got here, the AssertIsValidArray has run successfully

				PropertyInfo countPropInfo = genType.GetProperty("Count");

				int count = (int) countPropInfo.GetValue(instance, null);
				
				if (count == 0 || index + 1 > count)
				{
					return null;
				}

				PropertyInfo indexerPropInfo = genType.GetProperty("Item");

				return indexerPropInfo.GetValue(instance, new object[] { index });
			}
#endif
			
			if (list == null || list.Count == 0 || index + 1 > list.Count)
			{
				return null;
			}

			return list[index];
		}

		private static bool CheckForExistenceAndExtractIndex(ref String property, out int index)
		{
			bool isIndexed = property.IndexOf('[') != -1;

			index = -1;

			if (isIndexed)
			{
				int start = property.IndexOf('[') + 1;
				int len = property.IndexOf(']', start) - start;

				String indexStr = property.Substring(start, len);

				try
				{
					index = Convert.ToInt32(indexStr);
				}
				catch(Exception)
				{
					throw new RailsException("Could not convert (param {0}) index to Int32. Value is {1}", 
						property, indexStr);
				}

				property = property.Substring(0, start - 1);
			}

			return isIndexed;
		}

		private static bool AreEqual(object left, object right)
		{
			if (left == null || right == null) return false;

			if (left is String || right is String)
			{
				return String.Compare(left.ToString(), right.ToString()) == 0;
			}

			if (left.GetType() == right.GetType())
			{
				return right.Equals(left);
			}

			IConvertible convertible = left as IConvertible;

			if (convertible != null)
			{
				try
				{
					object newleft = convertible.ToType(right.GetType(), null);
					return (newleft.Equals(right));
				}
				catch(Exception)
				{
					// Do nothing
				}
			}

			return left.ToString().Equals(right.ToString());
		}

		/// <summary>
		/// Determines whether the present value matches the value on 
		/// the initialSetValue (which can be a single value or a set)
		/// </summary>
		/// <param name="value">Value from the datasource</param>
		/// <param name="initialSetValue">Value from the initial selection set</param>
		/// <param name="propertyOnInitialSet">Optional. Property to obtain the value from</param>
		/// <param name="isMultiple"><c>true</c> if the initial selection is a set</param>
		/// <returns><c>true</c> if it's selected</returns>
		protected internal static bool IsPresent(object value, object initialSetValue, 
		                                         PropertyInfo propertyOnInitialSet, bool isMultiple)
		{
			if (!isMultiple)
			{
				object valueToCompare = initialSetValue;
				
				if (propertyOnInitialSet != null)
				{
					valueToCompare = propertyOnInitialSet.GetValue(initialSetValue, null);
				}
				
				return AreEqual(value, valueToCompare);
			}
			else
			{
				foreach(object item in (IEnumerable) initialSetValue)
				{
					object valueToCompare = item;

					if (propertyOnInitialSet != null)
					{
						valueToCompare = propertyOnInitialSet.GetValue(item, null);
					}

					if (AreEqual(value, valueToCompare))
					{
						return true;
					}
				}
			}
			
			return false;
		}
		
		/// <summary>
		/// Gets the property get method.
		/// </summary>
		/// <param name="elem">Object specifying the type for which to get the method.</param>
		/// <param name="property">Property name.</param>
		/// <returns><see cref="MethodInfo"/> to be used to retrieve the property value.
		/// If <paramref name="property"/> is <c>null</c> <c>null</c> is returned.</returns>
		/// <remarks>This method is used to get the <see cref="MethodInfo"/> to retrieve
		/// specified property from the specified type.</remarks>
		/// <exception cref="ArgumentNullException">Thrown is <paramref name="elem"/> is <c>null</c>.</exception>
		protected internal static PropertyInfo GetMethod(object elem, String property)
		{
			if (elem == null) throw new ArgumentNullException("elem");
			if (property == null) return null;

			return GetMethod(elem.GetType(), property);
		}

		protected internal static PropertyInfo GetMethod(Type type, String property)
		{
			return type.GetProperty(property, PropertyFlags);
		}

		private static void AddChecked(IDictionary attributes)
		{
			attributes["checked"] = "checked";
		}

		private static void RemoveChecked(IDictionary attributes)
		{
			attributes.Remove("checked");
		}

		private static String CreateHtmlId(String name)
		{
			StringBuilder sb = new StringBuilder(name.Length);

			bool canUseUnderline = false;

			foreach(char c in name.ToCharArray())
			{
				switch(c)
				{
					case '.':
					case '[':
					case ']':
						if (canUseUnderline)
						{
							sb.Append('_');
							canUseUnderline = false;
						}
						break;
					default:
						canUseUnderline = true;
						sb.Append(c);
						break;
				}
				
			}

			return sb.ToString();
		}

		#endregion

	}
}