# Container image for the WIL_Afcon MVC web app (hosted on Render).
# Build context is the WIL_Afcon/ solution folder - see render.yaml.

# ---- Stage 1: build Tailwind + daisyUI CSS (same command as CI) ----
FROM node:22 AS css
WORKDIR /src
COPY WIL_Afcon/package.json WIL_Afcon/package-lock.json ./
RUN npm ci
# Tailwind scans the Razor views for class names, so copy the whole project
COPY WIL_Afcon/ ./
RUN npx @tailwindcss/cli -i ./wwwroot/css/input.css -o ./wwwroot/css/output.css --minify

# ---- Stage 2: build and publish the .NET app ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY WIL_Afcon/WIL_Afcon.csproj WIL_Afcon/
RUN dotnet restore WIL_Afcon/WIL_Afcon.csproj

COPY WIL_Afcon/ WIL_Afcon/
COPY --from=css /src/wwwroot/css/output.css WIL_Afcon/wwwroot/css/output.css
RUN dotnet publish WIL_Afcon/WIL_Afcon.csproj -c Release -o /app --no-restore

# ---- Stage 3: runtime image ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# Render routes traffic to the port in the PORT env var (set to 8080 in render.yaml)
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# Run as the non-root user that ships with the .NET images
USER app

ENTRYPOINT ["dotnet", "WIL_Afcon.dll"]
