window.launchConfetti = () => {
    confetti({
        particleCount: 150,
        spread: 100,
        origin: { y: 0.6 }
    });
};

window.playVictorySound = () => {
    const sound = document.getElementById("victory-sound");
    if (sound) {
        sound.currentTime = 0;
        sound.play();
    }
};

window.scrollToTopSmooth = () => {
    window.scrollTo({ top: 0, behavior: "smooth" });
};

