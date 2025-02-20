@echo off
:: Check if the script is running as Administrator
NET SESSION >nul 2>&1
if %errorlevel% neq 0 (
    echo This script is not running as Administrator.
    echo Relaunching as Administrator...
    :: Relaunch the script with Administrator privileges, and prevent a loop by checking the argument
    if not "%1"=="elevated" (
        start /min cmd /c "%~f0 elevated"
    )
    exit /b
)






:: If the script is running as Administrator, continue to apply registry changes

:: Apply registry changes

:: First registry entry
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\f8861c27-95e7-475c-865b-13c0cb3f9d6c" /v "Attributes" /t REG_DWORD /d 1 /f
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\f8861c27-95e7-475c-865b-13c0cb3f9d6c\253" /v "SettingValue" /t REG_DWORD /d 0 /f
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\f8861c27-95e7-475c-865b-13c0cb3f9d6c\254" /v "SettingValue" /t REG_DWORD /d 0 /f
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\f8861c27-95e7-475c-865b-13c0cb3f9d6c\255" /v "SettingValue" /t REG_DWORD /d 0 /f
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\f8861c27-95e7-475c-865b-13c0cb3f9d6c\DefaultPowerSchemeValues" /f

:: Second registry entry
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\b000397d-9b0b-483d-98c9-692a6060cfc0" /v "Attributes" /t REG_DWORD /d 1 /f
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\b000397d-9b0b-483d-98c9-692a6060cfc0\254" /v "SettingValue" /t REG_DWORD /d 0 /f
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\b000397d-9b0b-483d-98c9-692a6060cfc0\255" /v "SettingValue" /t REG_DWORD /d 0 /f
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\b000397d-9b0b-483d-98c9-692a6060cfc0\DefaultPowerSchemeValues" /f
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\b000397d-9b0b-483d-98c9-692a6060cfc0\DefaultPowerSchemeValues\381b4222-f694-41f0-9685-ff5bb260df2e" /v "ACSettingIndex" /t REG_DWORD /d 0xff /f
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\b000397d-9b0b-483d-98c9-692a6060cfc0\DefaultPowerSchemeValues\381b4222-f694-41f0-9685-ff5bb260df2e" /v "DCSettingIndex" /t REG_DWORD /d 0xff /f
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\b000397d-9b0b-483d-98c9-692a6060cfc0\DefaultPowerSchemeValues\8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c" /v "ACSettingIndex" /t REG_DWORD /d 0xff /f
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\b000397d-9b0b-483d-98c9-692a6060cfc0\DefaultPowerSchemeValues\8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c" /v "DCSettingIndex" /t REG_DWORD /d 0xff /f
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\b000397d-9b0b-483d-98c9-692a6060cfc0\DefaultPowerSchemeValues\a1841308-3541-4fab-bc81-f71556f20b4a" /v "ACSettingIndex" /t REG_DWORD /d 0xff /f
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\b000397d-9b0b-483d-98c9-692a6060cfc0\DefaultPowerSchemeValues\a1841308-3541-4fab-bc81-f71556f20b4a" /v "DCSettingIndex" /t REG_DWORD /d 0xff /f

:: Third registry entry
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\b000397d-9b0b-483d-98c9-692a6060cfbf" /v "Attributes" /t REG_DWORD /d 1 /f
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\b000397d-9b0b-483d-98c9-692a6060cfbf\253" /v "SettingValue" /t REG_DWORD /d 0 /f
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\b000397d-9b0b-483d-98c9-692a6060cfbf\254" /v "SettingValue" /t REG_DWORD /d 0 /f
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\b000397d-9b0b-483d-98c9-692a6060cfbf\255" /v "SettingValue" /t REG_DWORD /d 0 /f
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\b000397d-9b0b-483d-98c9-692a6060cfbf\DefaultPowerSchemeValues" /f
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\b000397d-9b0b-483d-98c9-692a6060cfbf\DefaultPowerSchemeValues\381b4222-f694-41f0-9685-ff5bb260df2e" /v "ACSettingIndex" /t REG_DWORD /d 0xff /f
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\b000397d-9b0b-483d-98c9-692a6060cfbf\DefaultPowerSchemeValues\381b4222-f694-41f0-9685-ff5bb260df2e" /v "DCSettingIndex" /t REG_DWORD /d 0xff /f
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\b000397d-9b0b-483d-98c9-692a6060cfbf\DefaultPowerSchemeValues\8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c" /v "ACSettingIndex" /t REG_DWORD /d 0xff /f
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\b000397d-9b0b-483d-98c9-692a6060cfbf\DefaultPowerSchemeValues\8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c" /v "DCSettingIndex" /t REG_DWORD /d 0xff /f
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\b000397d-9b0b-483d-98c9-692a6060cfbf\DefaultPowerSchemeValues\a1841308-3541-4fab-bc81-f71556f20b4a" /v "ACSettingIndex" /t REG_DWORD /d 0xff /f
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\b000397d-9b0b-483d-98c9-692a6060cfbf\DefaultPowerSchemeValues\a1841308-3541-4fab-bc81-f71556f20b4a" /v "DCSettingIndex" /t REG_DWORD /d 0xff /f

:: Apply the fourth registry entry
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\b000397d-9b0b-483d-98c9-692a6060cfbf" /v "Attributes" /t REG_DWORD /d 1 /f

reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\b000397d-9b0b-483d-98c9-692a6060cfbf\253" /v "SettingValue" /t REG_DWORD /d 0 /f
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\b000397d-9b0b-483d-98c9-692a6060cfbf\254" /v "SettingValue" /t REG_DWORD /d 0 /f
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\b000397d-9b0b-483d-98c9-692a6060cfbf\255" /v "SettingValue" /t REG_DWORD /d 0 /f

reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\b000397d-9b0b-483d-98c9-692a6060cfbf\DefaultPowerSchemeValues" /f

reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\b000397d-9b0b-483d-98c9-692a6060cfbf\DefaultPowerSchemeValues\381b4222-f694-41f0-9685-ff5bb260df2e" /v "ACSettingIndex" /t REG_DWORD /d 0xff /f
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\b000397d-9b0b-483d-98c9-692a6060cfbf\DefaultPowerSchemeValues\381b4222-f694-41f0-9685-ff5bb260df2e" /v "DCSettingIndex" /t REG_DWORD /d 0xff /f
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\b000397d-9b0b-483d-98c9-692a6060cfbf\DefaultPowerSchemeValues\381b4222-f694-41f0-9685-ff5bb260df2e" /v "ProvAcSettingIndex" /t REG_DWORD /d 0xfe /f
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\b000397d-9b0b-483d-98c9-692a6060cfbf\DefaultPowerSchemeValues\381b4222-f694-41f0-9685-ff5bb260df2e" /v "ProvDcSettingIndex" /t REG_DWORD /d 0xfe /f

reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\b000397d-9b0b-483d-98c9-692a6060cfbf\DefaultPowerSchemeValues\8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c" /v "ACSettingIndex" /t REG_DWORD /d 0xff /f
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\b000397d-9b0b-483d-98c9-692a6060cfbf\DefaultPowerSchemeValues\8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c" /v "DCSettingIndex" /t REG_DWORD /d 0xff /f
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\b000397d-9b0b-483d-98c9-692a6060cfbf\DefaultPowerSchemeValues\8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c" /v "ProvAcSettingIndex" /t REG_DWORD /d 0xfe /f
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\b000397d-9b0b-483d-98c9-692a6060cfbf\DefaultPowerSchemeValues\8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c" /v "ProvDcSettingIndex" /t REG_DWORD /d 0xfe /f

reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\b000397d-9b0b-483d-98c9-692a6060cfbf\DefaultPowerSchemeValues\a1841308-3541-4fab-bc81-f71556f20b4a" /v "ACSettingIndex" /t REG_DWORD /d 0xff /f
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\b000397d-9b0b-483d-98c9-692a6060cfbf\DefaultPowerSchemeValues\a1841308-3541-4fab-bc81-f71556f20b4a" /v "DCSettingIndex" /t REG_DWORD /d 0xff /f

reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\f8861c27-95e7-475c-865b-13c0cb3f9d6b\255" /v "SettingValue" /t REG_DWORD /d 0x1010101 /f
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\f8861c27-95e7-475c-865b-13c0cb3f9d6b\254" /v "SettingValue" /t REG_DWORD /d 0x1010101 /f

reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\f8861c27-95e7-475c-865b-13c0cb3f9d6c\255" /v "SettingValue" /t REG_DWORD /d 0x1010101 /f
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\f8861c27-95e7-475c-865b-13c0cb3f9d6c\254" /v "SettingValue" /t REG_DWORD /d 0x1010101 /f
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\f8861c27-95e7-475c-865b-13c0cb3f9d6c\253" /v "SettingValue" /t REG_DWORD /d 0x1010101 /f

:: End of script
exit /b
