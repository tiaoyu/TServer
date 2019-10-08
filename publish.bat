dotnet publish -c Debug TServer -o Publish
dotnet publish -c Debug TServer -o Publish
xcopy Envrionment\* Publish /S /E /Y /D
