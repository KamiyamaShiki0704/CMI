@echo off
pushd "%~dp0..\..\dist"
start /B powershell -WindowStyle Hidden -Command "Add-Type -Path './mod/CMI.dll'; [CMI.CMI]::Main()"
popd
