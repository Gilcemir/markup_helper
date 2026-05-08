@echo off
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0docformatter-update.ps1" %*
