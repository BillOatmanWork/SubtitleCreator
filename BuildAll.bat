dotnet publish -r win-x64 -c Release -p:PublishReadyToRun=true -p:PublishSingleFile=true --self-contained true -p:IncludeNativeLibrariesForSelfExtract=true
dotnet publish -r osx-x64 -c Release /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true --self-contained
dotnet publish -r linux-arm -c Release /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true --self-contained
dotnet publish -r linux-x64 -c Release /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true --self-contained

cd \repos\SubtitleCreator\Build

copy /Y "C:\repos\SubtitleCreator\bin\Release\net8.0\win-x64\publish\SubtitleCreator.exe" .
"C:\Program Files\7-Zip\7z" a -tzip SubtitleCreator-WIN.zip SubtitleCreator.exe SubtitleCreator.pdf

copy /Y "C:\repos\SubtitleCreator\bin\Release\net8.0\osx-x64\publish\SubtitleCreator" .
"C:\Program Files\7-Zip\7z" a -t7z SubtitleCreator-OSX.7z SubtitleCreator SubtitleCreator.pdf

copy /Y "C:\repos\SubtitleCreator\bin\Release\net8.0\linux-arm\publish\SubtitleCreator" .
"C:\Program Files\7-Zip\7z" a -t7z SubtitleCreator-RasPi-ARM.7z SubtitleCreator SubtitleCreator.pdf

copy /Y "C:\repos\SubtitleCreator\bin\Release\net8.0\linux-x64\publish\SubtitleCreator" .
"C:\Program Files\7-Zip\7z" a -t7z SubtitleCreator-LIN64.7z SubtitleCreator SubtitleCreator.pdf
