// ResponsivePopover.razor.js

let outsideClickListener;

export function addOutsideClickListener(container, dotNetObjectReference) {
    removeOutsideClickListener(); // Ensure no old listeners are active

    outsideClickListener = (event) => {
        if (container && !container.contains(event.target)) {
            dotNetObjectReference.invokeMethodAsync('ClosePopover');
        }
    };

    document.addEventListener('click', outsideClickListener);
    document.addEventListener('touchstart', outsideClickListener);
}

export function removeOutsideClickListener() {
    if (outsideClickListener) {
        document.removeEventListener('click', outsideClickListener);
        document.removeEventListener('touchstart', outsideClickListener);
        outsideClickListener = null;
    }
}
