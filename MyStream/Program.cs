string inputPath = @"C:\Users\yenzeal\workspace\stream-example\original.hex";
string outputPath = @"C:\Users\yenzeal\workspace\stream-example\final.txt";
ushort expectedChecksum = 0xE121; // 這是 HEX 檔案的 checksum 正確的 CRC-16 Modbus 的值

using FileStream inputStream = new FileStream(inputPath, FileMode.Open, FileAccess.Read);
using FileStream outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);

/****************************************************
 * 第一種寫法，完全沒有 pipeline 效果
 ****************************************************/
//using MemoryStream memoryStream = new MemoryStream();

//HexStreamTransformerVerifyChecksum transformer1 = new HexStreamTransformerVerifyChecksum(inputStream, 64, expectedChecksum);

// await transformer1.ProcessAsyncTo(memoryStream);

// Console.WriteLine($"Checksum Verification: {transformer1.Result}");

// memoryStream.Position = 0;
// HexStreamTransformerPureHex transformer2 = new HexStreamTransformerPureHex(memoryStream, 64);
// await transformer2.ProcessAsyncTo(outputStream);

// Console.WriteLine("Done");

/****************************************************
 * 有 pipeline 效果，但不直覺、不好看
 ****************************************************/
// HexStreamTransformerVerifyChecksum transformer1 = new HexStreamTransformerVerifyChecksum(inputStream, 64, expectedChecksum);
// await transformer1.PipeTo(outputStream, (stream) => new HexStreamTransformerPureHex(stream, 64));

// Console.WriteLine("Done");

/****************************************************
 * 第二種寫法，有 pipeline 效果，且比較直覺、好看
 ****************************************************/
HexStreamTransformerVerifyChecksum transformer = new HexStreamTransformerVerifyChecksum(inputStream, 64, expectedChecksum);
await transformer.PipeTo(stream => new HexStreamTransformerPureHex(stream, 64)).PipeTo(outputStream);

Console.WriteLine("Done");



