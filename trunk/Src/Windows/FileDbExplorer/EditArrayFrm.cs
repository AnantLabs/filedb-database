using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using FileDbNs;

namespace FileDbExplorer
{
    public partial class EditArrayFrm : Form
    {
        internal object Array { get; set; }
        DataTypeEnum _dataType;

        public EditArrayFrm( object arr, DataTypeEnum dataType )
        {
            InitializeComponent();
            Array = arr;
            _dataType = dataType;
        }

        private void EditArrayFrm_Load( object sender, EventArgs e )
        {
            var sb = new StringBuilder();

            switch( _dataType )
            {
                case DataTypeEnum.Bool:
                {
                    bool[] arr = (bool[]) Array;
                    if( arr != null )
                    {
                        foreach( bool val in arr )
                        {
                            if( sb.Length > 0 )
                                sb.AppendLine();
                            sb.Append( val.ToString() );
                        }
                    }
                }
                break;

                case DataTypeEnum.DateTime:
                {
                    DateTime[] arr = (DateTime[]) Array;
                    if( arr != null )
                    {
                        foreach( DateTime val in arr )
                        {
                            if( sb.Length > 0 )
                                sb.AppendLine();
                            sb.Append( val.ToString() );
                        }
                    }
                }
                break;

                case DataTypeEnum.Double:
                {
                    double[] arr = (double[]) Array;
                    if( arr != null )
                    {
                        foreach( double val in arr )
                        {
                            if( sb.Length > 0 )
                                sb.AppendLine();
                            sb.Append( val.ToString() );
                        }
                    }
                }
                break;

                case DataTypeEnum.Int32:
                {
                    Int32[] arr = (int[]) Array;
                    if( arr != null )
                    {
                        foreach( Int32 val in arr )
                        {
                            if( sb.Length > 0 )
                                sb.AppendLine();
                            sb.Append( val.ToString() );
                        }
                    }
                }
                break;

                case DataTypeEnum.String:
                {
                    String[] arr = (String[]) Array;
                    if( arr != null )
                    {
                        foreach( String val in arr )
                        {
                            if( sb.Length > 0 )
                                sb.AppendLine();
                            sb.Append( val.ToString() );
                        }
                    }
                }
                break;
            }

            TxtValues.Text = sb.ToString();
        }

        private void BtnOk_Click( object sender, EventArgs e )
        {
            const string ErrMsg = "Could not convert value '{0}' to DataType '{1}'";

            try
            {
                string text = TxtValues.Text;
                if( _dataType != DataTypeEnum.String )
                    text = text.Trim();

                if( text.Length == 0 )
                {
                    this.Array = null;
                }
                else
                {
                    string[] vals = text.Split( "\r\n".ToCharArray() );

                    switch( _dataType )
                    {
                        case DataTypeEnum.Bool:
                        {
                            var list = new List<bool>( vals.Length );
                            foreach( string s in vals )
                            {
                                bool val;
                                if( !bool.TryParse( s, out val ) )
                                    throw new Exception( string.Format( ErrMsg, s, _dataType.ToString() ) );
                                list.Add( val );
                            }
                            this.Array = list.ToArray();
                        }
                        break;

                        case DataTypeEnum.DateTime:
                        {
                            var list = new List<DateTime>( vals.Length );
                            foreach( string s in vals )
                            {
                                DateTime val;
                                if( !DateTime.TryParse( s, out val ) )
                                    throw new Exception( string.Format( ErrMsg, s, _dataType.ToString() ) );
                                list.Add( val );
                            }
                            this.Array = list.ToArray();
                        }
                        break;

                        case DataTypeEnum.Double:
                        {
                            var list = new List<double>( vals.Length );
                            foreach( string s in vals )
                            {
                                double val;
                                if( !double.TryParse( s, out val ) )
                                    throw new Exception( string.Format( ErrMsg, s, _dataType.ToString() ) );
                                list.Add( val );
                            }
                            this.Array = list.ToArray();
                        }
                        break;

                        case DataTypeEnum.Int32:
                        {
                            var list = new List<Int32>( vals.Length );
                            foreach( string s in vals )
                            {
                                Int32 val;
                                if( !Int32.TryParse( s, out val ) )
                                    throw new Exception( string.Format( ErrMsg, s, _dataType.ToString() ) );
                                list.Add( val );
                            }
                            this.Array = list.ToArray();
                        }
                        break;

                        case DataTypeEnum.String:
                        {
                            this.Array = vals;
                        }
                        break;
                    }
                }

                this.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Exclamation );
            }
        }
    }
}
