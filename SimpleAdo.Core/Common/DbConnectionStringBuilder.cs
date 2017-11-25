////
//// System.Data.Common.DbConnectionStringBuilder.cs
////
//// Author:
////	Sureshkumar T (tsureshkumar@novell.com)
////	Gert Driesen (drieseng@users.sourceforge.net
////
//// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
////
//// Permission is hereby granted, free of charge, to any person obtaining
//// a copy of this software and associated documentation files (the
//// "Software"), to deal in the Software without restriction, including
//// without limitation the rights to use, copy, modify, merge, publish,
//// distribute, sublicense, and/or sell copies of the Software, and to
//// permit persons to whom the Software is furnished to do so, subject to
//// the following conditions:
//// 
//// The above copyright notice and this permission notice shall be
//// included in all copies or substantial portions of the Software.
//// 
//// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace SimpleAdo.Common
{
    /// <summary> A database connection string builder. </summary>
	public class DbConnectionStringBuilder : IDictionary
	{
        /// <summary> The dictionary. </summary>
		private readonly Dictionary<string, object> _dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        /// <summary> True to use ODBC rules. </summary>
        private readonly bool _useOdbcRules;

        /// <summary> Default constructor. </summary>
		public DbConnectionStringBuilder()
			: this(false)
		{
		}

        /// <summary> Constructor. </summary>
        /// <param name="useOdbcRules"> True to use ODBC rules. </param>
		public DbConnectionStringBuilder(bool useOdbcRules)
		{
			_useOdbcRules = useOdbcRules;
		}

        /// <summary>
        /// Gets or sets a value indicating whether the browsable connection string.
        /// </summary>
        /// <value> True if browsable connection string, false if not. </value>
		public bool BrowsableConnectionString
		{
			get => throw new NotImplementedException();
		    set => throw new NotImplementedException();
		}

        /// <summary> Gets or sets the connection string. </summary>
        /// <value> The connection string. </value>
		public string ConnectionString
		{
			get
			{
				var sb = new StringBuilder();
				foreach (string key in Keys)
				{
					if (!_dictionary.TryGetValue(key, out object value))
					{
					    continue;
					}
				    string val = value.ToString();
					AppendKeyValuePair(sb, key, val, _useOdbcRules);
				}
				return sb.ToString();
			}
			set
			{
				Clear();
				if (value == null)
				{
				    return;
				}
			    if (value.Trim().Length == 0)
			    {
			        return;
			    }
			    ParseConnectionString(value);
			}
		}

        /// <summary> Gets or sets the number of.  </summary>
        /// <value> The count. </value>
		public virtual int Count => _dictionary.Count;

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="T:System.Collections.IDictionary" />
        /// object has a fixed size.
        /// </summary>
        /// <value>
        /// true if the <see cref="T:System.Collections.IDictionary" /> object has a fixed size;
        /// otherwise, false.
        /// </value>
	    public virtual bool IsFixedSize => false;

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="T:System.Collections.IDictionary" />
        /// object is read-only.
        /// </summary>
        /// <value>
        /// true if the <see cref="T:System.Collections.IDictionary" /> object is read-only; otherwise,
        /// false.
        /// </value>
	    public bool IsReadOnly => throw new NotImplementedException();

        /// <summary> Gets or sets the element with the specified key. </summary>
        /// <exception cref="ArgumentException"> Thrown when one or more arguments have unsupported or
        ///     illegal values. </exception>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <exception cref="CreateInvalidKeywordException"> Thrown when a Create Invalid Keyword error
        ///     condition occurs. </exception>
        /// <exception cref="T:System.ArgumentNullException"> . </exception>
        /// <exception cref="T:System.NotSupportedException"> The property is set and the
        ///     <see cref="T:System.Collections.IDictionary" /> object is read-only.-or- The property is
        ///     set, <paramref name="key" /> does not exist in the collection, and the
        ///     <see cref="T:System.Collections.IDictionary" /> has a fixed size. </exception>
        /// <param name="keyword"> The keyword. </param>
        /// <returns> The element with the specified key, or null if the key does not exist. </returns>
	    public virtual object this[string keyword]
		{
			get
			{
			    if (ContainsKey(keyword))
			    {
			        return _dictionary[keyword];
			    }
                else
			    {
			        throw new ArgumentException($"Keyword '{keyword}' does not exist");
			    }
			}
			set
			{
				if (value == null)
				{
					Remove(keyword);
					return;
				}

				if (keyword == null)
				{
				    throw new ArgumentNullException(nameof(keyword));
				}

				if (keyword.Length == 0)
				{
				    throw CreateInvalidKeywordException(keyword);
				}

				for (int i = 0; i < keyword.Length; i++)
				{
					char c = keyword[i];
					if (i == 0 && (char.IsWhiteSpace(c) || c == ';'))
					{
					    throw CreateInvalidKeywordException(keyword);
					}
				    if (i == (keyword.Length - 1) && char.IsWhiteSpace(c))
				    {
				        throw CreateInvalidKeywordException(keyword);
				    }
				    if (char.IsControl(c))
				    {
				        throw CreateInvalidKeywordException(keyword);
				    }
				}

				if (ContainsKey(keyword))
				{
				    _dictionary[keyword] = value;
				}
				else
				{
				    _dictionary.Add(keyword, value);
				}
			}
		}

        /// <summary>
        /// Gets an <see cref="T:System.Collections.ICollection" /> object containing the keys of the
        /// <see cref="T:System.Collections.IDictionary" /> object.
        /// </summary>
        /// <value>
        /// An <see cref="T:System.Collections.ICollection" /> object containing the keys of the
        /// <see cref="T:System.Collections.IDictionary" /> object.
        /// </value>
		public virtual ICollection Keys
		{
			get
			{
				var keys = new string[_dictionary.Keys.Count];
				_dictionary.Keys.CopyTo(keys, 0);
				var keyColl = new ReadOnlyCollection<string>(keys);
				return keyColl;
			}
		}

        /// <summary> Gets or sets a value indicating whether this object is synchronized. </summary>
        /// <value> True if this object is synchronized, false if not. </value>
		bool ICollection.IsSynchronized => throw new NotImplementedException();

        /// <summary> Gets or sets the synchronise root. </summary>
        /// <value> The synchronise root. </value>
	    object ICollection.SyncRoot => throw new NotImplementedException();

        /// <summary> Gets or sets the element with the specified key. </summary>
        /// <exception cref="T:System.ArgumentNullException"> . </exception>
        /// <exception cref="T:System.NotSupportedException"> The property is set and the
        ///     <see cref="T:System.Collections.IDictionary" /> object is read-only.-or- The property is
        ///     set, <paramref name="key" /> does not exist in the collection, and the
        ///     <see cref="T:System.Collections.IDictionary" /> has a fixed size. </exception>
        /// <param name="keyword"> The keyword. </param>
        /// <returns> The element with the specified key, or null if the key does not exist. </returns>
	    object IDictionary.this[object keyword]
		{
			get => this[(string) keyword];
	        set => this[(string) keyword] = value;
	    }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.ICollection" /> object containing the values in the
        /// <see cref="T:System.Collections.IDictionary" /> object.
        /// </summary>
        /// <value>
        /// An <see cref="T:System.Collections.ICollection" /> object containing the values in the
        /// <see cref="T:System.Collections.IDictionary" /> object.
        /// </value>
		public virtual ICollection Values
		{
			get
			{
				var values = new object[_dictionary.Values.Count];
				_dictionary.Values.CopyTo(values, 0);
				var valuesColl = new ReadOnlyCollection<object>(values);
				return valuesColl;
			}
		}

        /// <summary> Adds keyword. </summary>
        /// <param name="keyword"> The keyword. </param>
        /// <param name="value"> The <see cref="T:System.Object" /> to use as the value of the element to
        ///     add. </param>
		public void Add(string keyword, object value)
		{
			this[keyword] = value;
		}

        /// <summary> Appends a key value pair. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <exception cref="ArgumentException"> Thrown when one or more arguments have unsupported or
        ///     illegal values. </exception>
        /// <param name="builder"> The builder. </param>
        /// <param name="keyword"> The keyword. </param>
        /// <param name="value"> The <see cref="T:System.Object" /> to use as the value of the element to
        ///     add. </param>
        /// <param name="useOdbcRules"> True to use ODBC rules. </param>
		public static void AppendKeyValuePair(StringBuilder builder, string keyword, string value, bool useOdbcRules)
		{
			if (builder == null)
			{
			    throw new ArgumentNullException(nameof(builder));
			}
			if (keyword == null)
			{
			    throw new ArgumentNullException(nameof(keyword));
			}
		    if (keyword.Length == 0)
			{
			    throw new ArgumentException("Empty keyword is not valid.");
			}

			if (builder.Length > 0)
			{
			    builder.Append(';');
			}

		    builder.Append(!useOdbcRules ? keyword.Replace("=", "==") : keyword);
		    builder.Append('=');

			if (String.IsNullOrEmpty(value))
			{
			    return;
			}

			if (!useOdbcRules)
			{
				bool dquoteFound = (value.IndexOf('\"') > -1);
				bool squoteFound = (value.IndexOf('\'') > -1);

				if (dquoteFound && squoteFound)
				{
					builder.Append('\"');
					builder.Append(value.Replace("\"", "\"\""));
					builder.Append('\"');
				}
				else if (dquoteFound)
				{
					builder.Append('\'');
					builder.Append(value);
					builder.Append('\'');
				}
				else if (squoteFound || value.IndexOf('=') > -1 || value.IndexOf(';') > -1)
				{
					builder.Append('\"');
					builder.Append(value);
					builder.Append('\"');
				}
				else if (ValueNeedsQuoting(value))
				{
					builder.Append('\"');
					builder.Append(value);
					builder.Append('\"');
				}
				else
				{
				    builder.Append(value);
				}
			}
			else
			{
				int braces = 0;
				bool semicolonFound = false;
				int len = value.Length;
				bool needBraces = false;

				int lastChar = -1;

				for (int i = 0; i < len; i++)
				{
					int peek;
					if (i == (len - 1))
					{
					    peek = -1;
					}
					else
					{
					    peek = value[i + 1];
					}

				    char c = value[i];
					switch (c)
					{
						case '{':
							braces++;
							break;
						case '}':
							if (peek.Equals(c))
							{
								i++;
								continue;
							}
							else
							{
								braces--;
								if (peek != -1)
								{
								    needBraces = true;
								}
							}
							break;
						case ';':
							semicolonFound = true;
							break;
						default:
							break;
					}
					lastChar = c;
				}

				if (value[0] == '{' && (lastChar != '}' || (braces == 0 && needBraces)))
				{
					builder.Append('{');
					builder.Append(value.Replace("}", "}}"));
					builder.Append('}');
					return;
				}

				bool isDriver = (string.Compare(keyword, "Driver", StringComparison.OrdinalIgnoreCase) == 0);
				if (isDriver)
				{
					if (value[0] == '{' && lastChar == '}' && !needBraces)
					{
						builder.Append(value);
						return;
					}
					builder.Append('{');
					builder.Append(value.Replace("}", "}}"));
					builder.Append('}');
					return;
				}

				if (value[0] == '{' && (braces != 0 || lastChar != '}') && needBraces)
				{
					builder.Append('{');
					builder.Append(value.Replace("}", "}}"));
					builder.Append('}');
					return;
				}

				if (value[0] != '{' && semicolonFound)
				{
					builder.Append('{');
					builder.Append(value.Replace("}", "}}"));
					builder.Append('}');
					return;
				}

				builder.Append(value);
			}
		}

        /// <summary> Appends a key value pair. </summary>
        /// <param name="builder"> The builder. </param>
        /// <param name="keyword"> The keyword. </param>
        /// <param name="value"> The <see cref="T:System.Object" /> to use as the value of the element to
        ///     add. </param>
		public static void AppendKeyValuePair(StringBuilder builder, string keyword, string value)
		{
			AppendKeyValuePair(builder, keyword, value, false);
		}

        /// <summary>
        /// Removes all elements from the <see cref="T:System.Collections.IDictionary" /> object.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException"> The
        ///     <see cref="T:System.Collections.IDictionary" /> object is read-only. </exception>
		public virtual void Clear()
		{
			_dictionary.Clear();
		}

        /// <summary> Query if 'keyword' contains key. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="keyword"> The keyword. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
		public virtual bool ContainsKey(string keyword)
		{
			if (keyword == null)
			{
			    throw new ArgumentNullException(nameof(keyword));
			}
		    return _dictionary.ContainsKey(keyword);
		}

        /// <summary> Equivalent to. </summary>
        /// <param name="connectionStringBuilder"> The connection string builder. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
		public virtual bool EquivalentTo(DbConnectionStringBuilder connectionStringBuilder)
		{
			bool ret = true;
			try
			{
				if (Count != connectionStringBuilder.Count)
				{
				    ret = false;
				}
				else
				{
					foreach (string key in Keys)
					{
						if (!this[key].Equals(connectionStringBuilder[key]))
						{
							ret = false;
							break;
						}
					}
				}
			}
			catch (ArgumentException)
			{
				ret = false;
			}
			return ret;
		}

        /// <summary> Removes the given keyword. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="keyword"> The keyword. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
		public virtual bool Remove(string keyword)
		{
			if (keyword == null)
			{
			    throw new ArgumentNullException(nameof(keyword));
			}
			return _dictionary.Remove(keyword);
		}

        /// <summary> Determine if we should serialize. </summary>
        /// <param name="keyword"> The keyword. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
		public virtual bool ShouldSerialize(string keyword)
		{
			throw new NotImplementedException();
		}

        /// <summary> Copies to. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <exception cref="ArgumentException"> Thrown when one or more arguments have unsupported or
        ///     illegal values. </exception>
        /// <param name="array"> The array. </param>
        /// <param name="index"> Zero-based index of the. </param>
		void ICollection.CopyTo(Array array, int index)
		{
			if (array == null)
			{
			    throw new ArgumentNullException(nameof(array));
			}
		    if (!(array is KeyValuePair<string, object>[] arr))
			{
			    throw new ArgumentException("Target array type is not compatible with the type of items in the collection");
			}
			((ICollection<KeyValuePair<string, object>>) _dictionary).CopyTo(arr, index);
		}

        /// <summary>
        /// Adds an element with the provided key and value to the
        /// <see cref="T:System.Collections.IDictionary" /> object.
        /// </summary>
        /// <exception cref="T:System.ArgumentNullException"> . </exception>
        /// <exception cref="T:System.ArgumentException"> An element with the same key already exists in
        ///     the <see cref="T:System.Collections.IDictionary" /> object. </exception>
        /// <exception cref="T:System.NotSupportedException"> The
        ///     <see cref="T:System.Collections.IDictionary" /> is read-only.-or- The
        ///     <see cref="T:System.Collections.IDictionary" /> has a fixed size. </exception>
        /// <param name="keyword"> The <see cref="T:System.Object" /> to use as the key of the element
        ///     to add. </param>
        /// <param name="value"> The <see cref="T:System.Object" /> to use as the value of the element to
        ///     add. </param>
		void IDictionary.Add(object keyword, object value)
		{
			this.Add((string) keyword, value);
		}

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.IDictionary" /> object contains an
        /// element with the specified key.
        /// </summary>
        /// <exception cref="T:System.ArgumentNullException"> . </exception>
        /// <param name="keyword"> The object to test for containment. </param>
        /// <returns>
        /// true if the <see cref="T:System.Collections.IDictionary" /> contains an element with the key;
        /// otherwise, false.
        /// </returns>
		bool IDictionary.Contains(object keyword)
		{
			return ContainsKey((string) keyword);
		}

        /// <summary>
        /// Returns an <see cref="T:System.Collections.IDictionaryEnumerator" /> object for the
        /// <see cref="T:System.Collections.IDictionary" /> object.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IDictionaryEnumerator" /> object for the
        /// <see cref="T:System.Collections.IDictionary" /> object.
        /// </returns>
		IDictionaryEnumerator IDictionary.GetEnumerator()
		{
			return _dictionary.GetEnumerator();
		}

        /// <summary>
        /// Removes the element with the specified key from the
        /// <see cref="T:System.Collections.IDictionary" /> object.
        /// </summary>
        /// <exception cref="T:System.ArgumentNullException"> . </exception>
        /// <exception cref="T:System.NotSupportedException"> The
        ///     <see cref="T:System.Collections.IDictionary" /> object is read-only.-or- The
        ///     <see cref="T:System.Collections.IDictionary" /> has a fixed size. </exception>
        /// <param name="keyword"> The key of the element to remove. </param>
		void IDictionary.Remove(object keyword)
		{
			Remove((string) keyword);
		}

        /// <summary> Gets the enumerator. </summary>
        /// <returns> The enumerator. </returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return _dictionary.GetEnumerator();
		}

        /// <summary> Returns a string that represents the current object. </summary>
        /// <returns> A string that represents the current object. </returns>
		public override string ToString()
		{
			return ConnectionString;
		}

        /// <summary> Attempts to get value an object from the given string. </summary>
        /// <param name="keyword"> The keyword. </param>
        /// <param name="value"> [out] The value. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
		public virtual bool TryGetValue(string keyword, out object value)
		{
			bool found = ContainsKey(keyword);
			value = found ? this[keyword] : null;
			return found;
		}

        /// <summary> Creates invalid keyword exception. </summary>
        /// <param name="keyword"> The keyword. </param>
        /// <returns> The new invalid keyword exception. </returns>
		static ArgumentException CreateInvalidKeywordException(string keyword)
		{
			return new ArgumentException("A keyword cannot contain "
				+ "control characters, leading semicolons or "
				+ "leading or trailing whitespace.", keyword);
		}

        /// <summary> Creates connection string invalid exception. </summary>
        /// <param name="index"> Zero-based index of the. </param>
        /// <returns> The new connection string invalid exception. </returns>
		static ArgumentException CreateConnectionStringInvalidException(int index)
		{
			return new ArgumentException("Format of initialization "
				+ "string does not conform to specifications at "
				+ "index " + index + ".");
		}

        /// <summary> Value needs quoting. </summary>
        /// <param name="value"> The value. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
		static bool ValueNeedsQuoting(string value)
		{
			foreach (char c in value)
			{
				if (char.IsWhiteSpace(c))
				{
				    return true;
				}
			}
			return false;
		}

        /// <summary> Parse connection string. </summary>
        /// <param name="connectionString"> The connection string. </param>
		void ParseConnectionString(string connectionString)
		{
			if (_useOdbcRules)
			{
			    ParseConnectionStringOdbc(connectionString);
			}
			else
			{
			    ParseConnectionStringNonOdbc(connectionString);
			}
		}

        /// <summary> Parse connection string ODBC. </summary>
        /// <exception cref="CreateConnectionStringInvalidException"> Thrown when a Create Connection
        ///     String Invalid error condition occurs. </exception>
        /// <param name="connectionString"> The connection string. </param>
		void ParseConnectionStringOdbc(string connectionString)
		{
			bool inQuote = false;
			bool inDQuote = false;
			bool inName = true;
			bool inBraces = false;

			string name = string.Empty;
		    // ReSharper disable once RedundantAssignment
			string val = string.Empty;
			var sb = new StringBuilder();
			int len = connectionString.Length;

			for (int i = 0; i < len; i++)
			{
				char c = connectionString[i];
				int peek = (i == (len - 1)) ? -1 : connectionString[i + 1];

				switch (c)
				{
					case '{':
						if (inName)
						{
							sb.Append(c);
							continue;
						}

						if (sb.Length == 0)
						{
						    inBraces = true;
						}
					    sb.Append(c);
						break;
					case '}':
						if (inName || !inBraces)
						{
							sb.Append(c);
							continue;
						}

						if (peek == -1)
						{
							sb.Append(c);
							inBraces = false;
						}
						else if (peek.Equals(c))
						{
							sb.Append(c);
							sb.Append(c);
							i++;
						}
						else
						{
							int next = NextNonWhitespaceChar(connectionString, i);
							if (next != -1 && ((char) next) != ';')
							{
							    throw CreateConnectionStringInvalidException(next);
							}
							sb.Append(c);
							inBraces = false;
						}
						break;
					case ';':
						if (inName || inBraces)
						{
							sb.Append(c);
							continue;
						}

						if (name.Length > 0 && sb.Length > 0)
						{
							val = sb.ToString();
							name = name.ToLower().TrimEnd();
							this[name] = val;
						}
						else if (sb.Length > 0)
						{
						    throw CreateConnectionStringInvalidException(c);
						}
						inName = true;
						name = string.Empty;
						sb.Length = 0;
						break;
					case '=':
						if (inBraces || !inName)
						{
							sb.Append(c);
							continue;
						}

						name = sb.ToString();
						if (name.Length == 0)
						{
						    throw CreateConnectionStringInvalidException(c);
						}
						sb.Length = 0;
						inName = false;
						break;
					default:
					    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
						if (inDQuote || inQuote || inBraces)
						{
						    sb.Append(c);
						}
						else if (char.IsWhiteSpace(c))
						{
							// ignore leading whitespace
							if (sb.Length > 0)
							{
								int nextChar = SkipTrailingWhitespace(connectionString, i);
								if (nextChar == -1)
								{
								    sb.Append(c);
								}
								else
								{
								    i = nextChar;
								}
							}
						}
						else
						{
						    sb.Append(c);
						}
					    break;
				}
			}

			if ((inName && sb.Length > 0) || inDQuote || inQuote || inBraces)
			{
			    throw CreateConnectionStringInvalidException(len - 1);
			}

			if (name.Length > 0 && sb.Length > 0)
			{
				val = sb.ToString();
				name = name.ToLower().TrimEnd();
				this[name] = val;
			}
		}

        /// <summary> Parse connection string non ODBC. </summary>
        /// <exception cref="CreateConnectionStringInvalidException"> Thrown when a Create Connection
        ///     String Invalid error condition occurs. </exception>
        /// <param name="connectionString"> The connection string. </param>
		void ParseConnectionStringNonOdbc(string connectionString)
		{
			bool inQuote = false;
			bool inDQuote = false;
			bool inName = true;

			string name = string.Empty;
		    // ReSharper disable once RedundantAssignment
			string val = string.Empty;
			StringBuilder sb = new StringBuilder();
			int len = connectionString.Length;

			for (int i = 0; i < len; i++)
			{
				char c = connectionString[i];
				int peek = (i == (len - 1)) ? -1 : connectionString[i + 1];

				switch (c)
				{
					case '\'':
						if (inName)
						{
							sb.Append(c);
							continue;
						}

						if (inDQuote)
						{
						    sb.Append(c);
						}
						else if (inQuote)
						{
							if (peek == -1)
							{
							    inQuote = false;
							}
							else if (peek.Equals(c))
							{
								sb.Append(c);
								i++;
							}
							else
							{
								int next = NextNonWhitespaceChar(connectionString, i);
								if (next != -1 && ((char) next) != ';')
								{
								    throw CreateConnectionStringInvalidException(next);
								}
							    inQuote = false;
							}

							if (!inQuote)
							{
								val = sb.ToString();
								name = name.ToLower().TrimEnd();
								this[name] = val;
								inName = true;
								name = String.Empty;
								sb.Length = 0;
							}
						}
						else if (sb.Length == 0)
						{
						    inQuote = true;
						}
						else
						{
						    sb.Append(c);
						}
						break;
					case '"':
						if (inName)
						{
							sb.Append(c);
							continue;
						}

						if (inQuote)
						{
						    sb.Append(c);
						}
						else if (inDQuote)
						{
							if (peek == -1)
							{
							    inDQuote = false;
							}
							else if (peek.Equals(c))
							{
								sb.Append(c);
								i++;
							}
							else
							{
								int next = NextNonWhitespaceChar(connectionString, i);
								if (next != -1 && ((char) next) != ';')
								{
								    throw CreateConnectionStringInvalidException(next);
								}
								inDQuote = false;
							}
						}
						else if (sb.Length == 0)
						{
						    inDQuote = true;
						}
						else
						{
						    sb.Append(c);
						}
					    break;
					case ';':
						if (inName)
						{
							sb.Append(c);
							continue;
						}

						if (inDQuote || inQuote)
						{
						    sb.Append(c);
						}
						else
						{
							if (name.Length > 0 && sb.Length > 0)
							{
								val = sb.ToString();
								name = name.ToLower().TrimEnd();
								this[name] = val;
							}
							else if (sb.Length > 0)
							{
							    throw CreateConnectionStringInvalidException(c);
							}
							inName = true;
							name = String.Empty;
							sb.Length = 0;
						}
						break;
					case '=':
						if (inDQuote || inQuote || !inName)
						{
						    sb.Append(c);
						}
						else if (peek != -1 && peek.Equals(c))
						{
							sb.Append(c);
							i++;
						}
						else
						{
							name = sb.ToString();
							if (name.Length == 0)
							{
							    throw CreateConnectionStringInvalidException(c);
							}
							sb.Length = 0;
							inName = false;
						}
						break;
					default:
						if (inDQuote || inQuote)
						{
						    sb.Append(c);
						}
						else if (char.IsWhiteSpace(c))
						{
							// ignore leading whitespace
							if (sb.Length > 0)
							{
								int nextChar = SkipTrailingWhitespace(connectionString, i);
								if (nextChar == -1)
								{
								    sb.Append(c);
								}
								else
								{
								    i = nextChar;
								}
							}
						}
						else
						{
						    sb.Append(c);
						}
					    break;
				}
			}

			if ((inName && sb.Length > 0) || inDQuote || inQuote)
			{
			    throw CreateConnectionStringInvalidException(len - 1);
			}

			if (name.Length > 0 && sb.Length > 0)
			{
				val = sb.ToString();
				name = name.ToLower().TrimEnd();
				this[name] = val;
			}
		}

        /// <summary> Skip trailing whitespace. </summary>
        /// <param name="value"> The value. </param>
        /// <param name="index"> Zero-based index of the. </param>
        /// <returns> An int. </returns>
		static int SkipTrailingWhitespace(string value, int index)
		{
			int len = value.Length;
			for (int i = (index + 1); i < len; i++)
			{
				char c = value[i];
				if (c == ';')
				{
				    return (i - 1);
				}
				if (!char.IsWhiteSpace(c))
				{
				    return -1;
				}
			}
			return len - 1;
		}

        /// <summary> Next non whitespace character. </summary>
        /// <param name="value"> The value. </param>
        /// <param name="index"> Zero-based index of the. </param>
        /// <returns> An int. </returns>
		static int NextNonWhitespaceChar(string value, int index)
		{
			int len = value.Length;
			for (int i = (index + 1); i < len; i++)
			{
				char c = value[i];
				if (!char.IsWhiteSpace(c))
				{
				    return c;
				}
			}
			return -1;
		}
	}
}
