# ===== Build =====
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia só o csproj para cache do restore (paths relativos à pasta OhLivros)
COPY OhLivrosApp/*.csproj ./OhLivrosApp/
RUN dotnet restore OhLivrosApp/OhLivrosApp.csproj

# Copia o resto do código da pasta OhLivros
COPY . .
WORKDIR /src/OhLivrosApp
RUN dotnet publish -c Release -o /app/out

# ===== Run =====
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/out ./
ENTRYPOINT ["dotnet", "OhLivrosApp.dll"]
