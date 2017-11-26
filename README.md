# SimpleAdo.Sqlite

[![Build status](https://ci.appveyor.com/api/projects/status/eeqmnllnfx66fqdq?svg=true)](https://ci.appveyor.com/project/ellisnet/simpleado)

**SimpleAdo.Sqlite** is a light-weight ADO-style library designed for creating, reading and writing SQLite databases on various platforms - including on mobile devices. In keeping with Microsoft's database ADO.NET libraries, you typically open a SqliteConnection (implements IDbConnection) to a SQLite database via a connection string; execute SQL statements against the database via a SqliteCommand (implements IDbCommand); and read recordset records via a SqliteDataReader (implements IDataReader).

**SimpleAdo.Sqlite** is a .NET Standard library, and requires .NET Standard 1.1 compliance by the platform running it. For developers unfamiliar with .NET Standard libraries, it may be helpful to think of them as a new style of Portable Class Libraries (PCL); you consume the library in the same way that you would have consumed PCLs in your projects in the past. At the time of writing, .NET Standard 1.1 compliance means that it should be compatible with the following platforms - with updated information available [here](https://docs.microsoft.com/en-us/dotnet/standard/net-standard):
  * .NET Core 1.0 (and higher)
  * .NET Framework 4.5 (and higher)
  * Mono 4.6 (and higher)
  * Xamarin.iOS 10.0 (and higher)
  * Xamarin.Mac 3.0 (and higher)
  * Xamarin.Android 7.0 (and higher)
  * Universal Windows Platform (UWP) 10.0 (and higher)
  * Though Windows RT and Windows Phone support .NET Standard 1.1, these are unsupported platforms

**SimpleAdo.Sqlite** has a dependency on the **SQLitePCL.raw** library from Eric Sink - so there may be additional platform constraints; more information about that library is available [here](https://github.com/ericsink/SQLitePCL.raw).

**SimpleAdo.Sqlite** is provided in two libraries - **SimpleAdo.Core** and **SimpleAdo.Sqlite** - with the idea that there could, in the future, be additional implementations of *SimpleAdo.OtherDatabaseProvider*; and all implementations would share the *SimpleAdo.Core* functionality.  However, at the time of writing there is only the *SimpleAdo.Sqlite* implementation.

**SimpleAdo.Sqlite** is the successor to the **Portable.Data.Sqlite** that I previously created. SimpleAdo.Sqlite should have all of the functionality that is present in, and be a mostly pain-free drop-in replacement for, Portable.Data.Sqlite *except* that SimpleAdo.Sqlite does not contain an implementation of the `EncryptedTable<T>` class found in Portable.Data.Sqlite. I plan for the `EncryptedTable<T>` functionality to live on, but it will be moved to a new library I am working on. SimpleAdo.Sqlite does have the ability to store and retrieve encrypted values from individual columns in a table - with functionality identical to Portable.Data.Sqlite. If you are using Portable.Data.Sqlite and you need to keep using the `EncryptedTable<T>` functionality, you may find that you are having problems with the most recent pre-compiled packages [available on NuGet](https://www.nuget.org/packages/Portable.Data.Sqlite/). There is good news: a new version of Portable.Data.Sqlite is available on GitHub and has been tested - as of November 2017 - with Xamarin.iOS, Xamarin.Android, UWP and .NET 4.5.  This updated version removes a dependency on the **SQLitePCL** library - and is in the *develop* branch of the Portable.Data.Sqlite GitHub repository, available [here](https://github.com/ellisnet/Portable.Data.Sqlite/tree/develop).

Important Notes
---------------

  1. Some of the code in SimpleAdo.Sqlite was originally based on a portable (PCL) implementation of Mono.Data.Sqlite that was adapted by Matthew Leibowitz (@mattleibow) - available [here](https://github.com/mattleibow/Mono.Data.Sqlite). Continued thanks to Matt!
  2. The SQL transaction functionality - provided by SqliteTransaction - is untested as of November 2017. For reliable data operations, you are advised to not use the transaction functionality at this time.
  3. The latest released version of this library is [available via NuGet](https://www.nuget.org/packages/SimpleAdo.Sqlite/). From within Visual Studio and Xamarin Studio, search for **SimpleAdo.Sqlite**
  4. For various reasons, this library **does not include an encryption algorithm**.  All operating systems listed above have built-in AES encryption that can be used with this library (as one example of an encryption algorithm that works well).  If you choose to use the column-value-encryption features, it is up to you to specify the algorithm to use by implementing the *IObjectCryptEngine* interface.  This allows you to choose exactly how your data will be encrypted.  Taking a well understood encryption algorithm and implementing your own *encryption engine* should not be too difficult; see detailed information below.
  5. This library **does not implement full database encryption** - for that, please investigate SQLCipher - available [here](http://sqlcipher.net/). That functionality may also be possible by using a special version of the **SQLitePCL.raw** library, but this option is currently unexplored.
  6. The developer of this library welcomes all feedback, suggestions, issue/bug reports, and pull requests. Please log questions and issues in the SimpleAdo.Sqlite GitHub *Issues* section - available [here](https://github.com/ellisnet/SimpleAdo/issues).

Explanation of Sample Projects
------------------------------
In the Samples folder of this repository you will find the folders listed below, with samples for each of the following supported platforms:  Windows desktop (with .NET Framework 4.5 or higher); Universal Windows Platform (UWP - without Xamarin.Forms); and Xamarin.iOS, Xamarin.Android and UWP via Xamarin.Forms. Since I believe strongly in following the MVVM (Model-View-ViewModel) pattern with XAML-based client applications, all sample applications utilize MVVM.

These are no-frills Visual Studio *File - New Project* projects where I added some code to the initial "App/Window/Page" class to specify database path and encryption providers, and then a ViewModel where I create (and manipulate) some tables and records in it.  For the Xamarin.Forms sample app, there is nothing to see in the UI - to check things out, you should read the code and then see what is happening in the Output window of your IDE when you run the application in Debug configuration.

If you want to identify the code and files that I added to the *File - New Project* for demonstrating SQLite functionality, search for *ADDED TO SAMPLE TO DEMONSTRATE SimpleAdo.Sqlite*.  Any code that I added to the automatically generated project files, and any new files, are tagged with that comment.

Samples sub-folders:
  * **SampleApp.WindowsUniversal** - a sample UWP application (that does not use Xamarin.Forms) that runs some SimpleAdo.Sqlite code and reports the results in a textbox. Note that this sample is UWP version 10.0.16299 - based on .NET Core 2.0 - so it only runs on Windows 10 Fall (2017) Creators Edition (and newer).
  * **SampleApp.Wpf** - a sample project for Windows desktop (WPF) with .NET Framework 4.5 or higher,  that runs some SimpleAdo.Sqlite code and reports the results in a textbox. I find this one to be the most helpful for development/testing, because it is easy for me to examine the SQLite database file using Navicat for SQLite, and see what is happening in the database.  Navicat for SQLite is available [here](http://www.navicat.com/products/navicat-for-sqlite) - there is also an excellent free SQLite database tool available [here](http://sqlitebrowser.org/)
  * **SampleApp.XamarinForms** - a sample project for use with Xamarin.Forms (using a Shared Library) on iOS, Android, and UWP, that runs some SimpleAdo.Sqlite code and sends the output to the IDE Output window.
  * **SharedCode** - contains some shared code files that are used by the examples listed above.  The *SampleAesCryptEngine.cs* file contains the sample implementation of `IObjectCryptEngine` that is used in the samples (for encrypting and decrypting values of encrypted columns). The ViewModel logic is shared between the WindowsUniversal and WPF samples listed above, so those files are also located in this folder.

Implementing Encryption
-----------------------
**Using encryption with SimpleAdo.Sqlite is completely optional.** However, if you want to take advantage of its functionality for automatically encrypting and decrypting table column values, you will need to implement the `IObjectCryptEngine` interface. This is pretty easy since all supported platforms feature built-in AES encryption. If you choose, you may also use a third-party encryption library like Bouncy Castle - available [here](http://www.nuget.org/packages/Portable.BouncyCastle).

If you are implementing `IObjectCryptEngine`, you will need to create a class that has `EncryptObject()` and `DecryptObject<T>()` methods.  `EncryptObject()` will take just about any CLR Object and will serialize it and encrypt it, and then return the byte-array as a string; `DecryptObject<T>()` will take a byte-array-as-a-string, decrypt it and de-serialize it back to an object of the type specified as `<T>`.  So, `DecryptObject<MyObject>(myEncryptedString)` should decrypt `myEncryptedString` and turn it into a `MyObject` instance and return it.

An `Initialize()` method is also required by the `IObjectCryptEngine` interface - as shown in the example code below. This was done to allow for the use of a parameterless constructor, as required by the Xamarin.Forms DependencyService.  In the example below, it is used to set the crypto key (a.k.a. password) of the encryption; but since its parameter is a dictionary of objects, you could really use to pass in any number of parameters required by your crypt engine.  You could also implement it as an empty method, and create the constructors (and other methods) that you want for initializing your crypt engine.

A simple AES-based crypt engine class is provided below.

Notes:

* This implementation requires the [Newtonsoft.Json](https://www.newtonsoft.com/json) NuGet package to be added to your projects.
* The main problem with this implementation is its handling of the initialization vector; which should be randomized (but does not need to be encrypted).  See the final note for a solution.
* This implementation does not work with UWP versions lower than 10.0.16299 - i.e. it does not work with versions of Windows 10 prior to Fall (2017) Creators Update. See the next note for a solution.
* A slightly more robust version of this implementation - with a randomized initialization vector, and support for all versions of UWP - is included in the samples folder [here](https://github.com/ellisnet/SimpleAdo/blob/master/Samples/SharedCode/SampleAesCryptEngine.cs).

```c#
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using SimpleAdo;

//Example AES-based "crypt engine" for use with SimpleAdo.Sqlite,
//  should work on all platforms supported by SimpleAdo.Sqlite
//  except UWP prior to 10.0.16299.

//Disclaimer:
//  THIS SAMPLE CODE IS BEING PROVIDED FOR DEMONSTRATION PURPOSES ONLY, AND
//  IS NOT INTENDED FOR USE WITH SOFTWARE THAT MUST PROVIDE ACTUAL DATA SECURITY.
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE 
//  SOFTWARE.

public class SimpleAesCryptEngine : IObjectCryptEngine {

    string _cryptoKey;
    Aes _aesProvider;
    bool _initialized = false;

    private byte[] getBytes(string text, int requiredLength) {
        var result = new byte[requiredLength];
        byte[] textBytes = Encoding.UTF8.GetBytes(text);
        int offset = 0;
        while (offset < requiredLength) {
            int toCopy = (requiredLength >= (offset + textBytes.Length)) ?
                textBytes.Length : requiredLength - offset;
            Buffer.BlockCopy(textBytes, 0, result, offset, toCopy);
            offset += toCopy;
        }
        return result;
    }

    //Parameterless constructor required for Xamarin.Forms DependencyService
    public SimpleAesCryptEngine() { }

    public SimpleAesCryptEngine(string cryptoKey) {
        this.Initialize(cryptoKey);
    }

    public void Initialize(string cryptoKey) {
        this.Initialize(new Dictionary<string, object>() { { "CryptoKey", cryptoKey } });
    }

    public void Initialize(Dictionary<string, object> cryptoParams) {
        _cryptoKey = cryptoParams["CryptoKey"].ToString();
        _aesProvider = Aes.Create();
        _aesProvider.Key = getBytes(_cryptoKey, _aesProvider.Key.Length);
        //Here we are using the same value for all initialization vectors.
        //  This is NOT RECOMMENDED - it should be randomly generated;
        //  however, then you need a way to retrieve it for decryption.
        //  More info: http://en.wikipedia.org/wiki/Initialization_vector
        _aesProvider.IV = getBytes("THIS SHOULD BE RANDOM", _aesProvider.IV.Length);
        _initialized = true;
    }

    public T DecryptObject<T>(string stringToDecrypt, bool throwExceptionOnNullObject = false) {
        if (!_initialized) throw new Exception("Crypt engine is not initialized.");
        T result = default(T);
        if (!String.IsNullOrWhiteSpace(stringToDecrypt)) {
            byte[] bytesToDecrypt = Convert.FromBase64String(stringToDecrypt);
            byte[] decryptedBytes =
                _aesProvider.CreateDecryptor().TransformFinalBlock(bytesToDecrypt, 0, bytesToDecrypt.Length);
            result =
                JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(decryptedBytes));
        }
        else if (throwExceptionOnNullObject) {
                if (stringToDecrypt == null) { throw new ArgumentNullException(nameof(stringToDecrypt)); }
                if (String.IsNullOrWhiteSpace(stringToDecrypt)) { throw new ArgumentOutOfRangeException(nameof(stringToDecrypt)); }
        }
        return result;
    }

    public string EncryptObject(object objectToEncrypt) {
        if (!_initialized) throw new Exception("Crypt engine is not initialized.");
        string result = null;
        if (objectToEncrypt != null) {
            byte[] bytesToEncrypt =
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(objectToEncrypt));
            //Not sure if I should be using TransformFinalBlock() here, 
            //  or if it is more secure if I break the byte array into
            //  blocks and process one block at a time.
            byte[] encryptedBytes =
                _aesProvider.CreateEncryptor().TransformFinalBlock(bytesToEncrypt, 0, bytesToEncrypt.Length);
            result = Convert.ToBase64String(encryptedBytes);
        }
        return result;
    }

    public void Dispose() {
        _cryptoKey = null;
        if (_aesProvider != null) { _aesProvider.Dispose(); }
        _aesProvider = null;
    }
}
```

Examples - Part 1: Doing ADO Stuff
----------------------------------
Note: This library doesn't have a "full" implementation of ADO.NET as you might be used to with Microsoft SQL Server (System.Data.SqlClient).  This is partly due to limitations of the versions of SQLite that ship with mobile devices and partly due to thought that these are probably unnecessary in a mobile application - with a premium on the light-weightedness of the library.  As noted above, the transaction functionality provided by SqliteTransaction is untested as of November 2017.  Mainly, you want to stick to using IDbConnections (i.e. SqliteConnection), IDbCommands (i.e. SqliteCommand) and IDbDataReaders (i.e. SqliteDataReader).  Those are all you need for most SQLite operations.

Step 1) Opening a connection to our database (database file will be created if it doesn't exist), creating a table and adding/reading (i.e. INSERTing/SELECTing) a record.

```c#
//using SimpleAdo;
//using SimpleAdo.Sqlite;

string sql;
string databasePath = System.IO.Path.Combine(pathToDatabaseFolder, "mydatabase.sqlite");
using (IObjectCryptEngine myCryptEngine = new MyCryptEngine("my encryption password")) 
{
    using (var myConnection = new SqliteConnection(
        new SqliteConnectionStringBuilder { DatabaseFilePath = databasePath }.ConnectionString, myCryptEngine)) {
    //Note: If all you have for your connection string is a DatabaseFilePath, you could also just send that as the
    // connection string; and specify that connectionStringIsFilePath = true, like this:
    // using (var myConnection = new SqliteConnection(databasePath, myCryptEngine, true)) {
        sql = "CREATE TABLE IF NOT EXISTS [myTableName] " 
            + "(Id INTEGER PRIMARY KEY AUTOINCREMENT, FirstWord TEXT, SecondWord TEXT);";
        using (var myCommand = new SqliteCommand(sql, myConnection)) 
		{
            myConnection.SafeOpen(); //myConnection.SafeOpen() is the same as 
                //  myConnection.Open() - but doesn't throw an exception if the connection
                //  is already open.
            myCommand.ExecuteNonQuery();
        }
		//NOTE: All of the other example SQL in this section - Part 1: ADO stuff - goes here
    }
}
```
Here is what the above code does:

We identified a new SQLite database [file] called *mydatabase.sqlite* in the folder specified in `pathToDatabaseFolder`.  The recommended folder varies based on the platform (UWP, iOS, Android, etc.)  The recommended folders for storing databases on each platform are listed below.

Next we created an instance of `MyCryptEngine` which is our implementation of `IObjectCryptEngine` as described above, with our secret password.  If we weren't using any encryption functions, we just wouldn't provide this parameter while creating our `SqliteConnection` instance.

Next we created an instance of `SqliteConnection` based on a connection string with a path to our database [file]; and we passed in our crypt engine. Creating the new `SqliteConnection`, with the path to our file, created our database file if it didn't already exist (though this behavior can be changed via settings on the `SqliteConnectionStringBuilder` instance).

The `sql = "CREATE TABLE IF NOT EXISTS..."` line is our SQL statement to create our new table.  Using the `SqliteCommand.ExecuteNonQuery()` line, we can send pretty much any SQLite-legal SQL statements to our database.  Note that SQLite SQL is not the same as the T-SQL that you use with Microsoft's SQL products.  For example - as you will see below - instead of `SELECT TOP 1 * FROM myTableName` (T-SQL), we will use `SELECT * FROM myTableName LIMIT 1` (SQLite SQL).  Lots of information about SQLite's flavor of SQL is available [here](http://www.sqlite.org/lang.html)

Finally, we create a `SqliteCommand` with our SQL statement and connection, open the database connection and execute it.

```c#
        //Add/insert a record
        sql = "INSERT INTO [myTableName] (FirstWord, SecondWord) " +
            "VALUES (@firstword, @secondword);";
        int newRowId;
        using (var myCommand = new SqliteCommand(sql, myConnection)) 
        {
            //You can create the parameter yourself
            myCommand.Parameters.Add(new SqliteParameter("@firstword", "Hello"));
            //Or you can have the add operation create it - and the '@' at the beginning of the parameter
            //  name is optional
            myCommand.Parameters.Add("secondword", "SQLite");
            myConnection.SafeOpen();
            //Note: INTEGER columns in SQLite are always long/Int64 - including ID columns, so converting to int
            newRowId = Convert.ToInt32(myCommand.ExecuteReturnRowId());
        }

        //Select a record
        sql = "SELECT * FROM [myTableName] WHERE [Id] = @rowid;";
        using (var myCommand = new SqliteCommand(sql, myConnection)) 
        {
            myCommand.Parameters.Add(new SqliteParameter("@rowid", value: newRowId));
            myConnection.SafeOpen();
            using (SqliteDataReader myReader = myCommand.ExecuteReader()) 
            {
                while (myReader.Read()) 
                {
                    int rowId = myReader.GetInt32("Id");
                    string firstWord = myReader["FirstWord"].ToString() ?? "";
                    string lastWord = myReader.GetString("SecondWord") ?? "";
                    Console.WriteLine("{0} {1}!",  firstWord, lastWord); // Output: Hello SQLite!
                }
            }
        }

        //Select the 'TOP 1' record - via 'LIMIT 1'
        sql = "SELECT [Id] FROM [myTableName] ORDER BY [Id] DESC LIMIT 1;";
        using (var myCommand = new SqliteCommand(sql, myConnection)) 
        {
            myConnection.SafeOpen();
            int highestRowId = Convert.ToInt32(myCommand.ExecuteScalar());
            Console.WriteLine("The highest [Id] column value so far is: {0}", highestRowId);
        }
```
Here is what the above code does:

First we INSERTed a row into our table with the values ('*Hello*', '*SQLite*').  We used `SqliteCommand.ExecuteReturnRowId()` so we would get back the RowId (i.e. the value of our INTEGER PRIMARY KEY [Id] column). Note that SQLite mostly deals in the long/Int64 datatype for Integer values, so that is what `SqliteCommand.ExecuteReturnRowId()` returned, and we converted it to an int/Int32.

Next we SELECTed the row using `SqliteCommand.ExecuteReader()` to create a `SqliteDataReader`, and then `SqliteDataReader.Read()` to iterate through the rows of our recordset.  If more than row had been returned, the `while (myReader.Read()) {}` loop would repeat for each row - i.e. `SqliteDataReader.Read()` returns false when there are no more rows. Notice that when doing `SqliteDataReader.Read()`, there a few different ways to get a particular column value.  You can use `SqliteDataReader[columnName]` or `SqliteDataReader[columnIndex]`, which return `System.Object`; or you can use `SqliteDataReader.GetXXX(columnName)` or `SqliteDataReader.GetXXX(columnIndex)` to get a value of a particular type (e.g. `SqliteDataReader.GetInt32()` or `SqliteDataReader.GetString()` as shown above).

Finally, we used `SqliteCommand.ExecuteScalar()` to get a single value. It returns the value of the first column of the first returned row. But `SqliteCommand.ExecuteScalar()` just returns a `System.Object`; it doesn't know what datatype it is returning, so we had to convert that into an Integer.

Believe it or not, you can do almost everything you could want in your SQLite database with just the above code - and some knowledge of the [SQLite flavor of SQL](http://www.sqlite.org/lang.html).

Step 2) Working with an encrypted column.

```c#
        //Adding the encrypted column
        sql = "ALTER TABLE [myTableName] ADD COLUMN EncryptedColumn ENCRYPTED;";
        myConnection.SafeOpen();
        (new SqliteCommand(sql, myConnection)).ExecuteNonQuery();

        //Important note: When creating our table above, we used 'CREATE TABLE IF NOT EXISTS...'
        //  - so if the table already exists, it will not fail.  However, the ADD COLUMN SQL above
        //  *will* fail if there is already a column called 'EncryptedColumn' on our table.
        //  So, you can use code like the following to get a list of columns on the table and
        //  see if your to-be-created column already exists or not, before trying to add it.
        //IColumnInfo[] tableColumns = myConnection.GetTableColumnList("myTableName");
        //if (!tableColumns.Any(a => a.Name == "EncryptedColumn")) {
        //  (new SqliteCommand(sql, myConnection)).ExecuteNonQuery();
        //}

        //Adding some encrypted values
        string valueToEncrypt1 = "This string will be encrypted in the database.";
        Tuple<int, string, string> valueToEncrypt2  = 
            Tuple.Create(1, "This object will also", "be encrypted.");
        long encryptedRowId1, encryptedRowId2;
        sql = "INSERT INTO [myTableName] (EncryptedColumn) VALUES (@encrypted);";
        using (var myCommand = new SqliteCommand(sql, myConnection)) 
        {
            //add value #1
            myCommand.AddEncryptedParameter(new SqliteParameter("@encrypted", valueToEncrypt1));
            myConnection.SafeOpen();
            encryptedRowId1 = myCommand.ExecuteReturnRowId();
            //add value #2
            myCommand.Parameters.Clear();
            myCommand.AddEncryptedParameter(new SqliteParameter("@encrypted", valueToEncrypt2));
            encryptedRowId2 = myCommand.ExecuteReturnRowId();
        }

        //Check the encrypted values
        sql = "SELECT [EncryptedColumn] FROM [myTableName] WHERE [Id] = @rowid;";
        //get value #1
        using (var myCommand = new SqliteCommand(sql, myConnection)) 
        {
            myCommand.Parameters.Add(new SqliteParameter("@rowid", value: encryptedRowId1));
            myConnection.SafeOpen();
            using (SqliteDataReader myReader = myCommand.ExecuteReader()) 
            {
                while (myReader.Read()) 
                {
                    string encryptedValue = myReader.GetString("EncryptedColumn");
                    //Note: By default, GetDecrypted<T> will throw an exception if a NULL column value is encountered.
                    //  You can change this behavior by specifying DbNullHandling.ReturnTypeDefaultValue instead - this will
                    //  return the default value - as in default(T) - of the type specified as T.  'null' in the case of a string
                    string decryptedValue = myReader.GetDecrypted<string>("EncryptedColumn", dbNullHandling: DbNullHandling.ReturnTypeDefaultValue);
                    Console.WriteLine("The encrypted value is: " + encryptedValue);
                    //Output: The encrypted value is: (random characters)
                    Console.WriteLine("The decrypted value is: " + decryptedValue);
                    //Output: The decrypted value is: This string will be encrypted in the database.
                }
            }
        }
        //get value #2
        using (var myCommand = new SqliteCommand(sql, myConnection)) 
        {
            myCommand.Parameters.Add("@rowid", encryptedRowId2);
            myConnection.SafeOpen();
            Tuple<int, string, string> decryptedValue = myCommand.ExecuteDecrypt<Tuple<int, string, string>>();
            Console.WriteLine("The decrypted value is: {0} - {1} {2}",
                decryptedValue.Item1, decryptedValue.Item2, decryptedValue.Item3);
            //Output: The decrypted value is: 1 - This object will also be encrypted.
        }
```
Here is what the above code does:

First we added a column called *EncryptedColumn* to our table.  We set the datatype for the column to be *ENCRYPTED*, but that is just for show.  It really is *TEXT* - there is no such thing in SQLite as a column of datatype *ENCRYPTED*.  However, I like to do that, in case I look at the table column list - it helps me to know which columns are encrypted.  Note that if this SQL statement was executed again, we would get an exception because the column has already been added to the table, and we can't add it again.  There is a `CREATE TABLE IF NOT EXISTS` but not an `ADD COLUMN IF NOT EXISTS`; see the note above about how to check the list of columns on our table first, before trying to add a column. 

We added our values-to-be-encrypted using `SqliteParameter` almost as normal, but the parameter values were added with `SqliteCommand.AddEncryptedParameter(new SqliteParameter(columnName, valueToEncrypt))` - that is the only thing special we had to do.

Then, we got our decrypted value back, by using a `SqliteDataReader` just as before; but we used `SqliteDataReader.GetDecrypted<T>(columnName)`.  Note that, by default, this will cause an exception if the value in the table column is NULL, though there is an optional `DbNullHandling` enum parameter to control that behavior.  You can also use the `SqliteDataReader.TryDecrypt<T>()` method that will just return a false if the decryption couldn't happen (e.g. if the column value was NULL).

For our second decrypted value, we used the `SqliteCommand.ExecuteDecrypt<T>()` method.  This works just like `SqliteCommand.ExecuteScalar()` - discussed above - but will return a decrypted value from the first column of the first row of the recordset returned by the query.

**Important Final Notes for ADO**

  1. In the example above we were able to store an object (a Tuple) - not just a string or integer value - in an encrypted column, because SQLite doesn't really care what we put in there.  It will all be text, as far as SQLite is concerned. We could basically put just about any type of CLR object in an encrypted column... which is pretty powerful.
  2. However, searching for matching records based on values in an encrypted column becomes difficult, because you would have to search on an exact encrypted string; or decrypt all of the values in your table to see which records had a matching value. This was originally resolved in the *Portable.Data.Sqlite* library via the `EncryptedTable<T>` class - which is being moved to a different library. More information in the future...
  3. In SQLite there is a `user_version` integer (long/Int64) value that can be set and read at the database level. This is useful for storing the database schema (DDL) version for the database - i.e. an integer number that indicates what version of the database schema the database has.  As your database schema is updated - e.g. by adding tables or table columns - you can update the schema version; and then, in the future, you can determine what "version" your database is - to see if those tables/columns have already been added or not. Set the schema version with `SqliteConnection.SetDatabaseSchemaVersion()` and read it with `SqliteConnection.GetDatabaseSchemaVersion()`
  4. In SimpleAdo.Sqlite, there is a concept of *Maintenance Mode* for the database (i.e. for the `SqliteConnection` instance).  When the database is in "maintenance mode", normal data operations are blocked (i.e. an exception is thrown when you try to execute them). You would typically use this for backup operations, or schema-modifying operations, or log checkpointing, or similar operations.  For example, in the previous note, the `SqliteConnection.SetDatabaseSchemaVersion()` operation places the database in "maintenance mode" while setting the schema version.  You can turn on and off "maintenance mode" with `SqliteConnection.BeginDatabaseMaintenanceMode()` and `SqliteConnection.EndDatabaseMaintenanceMode()`. Then you can specify that a `SqliteCommand` should operate in maintenance mode, via the boolean `forMaintenance` constructor parameter. While in "maintenance mode", commands don't have `forMaintenance = true`, cannot be run; while commands that do have `forMaintenance = true` can only be run after turning on "maintenance mode" via `SqliteConnection.BeginDatabaseMaintenanceMode()`.
  5. In SimpleAdo.Sqlite, it is not possible to have multiple SqliteConnection instances that are interacting with the same database file. This is for safety and to prevent database corruption. If you create a SqliteConnection pointing to a database file that is in use by another (undisposed) SqliteConnection, you will see an exception. The exception to this is the 'Default Database' - see the next note.
  6. In SimpleAdo.Sqlite, there is a concept of a *Default Database*. This is mainly useful for applications that mostly interact with a single SQLite database file - i.e. the "main" database.  If you create a SqliteConnection instance with a valid connection string and set the `setAsDefaultDatabase` constructor parameter as `true`, this database will be specified as the "default database". There can only be one "default database", so future attempts to set a database as the default will fail. Once the "default database" has been set; multiple `SqliteConnection` instances can simultaneously exist that use it - using the parameterless `SqliteConnection` constructor (i.e. `new SqliteConnection()`).

Examples - Part 2: Using this Library with Xamarin.Forms
--------------------------------------------------------

Using the library with Xamarin.Forms is easy. Add the **SimpleAdo.Sqlite** NuGet package to all of your projects (Android, iOS and UWP - and core, if using the PCL/NetStandard shared code model) and it is mostly complete. The only other thing is to figure out how your shared code will get the path to the database file; and the encryption engine (if you are using this feature). The sample project available [here](https://github.com/ellisnet/SimpleAdo/tree/master/Samples/SampleApp.XamarinForms) uses platform specific database path providers (which implement the `IDatabasePath` interface) as described below; and two properties on the `App` (`Xamarin.Forms.Application`) class - `AppDatabasePath` and `AppCryptEngine` - to provide the necessary functionality to the shared code project.

A more elegant solution might use the Xamarin.Forms `DependencyService` to access these providers. Here are notes about how to modify the code presented above for that:

  * Assuming you are using an implementation of `IObjectCryptEngine` similar to the `SimpleAesCryptEngine` shown above and this class exists in the platform-specific projects, you will need to add a line above the class definition like the one below to tell Xamarin.Forms that you will be looking for this implementation of `IObjectCryptEngine` via the `DependencyService.Get<T>()` method.  With the addition of this extra line, the `SimpleAesCryptEngine` class from above should work in all platforms supported by Xamarin.Forms (see note above about versions of UWP that are supported).
```c#
[assembly: Xamarin.Forms.Dependency (typeof(SimpleAesCryptEngine))]
public class SimpleAesCryptEngine : IObjectCryptEngine {
...
```

  * Then, to get a handle to your crypt engine inside the Xamarin.Forms shared code (i.e. portable) project (where most of your application code lives), you will need to do something like this.  The Xamarin.Forms *DependencyService* does not allow us to send in parameters in the constructor of the *IObjectCryptEngine*, so we will instead pass them via the *Initialize()* method.  Now you can use myCryptEngine in your portable code as shown in all of the examples above.
```c#
IObjectCryptEngine myCryptEngine = DependencyService.Get<IObjectCryptEngine> ();
myCryptEngine.Initialize (new Dictionary<string, object>() { { "CryptoKey", "MY SECRET PASSWORD" } });
//or just: myCryptEngine.Initialize ("MY SECRET PASSWORD");
``` 

  * Next, we need a way to get the path to our database file, and this path will probably vary with each platform.  So, we want a database path interface - `IDatabasePath` - in our shared code:
```c#
public interface IDatabasePath
{
    string GetPath(string databaseName);
}
``` 
  * Then we will want a platform specific class in each of our platform projects (Android, iOS and UWP) that implements the `IDatabasePath` interface, that will return the path to our shared code.  The `IDatabasePath` interface only defines one method - `GetPath(string databaseName)`.  You pass in the name of your SQLite database file name, and it gives you back the appropriate path to the file (the returned path includes the database file name at the end).  Here are platform specific classes that implement `IDatabasePath`:
```c#
//Implementation of IDatabasePath for Xamarin.iOS
using System;

//Special note about iOS: You do *not* want to store the full path to your database anywhere in your application
//  with the intention of persistently "remembering" where the database is stored (i.e. between application stops/starts).  
//  When users update your app to its newest version on their devices, the path to your database file will change.
//  Things will work correctly if you get the path via the GetPath() method below every application start, because that 
//  will return the correct path - even after updates - but a persistently stored full database path will have an
//  incorrect path.

[assembly: Xamarin.Forms.Dependency (typeof(DatabasePath))]
public class DatabasePath : IDatabasePath
{
	public DatabasePath () { }

	public string GetPath (string databaseName)
	{
		string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal); // Documents folder
		string libraryPath = System.IO.Path.Combine(documentsPath, "..", "Library"); // Library folder
		return System.IO.Path.Combine(libraryPath, databaseName);
	}
}

//Implementation of IDatabasePath for Xamarin.Android
using System;

[assembly: Xamarin.Forms.Dependency (typeof(DatabasePath))]
public class DatabasePath : IDatabasePath
{
	public DatabasePath () { }

	public string GetPath (string databaseName)
	{
		string libraryPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
		return System.IO.Path.Combine(libraryPath, databaseName);
	}
}

//Implementation of IDatabasePath for UWP
using System;

[assembly: Xamarin.Forms.Dependency (typeof(DatabasePath))]
public class DatabasePath : IDatabasePath
{
	public DatabasePath () { }

	public string GetPath (string databaseName)
	{
		string libraryPath = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
		return System.IO.Path.Combine(libraryPath, databaseName);
	}
}
```

  * Now, we can get these platform specific database paths using the following code in the Portable code project:
```c#
string databasePath = DependencyService.Get<IDatabasePath>().GetPath("mydatabase.sqlite");
using (var myConnection = new SqliteAdoConnection(databasePath, myCryptEngine, true)) { 
	...
```

Except for those minor changes, all of the example code shown above should work properly in the shared/portable code section of your Xamarin.Forms application.
