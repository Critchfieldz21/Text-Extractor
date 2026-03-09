FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["Backend/TextExtractorBackend/TextExtractorBackend.csproj", "Backend/TextExtractorBackend/"]

# Restore dependencies
RUN dotnet restore "Backend/TextExtractorBackend/TextExtractorBackend.csproj"

# Copy all source code
COPY . .

# Build the project
WORKDIR "/src/Backend/TextExtractorBackend"
RUN dotnet build "TextExtractorBackend.csproj" -c Release -o /app/build

# Publish the project
FROM build AS publish
RUN dotnet publish "TextExtractorBackend.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0

# Install required system dependencies for PDF processing
RUN apt-get update && apt-get install -y \
    libgomp1 \
    libopenblas0 \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=publish /app/publish .

# Create data directory for SQLite
RUN mkdir -p /app/data

EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_EnableDiagnostics=0

ENTRYPOINT ["dotnet", "TextExtractorBackend.dll"]
