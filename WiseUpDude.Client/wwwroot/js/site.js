// Site.js - Bootstrap tooltip and popover initialization

// Initialize Bootstrap tooltips
window.initBootstrapTooltips = function() {
    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.forEach(function (tooltipTriggerEl) {
        if (tooltipTriggerEl._tooltip) {
            tooltipTriggerEl._tooltip.dispose();
        }
        tooltipTriggerEl._tooltip = new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Initialize popovers
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    popoverTriggerList.forEach(function (popoverTriggerEl) {
        if (popoverTriggerEl._popover) {
            popoverTriggerEl._popover.dispose();
        }
        popoverTriggerEl._popover = new bootstrap.Popover(popoverTriggerEl);
    });
};

// Initialize on page load
document.addEventListener('DOMContentLoaded', function() {
    initBootstrapTooltips();
});

// Scroll to element smoothly
window.scrollToElement = function(elementId) {
    var element = document.getElementById(elementId);
    if (element) {
        element.scrollIntoView({ behavior: 'smooth' });
    }
};

// Scroll to top smoothly
window.scrollToTopSmooth = function() {
    window.scrollTo({ top: 0, behavior: 'smooth' });
};

// Play sound function
window.playSound = function(soundUrl) {
    var audio = new Audio(soundUrl);
    audio.play().catch(function(error) {
        console.log('Error playing sound:', error);
    });
};