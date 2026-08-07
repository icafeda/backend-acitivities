# ============================
# 1. Build stage
# ============================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution
COPY . .

# Restore dependencies
RUN dotnet restore "API/API.csproj"

# Publish in Release mode
RUN dotnet publish "API/API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ============================
# 2. Runtime stage
# ============================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Railway automatically maps port 8080 → no need for $PORT
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

ENTRYPOINT ["dotnet", "API.dll"]
