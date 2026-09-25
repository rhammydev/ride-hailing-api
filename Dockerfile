FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Ride-Hailing-API.csproj", "./"]
RUN dotnet restore "Ride-Hailing-API.csproj"
COPY . .
RUN dotnet publish "Ride-Hailing-API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:${PORT}
ENTRYPOINT ["dotnet", "Ride-Hailing-API.dll"]
