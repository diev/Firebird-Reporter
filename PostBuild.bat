@echo off
rem For .NET Framework 4.8
setlocal
rem for %%i in (.) do set project=%%~nxi
set project=FireReporter
rem set release=Debug
set release=Release
set ymd=%date:~-4%-%date:~3,2%-%date:~0,2%

set packer="C:\Program Files\7-Zip\7z.exe"

set bin="%project%_%ymd%.zip"
set src="%project%_%ymd%_src.zip"
rem set pak="%project%_packages.zip"

if exist %bin% del %bin%
if exist %src% del %src%
rem if exist %pak% del %pak%

rem %packer% a %bin% .\%project%\bin\%release%\* -x!*.pdb
%packer% a %bin% .\%project%\bin\%release%\*.exe
%packer% a %bin% .\%project%\bin\%release%\*.exe.config
%packer% a %bin% .\%project%\bin\%release%\*.dll
%packer% a %bin% .\%project%\bin\%release%\*.xml
rem %packer% a %pak% packages\

set packer=%packer% a %src% -xr!bin -xr!obj

rem Append sources folders to pack with the %project%, next to it (shift using)
call :pack %project%
%packer% *.sln packages
endlocal
goto :eof

:pack
if /%1/ == // goto :eof
echo Pack %1
%packer% -r %1\*.cs %1\*.resx
%packer% %1\*.csproj %1\*.config %1\*.json
shift
goto pack
