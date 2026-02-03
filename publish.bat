@echo off
echo ========================================
echo   Bridgo IIS Publish
echo ========================================
echo.

cd /d "%~dp0"

echo [1/3] Temizleniyor...
if exist "publish" rmdir /s /q "publish"

echo [2/3] Build ve Publish ediliyor...
dotnet publish Bridgo.csproj -c Release -o publish --no-self-contained

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ========================================
    echo   HATA: Build basarisiz!
    echo ========================================
    pause
    exit /b 1
)

echo.
echo ========================================
echo   BASARILI!
echo   Cikti: %~dp0publish
echo ========================================
echo.
echo IIS'e deploy icin:
echo 1. IIS Manager'da site olustur
echo 2. Physical path olarak publish klasorunu sec
echo 3. Application Pool: .NET CLR Version = No Managed Code
echo.
pause
