FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/HelpDesk.API/HelpDesk.API.csproj", "src/HelpDesk.API/"]
COPY ["src/HelpDesk.Application/HelpDesk.Application.csproj", "src/HelpDesk.Application/"]
COPY ["src/HelpDesk.DataAccess/HelpDesk.DataAccess.csproj", "src/HelpDesk.DataAccess/"]
COPY ["src/HelpDesk.Domain/HelpDesk.Domain.csproj", "src/HelpDesk.Domain/"]
RUN dotnet restore "src/HelpDesk.API/HelpDesk.API.csproj"

COPY . .
RUN dotnet publish "src/HelpDesk.API/HelpDesk.API.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "HelpDesk.API.dll"]
