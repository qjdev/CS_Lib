@echo ±‡“Îlog4net.Wrap

@cd log4net.Wrap
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe log4net.Wrap.sln /t:Rebuild /property:Configuration=Release /l:FileLogger,Microsoft.Build.Engine;logfile=log4net.Wrap.log
@echo Close notepad to continue...
@if errorlevel 1 @notepad log4net.Wrap.log
@cd ..\