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
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "QueryLab.Api.dll"]
