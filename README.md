# SubsLtrMarker
Marks Hebrew subtitles with Unicode LTR and RTL characters to view on Plex.

Usage:
Run `SubsLtrMarker '<folder>'`

The app will go over the subtitles, and for every subtitle with Hebrew characters it will:
1. Create a copyh woth a .bak extension
2. Convert the encoding from CodePage 1255 to UTF-8, to avoid Rohmbs with question marks inside
3. Add LTRE (left-to-right Embedded) mark when a line starts with puctuation marks.
4. Add RTL (right-to-left) mark if after the punctuation mark at the beginning there's a number.

It should also start monitoring the filder for added subtitle files (doens't work at the moment).