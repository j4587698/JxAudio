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
    audio.onended = function() {
        player._interop.invokeMethodAsync("OnEnded")
    }
    audio.onerror = function() {
        player._interop.invokeMethodAsync("OnError", audio.error.message)
    }

    audio.onloadedmetadata = function() {
        console.log('音频的时长为：' + audio.duration + '秒');
    }
    
    audio.onplaying = function (){
        console.log('开始播放')
    }
}

export function dispose() {
    Data.remove("player")
}

export function play(url) {
    const player = Data.get("player")
    player._audio.src = url
    console.log("audio loaded")
    player._audio.play().catch(function(error) {
        console.log("audio play error", error)
    })
}