using Bogus;
using NuGetServer.Entities.DTO;

namespace NuGetServer.Tests.DataFakers;

public class NuGetPackageInfoFaker : FakerBase<NuGetPackageInfo>
{
    public NuGetPackageInfoFaker()
    {
        _faker = new Faker<NuGetPackageInfo>()
            .RuleFor(p => p.Id, f => f.Commerce.ProductName().Replace(" ", ""))
            .RuleFor(p => p.Version, f => $"{f.Random.Int(1, 5)}.{f.Random.Int(0, 9)}.{f.Random.Int(0, 9)}")
            .RuleFor(p => p.FileName, (f, p) => $"{p.Id}.{p.Version}.nupkg")
            .RuleFor(p => p.DownloadUrl, (f, p) => $"https://localhost/nuget/v3-flatcontainer/{p.Id}/{p.Version}/{p.FileName}")
            .RuleFor(p => p.Description, f => f.Lorem.Sentence())
            .RuleFor(p => p.Authors, f => f.Name.FullName());
    }
}
