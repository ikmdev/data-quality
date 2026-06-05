# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# OPTIMIZATION: Copy only the project file first to cache the NuGet restore step
COPY ["PIQI_Engine.Server/PIQI_Engine.Server.csproj", "PIQI_Engine.Server/"]
COPY ["PIQI.Components/PIQI.Components.csproj", "PIQI.Components/"]
COPY ["PIQI.Data/PIQI.Data.csproj", "PIQI.Data/"]
COPY ["PIQI.CustomSAMs/PIQI.CustomSAMs.csproj", "PIQI.CustomSAMs/"]
RUN dotnet restore "PIQI_Engine.Server/PIQI_Engine.Server.csproj"
RUN dotnet restore "PIQI.CustomSAMs/PIQI.CustomSAMs.csproj"

# Copy the rest of the source code and publish
COPY . .
RUN dotnet publish "PIQI_Engine.Server/PIQI_Engine.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false
RUN dotnet publish "PIQI.CustomSAMs/PIQI.CustomSAMs.csproj" -c Release -o /app/publish/Plugins /p:UseAppHost=false /p:PublishSingleFile=false

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