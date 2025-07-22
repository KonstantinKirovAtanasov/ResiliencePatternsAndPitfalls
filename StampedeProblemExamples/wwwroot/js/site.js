// Auto-scroll functionality for log container
window.scrollToBottom = (selector) => {
    const element = document.querySelector(selector);
    if (element) {
        element.scrollTop = element.scrollHeight;
    }
};

// Optional: Add smooth scrolling
window.smoothScrollToBottom = (selector) => {
    const element = document.querySelector(selector);
    if (element) {
        element.scrollTo({
            top: element.scrollHeight,
            behavior: 'smooth'
        });
    }
};
