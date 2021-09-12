SET package_version=2.2.2
cd ../../src
nuget restore
cd ../devops
PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& './build/build-all.ps1'"
cd ./publish
cd PublishUtil
dotnet build
cd bin
dotnet PublishUtil.dll %package_version%
cd ../..
cd ../install
call uninstall-global-all.bat %package_version%
cd ..
