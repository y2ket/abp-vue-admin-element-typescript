@echo off
cls
chcp 65001

echo. 启动网关管理服务

cd .\apigateway\LINGYUN.ApiGateway.HttpApi.Host

if '%1' equ '--publish' goto publish
if '%1' equ '--run' goto run
if '%1' equ '--restore' goto restore
if '%1' equ '' goto run


:publish
dotnet publish -c Release -o ..\..\Publish\apigateway-admin --no-cache --no-restore
copy Dockerfile ..\..\Publish\apigateway-admin\Dockerfile
exit

:run
dotnet run 
exit

:restore
dotnet restore
exit