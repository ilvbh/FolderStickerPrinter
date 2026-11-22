@echo off
setlocal
title Upload FolderStickerPrinter to GitHub

REM Run this file from anywhere; it always works from its own folder.
cd /d "%~dp0"

echo.
echo ==========================================
echo   Upload latest files to GitHub
echo ==========================================
echo.

REM Check that Git is installed.
where git >nul 2>&1
if errorlevel 1 (
    echo ERROR: Git was not found. Install Git for Windows first.
    pause
    exit /b 1
)

REM Check that this folder is inside the intended local Git repository.
git rev-parse --is-inside-work-tree >nul 2>&1
if errorlevel 1 (
    echo ERROR: This BAT file must be placed inside your cloned Git repository.
    pause
    exit /b 1
)

REM Confirm that the origin remote exists.
git remote get-url origin >nul 2>&1
if errorlevel 1 (
    echo ERROR: No Git remote named "origin" is configured.
    echo Check the repository with: git remote -v
    pause
    exit /b 1
)

REM Get the currently checked-out branch. Do not push from detached HEAD.
for /f "delims=" %%B in ('git branch --show-current') do set "BRANCH=%%B"
if not defined BRANCH (
    echo ERROR: Git is in detached-HEAD state. Check out a branch first.
    pause
    exit /b 1
)

echo Repository:
git remote get-url origin
echo Branch: %BRANCH%
echo.

REM Stage all additions, modifications, and deletions.
git add -A
if errorlevel 1 (
    echo ERROR: Could not stage changes.
    pause
    exit /b 1
)

REM Avoid creating an empty commit when nothing has changed.
git diff --cached --quiet
if not errorlevel 1 (
    echo No new changes to upload. Your local files are already committed.
    pause
    exit /b 0
)

echo Creating commit...
git commit -m "Update project files"
if errorlevel 1 (
    echo ERROR: Commit failed. Check Git's message above.
    pause
    exit /b 1
)

echo.
echo Pushing to GitHub...
git push -u origin "%BRANCH%"
if errorlevel 1 (
    echo.
    echo ERROR: Push failed.
    echo Check your internet connection, GitHub sign-in, and whether
    echo the remote branch has changes that you need to pull first.
    pause
    exit /b 1
)

echo.
echo SUCCESS: Latest committed files were uploaded to GitHub.
pause
exit /b 0
