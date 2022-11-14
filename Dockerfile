FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["GudSafe.WebApp/GudSafe.WebApp.csproj", "GudSafe.WebApp/"]
RUN dotnet restore "GudSafe.WebApp/GudSafe.WebApp.csproj"
COPY . .
WORKDIR "/src/GudSafe.WebApp"
RUN dotnet build "GudSafe.WebApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GudSafe.WebApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GudSafe.WebApp.dll"]
