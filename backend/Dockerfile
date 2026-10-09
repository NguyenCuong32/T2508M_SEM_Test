FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY BattleGame.Functions/BattleGame.Functions.csproj BattleGame.Functions/
RUN dotnet restore BattleGame.Functions/BattleGame.Functions.csproj
COPY BattleGame.Functions/ BattleGame.Functions/
RUN dotnet publish BattleGame.Functions/BattleGame.Functions.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated10.0
WORKDIR /home/site/wwwroot
COPY --from=build /app/publish .
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true
EXPOSE 80
