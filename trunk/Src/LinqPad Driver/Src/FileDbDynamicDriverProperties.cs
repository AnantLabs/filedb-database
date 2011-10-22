using System.Net;
using System.Xml.Linq;
using LINQPad.Extensibility.DataContext;

namespace FileDbDynamicDriverNs
{
    class FileDbDynamicDriverProperties
    {
        readonly IConnectionInfo _cxInfo;
        readonly XElement _driverData;

        public FileDbDynamicDriverProperties( IConnectionInfo cxInfo )
        {
            _cxInfo = cxInfo;
            if( _cxInfo != null )
                _driverData = cxInfo.DriverData;
            else
                _driverData = new XElement( "DriverData" );
        }

        public bool Persist
        {
            get { return _cxInfo == null? false : _cxInfo.Persist; }
            set { if( _cxInfo != null ) _cxInfo.Persist = value; }
        }

        public string Folder
        {
            get { return (string) _driverData.Element( "Folder" ) ?? string.Empty; }
            set { _driverData.SetElementValue( "Folder", value ); }
        }

        public string Extension
        {
            get { return (string) _driverData.Element( "Extension" ) ?? string.Empty; }

            set { _driverData.SetElementValue( "Extension", value ); }
        }

        public string FriendlyName
        {
            get
            {
                string friendlyName = (string) _driverData.Element( "FriendlyName" );
                if( string.IsNullOrEmpty( friendlyName ) )
                {
                    friendlyName = this.Folder;
                    if( !string.IsNullOrEmpty( friendlyName ) )
                        friendlyName = friendlyName.Substring( friendlyName.LastIndexOf( '\\' ) + 1 );
                    else
                        friendlyName = string.Empty;
                }
                return friendlyName; 
            }

            set { _driverData.SetElementValue( "FriendlyName", value ); }
        }

    }
}
