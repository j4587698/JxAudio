import Data from '../_content/BootstrapBlazor/modules/data.js'

export function init(interop) {
    console.log("init loader")
    const player = {}
    const audio = new Audio()
    player._audio = audio
    player._interop = interop
    Data.set("player", player)
    audio.ontimeupdate = function() {
        player._interop.invokeMethodAsync("OnTimeUpdate", audio.currentTime)
    }
    audio.ended = function() {
        player._interop.invokeMethodAsync("OnEnded")
    }
    audio.error = function() {
        player._interop.invokeMethodAsync("OnError", audio.error.message)
    }
}

export function dispose() {
    Data.remove("player")
}

export function play() {
    const player = Data.get("player")
    player._audio.src = "http://home.jvxiang.com:4533/rest/stream?u=j4587698&t=9da3c9c2714f7cac8688bcc28bc11d47&s=68ef00&f=json&v=1.8.0&c=NavidromeUI&id=f72fb644e228401b83a395a8d043b709&_=1713774716561"
    player._audio.load()
    
    console.log("audio loaded")
    player._audio.play().catch(function(error) {
        console.log("audio play error", error)
    })
}