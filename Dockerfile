FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PhysEdJournal.Core", "PhysEdJournal.Core/"]
COPY ["PhysEdJournal.Infrastructure", "PhysEdJournal.Infrastructure/"]
COPY ["PhysEdJournal.Api/PhysEdJournal.Api.csproj", "PhysEdJournal.Api/"]
RUN dotnet restore "PhysEdJournal.Api/PhysEdJournal.Api.csproj"
COPY . .
WORKDIR "/src/PhysEdJournal.Api"
RUN dotnet build "PhysEdJournal.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PhysEdJournal.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PhysEdJournal.Api.dll"]
