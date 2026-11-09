
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy project files first for efficient dependency caching.
COPY backend/SchoolAdmission.BLL/SchoolAdmission.BLL.csproj backend/SchoolAdmission.BLL/
COPY backend/SchoolAdmission.DAL/SchoolAdmission.DAL.csproj backend/SchoolAdmission.DAL/
COPY backend/SchoolAdmission.API/SchoolAdmission.API.csproj backend/SchoolAdmission.API/

# Restore API dependencies.
RUN dotnet restore backend/SchoolAdmission.API/SchoolAdmission.API.csproj

# Copy the remaining source code.
COPY . .

# Publish the API.
WORKDIR /src/backend/SchoolAdmission.API

RUN dotnet publish SchoolAdmission.API.csproj \
    --configuration Release \
    --output /app/publish \
    /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

# Render's default web service port.
ENV ASPNETCORE_URLS=http://+:10000

EXPOSE 10000

ENTRYPOINT ["dotnet", "SchoolAdmission.API.dll"]
