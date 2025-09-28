window.audioInterop = {
    toggleAudio: function(dotnetHelper) {
        var audio = document.getElementById('guideAudio');
        if (!audio) return false;
        if (audio.paused || audio.ended) {
            audio.play();
            audio.onended = function() {
                dotnetHelper.invokeMethodAsync('OnAudioEnded');
            };
            return true;
        } else {
            audio.pause();
            return false;
        }
    }
};
