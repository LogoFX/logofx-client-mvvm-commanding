cd ../
cd build
call build-all
cd ../publish
cd PublishUtil
dotnet build
cd bin
dotnet PublishUtil.dll
cd ../..