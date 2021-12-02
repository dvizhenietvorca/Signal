window.PlayAudio = (elementName) => {
    document.getElementById(elementName).play();
}

window.WakeLockMethod = () => {
    if (navigator.wakeLock)
        wakeLock = navigator.wakeLock.request('screen');
}