:: notice: files are taken from "C:\Program Files (x86)\DepCharter" and the sfx will also offer to extract there.

del DepCharterRelease.exe > NUL
"C:\Program Files\WinRAR\Rar.exe" a -r -ep1 -sfx -z"winrar_sfx.config" DepCharterRelease.rar "C:\Program Files (x86)\DepCharter\*.*"
pause