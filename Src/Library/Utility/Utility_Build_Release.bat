@echo ±‡“ÎUtility

@cd Utility
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe Utility.sln /t:Rebuild /property:Configuration=Release /l:FileLogger,Microsoft.Build.Engine;logfile=Utility.log
@echo Close notepad to continue...
@if errorlevel 1 @notepad Utility.log
@cd ..\