# ---- Build ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia csproj para restaurar
COPY OhLivros/OhLivrosApp/*.csproj ./OhLivros/OhLivrosApp/
RUN dotnet restore OhLivros/OhLivrosApp/OhLivrosApp.csproj

# Copia o restante código
COPY OhLivros ./OhLivros

# Publica
WORKDIR /src/OhLivros/OhLivrosApp
RUN dotnet publish OhLivrosApp.csproj -c Release -o /app/out

# ---- Runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Render fornece a PORT
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

COPY --from=build /app/out ./

CMD ["dotnet", "OhLivrosApp.dll"]

