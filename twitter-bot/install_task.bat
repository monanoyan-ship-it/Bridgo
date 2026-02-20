@echo off
REM Corplynk Twitter Bot - Windows Task Scheduler Setup
REM Runs every hour during TR business hours (12:00-21:00 = 09:00-18:00 UTC)
REM Bot randomly decides whether to tweet, targeting ~2 tweets/day

SET BOT_DIR=%~dp0
SET PYTHON_PATH=C:\Users\Ahmet\AppData\Local\Programs\Python\Python312\python.exe

echo === Corplynk Twitter Bot Scheduler Setup ===
echo.
echo Bot directory: %BOT_DIR%
echo Python: %PYTHON_PATH%
echo.

REM Remove old tasks
schtasks /delete /tn "CorplynkTweetMorning" /f 2>nul
schtasks /delete /tn "CorplynkTweetAfternoon" /f 2>nul
schtasks /delete /tn "CorplynkTwitterBot" /f 2>nul
schtasks /delete /tn "CorplynkTwitterBotStartup" /f 2>nul
echo Old tasks removed.
echo.

REM Create daily recurring hourly task (12:00-21:00 TR time)
REM RandomDelay (PT45M = 0-45 min) handled via XML import for natural timing
schtasks /create /tn "CorplynkTwitterBot" /tr "\"%PYTHON_PATH%\" \"%BOT_DIR%bot.py\"" /sc daily /st 12:00 /ri 60 /du 09:00 /f

REM Add RandomDelay via PowerShell (schtasks doesn't support it natively)
powershell -Command "$task = Get-ScheduledTask -TaskName 'CorplynkTwitterBot'; $task.Triggers[0].RandomDelay = 'PT45M'; Set-ScheduledTask -InputObject $task"
if %ERRORLEVEL% EQU 0 (
    echo SUCCESS: Hourly task created (12:00-21:00 TR, every hour, daily)
) else (
    echo FAILED: Could not create task. Try running as Administrator.
)

echo.
echo === Setup Complete ===
echo Bot runs every hour (12:00-21:00) and randomly decides whether to tweet.
echo Target: ~2 tweets/day at unpredictable times.
echo Min 2 hours between tweets.
echo.
echo To test manually: "%PYTHON_PATH%" "%BOT_DIR%bot.py"
echo To remove task:   schtasks /delete /tn "CorplynkTwitterBot" /f
pause
