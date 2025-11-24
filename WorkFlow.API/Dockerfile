FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY WorkFlow.sln .

COPY WorkFlow.API/WorkFlow.API.csproj WorkFlow.API/
COPY WorkFlow.Application/WorkFlow.Application.csproj WorkFlow.Application/
COPY WorkFlow.Infrastructure/WorkFlow.Infrastructure.csproj WorkFlow.Infrastructure/
COPY WorkFlow.Domain/WorkFlow.Domain.csproj WorkFlow.Domain/

RUN dotnet restore WorkFlow.API/WorkFlow.API.csproj

COPY . .

RUN dotnet publish WorkFlow.API/WorkFlow.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "WorkFlow.API.dll"]
