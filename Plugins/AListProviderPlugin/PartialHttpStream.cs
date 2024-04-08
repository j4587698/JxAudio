using AListSdkSharp.Api;
using AListSdkSharp.Vo;

namespace AListProviderPlugin;

public class PartialHttpStream(Fs fs, InfoOut infoOut): Stream
{
    private long _responseLength = infoOut.Data.Size;
    private long _position;
    
    public override void Flush()
    {
        
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (_position >= _responseLength)
            return 0; // End of stream
        using var stream = fs.RangeDownload(infoOut.Data.RawUrl, _position, count).Result;
        
        var bytesRead = stream.Read(buffer, offset, count);
        _position += bytesRead;
        return bytesRead;
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        if (_position >= _responseLength)
            return 0; // End of stream

        await using var stream = await fs.RangeDownload(infoOut.Data.RawUrl, _position, count, cancellationToken);
        
        var bytesRead = await stream.ReadAsync(buffer, offset, count, cancellationToken);
        _position += bytesRead;
        return bytesRead;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        switch (origin)
        {
            case SeekOrigin.Begin:
                _position = offset;
                break;
            case SeekOrigin.Current:
                _position += offset;
                break;
            case SeekOrigin.End:
                _position = _responseLength + offset;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(origin), "Invalid seek origin.");
        }
        return _position;
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException("This stream does not support setting length.");
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException("This stream does not support writing.");
    }

    public override bool CanRead { get; } = true;
    public override bool CanSeek { get; } = true;
    public override bool CanWrite { get; } = false;

    public override long Length => _responseLength;

    public override long Position
    {
        get => _position;
        set => Seek(value, SeekOrigin.Begin);
    }
}