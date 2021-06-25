FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /src
COPY ["Vk.Post.Predict/Vk.Post.Predict.csproj", "Vk.Post.Predict/"]
RUN dotnet restore "Vk.Post.Predict/Vk.Post.Predict.csproj"
COPY . .
WORKDIR "/src/Vk.Post.Predict"
RUN dotnet build "Vk.Post.Predict.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Vk.Post.Predict.csproj" -c Release -r linux-musl-x64 -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
CMD ASPNETCORE_URLS=http://*:$PORT ./Vk.Post.Predict
