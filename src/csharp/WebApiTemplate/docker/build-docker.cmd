@echo off
dotnet publish ../src/WebApiTemplate.App/WebApiTemplate.App.csproj --os linux --arch x64 -c Release -p:PublishProfile=DefaultContainer