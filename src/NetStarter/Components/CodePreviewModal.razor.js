let hljsModule = null;
let languagesRegistered = false;
let themeLoaded = false;

const CDN = 'https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.11.1/es';

const LANGUAGES = {
    csharp: `${CDN}/languages/csharp.min.js`,
    xml: `${CDN}/languages/xml.min.js`,
    json: `${CDN}/languages/json.min.js`,
    yaml: `${CDN}/languages/yaml.min.js`,
    bash: `${CDN}/languages/bash.min.js`,
    dockerfile: `${CDN}/languages/dockerfile.min.js`,
    ini: `${CDN}/languages/ini.min.js`,
    markdown: `${CDN}/languages/markdown.min.js`,
    plaintext: `${CDN}/languages/plaintext.min.js`,
};

export async function highlightCode(elementId, code, language, themeCssUrl) {
    // Load highlight.js core + languages once
    if (!hljsModule) {
        const mod = await import(`${CDN}/highlight.min.js`);
        hljsModule = mod.default;
    }

    if (!languagesRegistered) {
        const entries = Object.entries(LANGUAGES);
        const modules = await Promise.all(entries.map(([, url]) => import(url)));
        entries.forEach(([name], i) => {
            hljsModule.registerLanguage(name, modules[i].default);
        });
        languagesRegistered = true;
    }

    // Inject theme CSS once and wait for it to load
    if (!themeLoaded && themeCssUrl) {
        await new Promise((resolve) => {
            const link = document.createElement('link');
            link.rel = 'stylesheet';
            link.href = themeCssUrl;
            link.onload = resolve;
            link.onerror = resolve;
            document.head.appendChild(link);
        });
        themeLoaded = true;
    }

    const el = document.getElementById(elementId);
    if (!el) return;

    // Use hljs.highlight() API to get HTML string directly
    // (highlightElement() marks the DOM node and skips it on re-use)
    const result = hljsModule.highlight(code, { language });
    const lines = result.value.split('\n');
    el.className = `hljs language-${language}`;
    el.innerHTML = lines.map((line, i) =>
        `<span class="line"><span class="line-number">${i + 1}</span>${line}</span>`
    ).join('');
}
