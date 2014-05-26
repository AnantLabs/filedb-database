using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using FileDbNs;

namespace FileDbExplorer
{
    public partial class FrmAddField : Form
    {
        FileDb _fileDb;

        public FrmAddField( FileDb fileDb )
        {
            _fileDb = fileDb;
            InitializeComponent();

            cmbDataTypes.Items.Add( DataTypeEnum.Bool.ToString() );
            cmbDataTypes.Items.Add( DataTypeEnum.Byte.ToString() );
            cmbDataTypes.Items.Add( DataTypeEnum.DateTime.ToString() );
            cmbDataTypes.Items.Add( DataTypeEnum.Decimal.ToString() );
            cmbDataTypes.Items.Add( DataTypeEnum.Double.ToString() );
            cmbDataTypes.Items.Add( DataTypeEnum.Float.ToString() );
            cmbDataTypes.Items.Add( DataTypeEnum.Int32.ToString() );
            cmbDataTypes.Items.Add( DataTypeEnum.Int64.ToString() );
            cmbDataTypes.Items.Add( DataTypeEnum.Single.ToString() );
            cmbDataTypes.Items.Add( DataTypeEnum.String.ToString() );
            cmbDataTypes.Items.Add( DataTypeEnum.UInt32.ToString() );

            cmbDataTypes.SelectedItem = DataTypeEnum.String.ToString();
        }

        private void btnOK_Click( object sender, EventArgs e )
        {
            try
            {
                string sType = cmbDataTypes.SelectedItem as string,
                       name = txtFieldName.Text.Trim(),
                       defaultValue = chkNull.Checked ? null : txtDefaultValue.Text.Trim();

                if( name.Length == 0 )
                    throw new Exception( "You must provide a name for the new field" );

                this.Cursor = Cursors.WaitCursor;
                this.Update();

                DataTypeEnum dataType = (DataTypeEnum) Enum.Parse( typeof( DataTypeEnum ), sType );
                Field newField = new Field( name, dataType );
                _fileDb.AddField( newField, defaultValue );

                this.Cursor = Cursors.Default;
                this.DialogResult = DialogResult.OK;
            }
            catch( Exception ex )
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show( this, ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Error );
            }
        }

        private void chkNull_CheckedChanged( object sender, EventArgs e )
        {
            txtDefaultValue.Enabled = !chkNull.Checked;
            if( txtDefaultValue.Enabled )
                txtDefaultValue.Focus();
        }
    }
}
