namespace JxAudio.Web.Vo;

public class ResultVo(int code, string message, object? data)
{
    public int Code { get; set; } = code;

    public string? Message { get; set; } = message;

    public object? Data { get; set; } = data;

    public static ResultVo Success(int code = 200, string message = "success", object? data = null)
    {
        return new ResultVo(code, message, data);
    }
    
    public static ResultVo Fail(int code, string message = "fail", object? data = null)
    {
        return new ResultVo(code, message, data);
    }
}