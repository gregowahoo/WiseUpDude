﻿window.submitEditForm = (formElement) => {
    // Calls the underlying form's submit() method
    formElement.submit()
};

function preloadSounds() {
    const sounds = [
        "/sounds/correct.mp3",
        "/sounds/incorrect.mp3",
        "/sounds/applause.mp3"
    ];

    sounds.forEach(filePath => {
        const audio = new Audio(filePath);
        audio.load(); // Preloads the audio file
    });
}

// Call preloadSounds when the page loads
window.onload = () => {
    preloadSounds();
};

function playSound(filePath) {
    const audio = new Audio(filePath);
    audio.play().catch(error => {
        console.error("Error playing sound:", error);
    });
}
