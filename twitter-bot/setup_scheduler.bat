@echo off
REM Corplynk Twitter Bot - Windows Task Scheduler Setup
REM Posts tweets at random times during UTC+0 business hours (09:00-18:00)
REM Turkey is UTC+3, so local times: 12:00-21:00
REM
REM This script creates 2 scheduled tasks:
REM   1. Morning tweet (random between 12:00-16:00 local = 09:00-13:00 UTC)
REM   2. Afternoon tweet (random between 16:00-21:00 local = 13:00-18:00 UTC)

SET BOT_DIR=%~dp0
SET PYTHON_PATH=python

echo === Corplynk Twitter Bot Scheduler Setup ===
echo.
echo Bot directory: %BOT_DIR%
echo.

REM Create morning task (runs at 12:30 local time = 09:30 UTC)
schtasks /create /tn "CorplynkTweetMorning" /tr "\"%PYTHON_PATH%\" \"%BOT_DIR%bot.py\"" /sc daily /st 12:30 /f
echo Morning tweet task created (12:30 local / 09:30 UTC)

REM Create afternoon task (runs at 17:30 local time = 14:30 UTC)
schtasks /create /tn "CorplynkTweetAfternoon" /tr "\"%PYTHON_PATH%\" \"%BOT_DIR%bot.py\"" /sc daily /st 17:30 /f
echo Afternoon tweet task created (17:30 local / 14:30 UTC)

echo.
echo === Setup Complete ===
echo Tasks will run daily and post one tweet each time.
echo Check bot.log for results.
echo.
echo To test manually: python "%BOT_DIR%bot.py"
echo To remove tasks: schtasks /delete /tn "CorplynkTweetMorning" /f
echo                   schtasks /delete /tn "CorplynkTweetAfternoon" /f
pause
