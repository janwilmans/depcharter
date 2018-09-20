:: notice: files are taken from "C:\Program Files (x86)\DepCharter" so winrar will also offer to extract there.

del DepCharterRelease.exe > NUL
rem not installing in the correct default directory, and not opening the install dir
"C:\Program Files\WinRAR\Rar.exe" a -r -sfx -z"winrar_sfx.config" DepCharterRelease.rar "C:\Program Files (x86)\DepCharter\*.*"
pause