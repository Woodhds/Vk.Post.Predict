FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Vk.Post.Predict/Vk.Post.Predict.csproj", "Vk.Post.Predict/"]
RUN dotnet restore "Vk.Post.Predict/Vk.Post.Predict.csproj"
COPY . .
WORKDIR "/src/Vk.Post.Predict"
RUN dotnet build "Vk.Post.Predict.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Vk.Post.Predict.csproj" -c Release -r alpine-x64 --self-contained -o /app/publish /p:PublishSingleFile=true

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
CMD ASPNETCORE_URLS=http://*:$PORT ./Vk.Post.Predict
