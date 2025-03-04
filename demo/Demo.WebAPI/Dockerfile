#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["demo/Demo.WebAPI/Demo.WebAPI.csproj", "demo/Demo.WebAPI/"]
RUN dotnet restore "demo/Demo.WebAPI/Demo.WebAPI.csproj"
COPY . .
WORKDIR "/src/demo/Demo.WebAPI"
RUN dotnet build "Demo.WebAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Demo.WebAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Demo.WebAPI.dll"]