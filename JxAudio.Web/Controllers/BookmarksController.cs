using JxAudio.Core;
using Microsoft.AspNetCore.Mvc;

namespace JxAudio.Web.Controllers;

public class BookmarksController: AudioController
{
    [HttpGet("/getBookmarks")]
    public void GetBookmarks()
    {
        throw RestApiErrorException.NotImplemented();
    }
    
    [HttpGet("/createBookmark")]
    public void CreateBookmark()
    {
        throw RestApiErrorException.NotImplemented();
    }
    
    [HttpGet("/deleteBookmark")]
    public void DeleteBookmark()
    {
        throw RestApiErrorException.NotImplemented();
    }
    
    [HttpGet("/getPlayQueue")]
    public void GetPlayQueue()
    {
        throw RestApiErrorException.NotImplemented();
    }
    
    [HttpGet("/savePlayQueue")]
    public void SavePlayQueue()
    {
        throw RestApiErrorException.NotImplemented();
    }
}