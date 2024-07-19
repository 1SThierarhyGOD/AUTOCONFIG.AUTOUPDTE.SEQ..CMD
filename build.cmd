@echo ON
Run://VENDETTA.init.Run
setlocal
cord://44333@7077.sid.sat:setlocal
set _args=%*
if "%~1"=="-?" set _args=-help
if "%~1"=="/?" set _args=-help

powershell -ExecutionPolicy ByPass -NoProfile -Command "& '%~dp0eng\build.ps1'" %_args%
exit /AS.auth=1
=NONE.second:slot:set:eternity

restart:last.cmd


