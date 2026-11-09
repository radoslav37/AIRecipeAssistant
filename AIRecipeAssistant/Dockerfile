FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY AIRecipeAssistant.csproj ./
RUN dotnet restore AIRecipeAssistant.csproj

COPY . ./
RUN dotnet publish AIRecipeAssistant.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:10000
EXPOSE 10000

COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "AIRecipeAssistant.dll"]
