FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8060

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["FlightService.csproj", "./"]
RUN dotnet restore "FlightService.csproj"
COPY . .
RUN dotnet build "FlightService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FlightService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FlightService.dll"]