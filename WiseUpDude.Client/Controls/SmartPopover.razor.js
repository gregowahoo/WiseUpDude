// SmartPopover.razor.js
let outsideClickListener;

export function setPosition(popover, trigger) {
    if (!popover || !trigger) return;

    const triggerRect = trigger.getBoundingClientRect();
    const popoverRect = popover.getBoundingClientRect();
    const viewportWidth = window.innerWidth;
    const viewportHeight = window.innerHeight;
    const spaceAbove = triggerRect.top;
    const spaceBelow = viewportHeight - triggerRect.bottom;
    const popoverHeight = popoverRect.height;
    const popoverWidth = popoverRect.width;
    const margin = 8;

    let top;
    let left = triggerRect.left + window.scrollX + (triggerRect.width / 2) - (popoverWidth / 2);

    // Decide vertical placement - always prefer below, then above, then best fit
    if (spaceBelow >= popoverHeight + margin) {
        // Enough space below - ideal case
        top = triggerRect.bottom + window.scrollY + margin;
    } else if (spaceAbove >= popoverHeight + margin) {
        // Not enough space below, but enough above
        top = triggerRect.top + window.scrollY - popoverHeight - margin;
    } else {
        // Not enough space either way - place where there's more space
        if (spaceBelow > spaceAbove) {
            // More space below - place below but constrain to viewport
            top = triggerRect.bottom + window.scrollY + margin;
            // Ensure it doesn't go below viewport
            const maxTop = window.scrollY + viewportHeight - popoverHeight - margin;
            if (top > maxTop) {
                top = maxTop;
            }
        } else {
            // More space above - place above but constrain to viewport
            top = triggerRect.top + window.scrollY - popoverHeight - margin;
            // Ensure it doesn't go above viewport
            const minTop = window.scrollY + margin;
            if (top < minTop) {
                top = minTop;
            }
        }
    }

    // Adjust for left/right overflow
    if (left < 0) {
        left = 5 + window.scrollX;
    } else if (left + popoverWidth > viewportWidth) {
        left = viewportWidth - popoverWidth - 5 + window.scrollX;
    }

    popover.style.top = `${top}px`;
    popover.style.left = `${left}px`;
}

export function addOutsideClickListener(trigger, popover, dotNetObjectReference) {
    removeOutsideClickListener();
    outsideClickListener = (event) => {
        if (popover && !popover.contains(event.target) && trigger && !trigger.contains(event.target)) {
            dotNetObjectReference.invokeMethodAsync('ClosePopover');
        }
    };
    document.addEventListener('click', outsideClickListener, true);
    document.addEventListener('touchstart', outsideClickListener, true);
}

export function removeOutsideClickListener() {
    if (outsideClickListener) {
        document.removeEventListener('click', outsideClickListener, true);
        document.removeEventListener('touchstart', outsideClickListener, true);
        outsideClickListener = null;
    }
}
