using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
//using System.Data.Metadata.Edm;
//using System.Data.Services.Design;
using System.IO;
using System.Reflection;
using Microsoft.CSharp;

using LINQPad.Extensibility.DataContext;

namespace FileDbDynamicDriverNs
{
	internal class SchemaBuilder
	{
        internal static List<ExplorerItem> GetSchemaAndBuildAssembly( FileDbDynamicDriverProperties props, AssemblyName name,
            ref string nameSpace, ref string typeName )
        {
            // Generate the C# code

            string code = GenerateCode( props.Folder, props.Extension, nameSpace );

            // Compile the code into the assembly, using the assembly name provided:
            BuildAssembly( code, name );

            // populate the Schema Explorer from the database files in the selected folder
            List<ExplorerItem> schema = GetSchema( props, out typeName );

            return schema;
        }

        // Generate the C# code for our FileDbDataContext class, which we'll compile in
        // the BuildAssembly method
        //
        internal static string GenerateCode( string folder, string extension, string nameSpace )
        {
            DirectoryInfo dirInfo = new DirectoryInfo( folder );
            FileInfo[] files = dirInfo.GetFiles( String.Format( "*.{0}", extension ) );
            string dotExtension = "." + extension;

            var writer = new StringWriter();


            writer.WriteLine( "using System;" );
            writer.WriteLine( "using System.Collections.Generic;" );
            writer.WriteLine();
            writer.WriteLine( "namespace " + nameSpace );
            writer.WriteLine( '{' );

            // create a class for each database

            foreach( FileInfo fi in files )
            {
                string tablename = fi.Name.Replace( dotExtension, string.Empty );

                writer.WriteLine( string.Format( "public class {0} {{", tablename ) );

                // open the database and get the column names and create a Property for each

                FileDbNs.Fields fields = getFieldsFromDb( fi.FullName );

                foreach( FileDbNs.Field field in fields )
                {
                    string dataType = string.Empty;

                    switch( field.DataType )
                    {
                        case FileDbNs.DataTypeEnum.String:
                            dataType = field.IsArray ? "String[]" : "String";
                            break;
                        case FileDbNs.DataTypeEnum.Bool:
                            dataType = field.IsArray? "Boolean[]" : "Boolean";
                            break;
                        case FileDbNs.DataTypeEnum.Byte:
                            dataType = field.IsArray ? "Byte[]" : "Byte";
                            break;
                        case FileDbNs.DataTypeEnum.DateTime:
                            dataType = field.IsArray ? "DateTime[]" : "DateTime";
                            break;
                        case FileDbNs.DataTypeEnum.Float:
                            dataType = field.IsArray ? "Single[]" : "Single";
                            break;
                        case FileDbNs.DataTypeEnum.Double:
                            dataType = field.IsArray ? "Double[]" : "Double";
                            break;
                        case FileDbNs.DataTypeEnum.Int32:
                            dataType = field.IsArray ? "Int32[]" : "Int32";
                            break;
                        case FileDbNs.DataTypeEnum.UInt32:
                            dataType = field.IsArray ? "UInt32[]" : "UInt32";
                            break;
                    }

                    writer.WriteLine( string.Format( "  public {0} {1} {{ get; set; }}", dataType, field.Name ) );
                }
                 
                writer.WriteLine( '}' ); // class
                writer.WriteLine();
            }
            writer.WriteLine();

            // create the FileDbContext class

            writer.WriteLine( "public class FileDbContext {" );
            writer.WriteLine();
            writer.WriteLine( "string _dbPath;" );
            writer.WriteLine();

            // constructor

            writer.WriteLine( "public FileDbContext( string dbPath )" );
            writer.WriteLine( '{' );
            writer.WriteLine( "_dbPath = dbPath;" );
            writer.WriteLine( '}' );
            writer.WriteLine();

            // public Table properties

            foreach( FileInfo fi in files )
            {
                string tablename = fi.Name.Replace( dotExtension, string.Empty );
                // open the db file and get all the records
                writer.WriteLine( string.Format( "public IList<{0}> {1}", tablename, tablename ) );
                writer.WriteLine( '{' );
                writer.WriteLine( "  get" );
                writer.WriteLine( "  {" );
                writer.WriteLine( string.Format( "    IList<{0}> _{1};", tablename, tablename ) );
                writer.WriteLine( "    FileDbNs.FileDb db = new FileDbNs.FileDb();" );
                writer.WriteLine( "    try" );
                writer.WriteLine( "    {" );
                writer.WriteLine( string.Format( "      db.Open( System.IO.Path.Combine( _dbPath, @\"{0}\" ), false );", fi.Name ) );
                writer.WriteLine( string.Format( "      _{0} = db.SelectAllRecords<{1}>();", tablename, tablename ) );
                writer.WriteLine( "    }" );
                writer.WriteLine( "    finally" );
                writer.WriteLine( "    {" );
                writer.WriteLine( "      if( db.IsOpen )" );
                writer.WriteLine( "        db.Close();" );
                writer.WriteLine( "    }" );
                writer.WriteLine( string.Format( "    return _{0};", tablename ) );
                writer.WriteLine( "  }" );
                //writer.WriteLine( string.Format( "get {{ return _{0}; }}", tablename ) );
                writer.WriteLine( '}' );
                writer.WriteLine();
            }

            writer.WriteLine( '}' ); // class
            writer.WriteLine( '}' ); // namespace

            return writer.ToString();
        }

        // Use the CSharpCodeProvider to compile the generated code
        //
        static void BuildAssembly( string code, AssemblyName name )
        {
            CompilerResults results;

            Assembly asm = Assembly.GetExecutingAssembly();
            string fileDbPath = asm.Location;
            int idx = fileDbPath.LastIndexOf( '\\' );
            fileDbPath = fileDbPath.Substring( 0, idx );
            fileDbPath = Path.Combine( fileDbPath, "FileDb.dll" );

            using( var codeProvider = new CSharpCodeProvider( new Dictionary<string, string>() { { "CompilerVersion", "v3.5" } } ) )
            {
                //fileDbPath = @"C:\Dev\EzTools.NET\FileDb\LinqPad Driver\FileDbDynamicDriver\bin\Debug\FileDb.dll";
                var options = new CompilerParameters(
                    string.Format( "System.dll|System.Core.dll|{0}", fileDbPath ).Split( "|".ToCharArray() ),
                    //string.Format( "System.dll|System.Core.dll|System.Data.dll|System.Data.Linq.dll|{0}", fileDbPath ).Split( "|".ToCharArray() ),
                    name.CodeBase,
                    true );
                results = codeProvider.CompileAssemblyFromSource( options, code );
            }
            if( results.Errors.Count > 0 )
                throw new Exception
                    ( "Cannot compile typed context: " + results.Errors[0].ErrorText + " (line " + results.Errors[0].Line + ")" );
        }

        internal static List<ExplorerItem> GetSchema( FileDbDynamicDriverProperties props, out string typeName )
        {
            typeName = "FileDbContext";

            var tables = new List<ExplorerItem>();

            DirectoryInfo dirInfo = new DirectoryInfo( props.Folder );
            FileInfo[] files = dirInfo.GetFiles( String.Format( "*.{0}", props.Extension ) );

            foreach( FileInfo fi in files )
            {
                string tableName = fi.Name.Substring( 0, fi.Name.LastIndexOf( '.' ) );

                ExplorerItem item = new ExplorerItem( tableName, ExplorerItemKind.QueryableObject, ExplorerIcon.Table )
                {
                    IsEnumerable = true,
                    ToolTipText = "", // FormatTypeName (prop.PropertyType, false),

                    // Store the filename to the Tag property - we'll use it below
                    Tag = fi.FullName
                };

                tables.Add( item );
            }

            // Populate the columns (properties) of each entity:

            foreach( ExplorerItem table in tables )
            {
                FileDbNs.Fields fields = getFieldsFromDb( table.Tag as string );

                var columns = new List<ExplorerItem>();

                foreach( FileDbNs.Field field in fields )
                {
                    string name = string.Format( "{0} ({1})", field.Name, field.DataType.ToString() );
                    ExplorerItem item = new ExplorerItem( name, ExplorerItemKind.Property, ExplorerIcon.Column );
                    columns.Add( item );
                }
                table.Children = columns;
            }

            return tables;
        }

        static FileDbNs.Fields getFieldsFromDb( string dbFileName )
        {
            var fileDb = new FileDbNs.FileDb();

            try
            {
                fileDb.Open( dbFileName, false );
                return fileDb.Fields;
            }
            finally
            {
                if( fileDb.IsOpen )
                    fileDb.Close();
            }
        }
	}
}
