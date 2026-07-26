using System.Threading.Tasks;

namespace Cheer.Application.Interfaces;

/// <summary>
/// Abstração de armazenamento de objetos (R2, S3, ou disco local).
/// Permite trocar o backend sem modificar a lógica de negócio.
/// </summary>
public interface IStorageService
{
    /// <summary>Faz upload de um stream para o storage.</summary>
    /// <param name="key">Chave única (ex: "logos/team-id_guid.png")</param>
    /// <param name="content">Stream do arquivo</param>
    /// <param name="contentType">MIME type</param>
    /// <returns>URL pública do objeto</returns>
    Task<string> UploadAsync(string key, Stream content, string contentType);

    /// <summary>Remove um objeto do storage.</summary>
    Task DeleteAsync(string key);

    /// <summary>Devolve a URL pública para uma chave.</summary>
    string GetPublicUrl(string key);

    /// <summary>Extrai a chave de uma URL pública.</summary>
    string? ExtractKey(string? publicUrl);
}
