using NuGetServer.Entities.DTO;

namespace NuGetServer.Services;

public interface IPackageStorageService
{
    Task<(bool Success, string Message)> SavePackage(Stream packageStream, string fileName);
    Task<(Stream? Stream, string? ContentType)> GetPackageStream(string packageId, string version);
    Task<bool> DeletePackage(string packageId, string version);
    Task<List<NuGetPackageInfo>> GetAllPackages();
    Task<List<NuGetPackageInfo>> GetPackagesWithMetadata();
    Task<List<string>> GetPackageVersions(string packageId);
    Task<NuGetPackageInfo?> GetPackageMetadata(string packageId, string version);
    Task<bool> PackageExists(string packageId, string version);
}
