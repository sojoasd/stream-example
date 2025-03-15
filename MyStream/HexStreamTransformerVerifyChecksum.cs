using System.Text;

public class HexStreamTransformerVerifyChecksum : IStreamTransformer
{
    private readonly StreamReader _reader;
    private readonly int _chunkSize;
    private readonly ushort _expectedChecksum;
    private ushort _checksum;
    private string[] _breaks;
    private string _lastLine = ""; // 上次讀取時剩下的最後一行
    private string[] _lines = new string[0];
    public bool Result { get; private set; } // 驗證結果

    public HexStreamTransformerVerifyChecksum(Stream stream, int chunkSize, ushort expectedChecksum)
    {
        _reader = new StreamReader(stream);
        _chunkSize = chunkSize;
        _expectedChecksum = expectedChecksum;
        _checksum = 0xFFFF; // CRC-16 Modbus 初始值
        _breaks = new string[] { "\r\n", "\n" };
    }

    public async Task ProcessAsyncTo(Stream targetStream)
    {
        using (StreamWriter writer = new StreamWriter(targetStream, new UTF8Encoding(false), leaveOpen: true))
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

                await targetStream.WriteAsync(buffer, 0, bytesRead);

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

                        // 計算 CRC-16 Modbus
                        _checksum = Crc16Modbus.ComputeChecksum(dataBytes, _checksum);
                    }
                }
            }
        }

        await targetStream.FlushAsync();

        // 核對計算出的 CRC-16
        Result = _checksum == _expectedChecksum;
    }

}