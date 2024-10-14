# TabDeb API

## How to publish

1. dotnet publish "C:\Users\muanl\repos\TabDeb-API" --output "C:\Users\muanl\repos\TabDeb-API\bin\Release\net8.0\publish" --configuration "Release" --framework "net8.0" /p:GenerateRuntimeConfigurationFiles=true --runtime linux-x64 --self-contained False

2. Zip bin publish file

3. Update lambda code with zip