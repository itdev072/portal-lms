# Stage 1: build the React (Vite) front end.
FROM node:22-slim AS client-build
WORKDIR /client
COPY client/package*.json ./
RUN npm install
COPY client/ ./
RUN npm run build

# Stage 2: build the ASP.NET Core API, with the React build copied into wwwroot so
# the API serves the SPA itself — one container, one Render service, no CORS.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/Portal.Api/Portal.Api.csproj src/Portal.Api/
RUN dotnet restore src/Portal.Api/Portal.Api.csproj
COPY src/Portal.Api/ src/Portal.Api/
COPY --from=client-build /client/dist/ src/Portal.Api/wwwroot/
RUN dotnet publish src/Portal.Api/Portal.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# Render sets $PORT at runtime and routes external traffic to it.
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

# App_Data (the JSON-file data store) lives under the working directory and is
# reseeded automatically if missing, so no volume is required to start —
# see README.md for the caveat about this on Render's free tier (the
# filesystem is not persistent there, so data resets on restart/redeploy).
ENTRYPOINT ["dotnet", "Portal.Api.dll"]
