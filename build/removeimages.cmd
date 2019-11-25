SET nameId=""

for /f %%i in ('docker images -q piraeus-silo:v2.0') DO SET nameId=%%i

ECHO %nameId%

IF %nameId% == "" (
    ECHO not found
) ELSE (
    ECHO found
    SET nameId=""
)

CALL :func "piraeus-silo"
CALL :func "piraeus-mgmt-api"
CALL :remove "piraeus-websocket-gateway"
CALL :remove "piraeus-tcp-gateway"
CALL :remove "piraeus-udp-gateway"
CALL :remove "piraeus-http-gateway"
CALL :remove "piraeus-silo:v3.0"
CALL :remove "piraeus-mgmt-api:v3.0"
CALL :remove "piraeus-websocket-gateway:v3.0"
CALL :remove "piraeus-tcp-gateway:v3.0"
CALL :remove "piraeus-udp-gateway:v3.0"
CALL :remove "piraeus-http-gateway:v3.0"
CALL :remove "skunklab/piraeus-silo:v3.0"
CALL :remove "skunklab/piraeus-mgmt-api:v3.0"
CALL :remove "skunklab/piraeus-websocket-gateway:v3.0"
CALL :remove "skunklab/piraeus-tcp-gateway:v3.0"
CALL :remove "skunklab/piraeus-udp-gateway:v3.0"
CALL :remove "skunklab/piraeus-http-gateway:v3.0"

ECHO "done"
EXIT /B %ERRORLEVEL%

:remove
SET nameId=""
for /f %%i in ('docker images -q "%~1"') DO SET nameId=%%i

ECHO " %nameId% "

IF %nameId% == "" (
    ECHO not found
) ELSE (
    ECHO removing image "%~1"
    docker rmi %~1
    SET nameId=""
    SET image=""
)
EXIT /B 0
