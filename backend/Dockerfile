# multi-stage build for the api. the build stage restores + publishes; the
# final stage is the slim aspnet runtime with just the published output.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy the solution + project files first and restore, so docker can cache the
# restore layer when only source code changes.
COPY global.json ./
COPY Directory.Build.props ./
COPY Threes.Registration.sln ./
COPY src/Threes.Registration.Domain/*.csproj src/Threes.Registration.Domain/
COPY src/Threes.Registration.Application/*.csproj src/Threes.Registration.Application/
COPY src/Threes.Registration.Persistence/*.csproj src/Threes.Registration.Persistence/
COPY src/Threes.Registration.Infrastructure/*.csproj src/Threes.Registration.Infrastructure/
COPY src/Threes.Registration.Api/*.csproj src/Threes.Registration.Api/
COPY tests/Threes.Registration.UnitTests/*.csproj tests/Threes.Registration.UnitTests/
COPY tests/Threes.Registration.IntegrationTests/*.csproj tests/Threes.Registration.IntegrationTests/
RUN dotnet restore src/Threes.Registration.Api/Threes.Registration.Api.csproj

# now the rest of the source, then publish.
COPY . .
RUN dotnet publish src/Threes.Registration.Api/Threes.Registration.Api.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Threes.Registration.Api.dll"]
