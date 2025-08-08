namespace NuGetServer.Tests.Interfaces; 
public interface IFaker<T>
{
    public T Generate();
    public List<T> Generate(int count = 5);
    public List<T> Generate(int count, int startIndex = 0);
}
