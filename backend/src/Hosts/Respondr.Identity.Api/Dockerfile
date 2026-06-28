FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "backend/src/Hosts/Respondr.Identity.Api/Respondr.Identity.Api.csproj"
RUN dotnet publish "backend/src/Hosts/Respondr.Identity.Api/Respondr.Identity.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Respondr.Identity.Api.dll"]
