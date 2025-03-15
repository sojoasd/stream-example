public interface IStreamTransformer
{
    Task ProcessAsyncTo(Stream targetStream);
}