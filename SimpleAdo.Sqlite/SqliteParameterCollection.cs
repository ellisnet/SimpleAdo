/* Copyright 2017 Ellisnet - Jeremy Ellis (jeremy@ellisnet.com)

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SimpleAdo.Sqlite
{
    /// <summary> Collection of sqlite parameters. This class cannot be inherited. </summary>
	public sealed class SqliteParameterCollection : IDataParameterCollection {

        /// <summary> Default constructor. </summary>
		internal SqliteParameterCollection()
		{
			m_parameters = new List<SqliteParameter>();
		}

        /// <summary> Check for duplicate parameter. </summary>
        /// <exception cref="InvalidOperationException"> Thrown when the requested operation is invalid. </exception>
        /// <param name="parameterName"> Name of the parameter. </param>
	    private void CheckForDuplicateParameter(string parameterName)
	    {
	        parameterName =
	            ((parameterName[0] == '@') ? parameterName : $"@{parameterName}").ToLower();
	        if (m_parameters.Any(a =>
	            a.ParameterName.ToLower() == parameterName || ($"@{a.ParameterName}").ToLower() == parameterName))
	        {
                throw new InvalidOperationException($"The specified parameter name '{parameterName}' already exists in the collection; multiple parameters with the same name cannot be added.");
	        }
        }

        /// <summary> Adds a parameter with the specified name and value. </summary>
        /// <exception cref="ArgumentException"> Thrown when one or more arguments have unsupported or
        ///     illegal values. </exception>
        /// <param name="parameterName"> Name of the parameter. </param>
        /// <param name="value"> The value. </param>
        /// <returns> An int. </returns>
        public int Add(string parameterName, object value)
        {
		    if (String.IsNullOrWhiteSpace(parameterName))
		    {
		        throw new ArgumentException("The parameter name appears to be invalid.", nameof(parameterName));
		    }
            CheckForDuplicateParameter(parameterName);
            var parameter = new SqliteParameter(parameterName, value);
			m_parameters.Add(parameter);
            return m_parameters.Count - 1;
        }

        /// <summary> Adds value. </summary>
        /// <exception cref="ArgumentException"> Thrown when one or more arguments have unsupported or
        ///     illegal values. </exception>
        /// <param name="value"> The value. </param>
        /// <returns> An int. </returns>
		public int Add(object value)
		{
		    var parameter = (SqliteParameter)value;

		    if (String.IsNullOrWhiteSpace(parameter.ParameterName))
		    {
		        throw new ArgumentException("The parameter name appears to be invalid.", nameof(value));
		    }
            CheckForDuplicateParameter(parameter.ParameterName);

            m_parameters.Add(parameter);
			return m_parameters.Count - 1;
		}

        /// <summary> Adds a range. </summary>
        /// <param name="values"> An Array of items to append to this. </param>
		public void AddRange(Array values)
		{
			foreach (var obj in values)
			{
			    Add(obj);
			}
		}

        /// <summary> Query if this object contains the given parameterName. </summary>
        /// <param name="value"> Name of the parameter. </param>
        /// <returns> True if the object is in this collection, false if not. </returns>
		public bool Contains(object value)
		{
			return m_parameters.Contains((SqliteParameter) value);
		}

        /// <summary> Query if this object contains the given parameterName. </summary>
        /// <param name="value"> Name of the parameter. </param>
        /// <returns> True if the object is in this collection, false if not. </returns>
		public bool Contains(string value)
		{
			return IndexOf(value) != -1;
		}

        /// <summary> Copies to. </summary>
        /// <exception cref="NotSupportedException"> Thrown when the requested operation is not supported. </exception>
        /// <param name="array"> The array. </param>
        /// <param name="index"> Zero-based index of the. </param>
		public void CopyTo(Array array, int index)
		{
			throw new NotSupportedException();
		}

        /// <summary> Clears this object to its blank/initial state. </summary>
		public void Clear()
		{
			m_parameters.Clear();
		}

        /// <summary> Gets the enumerator. </summary>
        /// <returns> The enumerator. </returns>
		public IEnumerator GetEnumerator()
		{
			return m_parameters.GetEnumerator();
		}

        /// <summary> Gets a parameter. </summary>
        /// <param name="index"> Zero-based index of the. </param>
        /// <returns> The parameter. </returns>
		private IDbDataParameter GetParameter(int index)
		{
			return m_parameters[index];
		}

        /// <summary> Gets a parameter. </summary>
        /// <param name="parameterName"> Name of the parameter. </param>
        /// <returns> The parameter. </returns>
		private IDbDataParameter GetParameter(string parameterName)
		{
			return m_parameters[IndexOf(parameterName)];
		}

        /// <summary> Index of the given parameter name. </summary>
        /// <param name="value"> The value. </param>
        /// <returns> An int. </returns>
		public int IndexOf(object value)
		{
			return m_parameters.IndexOf((SqliteParameter) value);
		}

        /// <summary> Index of the given parameter name. </summary>
        /// <param name="parameterName"> Name of the parameter. </param>
        /// <returns> An int. </returns>
		public int IndexOf(string parameterName)
		{
			return m_parameters.FindIndex(x => x.ParameterName == parameterName);
		}

        /// <summary> Inserts. </summary>
        /// <exception cref="ArgumentException"> Thrown when one or more arguments have unsupported or
        ///     illegal values. </exception>
        /// <param name="index"> Zero-based index of the. </param>
        /// <param name="value"> The value. </param>
		public void Insert(int index, object value)
		{
		    var parameter = (SqliteParameter)value;

		    if (String.IsNullOrWhiteSpace(parameter.ParameterName))
		    {
		        throw new ArgumentException("The parameter name appears to be invalid.", nameof(value));
		    }
		    CheckForDuplicateParameter(parameter.ParameterName);

            m_parameters.Insert(index, parameter);
		}

        /// <summary> Removes the given value. </summary>
        /// <param name="value"> The value. </param>
		public void Remove(object value)
		{
			m_parameters.Remove((SqliteParameter) value);
		}

        /// <summary> Remove the paramater from the collection with the matching parameterName. </summary>
        /// <param name="index"> Zero-based index of the. </param>
		public void RemoveAt(int index)
		{
			m_parameters.RemoveAt(index);
		}

        /// <summary> Remove the paramater from the collection with the matching parameterName. </summary>
        /// <param name="parameterName"> Name of the parameter. </param>
		public void RemoveAt(string parameterName)
		{
			RemoveAt(IndexOf(parameterName));
		}

        /// <summary> Sets a parameter. </summary>
        /// <param name="index"> Zero-based index of the. </param>
        /// <param name="value"> The value. </param>
		private void SetParameter(int index, IDbDataParameter value)
		{
			m_parameters[index] = (SqliteParameter) value;
		}

        /// <summary> Sets a parameter. </summary>
        /// <param name="parameterName"> Name of the parameter. </param>
        /// <param name="value"> The value. </param>
		private void SetParameter(string parameterName, IDbDataParameter value)
		{
			SetParameter(IndexOf(parameterName), value);
		}

        /// <summary> Gets or sets the number of.  </summary>
        /// <value> The count. </value>
		public int Count => m_parameters.Count;

        /// <summary> Gets or sets a value indicating whether this object is fixed size. </summary>
        /// <value> True if this object is fixed size, false if not. </value>
	    public bool IsFixedSize => false;

        /// <summary> Gets or sets a value indicating whether this object is read only. </summary>
        /// <value> True if this object is read only, false if not. </value>
	    public bool IsReadOnly => false;

        /// <summary> Gets or sets a value indicating whether this object is synchronized. </summary>
        /// <value> True if this object is synchronized, false if not. </value>
	    public bool IsSynchronized => false;

        /// <summary> Gets or sets the synchronise root. </summary>
        /// <value> The synchronise root. </value>
	    public object SyncRoot => throw new NotSupportedException();

        /// <summary>
        /// Indexer to get or set items within this collection using array index syntax.
        /// </summary>
        /// <param name="index"> Name of the parameter. </param>
        /// <returns> The indexed item. </returns>
	    public SqliteParameter this[int index]
		{
			get => m_parameters[index];
		    set => m_parameters[index] = value;
		}

	    // ReSharper disable once InconsistentNaming
        /// <summary> Options for controlling the operation. </summary>
        readonly List<SqliteParameter> m_parameters;

        /// <summary>
        /// Indexer to get or set items within this collection using array index syntax.
        /// </summary>
        /// <param name="index"> Name of the parameter. </param>
        /// <returns> The indexed item. </returns>
        object IList.this[int index] {
            get => this.GetParameter(index);
            set => this.SetParameter(index, (SqliteParameter)value);
        }

        /// <summary>
        /// Indexer to get or set items within this collection using array index syntax.
        /// </summary>
        /// <param name="parameterName"> Name of the parameter. </param>
        /// <returns> The indexed item. </returns>
        object IDataParameterCollection.this[string parameterName] {
            get => this.GetParameter(parameterName);
            set => this.SetParameter(parameterName, (SqliteParameter)value);
        }
    }
}
