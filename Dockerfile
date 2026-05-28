FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish src/LibraryProject.Api/LibraryProject.Api.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
EXPOSE 5156
ENV ASPNETCORE_URLS=http://+:5156
ENTRYPOINT ["dotnet", "LibraryProject.Api.dll"]
