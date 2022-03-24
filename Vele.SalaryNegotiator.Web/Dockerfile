#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Vele.SalaryNegotiator.Web/Vele.SalaryNegotiator.Web.csproj", "Vele.SalaryNegotiator.Web/"]
RUN dotnet restore "Vele.SalaryNegotiator.Web/Vele.SalaryNegotiator.Web.csproj"
COPY . .
WORKDIR "/src/Vele.SalaryNegotiator.Web"
RUN dotnet build "Vele.SalaryNegotiator.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Vele.SalaryNegotiator.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Vele.SalaryNegotiator.Web.dll"]