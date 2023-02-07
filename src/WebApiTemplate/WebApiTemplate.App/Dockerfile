FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
EXPOSE 8558
EXPOSE 8081
EXPOSE 9110

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["WebApiTemplate.App/WebApiTemplate.App.csproj", "WebApiTemplate.App/"]
RUN dotnet restore "WebApiTemplate.App/WebApiTemplate.App.csproj"
COPY . .
WORKDIR "/src/WebApiTemplate.App"
RUN dotnet build "WebApiTemplate.App.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebApiTemplate.App.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebApiTemplate.App.dll"]
