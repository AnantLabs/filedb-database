
#define APP_NAME "FileDb Explorer"
#define APP_NAME_VER "FileDb Database Engine DLL and Explorer"

[ISSI]
;; The WizardImageFile
;#define ISSI_WizardImageFile "Misc Files\intro.bmp"
;#define ISSI_WizardImageFile_x 164
;; The WizardSmallImageFile
;#define ISSI_WizardSmallImageFile "Misc Files\sm_wiz_img.bmp"
;#define ISSI_WizardSmallImageFile_x 60
;#define ISSI_WizardSmallImageFile_Align
;; Languages
#define ISSI_English
#define ISSI_Spanish
;#define ISSI_Portuguese
;; .NET Detection
;#define ISSI_NetDetect

;; Include ISSI
;#define ISSI_IncludePath "ISSI"
;#include "ISSI\_issi.isi"


[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{F5D3B8CD-700B-453c-B8D5-8BC456245185}
AppName={#APP_NAME}
AppVerName={#APP_NAME_VER}
AppPublisher=EzTools Software
AppPublisherURL=http://www.eztools-software.com
AppSupportURL=http://www.eztools-software.com
AppUpdatesURL=http://www.eztools-software.com
DefaultDirName={pf}\EzTools\FileDb
DefaultGroupName=EzTools\FileDb
;SetupIconFile=Files\FileDb.ico
;UninstallDisplayIcon={app}\Explorer\FileDbExplorer.exe
LicenseFile=Files\license.rtf
Compression=lzma
SolidCompression=yes
OutputDir=.\
OutputBaseFilename=FileDbSetup

; "ArchitecturesInstallIn64BitMode=x64" requests that the install be
; done in "64-bit mode" on x64, meaning it should use the native
; 64-bit Program Files directory and the 64-bit view of the registry.
; On all other architectures it will install in "32-bit mode".

;ArchitecturesInstallIn64BitMode=x64

; Note: We don't set ProcessorsAllowed because we want this
; installation to run on all architectures (including Itanium,
; since it's capable of running 32-bit code too).

[Languages]
Name: eng; MessagesFile: compiler:Default.isl
Name: fre; MessagesFile: compiler:Languages\French.isl
Name: ger; MessagesFile: compiler:Languages\German.isl
Name: bra; MessagesFile: compiler:Languages\BrazilianPortuguese.isl
Name: cze; MessagesFile: compiler:Languages\Czech.isl
Name: dan; MessagesFile: compiler:Languages\Danish.isl
Name: du; MessagesFile: compiler:Languages\Dutch.isl
Name: fin; MessagesFile: compiler:Languages\Finnish.isl
Name: it; MessagesFile: compiler:Languages\Italian.isl
Name: nor; MessagesFile: compiler:Languages\Norwegian.isl
Name: pol; MessagesFile: compiler:Languages\Polish.isl
Name: por; MessagesFile: compiler:Languages\Portuguese.isl

[Tasks]
Name: desktopicon; Description: {cm:CreateDesktopIcon}; GroupDescription: {cm:AdditionalIcons}; Flags: unchecked
Name: quicklaunchicon; Description: {cm:CreateQuickLaunchIcon}; GroupDescription: {cm:AdditionalIcons}; Flags: unchecked

[Files]
; Files
Source: Files\Protected\FileDb.dll; DestDir: {app}; Flags: replacesameversion
Source: Files\FileDb.xml; DestDir: {app};

Source: Files\Protected\FileDbPcl.dll; DestDir: {app}; Flags: replacesameversion
Source: Files\FileDbPcl.xml; DestDir: {app};

;Source: Files\Protected\FileDbRt.dll; DestDir: {app}; Flags: replacesameversion
;Source: Files\FileDbRt.xml; DestDir: {app};

Source: Files\Protected\FileDbExplorer.exe; DestDir: {app}; Flags: replacesameversion
Source: Files\FileDbExplorer.exe.config; DestDir: {app}
Source: Files\TenTec.Windows.iGridLib.iGrid.v3.0.dll; DestDir: {app}
Source: Files\website.url; DestDir: {app}
Source: Files\Help.html; DestDir: {app}

; Samples
;WPF
Source: Files\Samples\WPF\*; DestDir: {app}\Samples\WPF
Source: Files\Samples\WPF\Properties\*; DestDir: {app}\Samples\WPF\Properties
;WinForms
Source: Files\Samples\WinForms\*; DestDir: {app}\Samples\WinForms
Source: Files\Samples\WinForms\Properties\*; DestDir: {app}\Samples\WinForms\Properties
;WinPhoneRT
Source: Files\Samples\WindowsPhoneRT\*; DestDir: {app}\Samples\WindowsPhoneRT
Source: Files\Samples\WindowsPhoneRT\Properties\*; DestDir: {app}\Samples\WindowsPhoneRT\Properties
Source: Files\Samples\WindowsPhoneRT\Assets\*; DestDir: {app}\Samples\WindowsPhoneRT\Assets
;WinPhoneSL
Source: Files\Samples\WindowsPhoneSL\*; DestDir: {app}\Samples\WindowsPhoneSL
Source: Files\Samples\WindowsPhoneSL\Properties\*; DestDir: {app}\Samples\WindowsPhoneSL\Properties
Source: Files\Samples\WindowsPhoneSL\Assets\*; DestDir: {app}\Samples\WindowsPhoneSL\Assets
Source: Files\Samples\WindowsPhoneSL\Assets\Tiles\*; DestDir: {app}\Samples\WindowsPhoneSL\Assets\Tiles
Source: Files\Samples\WindowsPhoneSL\Resources\*; DestDir: {app}\Samples\WindowsPhoneSL\Resources
;Shared database
Source: Files\Samples\Northwind Database\*; DestDir: {app}\Samples\Northwind Database

; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: {group}\Run {#APP_NAME}; Filename: {app}\FileDbExplorer.exe; WorkingDir: {app}\Explorer
Name: {group}\Uninstall {#APP_NAME}; Filename: {app}\unins000.exe
Name: {commondesktop}\{#APP_NAME}; Filename: {app}\FileDbExplorer.exe; Tasks: desktopicon
Name: {userappdata}\Microsoft\Internet Explorer\Quick Launch\{#APP_NAME}; Filename: {app}\FileDbExplorer.exe; Tasks: quicklaunchicon
;Name: {group}\Readme; Filename: {app}\Readme.html
Name: {group}\Help; Filename: {app}\Help.html
Name: {group}\Samples\FileDb WPF Sample Project; Filename: {app}\Samples\WPF\SampleApp.csproj
Name: {group}\Samples\FileDb WinForms Sample Project; Filename: {app}\Samples\WinForms\SampleApp.csproj
Name: {group}\Samples\FileDb Windows Phone Silverlight Sample Project; Filename: {app}\Samples\WindowsPhoneSL\WindowsPhoneSL.csproj
Name: {group}\Samples\FileDb Windows Phone RT Sample Project; Filename: {app}\Samples\WindowsPhoneRT\WindowsPhoneRT.csproj
;Name: {group}\Samples\FileDb Silverlight Sample Project; Filename: {app}\Samples\Silverlight\SampleApp.csproj

[Run]
;Filename: {app}\ezview.exe; Flags: hidewizard; Parameters: /verysilent
Filename: {app}\FileDbExplorer.exe; Description: {cm:LaunchProgram,{#APP_NAME}}; Flags: nowait postinstall skipifsilent
Filename: {app}\Samples\WPF\SampleApp.csproj; Flags: nowait postinstall skipifsilent shellexec; Description: Open FileDb WPF Sample Project
Filename: {app}\Samples\WinForms\SampleApp.csproj; Flags: nowait postinstall skipifsilent shellexec; Description: Open FileDb WinForms Sample Project
Filename: {app}\Samples\WindowsPhoneSL\WindowsPhoneSL.csproj; Flags: nowait postinstall skipifsilent shellexec; Description: Open FileDb Windows Phone Silverlight Sample Project
Filename: {app}\Samples\WindowsPhoneRT\WindowsPhoneRT.csproj; Flags: nowait postinstall skipifsilent shellexec; Description: Open FileDb Windows Phone RT Sample Project
;Filename: {app}\Samples\Silverlight\SampleApp.csproj; Flags: nowait postinstall skipifsilent shellexec; Description: Open FileDb Silverlight Sample Project
;Filename: {app}\Readme.html; Flags: nowait postinstall skipifsilent shellexec unchecked; Description: Open Readme file

[Registry]
Root: HKCR; Subkey: .fdb; ValueType: string; ValueData: filedbfile; Flags: uninsdeletekey
Root: HKCR; Subkey: filedbfile; ValueType: string; ValueData: FileDb database file; Flags: uninsdeletekey
Root: HKCR; Subkey: filedbfile\shell\open\command; ValueType: string; ValueData: {app}\FileDbExplorer.exe %1

[Code]
function NextButtonClick(CurPageID: Integer): Boolean;
var
  nResultCode: Integer;
  sExe : String;
  sUninstallFile : String;
begin
	Result := True;

	case CurPageID of
		wpSelectDir:
		begin
			sExe := WizardDirValue + '\Explorer\FileDbExplorer.exe';
			sUninstallFile := WizardDirValue + '\unins000.exe';

			if fileExists(sUninstallFile) and fileExists(sExe) then
			begin
				Result := False;
				if MsgBox('You must uninstall the previous version - do it now?', mbInformation, MB_YESNO) = IDYES then
					Exec( sUninstallFile, '', '', SW_SHOW, ewWaitUntilTerminated, nResultCode);
				if not (fileExists(sUninstallFile) and fileExists(sExe)) then begin
					Result := True;
				end else
					MsgBox('You must uninstall the previous version or Cancel to exit', mbInformation, MB_OK);
			end;
		end;
	end;
end;
