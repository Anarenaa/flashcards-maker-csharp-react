# 1. Runtime stage (base image)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# 2. Build stage (SDK)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files for dependency restoration (optimizes caching)
COPY ["WebAPI/WebAPI.csproj", "WebAPI/"]
COPY ["Core/Core.csproj", "Core/"]
COPY ["Services/Services.csproj", "Services/"]
COPY ["Repositories/Repositories.csproj", "Repositories/"]

RUN dotnet restore "WebAPI/WebAPI.csproj"

# Copy the rest of the source code
COPY . .

# Build the project
WORKDIR "/src/WebAPI"
RUN dotnet build "WebAPI.csproj" -c Release -o /app/build

# 3. Publish stage
FROM build AS publish
RUN dotnet publish "WebAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 4. Final image stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Run the application
ENTRYPOINT ["dotnet", "WebAPI.dll"]