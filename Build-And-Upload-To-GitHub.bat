@echo off
setlocal
title Build Folder Sticker Printer and Upload to GitHub

REM Run this BAT file from the FolderStickerPrinter project root.
REM Expected project path: .\FolderStickerPrinter\FolderStickerPrinter.csproj

cd /d "%~dp0"

set "PROJECT=FolderStickerPrinter\FolderStickerPrinter.csproj"
set "OUTPUT=%~dp0publish"

echo.
echo ==========================================
echo   Folder Sticker Printer - Build and Push
echo ==========================================
echo.

if not exist "%PROJECT%" (
    echo ERROR: Cannot find:
    echo   %PROJECT%
    echo.
    echo Place this BAT file in the folder that contains
    echo the FolderStickerPrinter folder and .git folder.
    pause
    exit /b 1
)

where dotnet >nul 2>&1
if errorlevel 1 (
    echo ERROR: .NET SDK was not found.
    echo Install the .NET 8 SDK, then run this file again.
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

where git >nul 2>&1
if errorlevel 1 (
    echo ERROR: Git was not found.
    echo Install Git for Windows, then run this file again.
    echo https://git-scm.com/download/win
    pause
    exit /b 1
)

git rev-parse --is-inside-work-tree >nul 2>&1
if errorlevel 1 (
    echo ERROR: This folder is not inside a Git repository.
    echo Clone your GitHub repository first, then put this BAT file
    echo in the repository's top-level folder.
    pause
    exit /b 1
)

echo [1/3] Building the Windows application...
if exist "%OUTPUT%" rmdir /s /q "%OUTPUT%"

dotnet publish "%PROJECT%" -c Release -r win-x64 --self-contained true ^
  -p:PublishSingleFile=true ^
  -p:IncludeNativeLibrariesForSelfExtract=true ^
  -o "%OUTPUT%"

if errorlevel 1 (
    echo.
    echo ERROR: Build failed. Nothing has been uploaded.
    pause
    exit /b 1
)

echo.
echo [2/3] Checking Git changes...
git add -A
if errorlevel 1 (
    echo ERROR: Could not stage files with Git.
    pause
    exit /b 1
)

git diff --cached --quiet
if errorlevel 1 (
    set "HAS_CHANGES=1"
) else (
    set "HAS_CHANGES=0"
)

if "%HAS_CHANGES%"=="1" (
    set /p "COMMIT_MESSAGE=Enter a commit message (or press Enter for the default): "
    if not defined COMMIT_MESSAGE set "COMMIT_MESSAGE=Update Folder Sticker Printer"
    git commit -m "%COMMIT_MESSAGE%"
    if errorlevel 1 (
        echo ERROR: Git commit failed. Check your Git configuration.
        pause
        exit /b 1
    )
) else (
    echo No source changes to commit.
)

echo.
echo [3/3] Uploading changes to GitHub...
git push -u origin HEAD
if errorlevel 1 (
    echo.
    echo ERROR: Push failed.
    echo Check your internet connection, GitHub sign-in, and remote URL.
    echo Your local EXE was still built successfully.
    pause
    exit /b 1
)

echo.
echo ==========================================
echo SUCCESS!
echo ==========================================
echo Your Windows application was built here:
echo %OUTPUT%
echo.
echo Your source changes were pushed to GitHub.
echo If GitHub Actions is configured, it should now
echo start building the downloadable artifact.
echo.
pause
endlocal
