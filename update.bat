rmdir /s /q "OpenBlasSharp\lapack"
xcopy "CodeGenerator\bin\Debug\net8.0\lapack" "OpenBlasSharp\lapack" /e /i
rmdir /s /q "OpenBlasSharp\blas"
xcopy "CodeGenerator\bin\Debug\net8.0\blas" "OpenBlasSharp\blas" /e /i
pause
