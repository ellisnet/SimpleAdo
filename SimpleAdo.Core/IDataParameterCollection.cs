using System.Collections;

namespace SimpleAdo
{
    /// <summary> Interface for data parameter collection. </summary>
    public interface IDataParameterCollection : IList
    {
        /// <summary> Query if this object contains the given parameterName. </summary>
        /// <param name="parameterName"> Name of the parameter. </param>
        /// <returns> True if the object is in this collection, false if not. </returns>
        bool Contains(string parameterName);

        /// <summary> Index of the given parameter name. </summary>
        /// <param name="parameterName"> Name of the parameter. </param>
        /// <returns> An int. </returns>
        int IndexOf(string parameterName);

        /// <summary> Remove the paramater from the collection with the matching parameterName. </summary>
        /// <param name="parameterName"> Name of the parameter. </param>
        void RemoveAt(string parameterName);

        /// <summary> Adds a parameter with the specified name and value. </summary>
        /// <param name="parameterName"> Name of the parameter. </param>
        /// <param name="value"> The value. </param>
        /// <returns> An int. </returns>
        int Add(string parameterName, object value);

        /// <summary>
        /// Indexer to get or set items within this collection using array index syntax.
        /// </summary>
        /// <param name="parameterName"> Name of the parameter. </param>
        /// <returns> The indexed item. </returns>
        object this[string parameterName] 
        { 
            get; 
            set; 
        }
    }
}
