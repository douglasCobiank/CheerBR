# 1. Estágio de Build (Usa o SDK do .NET 10)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 2. Copia os arquivos de projeto (.csproj) mantendo a estrutura de pastas
# Substitua "NomeDaPastaDoSeuProjetoPrincipal" pelo nome real da pasta da sua API
COPY Cheer.Application/Cheer.Application.csproj Cheer.Application/
COPY Cheer.Infrastructure/Cheer.Infrastructure.csproj Cheer.Infrastructure/
COPY Cheer.Api/*.csproj Cheer.Api/

# 3. Restaura as dependências de toda a solução
RUN dotnet restore "Cheer.Api/*.csproj"

# 4. Copia o restante de todo o código fonte
COPY . .

# 5. Compila e publica os arquivos finais na pasta /app/publish
WORKDIR "/src/Cheer.Api"
RUN dotnet publish -c Release -o /app/publish

# 6. Estágio de Execução (Usa o Runtime do .NET 10, que é bem mais leve)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Define a porta padrão que o Render utiliza
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

# Executa a sua API (Substitua pelo nome real do arquivo .csproj da sua API)
ENTRYPOINT ["dotnet", "Cheer.Api.dll"]