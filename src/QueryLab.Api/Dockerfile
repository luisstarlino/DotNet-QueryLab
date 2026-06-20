FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/QueryLab.Api/QueryLab.Api.csproj", "src/QueryLab.Api/"]
COPY ["src/QueryLab.Domain/QueryLab.Domain.csproj", "src/QueryLab.Domain/"]
COPY ["src/QueryLab.Infra/QueryLab.Infra.csproj", "src/QueryLab.Infra/"]
RUN dotnet restore "src/QueryLab.Api/QueryLab.Api.csproj"

COPY . .
WORKDIR "/src/src/QueryLab.Api"
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080

# Npgsql tenta carregar a lib GSSAPI/Kerberos no startup; a imagem aspnet mínima
# não a inclui. Sem ela aparece o aviso "libgssapi_krb5.so.2: cannot open shared
# object file". Não é fatal com auth por senha, mas instalar elimina o ruído.
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "QueryLab.Api.dll"]
