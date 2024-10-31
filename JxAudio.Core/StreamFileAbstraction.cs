namespace JxAudio.Core;

public class StreamFileAbstraction(string name, Stream stream): TagLib.File.IFileAbstraction
{
    public void CloseStream(Stream stream)
    {
        stream.Close();
        stream.Dispose();
    }

    public string Name { get; } = name;
    public Stream ReadStream { get; } = stream;

    public Stream? WriteStream => null;
}