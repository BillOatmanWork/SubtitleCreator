set drive=%~d1
set folder=%~p1
set filename=%~n1
c:
cd\comskip
comskip --ini=comskipper.ini "%drive%%folder%%filename%.ts"

REM -ffmpegpath="C:\ffmpeg-2024-08-15-git-1f801dfdb5-full_build\bin" -infile="C:\TestData\Recipe Rehab S02E25.ts" -model=medium -nomerge
