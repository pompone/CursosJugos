FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS build
WORKDIR /src

COPY ["GestionFormacion/GestionFormacion.csproj", "GestionFormacion/"]
RUN dotnet restore "GestionFormacion/GestionFormacion.csproj"

COPY . .

WORKDIR "/src/GestionFormacion"
RUN dotnet publish "GestionFormacion.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "GestionFormacion.dll"]
