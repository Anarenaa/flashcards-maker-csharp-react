# 1. Етап рантайму (базовий образ)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
# Render зазвичай використовує 8080 для .NET контейнерів
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# 2. Етап збірки (SDK)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копіюємо файли проєктів для відновлення пакетів (це прискорює збірку завдяки кешуванню)
COPY ["App/App.csproj", "App/"]
COPY ["Core/Core.csproj", "Core/"]
COPY ["Services/Services.csproj", "Services/"]
COPY ["Repositories/Repositories.csproj", "Repositories/"]

RUN dotnet restore "App/App.csproj"

# Копіюємо весь інший код
COPY . .

# Збірка проєкту
WORKDIR "/src/App"
RUN dotnet build "App.csproj" -c Release -o /app/build

# 3. Етап публікації
FROM build AS publish
RUN dotnet publish "App.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 4. Фінальний образ
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Запуск застосунку
ENTRYPOINT ["dotnet", "App.dll"]