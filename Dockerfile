# syntax=docker/dockerfile:1
FROM node:22-alpine AS web-build
WORKDIR /web
COPY src/LarFinance.Web/package*.json ./
RUN npm ci
COPY src/LarFinance.Web/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS api-build
WORKDIR /source
COPY . .
RUN dotnet publish src/LarFinance.Api/LarFinance.Api.csproj -c Release -o /app --no-self-contained

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app
COPY --from=api-build /app .
COPY --from=web-build /web/dist ./wwwroot
RUN mkdir -p App_Data
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "LarFinance.Api.dll"]
