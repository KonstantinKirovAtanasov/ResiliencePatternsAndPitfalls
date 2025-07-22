window.scrollToBottom = (selector) => {
    const element = document.querySelector(selector);
    if (element) {
        element.scrollTop = element.scrollHeight;
    }
};
