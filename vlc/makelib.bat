cls
call "%VS110COMNTOOLS%\VsDevCmd.bat"
del libvlc.def
echo EXPORTS > libvlc.def
for /f "usebackq tokens=4,* delims=_ " %%i in (`dumpbin /exports libvlc.dll`) do @if %%i==libvlc echo %%i_%%j >> libvlc.def
del libvlc.lib
lib /def:"libvlc.def" /out:"libvlc.lib" /machine:x86