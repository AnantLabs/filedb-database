rem
rem  You can simplify development by updating this batch file and then calling it from the 
rem  project's post-build event.
rem
rem  It copies the output .DLL (and .PDB) to LINQPad's drivers folder, so that LINQPad
rem  picks up the drivers immediately (without needing to click 'Add Driver').
rem
rem  The final part of the directory is the name of the assembly plus its public key token in brackets.
rem To get the public key token, use C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools\sn.exe -Tp $(TargetPath)

xcopy /i/y FileDb.dll "C:\ProgramData\LINQPad\Drivers\DataContext\4.0\FileDbDynamicDriver (9f29ae367fa08336)\"
xcopy /i/y FileDbDynamicDriver.dll "C:\ProgramData\LINQPad\Drivers\DataContext\4.0\FileDbDynamicDriver (9f29ae367fa08336)\"
xcopy /i/y FileDbDynamicDriver.pdb "C:\ProgramData\LINQPad\Drivers\DataContext\4.0\FileDbDynamicDriver (9f29ae367fa08336)\"

pause
