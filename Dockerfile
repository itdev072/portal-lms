# Multi-stage build: compile with the full SDK, run on the smaller ASP.NET runtime image.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/Portal.Web/Portal.Web.csproj src/Portal.Web/
RUN dotnet restore src/Portal.Web/Portal.Web.csproj
COPY src/Portal.Web/ src/Portal.Web/
RUN dotnet publish src/Portal.Web/Portal.Web.csproj -c Release -o /app --no-restore

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
ENTRYPOINT ["dotnet", "Portal.Web.dll"]
