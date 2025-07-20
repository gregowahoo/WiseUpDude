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

// Bootstrap Popover functions
window.initializePopovers = () => {
    // Dispose existing popovers first
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    popoverTriggerList.forEach(function (el) {
        if (el._popoverInstance) {
            el._popoverInstance.dispose();
        }
        // Ensure title is never null or undefined
        let title = el.getAttribute('data-bs-title');
        if (title === null || title === undefined) {
            title = "";
        }
        el._popoverInstance = new bootstrap.Popover(el, { title: title });
    });
    // Initialize dismiss-on-next-click popovers (interactive)
    var dismissList = [].slice.call(document.querySelectorAll('.popover-dismiss'));
    dismissList.forEach(function (el) {
        if (el._popoverInstance) {
            el._popoverInstance.dispose();
        }
        let title = el.getAttribute('data-bs-title');
        if (title === null || title === undefined) {
            title = "";
        }
        el._popoverInstance = new bootstrap.Popover(el, { trigger: 'focus', html: true, title: title });
        // Prevent popover from closing when clicking inside
        el.addEventListener('shown.bs.popover', function () {
            var popover = document.querySelector('.popover');
            if (popover) {
                popover.addEventListener('mousedown', function (e) {
                    e.stopPropagation();
                });
            }
        });
    });
};

window.disposeAllPopovers = () => {
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    popoverTriggerList.forEach(function (el) {
        if (el._popoverInstance) {
            el._popoverInstance.dispose();
            el._popoverInstance = null;
        }
    });
};

window.showPopover = (elementId) => {
    const element = document.getElementById(elementId);
    if (element && element._popoverInstance) {
        element._popoverInstance.show();
    }
};

window.hidePopover = (elementId) => {
    const element = document.getElementById(elementId);
    if (element && element._popoverInstance) {
        element._popoverInstance.hide();
    }
};

window.togglePopover = (elementId) => {
    const element = document.getElementById(elementId);
    if (element && element._popoverInstance) {
        element._popoverInstance.toggle();
    }
};
