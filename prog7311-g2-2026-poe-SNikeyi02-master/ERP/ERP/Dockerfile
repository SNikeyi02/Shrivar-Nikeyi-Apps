# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

# SDK image for compiling the application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["ERP/ERP.csproj", "ERP/"]
COPY ["ERP.Api/ERP.Api.csproj", "ERP.Api/"]

# Restore dependencies
RUN dotnet restore "ERP/ERP.csproj"

# Copy the entire source tree and build the frontend
COPY . .
WORKDIR "/src/ERP"
RUN dotnet build "ERP.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ERP.csproj" -c Release -o /app/publish /p:UseAppHost=false /p:ErrorOnDuplicatePublishOutputFiles=false

# Final production stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ERP.dll"]