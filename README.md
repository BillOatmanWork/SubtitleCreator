# SubtitleCreator
SubtitleCreator is a command line utility to generate subtitles for a video file and optionally put into a MKV container. It is written in C# with executables available for multiple operating systems.
It uses [Whisper.NET](https://github.com/sandrohanea/whisper.net) and [ffmpeg](https://ffmpeg.org) to generate the subtitles.

## Installation
- Decompress the proper file for your operating system into a folder. 
- Decompress the runtime file such that the end result is a runtime folder in the same folder as the SubtitleCreator executable.
- The ffmpeg executable must be available on your system.  It can be downloaded from [ffmpeg.org](https://ffmpeg.org/download.html)

## Parameters (Case Insensitive)
- -ffmpegpPath=Path to the ffmpeg executable.  Just the folder, the exe is assumed to be ffmpeg.exe or ffmpeg.
- -inFile=The video file the subtitles will be generated for.
- Optional: -nomerge  By default once the subtitle file is created, it is merged into a MKV container along with the video file. If this parameter is used, the MKV container will not be created and the subtitle file will not be deleted.
- Optional: -translate  If this is used, subtitles will be translated to English.  Do not use if the audio is already in English.
- Optional: -audioLanguage=<language>  The Whisper audio language detection feature has problems now.  So this should be specified if the audio is not in english. Possible values are eng = English, fra = French, spa = spanish. English is the default.
- Optional: -language=The language of the audio and therefore the subtitles. en for example is english. This is used for the naming of the subtitles file. Default is none.
- Optional: -Model=<Language Model>  Options are Small/Medium/Large.  Bigger is better quality, but also slower. Default = Medium.

## Example
For the Emby software package (and I believe also for Jellyfin and Plex) you can specify a command to be executed after a recording has completed.  
This is a windows batch file that will run SubtitleCreator to create an SRT subtitle file for the recording automatically in Emby.

```
set drive=%~d1
set folder=%~p1
set filename=%~n1
c:
cd\SubtitleCreator
SubtitleCreator -ffmpegpath="C:\ffmpeg\bin" -infile="%drive%%folder%%filename%.ts" -model=medium -nomerge
```

## Thanks to
- [@trananh1992](https://github.com/trananh1992/WinWhisper-SRT) for creating the WinWhisper-SRT project.