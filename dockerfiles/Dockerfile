# Dockerfile para a aplicação SkillBridge API
# Uso: docker build -t skillbridge-api -f dockerfiles/Dockerfile .

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SkillBridge.API/SkillBridge.API.csproj", "SkillBridge.API/"]
RUN dotnet restore "SkillBridge.API/SkillBridge.API.csproj"
COPY . .
WORKDIR "/src/SkillBridge.API"
RUN dotnet build "SkillBridge.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SkillBridge.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SkillBridge.API.dll"]

