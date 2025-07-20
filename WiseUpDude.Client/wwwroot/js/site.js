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

// Ensure Bootstrap tooltips and popovers are initialized correctly
window.initBootstrapTooltipsAndPopovers = function() {
    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.forEach(function (tooltipTriggerEl) {
        if (tooltipTriggerEl._tooltipInstance) {
            tooltipTriggerEl._tooltipInstance.dispose();
        }
        tooltipTriggerEl._tooltipInstance = new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Initialize popovers
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    popoverTriggerList.forEach(function (popoverTriggerEl) {
        if (popoverTriggerEl._popoverInstance) {
            popoverTriggerEl._popoverInstance.dispose();
        }
        popoverTriggerEl._popoverInstance = new bootstrap.Popover(popoverTriggerEl);
    });
};

// Initialize on page load
document.addEventListener('DOMContentLoaded', function() {
    initBootstrapTooltips();
    initBootstrapPopovers();
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

// Bootstrap Popover functions for WASM
window.initializePopovers = () => {
    // Dispose existing popovers first
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    popoverTriggerList.forEach(function (el) {
        if (el._popoverInstance) {
            el._popoverInstance.dispose();
        }
        // Always provide a string for title
        var title = el.getAttribute('data-bs-title');
        if (title === null) title = "";
        el._popoverInstance = new bootstrap.Popover(el, {
            title: title,
            html: el.getAttribute('data-bs-html') === 'true',
            customClass: el.getAttribute('data-bs-custom-class'),
            trigger: el.getAttribute('data-bs-trigger') || 'click',
            placement: el.getAttribute('data-bs-placement') || 'top',
            content: el.getAttribute('data-bs-content')
        });
    });
    // Initialize dismiss-on-next-click popovers (interactive)
    var dismissList = [].slice.call(document.querySelectorAll('.popover-dismiss'));
    dismissList.forEach(function (el) {
        if (el._popoverInstance) {
            el._popoverInstance.dispose();
        }
        el._popoverInstance = new bootstrap.Popover(el, { trigger: 'focus', html: true });
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

// Debug: Log page reload/navigation events
window.addEventListener('beforeunload', function (e) {
    console.log('DEBUG: beforeunload event fired. Page is reloading or navigating away.', e);
});
window.addEventListener('popstate', function (e) {
    console.log('DEBUG: popstate event fired. Browser navigation occurred.', e);
});