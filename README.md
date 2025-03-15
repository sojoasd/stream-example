# stream-example

## Stream
- stream 只是一個容器，裡面可以放任何型態的資料，例如字串、數字、物件等等。
- 此範例展示如何使用 stream 來處理 Intel hex 格式的檔案。

## Transformer
- Transformer 是一個可以將 stream 轉換成另一個 stream 的物件。
- 簡單來說，要操作 stream 必須透過 transformer 來進行。
- 此範例展示如何使用 
    - HexStreamTransformerVerifyChecksum 來將 Intel hex 格式的檔案透過 crc 16 modbus 來驗證 checksum。
    - HHexStreamTransformerPureHex 來將 Intel hex 格式的檔案轉換成純粹的 hex 字串。

## Pipeline
- 使用 pipeline 可以將多個 transformer 串接在一起，讓 stream 依序經過每個 transformer。同時也可以美化程式碼，提高可讀性。
- C# 必須要自己實作 pipeline，這裡提供一個簡單的範例。
```csharp
await transformer.PipeTo(stream => new HexStreamTransformerPureHex(stream, 64)).PipeTo(outputStream);
```
