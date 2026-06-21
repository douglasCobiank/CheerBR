# 1. Estágio de Build (.NET 10)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 2. Encontra e copia os arquivos .csproj em qualquer subpasta da estrutura
COPY **/Cheer.Application/Cheer.Application.csproj ./Cheer.Application/
COPY **/Cheer.Domain/Cheer.Domain.csproj ./Cheer.Domain/
COPY **/Cheer.Infrastructure/Cheer.Infrastructure.csproj ./Cheer.Infrastructure/
COPY **/Cheer.Api/Cheer.Api.csproj ./Cheer.Api/

# 3. Restaura as dependências usando o caminho relativo do container
RUN dotnet restore "Cheer.Api/Cheer.Api.csproj"

# 4. Copia absolutamente todo o resto do código fonte
COPY . .

# 5. Encontra a pasta da API para compilar e publicar os arquivos finais
RUN dotnet publish **/Cheer.Api/Cheer.Api.csproj -c Release -o /app/publish

# 6. Estágio de Execução
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "Cheer.Api.dll"]