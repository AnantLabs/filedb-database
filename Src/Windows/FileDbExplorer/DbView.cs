using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.IO;

using FileDbNs;

namespace FileDbExplorer
{
    internal enum NodeType { None, Databases, Database, Field }

    //=========================================================================
    internal class NodeInfo
    {
        internal NodeType NodeType;
        internal object Tag;

        internal NodeInfo( NodeType nodeType )
        {
            NodeType = nodeType;
        }

        internal NodeInfo( NodeType nodeType, object tag )
        {
            NodeType = nodeType;
            Tag = tag;
        }
    }

    //=========================================================================
    internal class DbView
    {
        const int Img_Databases = 0, Img_Db = 1, Img_Field = 3;

        internal TreeView DbTree;

        TreeNode _rootNode;

        internal DbView( TreeView dbTree )
        {
            DbTree = dbTree;
            _rootNode = DbTree.Nodes.Add( null, "Databases", Img_Databases, Img_Databases );
            _rootNode.Tag = new NodeInfo( NodeType.Databases );
            //_rootNode.ContextMenuStrip = MainFrm.TheAppWnd.CtxMnuDatabase;
        }

        //---------------------------------------------------------------------
        internal void OpenDatabase( string dbFile )
        {
            DbTree.SuspendLayout();
            try
            {
                FileDb fileDb = new FileDb();
                fileDb.Open( dbFile, false );
                OpenDatabase( fileDb );
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Exclamation );
            }
            finally
            {
                DbTree.ResumeLayout();
            }
        }

        internal void OpenDatabase( FileDb fileDb )
        {
            string dbFile = fileDb.DbFileName;
            string sDatabaseName = Path.GetFileName( dbFile );
            //Path.GetDirectoryName( dbFile );
            TreeNode dbNode = _rootNode.Nodes.Add( null, sDatabaseName, Img_Db, Img_Db );
            dbNode.Tag = new NodeInfo( NodeType.Database, fileDb );
            dbNode.ContextMenuStrip = MainFrm.TheAppWnd.CtxMnuDatabase;
            _rootNode.Expand();

            CreateFieldNodes( fileDb, dbNode );

            DbTree.SelectedNode = dbNode;
            //dbNode.Expand();
        }

        internal FileDb GetDb( string dbName )
        {
            foreach( TreeNode node in _rootNode.Nodes )
            {
                if( string.Compare( node.Text, dbName, true ) == 0 )
                    return (node.Tag as NodeInfo).Tag as FileDb;
            }
            return null;
        }

        internal void CreateFieldNodes( FileDb fileDb, TreeNode dbNode )
        {
            dbNode.Nodes.Clear();

            foreach( Field field in fileDb.Fields )
            {
                var sb = new StringBuilder( 100 );
                sb.Append( field.Name );
                sb.Append( " [" );
                sb.Append( field.DataType.ToString() );

                if( field.IsPrimaryKey )
                {
                    sb.Append( ' ' );
                    sb.Append( "PK" );
                }
                if( field.IsAutoInc )
                {
                    sb.Append( ' ' );
                    sb.Append( "AutoInc" );
                }
                if( field.IsArray )
                {
                    sb.Append( ' ' );
                    sb.Append( "Array" );
                }
                sb.Append( ']' );

                TreeNode fieldNode = dbNode.Nodes.Add( null, sb.ToString(), Img_Field, Img_Field );
                fieldNode.Tag = new NodeInfo( NodeType.Field, field.Name );
                fieldNode.ContextMenuStrip = MainFrm.TheAppWnd.CtxMnuField;
            }
        }

        internal void CloseAll()
        {
            foreach( TreeNode node in _rootNode.Nodes )
            {
                NodeInfo nodeInfo = node.Tag as NodeInfo;
                FileDb fileDb = nodeInfo.Tag as FileDb;
                fileDb.Close();
            }
        }

    }
}
