public static class StreamPipe
{
    public static async Task PipeTo(this IStreamTransformer firstTransformer, Stream finalOutput, params Func<Stream, IStreamTransformer>[] transformerFactories)
    {
        Stream currentStream = new MemoryStream();

        // 讓第一個 Transformer 寫入 currentStream
        await firstTransformer.ProcessAsyncTo(currentStream);
        currentStream.Position = 0;

        foreach (var factory in transformerFactories)
        {
            Stream nextStream = new MemoryStream();

            // 建立並執行下一個 Transformer
            IStreamTransformer nextTransformer = factory(currentStream);
            await nextTransformer.ProcessAsyncTo(nextStream);

            // 更新 currentStream，準備給下一個 Transformer 使用
            currentStream.Dispose();
            currentStream = nextStream;
            currentStream.Position = 0;
        }

        // 最後一個 Transformer 的輸出寫入 finalOutput
        await currentStream.CopyToAsync(finalOutput);
        currentStream.Dispose();
    }
    
    public static async Task<IStreamTransformer> PipeTo(this IStreamTransformer transformer, Func<Stream, IStreamTransformer> nextTransformerFactory)
    {
        MemoryStream intermediateStream = new MemoryStream();

        // 讓當前 Transformer 處理並寫入 intermediateStream
        await transformer.ProcessAsyncTo(intermediateStream);
        intermediateStream.Position = 0;

        // 產生並回傳新的 Transformer
        return nextTransformerFactory(intermediateStream);
    }

    public static async Task<IStreamTransformer> PipeTo(this Task<IStreamTransformer> previousTask, Func<Stream, IStreamTransformer> nextTransformerFactory)
    {
        IStreamTransformer previousTransformer = await previousTask;
        return await previousTransformer.PipeTo(nextTransformerFactory);
    }

    public static async Task PipeTo(this IStreamTransformer transformer, Stream finalOutput)
    {
        // 讓最後一個 Transformer 直接輸出到 finalOutput
        await transformer.ProcessAsyncTo(finalOutput);
    }

    public static async Task PipeTo(this Task<IStreamTransformer> previousTask, Stream finalOutput)
    {
        IStreamTransformer previousTransformer = await previousTask;
        await previousTransformer.PipeTo(finalOutput);
    }
}
