using JxAudio.Core;
using Microsoft.AspNetCore.Mvc;

namespace JxAudio.Web.Controllers;

public class InternetRadioController: AudioController
{
    [HttpGet("/getInternetRadioStations")]
    public void GetInternetRadioStations()
    {
        throw RestApiErrorException.NotImplemented();
    }
    
    [HttpGet("/createInternetRadioStation")]
    public void CreateInternetRadioStation()
    {
        throw RestApiErrorException.NotImplemented();
    }
    
    [HttpGet("/updateInternetRadioStation")]
    public void UpdateInternetRadioStation()
    {
        throw RestApiErrorException.NotImplemented();
    }
    
    [HttpGet("/deleteInternetRadioStation")]
    public void DeleteInternetRadioStation()
    {
        throw RestApiErrorException.NotImplemented();
    }
}