// ============================================================================
// CÓNDOR TÁCTICO 3D - SISTEMA DE ICONOGRAFÍA VECTORIAL PROPIA (CERO EMOJIS)
// Emblemas SVG multicapa diseñados a medida con gradientes metálicos y neón
// ============================================================================

window.IconosJuego = (function () {
    const svgDefs = `
        <svg style="position:absolute; width:0; height:0; overflow:hidden;" aria-hidden="true">
            <defs>
                <linearGradient id="grad-oro" x1="0%" y1="0%" x2="100%" y2="100%">
                    <stop offset="0%" stop-color="#fff3b0" />
                    <stop offset="45%" stop-color="#ffb800" />
                    <stop offset="100%" stop-color="#c67c00" />
                </linearGradient>
                <linearGradient id="grad-cian" x1="0%" y1="0%" x2="100%" y2="100%">
                    <stop offset="0%" stop-color="#b8ffff" />
                    <stop offset="50%" stop-color="#00e5ff" />
                    <stop offset="100%" stop-color="#0066cc" />
                </linearGradient>
                <linearGradient id="grad-verde" x1="0%" y1="0%" x2="100%" y2="100%">
                    <stop offset="0%" stop-color="#b9f6ca" />
                    <stop offset="50%" stop-color="#00e676" />
                    <stop offset="100%" stop-color="#008c4a" />
                </linearGradient>
                <linearGradient id="grad-rojo" x1="0%" y1="0%" x2="100%" y2="100%">
                    <stop offset="0%" stop-color="#ff8a80" />
                    <stop offset="50%" stop-color="#ff1744" />
                    <stop offset="100%" stop-color="#9b0020" />
                </linearGradient>
                <linearGradient id="grad-violeta" x1="0%" y1="0%" x2="100%" y2="100%">
                    <stop offset="0%" stop-color="#ea80fc" />
                    <stop offset="50%" stop-color="#d500f9" />
                    <stop offset="100%" stop-color="#6a0080" />
                </linearGradient>
            </defs>
        </svg>
    `;

    const catalogo = {
        jugar: `<svg viewBox="0 0 32 32" class="ico-svg"><polygon points="6,4 28,16 6,28 11,16" fill="url(#grad-verde)"/><polygon points="11,11 21,16 11,21 13,16" fill="#ffffff" opacity="0.85"/><path d="M2,10 L7,16 L2,22" fill="none" stroke="#00e676" stroke-width="2"/></svg>`,

        campana: `<svg viewBox="0 0 32 32" class="ico-svg"><polygon points="16,3 29,10 29,22 16,29 3,22 3,10" fill="none" stroke="url(#grad-cian)" stroke-width="2"/><polygon points="7,21 13,11 18,18 21,13 26,21" fill="url(#grad-cian)" opacity="0.85"/><circle cx="16" cy="8" r="2" fill="url(#grad-oro)"/></svg>`,

        multijugador: `<svg viewBox="0 0 32 32" class="ico-svg"><path d="M10,6 L16,18 L10,15 L4,18 Z" fill="url(#grad-cian)"/><path d="M22,10 L28,22 L22,19 L16,22 Z" fill="url(#grad-oro)"/><path d="M6,25 C12,21 20,21 26,25" fill="none" stroke="#00e5ff" stroke-width="2" stroke-dasharray="3,2"/></svg>`,

        tienda: `<svg viewBox="0 0 32 32" class="ico-svg"><polygon points="16,2 28,8 28,24 16,30 4,24 4,8" fill="none" stroke="url(#grad-oro)" stroke-width="2"/><polygon points="16,7 23,11 16,15 9,11" fill="url(#grad-oro)"/><polygon points="9,13 15,17 15,25 9,21" fill="url(#grad-cian)" opacity="0.85"/><polygon points="23,13 17,17 17,25 23,21" fill="url(#grad-cian)" opacity="0.55"/></svg>`,

        hangar: `<svg viewBox="0 0 32 32" class="ico-svg"><path d="M16,2 L19,11 L29,7 L23,18 L27,26 L19,23 L16,29 L13,23 L5,26 L9,18 L3,7 L13,11 Z" fill="url(#grad-cian)"/><polygon points="16,7 18,14 16,22 14,14" fill="url(#grad-oro)"/></svg>`,

        progreso: `<svg viewBox="0 0 32 32" class="ico-svg"><polyline points="5,24 16,17 27,24" fill="none" stroke="url(#grad-cian)" stroke-width="2.6"/><polyline points="5,18 16,11 27,18" fill="none" stroke="url(#grad-oro)" stroke-width="2.6"/><polygon points="16,3 18.2,7.5 23,8.2 19.5,11.6 20.3,16.4 16,14.1 11.7,16.4 12.5,11.6 9,8.2 13.8,7.5" fill="url(#grad-oro)"/></svg>`,

        ajustes: `<svg viewBox="0 0 32 32" class="ico-svg"><circle cx="16" cy="16" r="5" fill="none" stroke="url(#grad-oro)" stroke-width="2.5"/><path d="M16,3 L18,6 L14,6 Z M16,29 L18,26 L14,26 Z M3,16 L6,18 L6,14 Z M29,16 L26,18 L26,14 Z M6.8,6.8 L9.5,8 L8,9.5 Z M25.2,25.2 L22.5,24 L24,22.5 Z M25.2,6.8 L24,9.5 L22.5,8 Z M6.8,25.2 L8,22.5 L9.5,24 Z" fill="url(#grad-cian)"/><circle cx="16" cy="16" r="9.5" fill="none" stroke="url(#grad-cian)" stroke-width="1.8" stroke-dasharray="4,3"/></svg>`,

        logros: `<svg viewBox="0 0 32 32" class="ico-svg"><path d="M9,5 H23 V13 C23,17.5 19.8,20.5 16,20.5 C12.2,20.5 9,17.5 9,13 Z" fill="url(#grad-oro)"/><path d="M9,7 H4 V12 C4,14.5 6,16 9,16 M23,7 H28 V12 C28,14.5 26,16 23,16" fill="none" stroke="url(#grad-cian)" stroke-width="2"/><rect x="13" y="20.5" width="6" height="4.5" fill="url(#grad-oro)"/><polygon points="9,25 23,25 25,28 7,28" fill="url(#grad-cian)"/></svg>`,

        salir: `<svg viewBox="0 0 32 32" class="ico-svg"><path d="M12,5 H6 V27 H12" fill="none" stroke="url(#grad-rojo)" stroke-width="2.5"/><path d="M11,16 H27 M21,10 L27,16 L21,22" fill="none" stroke="url(#grad-rojo)" stroke-width="2.5"/></svg>`,

        moneda: `<svg viewBox="0 0 32 32" class="ico-svg"><polygon points="16,2 28,9 28,23 16,30 4,23 4,9" fill="url(#grad-oro)" stroke="#fff3b0" stroke-width="1.2"/><polygon points="16,6 24,11 24,21 16,26 8,21 8,11" fill="#1a1200" opacity="0.35"/><path d="M16,8 L18.5,14 L23,11 L19.5,18 L16,24 L12.5,18 L9,11 L13.5,14 Z" fill="#fff3b0"/></svg>`,

        regalo: `<svg viewBox="0 0 32 32" class="ico-svg"><polygon points="6,13 26,13 24,28 8,28" fill="#1a1200" stroke="url(#grad-oro)" stroke-width="2"/><rect x="4" y="9" width="24" height="4" fill="url(#grad-oro)"/><rect x="14" y="9" width="4" height="19" fill="url(#grad-cian)"/><path d="M16,9 C12,3 7,5 9,9 C11,9 14,9 16,9 C18,9 21,9 23,9 C25,5 20,3 16,9 Z" fill="none" stroke="#fff3b0" stroke-width="2"/></svg>`,

        vida: `<svg viewBox="0 0 32 32" class="ico-svg"><polygon points="16,3 28,9 28,23 16,29 4,23 4,9" fill="none" stroke="url(#grad-rojo)" stroke-width="2"/><path d="M13,9 H19 V13 H23 V19 H19 V23 H13 V19 H9 V13 H13 Z" fill="url(#grad-rojo)"/></svg>`,

        escudo: `<svg viewBox="0 0 32 32" class="ico-svg"><path d="M16,2 L28,7 V16 C28,23 22.5,27.8 16,30 C9.5,27.8 4,23 4,16 V7 Z" fill="none" stroke="url(#grad-cian)" stroke-width="2.2"/><path d="M16,6 L24,9.5 V15.5 C24,20.5 20.5,24 16,26 C11.5,24 8,20.5 8,15.5 V9.5 Z" fill="url(#grad-cian)" opacity="0.75"/></svg>`,

        velocidad: `<svg viewBox="0 0 32 32" class="ico-svg"><polygon points="18,2 7,17 16,17 13,30 25,14 16,14" fill="url(#grad-oro)"/></svg>`,

        fuego: `<svg viewBox="0 0 32 32" class="ico-svg"><polygon points="11,4 13,4 14,22 10,22" fill="url(#grad-cian)"/><polygon points="19,4 21,4 22,22 18,22" fill="url(#grad-cian)"/><polygon points="7,22 25,22 23,28 9,28" fill="url(#grad-oro)"/></svg>`,

        poder: `<svg viewBox="0 0 32 32" class="ico-svg"><circle cx="16" cy="16" r="12" fill="none" stroke="url(#grad-cian)" stroke-width="1.8" stroke-dasharray="5,3"/><polygon points="16,4 19,13 28,16 19,19 16,28 13,19 4,16 13,13" fill="url(#grad-oro)"/><circle cx="16" cy="16" r="3.5" fill="#ffffff"/></svg>`,

        estrellaLlena: `<svg viewBox="0 0 32 32" class="ico-svg"><polygon points="16,2 20,11 30,12 22.5,19 24.5,29 16,24 7.5,29 9.5,19 2,12 12,11" fill="url(#grad-oro)" stroke="#fff3b0" stroke-width="1"/></svg>`,

        estrellaVacia: `<svg viewBox="0 0 32 32" class="ico-svg"><polygon points="16,2 20,11 30,12 22.5,19 24.5,29 16,24 7.5,29 9.5,19 2,12 12,11" fill="rgba(255,255,255,0.06)" stroke="rgba(255,255,255,0.28)" stroke-width="1.5"/></svg>`,

        jefe: `<svg viewBox="0 0 32 32" class="ico-svg"><polygon points="2,8 10,13 16,5 22,13 30,8 26,22 16,29 6,22" fill="url(#grad-rojo)"/><polygon points="10,16 15,18 11,21" fill="#ffffff"/><polygon points="22,16 17,18 21,21" fill="#ffffff"/></svg>`,

        candado: `<svg viewBox="0 0 32 32" class="ico-svg"><rect x="7" y="14" width="18" height="14" rx="2" fill="none" stroke="rgba(255,255,255,0.5)" stroke-width="2"/><path d="M11,14 V10 C11,7 13,5 16,5 C19,5 21,7 21,10 V14" fill="none" stroke="rgba(255,255,255,0.5)" stroke-width="2"/><circle cx="16" cy="21" r="2" fill="url(#grad-oro)"/></svg>`,

        check: `<svg viewBox="0 0 32 32" class="ico-svg"><polyline points="5,17 13,25 27,8" fill="none" stroke="url(#grad-verde)" stroke-width="3.5"/></svg>`,

        cerrar: `<svg viewBox="0 0 32 32" class="ico-svg"><path d="M7,7 L25,25 M25,7 L7,25" fill="none" stroke="currentColor" stroke-width="3"/></svg>`,

        pausa: `<svg viewBox="0 0 32 32" class="ico-svg"><rect x="8" y="6" width="5" height="20" fill="url(#grad-cian)"/><rect x="19" y="6" width="5" height="20" fill="url(#grad-cian)"/></svg>`,

        copiar: `<svg viewBox="0 0 32 32" class="ico-svg"><rect x="10" y="8" width="16" height="19" fill="none" stroke="url(#grad-cian)" stroke-width="2"/><path d="M6,22 V4 H21" fill="none" stroke="url(#grad-oro)" stroke-width="2"/></svg>`,

        wifi: `<svg viewBox="0 0 32 32" class="ico-svg"><path d="M3,12 C10,5 22,5 29,12 M7,17 C12,12 20,12 25,17 M11,22 C14,19 18,19 21,22" fill="none" stroke="url(#grad-cian)" stroke-width="2.5"/><circle cx="16" cy="26" r="2.5" fill="url(#grad-oro)"/></svg>`,

        internet: `<svg viewBox="0 0 32 32" class="ico-svg"><circle cx="16" cy="16" r="12" fill="none" stroke="url(#grad-cian)" stroke-width="2"/><ellipse cx="16" cy="16" rx="6" ry="12" fill="none" stroke="url(#grad-oro)" stroke-width="1.6"/><line x1="4" y1="16" x2="28" y2="16" stroke="url(#grad-cian)" stroke-width="1.6"/></svg>`,

        rotar: `<svg viewBox="0 0 32 32" class="ico-svg"><path d="M6,16 C6,10.5 10.5,6 16,6 C20,6 23.5,8.5 25,12 M26,16 C26,21.5 21.5,26 16,26 C12,26 8.5,23.5 7,20" fill="none" stroke="url(#grad-cian)" stroke-width="2.4"/><polygon points="26,6 27,14 19,12" fill="url(#grad-oro)"/><polygon points="6,26 5,18 13,20" fill="url(#grad-oro)"/></svg>`,

        meteoro: `<svg viewBox="0 0 32 32" class="ico-svg"><polygon points="12,3 24,7 29,18 21,29 9,27 4,16 6,7" fill="url(#grad-rojo)" stroke="#ffb800" stroke-width="1.5"/><circle cx="12" cy="14" r="2.5" fill="#ffd54f"/><circle cx="20" cy="20" r="1.8" fill="#ffd54f"/><path d="M4,28 L10,22 M2,20 L6,18 M8,30 L12,25" stroke="#ff3d00" stroke-width="2"/></svg>`,

        alerta: `<svg viewBox="0 0 32 32" class="ico-svg"><polygon points="16,3 30,27 2,27" fill="none" stroke="url(#grad-oro)" stroke-width="2.5"/><line x1="16" y1="11" x2="16" y2="19" stroke="url(#grad-oro)" stroke-width="2.8" stroke-linecap="round"/><circle cx="16" cy="23.5" r="1.5" fill="#ffd54f"/></svg>`,

        pantalla: `<svg viewBox="0 0 32 32" class="ico-svg"><path d="M4,11 V4 H11 M21,4 H28 V11 M28,21 V28 H21 M11,28 H4 V21" fill="none" stroke="url(#grad-cian)" stroke-width="2.5" stroke-linecap="round"/><rect x="8" y="8" width="16" height="16" rx="1.5" fill="none" stroke="url(#grad-oro)" stroke-width="1.6"/></svg>`,

        usuario: `<svg viewBox="0 0 32 32" class="ico-svg"><circle cx="16" cy="10" r="6" fill="url(#grad-cian)"/><path d="M5,28 C5,20 10,18 16,18 C22,18 27,20 27,28 Z" fill="url(#grad-cian)" opacity="0.85"/></svg>`,

        emp: `<svg viewBox="0 0 32 32" class="ico-svg"><circle cx="16" cy="16" r="12" fill="none" stroke="url(#grad-cian)" stroke-width="2" stroke-dasharray="6,3"/><circle cx="16" cy="16" r="7" fill="none" stroke="url(#grad-violeta)" stroke-width="2"/><polygon points="17,6 11,16 16,16 15,26 21,16 16,16" fill="#ffffff"/></svg>`,

        radar: `<svg viewBox="0 0 32 32" class="ico-svg"><circle cx="16" cy="16" r="13" fill="none" stroke="url(#grad-cian)" stroke-width="2"/><circle cx="16" cy="16" r="8" fill="none" stroke="rgba(0,229,255,0.45)" stroke-width="1.4"/><line x1="16" y1="16" x2="26" y2="9" stroke="url(#grad-verde)" stroke-width="2.4"/><circle cx="21" cy="11" r="2.2" fill="url(#grad-rojo)"/><circle cx="16" cy="16" r="2" fill="url(#grad-cian)"/></svg>`,

        diana: `<svg viewBox="0 0 32 32" class="ico-svg"><path d="M4,10 V4 H10 M22,4 H28 V10 M28,22 V28 H22 M10,28 H4 V22" fill="none" stroke="url(#grad-rojo)" stroke-width="2.2"/><circle cx="16" cy="16" r="5" fill="none" stroke="url(#grad-oro)" stroke-width="1.8"/><circle cx="16" cy="16" r="1.8" fill="#ff1744"/></svg>`
    };

    function inyectarDefs() {
        if (!document.getElementById('svg-defs-condor')) {
            const div = document.createElement('div');
            div.id = 'svg-defs-condor';
            div.innerHTML = svgDefs;
            document.body.prepend(div);
        }
    }

    function obtener(nombre, claseExtra = '') {
        const svg = catalogo[nombre] || catalogo.poder;
        if (!claseExtra) return svg;
        return svg.replace('class="ico-svg"', `class="ico-svg ${claseExtra}"`);
    }

    return {
        inyectarDefs,
        obtener
    };
})();
