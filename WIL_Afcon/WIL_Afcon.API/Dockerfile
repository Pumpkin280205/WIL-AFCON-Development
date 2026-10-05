# Container image for WIL_Afcon.API (hosted on Render).
# Build context is the WIL_Afcon/ solution folder - see render.yaml.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first so the package layer is cached between builds
COPY WIL_Afcon.API/WIL_Afcon.API.csproj WIL_Afcon.API/
RUN dotnet restore WIL_Afcon.API/WIL_Afcon.API.csproj

COPY WIL_Afcon.API/ WIL_Afcon.API/
RUN dotnet publish WIL_Afcon.API/WIL_Afcon.API.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# Render routes traffic to the port in the PORT env var (set to 8080 in render.yaml)
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# Run as the non-root user that ships with the .NET images
USER app

ENTRYPOINT ["dotnet", "WIL_Afcon.API.dll"]
