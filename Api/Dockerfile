FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Packages.props ./
COPY Directory.Build.props ./
COPY global.json ./
COPY Auth.Api/Auth.Api.csproj Auth.Api/
COPY Auth.Application/Auth.Application.csproj Auth.Application/
COPY Auth.Infrastructure/Auth.Infrastructure.csproj Auth.Infrastructure/
COPY Auth.Domain/Auth.Domain.csproj Auth.Domain/
COPY Auth.Shared/Auth.Shared.csproj Auth.Shared/
RUN dotnet restore Auth.Api/Auth.Api.csproj

COPY . .
RUN dotnet publish Auth.Api/Auth.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Auth.Api.dll"]
