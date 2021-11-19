@echo off
dotnet publish -o "D:\Programming\VS 2019\BepInExInstaller\BepInExInstaller\bin\Release\any" -r any --self-contained true
dotnet publish -o "D:\Programming\VS 2019\BepInExInstaller\BepInExInstaller\bin\Release\linux64" -r linux-x64 --self-contained true
dotnet publish -o "D:\Programming\VS 2019\BepInExInstaller\BepInExInstaller\bin\Release\linuxarm64" -r linux-arm64 --self-contained true
dotnet publish -o "D:\Programming\VS 2019\BepInExInstaller\BepInExInstaller\bin\Release\osx" -r osx-x64 --self-contained true