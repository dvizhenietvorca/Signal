window.PlayAudio = (elementName) => {
    document.getElementById(elementName).play();
}

window.WakeLockMethod = () => {
    wakeLock = navigator.wakeLock.request('screen');
}