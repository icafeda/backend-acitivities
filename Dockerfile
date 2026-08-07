# ============================
# 1. Build stage
# ============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore "API/API.csproj"
RUN dotnet publish "API/API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ============================
# 2. Runtime stage
# ============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy publish output
COPY --from=build /app/publish .

# Railway injects PORT automatically
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

ENTRYPOINT ["dotnet", "API.dll"]

