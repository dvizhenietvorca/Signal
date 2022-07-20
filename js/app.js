window.PlayAudio = (elementName) => {
    //debugger;
    document.getElementById(elementName).play();
}

window.VolumeChange = (volume) => {
    //debugger;
    document.getElementById("pre").volume = volume;
    document.getElementById("roar").volume = volume;
}

window.WakeLockMethod = () => {
    if (navigator.wakeLock)
        wakeLock = navigator.wakeLock.request('screen');
}