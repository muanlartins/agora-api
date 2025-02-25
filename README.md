## Deploy

1. `dotnet publish "C:\Users\muanlartins\repos\agora-api" --output "C:\Users\muanlartins\repos\agora-api\bin\Release\net8.0\publish" --configuration "Release" --framework "net8.0" /p:GenerateRuntimeConfigurationFiles=true --runtime linux-x64 --self-contained False`

2. Zip bin publish file

3. Update lambda code with zip

## Dev

1. Make sure you have the environment variables set

2. `dotnet run`