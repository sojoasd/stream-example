using System.Text;

public class HexStreamTransformerPureHex : IStreamTransformer
{
    private readonly StreamReader _reader;
    private readonly int _chunkSize;
    private string[] _breaks;
    private string _lastLine = ""; // 上次讀取時剩下的最後一行
    private string[] _lines = new string[0];

    public HexStreamTransformerPureHex(Stream stream, int chunkSize)
    {
        _reader = new StreamReader(stream);
        _chunkSize = chunkSize;
        _breaks = new string[] { "\r\n", "\n" };
    }

    public async Task ProcessAsyncTo(Stream targetStream)
    {
        using (StreamWriter writer = new StreamWriter(targetStream, Encoding.UTF8, leaveOpen: true))
        {
            byte[] buffer = new byte[_chunkSize];
            int bytesRead;

            while ((bytesRead = await _reader.BaseStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                if (bytesRead == 0) break; // 讀取完畢，跳出

                // 解析 HEX 行
                string hexString = _lastLine + Encoding.UTF8.GetString(buffer, 0, bytesRead);
                _lines = hexString.Split(_breaks, StringSplitOptions.None);

                // 最後一行可能是不完整的，先保留
                if (buffer.Length == 0)
                {
                    _lastLine = "";
                }
                else
                {
                    // 最後一行可能不完整，先留著
                    _lastLine = _lines[_lines.Length - 1];
                    // 把最後一行拿掉，留到下一次讀取時再處理
                    _lines = _lines.Take(_lines.Length - 1).ToArray();
                }

                foreach (var line in _lines)
                {
                    if (line.Length == 0 || !line.StartsWith(":")) continue;

                    byte[] fullLineBytes = ByteConvert.convertHexStringToBytes(line.Substring(1));

                    // 解析 Intel HEX 結構
                    int dataLength = fullLineBytes[0]; // 數據長度

                    if (dataLength == 0) continue; // 過濾掉 `:00000001FF`

                    if (fullLineBytes[3] == 0x00)
                    {
                        byte[] dataBytes = fullLineBytes[4..(4 + dataLength)]; // 只取數據部分

                        //await targetStream.WriteAsync(dataBytes);
                        //convert dataBytes to hex string to target stream
                        await writer.WriteAsync(BitConverter.ToString(dataBytes).Replace("-", ""));

                    }
                }
            }
        }

        await targetStream.FlushAsync();
    }
}