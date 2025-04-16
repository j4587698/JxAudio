import Data from '../_content/BootstrapBlazor/modules/data.js'

export function init(interop) {
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
        player._interop.invokeMethodAsync("OnLoaded")
    }
    
    audio.onplaying = function (){
        player._interop.invokeMethodAsync("OnPlaying")
    }
}

export function dispose() {
    Data.remove("player")
}

export function play(id, mimeType) {
    const player = Data.get("player")
    let suffix = 'raw'
    if (player._audio.canPlayType(mimeType) === "") {
        suffix = 'mp3'
    }
    player._audio.src = `/api/Track/Stream?trackId=${id}&format=${suffix}`
    player._audio.play().catch(function(error) {
        console.log("audio play error", error)
    })
}

export function playOrPause() {
    const player = Data.get("player")
    if (player._audio.paused) {
        player._audio.play().catch(function(error) {
            console.log("audio play error", error)
        })
    } else {
        player._audio.pause()
    }

}

export function stop() {
    const player = Data.get("player")
    player._audio.pause()
    player._audio.currentTime = 0
}

export function setVolume(volume) {
    const player = Data.get("player")
    player._audio.volume = volume
}

export function getVolume() {
    const player = Data.get("player")
    return player._audio.volume
}

export function setMuted(muted) {
    const player = Data.get("player")
    player._audio.muted = muted
}

export function getMuted() {
    const player = Data.get("player")
    return player._audio.muted
}

export function setCurrentTime(time) {
    const player = Data.get("player")
    player._audio.currentTime = time
}