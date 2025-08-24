// Store the last click coordinates
let lastClickX = 0;
let lastClickY = 0;

// Track clicks to get coordinates
document.addEventListener('click', function(event) {
    lastClickX = event.clientX;
    lastClickY = event.clientY;
});

// Function to get last click coordinates
function getLastClickCoordinates() {
    return [lastClickX, lastClickY];
}

// Function to adjust popup position to keep it in viewport
function adjustPopupPosition(x, y) {
    const popup = document.querySelector('.interest-calc-popup');
    if (!popup) return [x, y];
    
    const viewportWidth = window.innerWidth;
    const viewportHeight = window.innerHeight;
    const popupWidth = popup.offsetWidth;
    const popupHeight = popup.offsetHeight;
    
    // Adjust X position if needed
    if (x + popupWidth + 20 > viewportWidth) {
        x = Math.max(20, viewportWidth - popupWidth - 20);
    }
    
    // Adjust Y position if needed
    if (y + popupHeight + 20 > viewportHeight) {
        y = Math.max(20, viewportHeight - popupHeight - 20);
    }
    
    return [x, y];
}

// Function to handle clicks outside the popup - not needed anymore
function addClickOutsideListener() {
    // We now use the Blazor overlay approach instead
}
