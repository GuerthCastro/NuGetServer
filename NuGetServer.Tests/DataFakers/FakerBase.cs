using Bogus;
using NuGetServer.Tests.Interfaces;

namespace NuGetServer.Tests.DataFakers;

public abstract class FakerBase<T> : IFaker<T> where T : class
{
    internal Faker<T> _faker;

    public T Generate()
    {
        return _faker.Generate();
    }

    public List<T> Generate(int count = 5)
    {
        return _faker.Generate(count);
    }


    public virtual List<T> Generate(int count, int startIndex = 0)
    {
        return _faker.Generate(count);
    }
}
