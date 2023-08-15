FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/TinyAdmin.Web/TinyAdmin.Web.csproj", "src/TinyAdmin.Web/"]
RUN dotnet restore "src/TinyAdmin.Web/TinyAdmin.Web.csproj"
COPY . .
WORKDIR "/src/src/TinyAdmin.Web"
RUN dotnet build "TinyAdmin.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TinyAdmin.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TinyAdmin.Web.dll"]
