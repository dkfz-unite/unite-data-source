FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
ENV ASPNETCORE_HTTP_PORTS=80
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore
ARG USER
ARG TOKEN
WORKDIR /src
RUN dotnet nuget add source https://nuget.pkg.github.com/dkfz-unite/index.json -n github -u ${USER} -p ${TOKEN} --store-password-in-clear-text
COPY ["Unite.Data.Source.Web/Unite.Data.Source.Web.csproj", "Unite.Data.Source.Web/"]
RUN dotnet restore "Unite.Data.Source.Web/Unite.Data.Source.Web.csproj"

FROM restore as build
COPY . .
WORKDIR "/src/Unite.Data.Source.Web"
RUN dotnet build --no-restore "Unite.Data.Source.Web.csproj" -c Release

FROM build AS publish
RUN dotnet publish --no-build "Unite.Data.Source.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Unite.Data.Source.Web.dll"]