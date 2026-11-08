@echo off
echo ========================================================
echo   Publishing eBike Backend (Self-Contained for IIS)
echo ========================================================
cd /d "%~dp0"

dotnet publish src/eBike.API/eBike.API.csproj -c Release -r win-x64 --self-contained true -o ./publish-win

if %ERRORLEVEL% EQU 0 (
    if exist "src\eBike.API\appsettings.Production.json" (
        copy /y "src\eBike.API\appsettings.Production.json" "publish-win\appsettings.json" >nul
        echo [OK] Production database settings applied to publish-win\appsettings.json
    )
    echo.
    echo [SUCCESS] Project successfully published to ./publish-win folder!
) else (
    echo.
    echo [FAILED] Publish failed with errors.
)
pause
