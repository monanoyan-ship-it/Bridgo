@echo off
REM Corplynk Twitter Bot - Windows Task Scheduler Setup
REM Runs every hour during TR business hours (12:00-21:00 = 09:00-18:00 UTC)
REM Bot randomly decides whether to tweet each hour (~2 tweets/day)
REM This makes tweet times unpredictable and natural-looking

SET BOT_DIR=%~dp0
SET PYTHON_PATH=python

echo === Corplynk Twitter Bot Scheduler Setup ===
echo.
echo Bot directory: %BOT_DIR%
echo.

REM Remove old fixed-time tasks if they exist
schtasks /delete /tn "CorplynkTweetMorning" /f 2>nul
schtasks /delete /tn "CorplynkTweetAfternoon" /f 2>nul
echo Old fixed-time tasks removed (if any).
echo.

REM Create hourly task: runs every 1 hour from 12:00 to 20:00 (TR time)
REM Bot internally checks UTC business hours and randomly decides to tweet
schtasks /create /tn "CorplynkTwitterBot" /tr "\"%PYTHON_PATH%\" \"%BOT_DIR%bot.py\"" /sc hourly /mo 1 /st 12:00 /et 21:00 /f
echo Hourly task created (12:00-21:00 TR, every hour)

echo.
echo === Setup Complete ===
echo Bot runs every hour and randomly decides whether to tweet.
echo Target: ~2 tweets/day at unpredictable times.
echo Min 2 hours between tweets.
echo.
echo Check bot.log for results.
echo.
echo To test manually: python "%BOT_DIR%bot.py"
echo To remove task:   schtasks /delete /tn "CorplynkTwitterBot" /f
pause
