window.submitEditForm = (formElement) => {
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

// Save data to sessionStorage
function saveToSessionStorage(key, value) {
    sessionStorage.setItem(key, value);
}

// Load data from sessionStorage
function loadFromSessionStorage(key) {
    return sessionStorage.getItem(key);
}

// Remove data from sessionStorage
function removeFromSessionStorage(key) {
    sessionStorage.removeItem(key);
}

window.initBootstrapTooltips = () => {
    // Dispose existing tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.forEach(function (el) {
        if (el._tooltipInstance) {
            el._tooltipInstance.dispose();
        }
        el._tooltipInstance = new bootstrap.Tooltip(el);
    });
};

window.initTippyPopovers = function () {
    if (typeof tippy === 'function') {
        tippy('[data-tippy-content]', {
            allowHTML: true,
            interactive: true,
            placement: 'top',
            theme: 'light-border',
            maxWidth: 350,
        });
    }
};
