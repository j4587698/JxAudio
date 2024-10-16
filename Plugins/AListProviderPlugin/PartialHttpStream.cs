﻿using AListSdkSharp.Api;
using AListSdkSharp.Vo;

namespace AListProviderPlugin;

public class PartialHttpStream(Fs fs, InfoOut infoOut): Stream
{
    private long _responseLength = infoOut.Data.Size;
    private long _position;
    private Stream? _stream;
    
    public override void Flush()
    {
        
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (_position >= _responseLength)
            return 0; // End of stream
        _stream ??= fs.RangeDownload(infoOut.Data.RawUrl, _position, _responseLength - _position).Result;

        // var bytesRead = 0;
        // while (bytesRead != count)
        // {
        var bytesRead = _stream.Read(buffer, offset, count);
        //     bytesRead += read;
        // }
        _position += bytesRead;
        return bytesRead;
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        if (_position >= _responseLength)
            return 0; // End of stream

        _stream ??= await fs.RangeDownload(infoOut.Data.RawUrl, _position, _responseLength - _position, cancellationToken);
        
        var bytesRead = 0;
        while (bytesRead != count)
        {
            var read = await _stream.ReadAsync(buffer, offset + bytesRead, count - bytesRead, cancellationToken);
            bytesRead += read;
        }
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
        _stream?.Dispose();
        _stream = null;
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

    public override ValueTask DisposeAsync()
    {
        _stream?.Dispose();
        return base.DisposeAsync();
    }
}