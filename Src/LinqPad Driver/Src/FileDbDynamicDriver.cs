using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Data.Services.Client;
using LINQPad.Extensibility.DataContext;

namespace FileDbDynamicDriverNs
{
    /// <summary>
    /// Sample dynamic driver. This lets users connect to an ADO.NET Data Services URI, builds the
    /// type data context dynamically, and returns objects for the Schema Explorer.
    /// </summary>
    /// 
    public class FileDbDynamicDriver : DynamicDataContextDriver
    {
        public override string Name { get { return "FileDb - the free local database for .NET"; } }

        public override string Author { get { return "EzTools Software (www.eztools-software.com)"; } }

        public override string GetConnectionDescription( IConnectionInfo cxInfo )
        {
            string friendlyName = new FileDbDynamicDriverProperties( cxInfo ).FriendlyName;
            return friendlyName;
        }

        public override ParameterDescriptor[] GetContextConstructorParameters( IConnectionInfo cxInfo )
        {
            // We need to pass the chosen folder into the DataServiceContext's constructor:
            return new[] { new ParameterDescriptor( "DbFolder", "System.String" ) };
        }

        public override object[] GetContextConstructorArguments( IConnectionInfo cxInfo )
        {
            // We need to pass the chosen folder into the DataServiceContext's constructor:
            return new object[] { new FileDbDynamicDriverProperties( cxInfo ).Folder };
        }

        public override IEnumerable<string> GetAssembliesToAdd()
        {
            // We need the following assembly for compiliation and autocompletion:
            return new[] { "FileDb.dll" };
        }

        public override IEnumerable<string> GetNamespacesToAdd()
        {
            // Import the commonly used namespaces as a courtesy to the user:
            return new[] { "FileDbNs" };
        }

        public override bool AreRepositoriesEquivalent( IConnectionInfo r1, IConnectionInfo r2 )
        {
            // Two repositories point to the same endpoint if their folders are the same.
            //return object.Equals( r1.DriverData.Element( "Folder" ), r2.DriverData.Element( "Folder" ) );
            return string.Compare( (string) r1.DriverData.Element( "Folder" ), (string) r2.DriverData.Element( "Folder" ), true ) == 0;
        }

        public override void InitializeContext( IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager )
        {
            // This method gets called after a DataServiceContext has been instantiated. It gives us a chance to
            // perform further initialization work.
            //
            // And as it happens, we have an interesting problem to solve! The typed data service context class
            // that Astoria's EntityClassGenerator generates handles the ResolveType delegate as follows:
            //
            //   return this.GetType().Assembly.GetType (string.Concat ("<namespace>", typeName.Substring (19)), true);
            //
            // Because LINQPad subclasses the typed data context when generating a query, GetType().Assembly returns
            // the assembly of the user query rather than the typed data context! To work around this, we must take
            // over the ResolveType delegate and resolve using the context's base type instead:
/*
            var dsContext = (DataServiceContext) context;
            var typedDataServiceContextType = context.GetType().BaseType;

            dsContext.ResolveType = name => typedDataServiceContextType.Assembly.GetType( typedDataServiceContextType.Namespace + 
                "." + name.Split( '.' ).Last() );

            // The next step is to feed any supplied credentials into the Astoria service.
            // (This could be enhanced to support other authentication modes, too).
            var props = new FileDbDynamicDriverProperties( cxInfo );
            dsContext.Credentials = props.GetCredentials();

            // Finally, we handle the SendingRequest event so that it writes the request text to the SQL translation window:
            dsContext.SendingRequest += ( sender, e ) => executionManager.SqlTranslationWriter.WriteLine( e.Request.RequestUri );
*/ 
        }

        public override bool ShowConnectionDialog( IConnectionInfo cxInfo, bool isNewConnection )
        {
            var connDlg = new ConnectionDialog( cxInfo );
            #if true
            bool? result = connDlg.ShowDialog();
            #else
            bool result = (connDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK);
            if( !connDlg.CancelOrOkClicked )
                result = (connDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK);
            #endif

            return (bool) result;
        }

        public override List<ExplorerItem> GetSchemaAndBuildAssembly( IConnectionInfo cxInfo, AssemblyName assemblyToBuild,
            ref string nameSpace, ref string typeName )
        {
            return SchemaBuilder.GetSchemaAndBuildAssembly(
                new FileDbDynamicDriverProperties( cxInfo ),
                assemblyToBuild,
                ref nameSpace,
                ref typeName );
        }
    }
}