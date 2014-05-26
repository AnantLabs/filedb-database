using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Windows.Forms;

namespace Utils
{
    static class Helpers
    {
        internal static void RestoreFormPos( Form form, string subKey )
        {
            RegistryKey key = null;
            try
            {
                key = Registry.CurrentUser.OpenSubKey( subKey );
                if( key != null )
                {
                    int L, T, W, H;
                    W = (int) key.GetValue( "W", form.Width );
                    H = (int) key.GetValue( "H", form.Height );
                    L = (int) key.GetValue( "L", form.Left );
                    T = (int) key.GetValue( "T", form.Top );
                    form.Size = new System.Drawing.Size( W, H );
                    form.Location = new System.Drawing.Point( L, T );
                    //mSplitterMain.SplitterDistance = (int) key.GetValue( "SplitterMain", mSplitterMain.SplitterDistance );

                    form.WindowState = (FormWindowState) (int) key.GetValue( "WndState", form.WindowState );
                }
            }
            catch( Exception ex )
            {
                MessageBox.Show( form, ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Error );
            }
            finally
            {
                if( key != null )
                    key.Close();
            }
        }

        internal static void SaveFormPos( Form form, string subKey )
        {
            RegistryKey key = null;
            try
            {
                key = Registry.CurrentUser.CreateSubKey( subKey );
                key.SetValue( "WndState", (int) form.WindowState );
                if( form.WindowState == FormWindowState.Normal )
                {
                    key.SetValue( "W", form.Size.Width );
                    key.SetValue( "H", form.Size.Height );
                    key.SetValue( "L", form.Location.X );
                    key.SetValue( "T", form.Location.Y );
                }
                else
                {
                    key.SetValue( "W", form.RestoreBounds.Width );
                    key.SetValue( "H", form.RestoreBounds.Height );
                    key.SetValue( "L", form.RestoreBounds.X );
                    key.SetValue( "T", form.RestoreBounds.Y );
                }
                //key.SetValue( "SplitterMain", mSplitterMain.SplitterDistance );
            }
            catch( Exception ex )
            {
                MessageBox.Show( form, ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Error );
            }
            finally
            {
                if( key != null )
                    key.Close();
            }
        }
    }
}
