window.playSound = (id) => {
    const sound = document.getElementById(id);
    if (sound) {
        sound.currentTime = 0;
        sound.play();
    }
};
