set drive=%~d1
set folder=%~p1
set filename=%~n1
c:
cd\comskip
comskip --ini=comskipper.ini "%drive%%folder%%filename%.ts"

cd\SubtitleCreator
SubtitleCreator -ffmpegpath="C:\ffmpeg\bin" -infile="%drive%%folder%%filename%.ts" -model=medium -nomerge

