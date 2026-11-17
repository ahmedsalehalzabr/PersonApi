# ============================================
# Stage 1: Build
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["PersonApi.csproj", "./"]
RUN dotnet restore "PersonApi.csproj"

COPY . .
RUN dotnet publish "PersonApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ============================================
# Stage 2: Runtime
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# تم التعديل هنا: تثبيت المنفذ على 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# تم التعديل هنا: المنفذ 8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "PersonApi.dll"]