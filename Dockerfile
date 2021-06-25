﻿FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Vk.Post.Predict/Vk.Post.Predict.csproj", "Vk.Post.Predict/"]
RUN dotnet restore "Vk.Post.Predict/Vk.Post.Predict.csproj"
COPY . .
WORKDIR "/src/Vk.Post.Predict"
RUN dotnet build "Vk.Post.Predict.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Vk.Post.Predict.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["CMD", "ASPNETCORE_URLS=http://*:$PORT", "dotnet", "Vk.Post.Predict.dll"]