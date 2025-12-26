FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["RealEstateApi.csproj", "."]
RUN dotnet restore "./RealEstateApi.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "RealEstateApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RealEstateApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RealEstateApi.dll"]