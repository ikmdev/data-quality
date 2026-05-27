# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# OPTIMIZATION: Copy only the project file first to cache the NuGet restore step
COPY ["PIQI_Engine.Server/PIQI_Engine.Server.csproj", "PIQI_Engine.Server/"]
RUN dotnet restore "PIQI_Engine.Server/PIQI_Engine.Server.csproj"

# Copy the rest of the source code and publish
COPY . .
RUN dotnet publish "PIQI_Engine.Server/PIQI_Engine.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Run the application
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# 1. Force the ReferenceApp environment to safely expose Swagger
ENV ASPNETCORE_ENVIRONMENT=ReferenceApp

# 2. Explicitly tell .NET to listen on port 8080 to match your EXPOSE command
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "PIQI_Engine.Server.dll"]