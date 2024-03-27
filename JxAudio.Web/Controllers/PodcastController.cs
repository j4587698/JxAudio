using JxAudio.Core;
using Microsoft.AspNetCore.Mvc;

namespace JxAudio.Web.Controllers;

public class PodcastController: AudioController
{
    [HttpGet("/getPodcasts")]
    public void GetPodcasts()
    {
        throw RestApiErrorException.NotImplemented();
    }
    
    [HttpGet("/getNewestPodcasts")]
    public void GetNewestPodcasts()
    {
        throw RestApiErrorException.NotImplemented();
    }
    
    [HttpGet("/refreshPodcasts")]
    public void RefreshPodcasts()
    {
        throw RestApiErrorException.NotImplemented();
    }
    
    [HttpGet("/createPodcastChannel")]
    public void CreatePodcastChannel()
    {
        throw RestApiErrorException.NotImplemented();
    }
    
    [HttpGet("/deletePodcastChannel")]
    public void DeletePodcastChannel()
    {
        throw RestApiErrorException.NotImplemented();
    }
    
    [HttpGet("/deletePodcastEpisode")]
    public void DeletePodcastEpisode()
    {
        throw RestApiErrorException.NotImplemented();
    }
    
    [HttpGet("/downloadPodcastEpisode")]
    public void DownloadPodcastEpisode()
    {
        throw RestApiErrorException.NotImplemented();
    }
}