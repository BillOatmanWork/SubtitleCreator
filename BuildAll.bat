dotnet publish -r win-x64 -c Release -p:PublishReadyToRun=true -p:PublishSingleFile=true --self-contained true -p:IncludeNativeLibrariesForSelfExtract=true
dotnet publish -r osx.10.14-x64 -c Release /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true --self-contained
dotnet publish -r ubuntu.18.04-x64 -c Release /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true --self-contained
dotnet publish -r linux-x64 -c Release /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true --self-contained

cd \repos\SubtitleCreator\Build

copy /Y "C:\repos\SubtitleCreator\bin\Release\net8.0\win-x64\publish\SubtitleCreator.exe" .
"C:\Program Files\7-Zip\7z" a -tzip SubtitleCreator-WIN.zip SubtitleCreator.exe

copy /Y "C:\repos\SubtitleCreator\bin\Release\net8.0\osx.10.14-x64\publish\SubtitleCreator" .
"C:\Program Files\7-Zip\7z" a -t7z SubtitleCreator-OSX.7z SubtitleCreator

copy /Y "C:\repos\SubtitleCreator\bin\Release\net8.0\ubuntu.18.04-x64\publish\SubtitleCreator" .
"C:\Program Files\7-Zip\7z" a -t7z SubtitleCreator-UBU.7z SubtitleCreator

copy /Y "C:\repos\SubtitleCreator\bin\Release\net8.0\linux-x64\publish\SubtitleCreator" .
"C:\Program Files\7-Zip\7z" a -t7z SubtitleCreator-LIN64.7z SubtitleCreator
