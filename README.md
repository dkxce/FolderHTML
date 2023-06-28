# FolderHTML Directory Tree View

Folder Tree view in Console, Txt, HTML    

**USAGE**:
> folderhtml [flags] [<path> [line] [line] ... [line]]

**SAMPLE**:     
> folderhtml  -s -m -c -a -T -H -e=filelist.log      

**CONSOLE RESULT**:    
```cs
*
* CmdL: folderhtml  -s -m -c -a -T -H -e=filelist.log 
* Path: ...
* TEXT: filelist.txt, open: True
* HTML: filelist.html, open: True
* Files: 3
*
Created at 28.06.2023 12:40:33 by (c) FolderHTML
|
| FolderHTML.exe         14.5 KB (28.06.2023 12:40:26) [27.06.2023 16:42:56] {Archive}
| test.cmd            85 B (28.06.2023 12:36:54) [28.06.2023 12:30:28] {Archive}
|
```
**TEXT RESULT**:    
```cs
Created at 28.06.2023 12:40:33 by (c) FolderHTML
|
|   FolderHTML.exe                       14.5 KB (28.06.2023 12:40:26) [27.06.2023 16:42:56] {Archive}
|   test.cmd                                85 B (28.06.2023 12:36:54) [28.06.2023 12:30:28] {Archive}
|
```
**HTML RESULT**:    
<TT>Created at 28.06.2023 12:40:33 <span style="color:silver;">by &copy; <b>FolderHTML</b></span></TT><br/>
<TT>|</TT><br/>
<TT>|&nbsp;&nbsp; <a href="FolderHTML.exe">FolderHTML.exe</a>&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;&nbsp; <b>&nbsp; &nbsp; &nbsp; 14.5 KB</b> (28.06.2023 12:40:26) [27.06.2023 16:42:56] {Archive}</TT><br/>
<TT>|&nbsp;&nbsp; <a href="test.cmd">test.cmd</a>&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;&nbsp; <b>&nbsp; &nbsp; &nbsp; &nbsp;&nbsp; 85 B</b> (28.06.2023 12:36:54) [28.06.2023 12:30:28] {Archive}</TT><br/>
<TT>|</TT><br/>


**HELP**:
```batch
Usage:
          > folderhtml [flags] [<path> [line] [line] ... [line]]

Flags:
  -?      - Help
  -s      - Add File Sizes
  -m      - Add File Modified
  -c      - Add File Created
  -a      - Add File Attributes
  -w      - Wait on done
  -h      - Write Out HTML file filelist.html
  -H      - Write Out HTML file filelist.html and open it
  -t      - Write Out TEXT file filelist.html
  -T      - Write Out TEXT file filelist.html and open it
  -d=..   - File Names Space Alignment (0..99) default = 30
  -H=...  - Write Out custom HTML file filelist.html
  -H=...  - Write Out custom HTML file filelist.html and open it
  -t=...  - Write Out custom TEXT file filelist.html
  -T=...  - Write Out custom TEXT file filelist.html and open it
  -e=...  - Exclude file(s) with name(s)

Example:
          > folderhtml
          > folderhtml -s -m -c -a -d=50
          > folderhtml -h %CD% "DEMO LINE"
          > folderhtml -t -H %CD% "LINE 1" "LINE 2"
```
