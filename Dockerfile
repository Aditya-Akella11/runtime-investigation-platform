# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY backend/src/RuntimeInvestigation.API/RuntimeInvestigation.API.csproj backend/src/RuntimeInvestigation.API/
COPY backend/src/RuntimeInvestigation.Application/RuntimeInvestigation.Application.csproj backend/src/RuntimeInvestigation.Application/
COPY backend/src/RuntimeInvestigation.Domain/RuntimeInvestigation.Domain.csproj backend/src/RuntimeInvestigation.Domain/
COPY backend/src/RuntimeInvestigation.Infrastructure/RuntimeInvestigation.Infrastructure.csproj backend/src/RuntimeInvestigation.Infrastructure/
COPY backend/src/RuntimeInvestigation.Shared/RuntimeInvestigation.Shared.csproj backend/src/RuntimeInvestigation.Shared/
RUN dotnet restore backend/src/RuntimeInvestigation.API/RuntimeInvestigation.API.csproj
COPY backend/. backend/
WORKDIR /src/backend/src/RuntimeInvestigation.API
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "RuntimeInvestigation.API.dll"]
