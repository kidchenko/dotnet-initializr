let hljsModule = null;
let themeLoaded = false;

export async function highlightCode(elementId, code, language, themeCssUrl) {
    // Load highlight.js ESM module once
    if (!hljsModule) {
        const mod = await import(
            'https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.11.1/es/highlight.min.js'
        );
        hljsModule = mod.default;
    }

    // Inject theme CSS once
    if (!themeLoaded && themeCssUrl) {
        const link = document.createElement('link');
        link.rel = 'stylesheet';
        link.href = themeCssUrl;
        document.head.appendChild(link);
        themeLoaded = true;
    }

    const el = document.getElementById(elementId);
    if (!el) return;

    // Set content and language, then highlight
    el.textContent = code;
    el.className = `language-${language}`;
    hljsModule.highlightElement(el);

    // Add line numbers by wrapping each line in a span
    const highlighted = el.innerHTML;
    const lines = highlighted.split('\n');
    el.innerHTML = lines.map((line, i) =>
        `<span class="line"><span class="line-number">${i + 1}</span>${line}</span>`
    ).join('\n');
}
