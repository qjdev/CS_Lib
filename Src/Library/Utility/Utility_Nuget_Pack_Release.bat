@echo Nuget´ò°üUtility

@cd Utility\Utility
nuget pack Utility.csproj -Prop Configuration=Release
@cd ..\..\