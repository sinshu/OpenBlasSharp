rmdir /s /q "OpenBlasSharp\lapack"
xcopy "CodeGenerator\bin\Debug\net8.0\lapack" "OpenBlasSharp\lapack" /e /i
pause
