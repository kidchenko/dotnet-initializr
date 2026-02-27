export function afterStarted(blazor) {
    const screen = document.getElementById('loading-screen');
    if (screen) {
        screen.style.opacity = '0';
        setTimeout(() => screen.remove(), 400);
    }
}
