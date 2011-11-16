rem
rem  You can simplify development by updating this batch file and then calling it from the 
rem  project's post-build event.
rem
rem  It copies the output .DLL (and .PDB) to LINQPad's drivers folder, so that LINQPad
rem  picks up the drivers immediately (without needing to click 'Add Driver').
rem
rem  NB: The target directory may not be correct for your computer!
rem  You can obtain the first part of the directory by running the following query:
rem
rem    Path.Combine (
rem       Environment.GetFolderPath (Environment.SpecialFolder.CommonApplicationData),
rem       @"LINQPad\Drivers\DataContext\3.5\")   
rem
rem  The final part of the directory is the name of the assembly plus its public key token in brackets.

xcopy /i/y FileDb.dll "c:\ProgramData\LINQPad\Drivers\DataContext\3.5\FileDbDynamicDriver (9f29ae367fa08336)\"
xcopy /i/y FileDbDynamicDriver.dll "c:\ProgramData\LINQPad\Drivers\DataContext\3.5\FileDbDynamicDriver (9f29ae367fa08336)\"
xcopy /i/y FileDbDynamicDriver.pdb "c:\ProgramData\LINQPad\Drivers\DataContext\3.5\FileDbDynamicDriver (9f29ae367fa08336)\"

pause