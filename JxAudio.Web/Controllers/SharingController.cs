using JxAudio.Core;
using Microsoft.AspNetCore.Mvc;

namespace JxAudio.Web.Controllers;

public class SharingController: AudioController
{
    [HttpGet("/getShares")]
    public void GetShares()
    {
        throw RestApiErrorException.NotImplemented();
    }

    [HttpGet("/createShare")]
    public void CreateShare()
    {
        throw RestApiErrorException.NotImplemented();
    }

    [HttpGet("/updateShare")]
    public void UpdateShare()
    {
        throw RestApiErrorException.NotImplemented();
    }

    [HttpGet("/deleteShare")]
    public void DeleteShare()
    {
        throw RestApiErrorException.NotImplemented();
    }
}