# ============================
# 1. Build stage
# ============================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy toàn bộ solution
COPY . .

# Restore dependencies
RUN dotnet restore "API/API.csproj"

# Publish ở chế độ Release
RUN dotnet publish "API/API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ============================
# 2. Runtime stage
# ============================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Fix lỗi PostgreSQL SSL: libgssapi_krb5.so.2 missing
RUN apt-get update && apt-get install -y \
    libkrb5-3 \
    libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

# Copy từ build stage
COPY --from=build /app/publish .

# Render sẽ tự inject biến PORT
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

# Chạy API
ENTRYPOINT ["dotnet", "API.dll"]
