// ============================================================================
// CÓNDOR TÁCTICO 3D - GESTOR DE INTERFAZ DE USUARIO Y PUENTE NATIVO C#
// Lenguaje 100% claro, amigable e intuitivo para cualquier persona
// ============================================================================

window.InterfazJuego = (function () {
    // Catálogo amigable y claro (sincronizado con GestorEstadoGlobal.cs)
    const catalogoArticulos = [
        // Aviones 3D
        { id: 'NAVE_CONDOR_X', nombre: 'Cóndor Dorado X', categoria: 'Aeronave', precio: 0, color: 0x00dcff, desc: 'El avión insignia oficial: alas de cóndor invertidas, doble cañón dorado y gran equilibrio.' },
        { id: 'NAVE_F35', nombre: 'Caza F-35 Relámpago', categoria: 'Aeronave', precio: 0, color: 0x00e5ff, desc: 'Caza supersónico ultraligero: máxima velocidad de giro y cadencia rápida.' },
        { id: 'NAVE_F22', nombre: 'Caza F-22 Raptor', categoria: 'Aeronave', precio: 450, color: 0x90caf9, desc: 'Caza de superioridad aérea: toberas 2D vectoriales, ala en diamante y maniobrabilidad extrema.' },
        { id: 'NAVE_B2', nombre: 'Bombardero B-2 Espíritu', categoria: 'Aeronave', precio: 600, color: 0x00e676, desc: 'Ala furtiva pesada: dispara 3 láseres al mismo tiempo y tiene +30% de escudo.' },

        // Pinturas / Colores 3D
        { id: 'SKIN_ESTANDAR', nombre: 'Azul Cielo Eléctrico', categoria: 'Apariencia', precio: 0, color: 0x00dcff, desc: 'Acabado metálico azul brillante con luces de neón cian.' },
        { id: 'SKIN_ORO_IMPERIAL', nombre: 'Sol de Oro Inca', categoria: 'Apariencia', precio: 500, color: 0xffb800, desc: 'Pintura dorada de lujo inspirada en el emblema del Cóndor.' },
        { id: 'SKIN_CARMESI', nombre: 'Rojo Fénix', categoria: 'Apariencia', precio: 350, color: 0xff3d57, desc: 'Color rojo intenso de escuadrones de alta velocidad.' },
        { id: 'SKIN_ARTICO_ANDINO', nombre: 'Verde Esmeralda', categoria: 'Apariencia', precio: 300, color: 0x00e676, desc: 'Brillo energético verde esmeralda de alta visibilidad.' },
        { id: 'SKIN_FURTIVO_NOCTURNO', nombre: 'Violeta Galáctico', categoria: 'Apariencia', precio: 400, color: 0xd500f9, desc: 'Estilo nocturno con estelas de energía púrpura.' },

        // Mejoras Permanentes (Niveles 1 a 5)
        { id: 'MEJORA_BLINDAJE', nombre: 'Más Vida (Blindaje)', categoria: 'Mejora', precioBase: 180, nivelMax: 5, desc: 'Aumenta la resistencia de tu avión en +20 puntos de vida por cada nivel.' },
        { id: 'MEJORA_ESCUDO', nombre: 'Más Escudo Protector', categoria: 'Mejora', precioBase: 200, nivelMax: 5, desc: 'Aumenta tu escudo recargable en +15 puntos por cada nivel.' },
        { id: 'MEJORA_MOTOR', nombre: 'Motores Más Rápidos', categoria: 'Mejora', precioBase: 200, nivelMax: 5, desc: 'Hace que tu avión se mueva un +10% más rápido por nivel.' },
        { id: 'MEJORA_CANON', nombre: 'Disparo Rápido', categoria: 'Mejora', precioBase: 220, nivelMax: 5, desc: 'Tus cañones disparan mucho más seguido en cada nivel.' },
        { id: 'MEJORA_ECONOMIA', nombre: 'Imán de Monedas', categoria: 'Mejora', precioBase: 200, nivelMax: 5, desc: 'Ganas un +20% extra de monedas y experiencia en cada partida.' },

        // Superpoderes [Tecla Q]
        { id: 'PODER_EMP', nombre: 'Onda Paralizante', categoria: 'Poder', precio: 0, desc: 'Borra todos los disparos enemigos en pantalla y congela a los rivales por 4 segundos.' },
        { id: 'PODER_ESCUDO', nombre: 'Escudo Invencible', categoria: 'Poder', precio: 400, desc: 'Te vuelve totalmente inmune al daño durante 5 segundos y recarga tu escudo.' },
        { id: 'PODER_OVERDRIVE', nombre: 'Fuego Triple Rápido', categoria: 'Poder', precio: 550, desc: 'Dispara ráfagas triples doradas a máxima velocidad durante 6 segundos.' },
        { id: 'PODER_TERMOBARICO', nombre: 'Explosión Solar', categoria: 'Poder', precio: 750, desc: 'Lanza una onda gigante que daña gravemente a todos los enemigos y jefes.' }
    ];

    const misionesCampana = [
        // MAPA 1: VALLE DEL TITICACA (NIVELES 1 Y 2)
        { nivel: 1, titulo: 'Misión 1: Valle del Titicaca - Incursión Inicial', lugar: 'Lago Titicaca y Cordillera', duracion: '~2 min 30 s (10 Oleadas + Jefe)', peligro: 'Viento de montaña suave', jefe: 'Comandante Kuntur-I', recompensa: 350, desc: 'Defiende el valle sagrado de las patrullas invasoras, esquiva la lluvia de meteoros y derrota a su líder inicial.' },
        { nivel: 2, titulo: 'Misión 2: Valle del Titicaca - Bastión Fluvial', lugar: 'Cuenca del Lago y Cañón Ribereño', duracion: '~2 min 30 s (10 Oleadas + Jefe)', peligro: 'Oleadas aéreas coordinadas', jefe: 'Crucero Fluvial Titikaka', recompensa: 480, desc: 'La flota invasora intenta tomar el estrecho fluvial. Corta su suministro y destruye el crucero fluvial de asalto.' },

        // MAPA 2: CAÑÓN DE LA TORMENTA (NIVELES 3 Y 4)
        { nivel: 3, titulo: 'Misión 3: Cañón de la Tormenta - Vuelo a Baja Cota', lugar: 'Cañón Oriental bajo Relámpagos', duracion: '~2 min 30 s (10 Oleadas + Jefe)', peligro: 'Turbulencia y tormenta eléctrica', jefe: 'Comandante Tempestad', recompensa: 650, desc: 'Vuela por el estrecho cañón sorteando relámpagos continuos y cazas furtivos a alta velocidad.' },
        { nivel: 4, titulo: 'Misión 4: Cañón de la Tormenta - Ojo del Ciclón', lugar: 'Desfiladero Central y Garganta del Rayo', duracion: '~2 min 30 s (10 Oleadas + Jefe)', peligro: 'Descargas electrostáticas continuas', jefe: 'Escuadrón Fantasma B-2', recompensa: 850, desc: 'Penetra en el centro de la tormenta, abate a la escolta pesada y derriba al bombardero furtivo.' },

        // MAPA 3: GLACIAR AUSTRAL (NIVELES 5 Y 6)
        { nivel: 5, titulo: 'Misión 5: Glaciar Austral - Grietas Árticas', lugar: 'Cumbres Nevadas bajo la Aurora', duracion: '~2 min 30 s (10 Oleadas + Jefe)', peligro: 'Ventiscas criogénicas y meteoros de hielo', jefe: 'Vanguardia Ártica Illimani', recompensa: 1100, desc: 'Navega las gargantas de hielo eterno y elimina los puestos avanzados de la división polar enemiga.' },
        { nivel: 6, titulo: 'Misión 6: Glaciar Austral - Fortaleza Helada', lugar: 'Macizo de Hielo y Glaciar Profundo', duracion: '~2 min 30 s (10 Oleadas + Jefe)', peligro: 'Campos de stasis y fuego concentrado', jefe: 'Crucero Glaciar Illimani', recompensa: 1400, desc: 'Asalta la fortaleza aérea congelada que custodia el paso andino hacia el sur y desactiva su reactor.' },

        // MAPA 4: VOLCÁN EN ERUPCIÓN (NIVELES 7 Y 8)
        { nivel: 7, titulo: 'Misión 7: Volcán en Erupción - Río de Magma', lugar: 'Cordillera Volcánica del Sur', duracion: '~2 min 30 s (10 Oleadas + Jefe)', peligro: 'Ríos de lava ardiente y meteoros incandescentes', jefe: 'Caza Ígneo Quimera', recompensa: 1750, desc: 'Sobrevive a la radiación térmica y al bombardeo de asteroides volcánicos sobre el río de magma ardiente.' },
        { nivel: 8, titulo: 'Misión 8: Volcán en Erupción - Caldera del Averno', lugar: 'Caldera Central del Volcán', duracion: '~2 min 30 s (10 Oleadas + Jefe)', peligro: 'Erupción piroclástica extrema', jefe: 'Valkiria Volcánica X-9', recompensa: 2200, desc: 'Enfrenta a la descomunal nave insignia Valkiria en el corazón del volcán y desactiva su núcleo térmico.' },

        // MAPA 5: FORTALEZA ESPACIAL (NIVELES 9 Y 10)
        { nivel: 9, titulo: 'Misión 9: Fortaleza Espacial - Órbita Baja', lugar: 'Frontera Exoatmosférica y Campo de Restos', duracion: '~2 min 30 s (10 Oleadas + Jefe)', peligro: 'Baterías láser espaciales', jefe: 'Dreadnought Centinela Apu', recompensa: 2800, desc: 'Rompe el cerco orbital de satélites artillados antes de que la estación nodriza active su rayo aniquilador.' },
        { nivel: 10, titulo: 'Misión 10: Fortaleza Espacial - Batalla Final Orbital', lugar: 'Núcleo de la Estación Espacial Imperial', duracion: '~2 min 30 s (10 Oleadas + Jefe)', peligro: 'Enjambre imperial y defensa total', jefe: 'Acorazado Orbital Apu-Supay', recompensa: 3800, desc: 'La batalla definitiva por la supervivencia de la Tierra: destruye el núcleo del colosal acorazado Apu-Supay.' }
    ];

    const listaLogros = [
        { id: 'LOGRO_PRIMERA_VICTORIA', titulo: 'Primera Victoria', desc: 'Gana tu primera partida en cualquier modo.', premio: 150 },
        { id: 'LOGRO_CAZADOR_10', titulo: 'Piloto Defensor', desc: 'Derriba un total de 10 aviones enemigos.', premio: 250 },
        { id: 'LOGRO_CAZADOR_50', titulo: 'As de los Cielos', desc: 'Derriba un total de 50 aviones enemigos.', premio: 600 },
        { id: 'LOGRO_COMBO_5', titulo: 'Puntería Perfecta', desc: 'Consigue un Combo x5 sin recibir daño.', premio: 300 },
        { id: 'LOGRO_PRIMER_JEFE', titulo: 'Cazador de Gigantes', desc: 'Derrota a tu primer Jefe Final en el Modo Casual.', premio: 350 },
        { id: 'LOGRO_CAMPANA_COMPLETA', titulo: 'Héroe del Cóndor', desc: 'Completa las 10 misiones del Modo Campaña.', premio: 2500 },
        { id: 'LOGRO_PRIMERA_COMPRA', titulo: 'Avión Mejorado', desc: 'Compra tu primer artículo o mejora en la Tienda.', premio: 150 },
        { id: 'LOGRO_MEJORA_MAX', titulo: 'Potencia Máxima', desc: 'Sube cualquier mejora al Nivel 5 máximo.', premio: 700 },
        { id: 'LOGRO_MILLONARIO', titulo: 'Tesoro Dorado', desc: 'Reúne 2,000 monedas o más en tu cuenta.', premio: 400 },
        { id: 'LOGRO_MULTIJUGADOR', titulo: 'Vuelo con Amigos', desc: 'Juega una partida en el Modo Multijugador.', premio: 300 }
    ];

    let estado = {
        nombrePiloto: 'CÓNDOR-01',
        monedas: 600,
        xp: 0,
        nivelMaxCampana: 1,
        aeronaveEquipada: 'NAVE_CONDOR_X',
        skinEquipada: 'SKIN_ESTANDAR',
        poderEquipado: 'PODER_EMP',
        comprados: ['NAVE_CONDOR_X', 'NAVE_F35', 'NAVE_F22', 'SKIN_ESTANDAR', 'PODER_EMP'],
        mejoras: { MEJORA_BLINDAJE: 0, MEJORA_ESCUDO: 0, MEJORA_MOTOR: 0, MEJORA_CANON: 0, MEJORA_ECONOMIA: 0 },
        estrellasNivel: {},
        logros: [],
        partidas: 0,
        victorias: 0,
        bajas: 0,
        jefes: 0,
        comboMax: 1,
        ultimoDiaRecompensa: '',
        rachaDias: 0,
        config: {
            volumenEfectos: 85,
            audioSilenciado: false,
            controlRaton: false,
            pantallaCompleta: false,
            idioma: 'ES',
            dificultad: 'normal',
            calidadGrafica: 'alta',
            intensidadSacudida: 1.0
        },
        red: {
            ipLocal: '192.168.1.50',
            puerto: 15050,
            codigoSala: 'CT-C0A80132-3ACA-3',
            salasLan: [],
            conectados: 1
        }
    };

    let ultimaConfigPartida = { modalidad: 'CampanaCasual', nivel: 1 };
    let salaMultijugador = null;
    let seccionModalActual = null;

    // ========================================================================
    // PUENTE CON C# WEBVIEW2 Y PERSISTENCIA AUTOMÁTICA
    // ========================================================================
    function enviarComandoNativo(accion, datos = {}) {
        try {
            if (window.chrome && window.chrome.webview) {
                window.chrome.webview.postMessage(JSON.stringify({ accion, ...datos }));
            }
        } catch (e) { }
    }

    function guardarEstadoLocal() {
        try {
            localStorage.setItem('condor_tactico_3d_estado', JSON.stringify(estado));
        } catch (e) { }
        enviarComandoNativo('guardarEstado', { estado });
    }

    function cargarEstadoInicial() {
        try {
            const guardado = localStorage.getItem('condor_tactico_3d_estado');
            if (guardado) {
                const parsed = JSON.parse(guardado);
                estado = Object.assign(estado, parsed);
            }
        } catch (e) { }

        if (window.chrome && window.chrome.webview) {
            window.chrome.webview.addEventListener('message', ev => {
                try {
                    const msg = typeof ev.data === 'string' ? JSON.parse(ev.data) : ev.data;
                    if (msg.tipo === 'sincronizarPerfil' && msg.datos) {
                        estado = Object.assign(estado, msg.datos);
                        refrescarMenuPrincipal();
                    } else if (msg.tipo === 'estadoRed' && msg.red) {
                        estado.red = Object.assign(estado.red, msg.red);
                        actualizarListaLobbyRed();
                        if (msg.red.paquetes && msg.red.paquetes.length > 0) {
                            procesarPaquetesLobby(msg.red.paquetes);
                            if (window.Motor3D && window.Motor3D.sincronizarRed) {
                                window.Motor3D.sincronizarRed(msg.red.paquetes, msg.red.modalidad);
                            }
                        }
                    }
                } catch (e) { }
            });
            enviarComandoNativo('solicitarEstadoInicial');
        }
    }

    function procesarPaquetesLobby(paquetes) {
        if (!paquetes || paquetes.length === 0 || !salaMultijugador) return;
        let huboCambio = false;
        for (let i = 0; i < paquetes.length; i++) {
            const p = paquetes[i];
            if (p.piloto === estado.nombrePiloto) continue;

            if (p.tipo === 'JOIN') {
                const existe = salaMultijugador.jugadores.find(j => j.id === p.id || j.nombre === p.piloto);
                if (!existe && salaMultijugador.jugadores.length < salaMultijugador.jugadoresRequeridos) {
                    salaMultijugador.jugadores.push({
                        id: p.id || ('REMOTO_' + Date.now()),
                        nombre: p.piloto || 'Piloto Remoto',
                        aeronave: 'Caza F-35 Relámpago',
                        equipo: p.equipo || 2,
                        listo: false,
                        ping: 6
                    });
                    mostrarToast('PILOTO CONECTADO: ' + (p.piloto || 'Nuevo Piloto'));
                    huboCambio = true;
                }
            } else if (p.tipo === 'READY') {
                const jug = salaMultijugador.jugadores.find(j => j.id === p.id || j.nombre === p.piloto);
                if (jug) {
                    jug.listo = true;
                    huboCambio = true;
                    mostrarToast(`PILOTO ${p.piloto || ''} MARCÓ: ¡LISTO!`);
                }
            } else if (p.tipo === 'UNREADY') {
                const jug = salaMultijugador.jugadores.find(j => j.id === p.id || j.nombre === p.piloto);
                if (jug) {
                    jug.listo = false;
                    huboCambio = true;
                }
            } else if (p.tipo === 'START') {
                if (!salaMultijugador.esHost) {
                    mostrarToast('¡EL ANFITRIÓN INICIÓ EL DESPEGUE! ENTRANDO A COMBATE...');
                    cerrarModal();
                    lanzarPartida(salaMultijugador.modalidad, 1);
                    return;
                }
            } else if (p.tipo === 'DENIED_PASSWORD') {
                salaMultijugador = null;
                mostrarToast('⚠️ CONTRASEÑA INCORRECTA: El anfitrión de la sala rechazó la conexión.');
                window.Motor3D.sonarEfecto('peligro');
                const pestanas = document.getElementById('modal-pestanas');
                const cuerpo = document.getElementById('modal-cuerpo');
                if (pestanas && cuerpo) renderizarModalMultijugador(pestanas, cuerpo, 'explorador');
                return;
            } else if (p.tipo === 'DISCONNECT') {
                salaMultijugador.jugadores = salaMultijugador.jugadores.filter(j => j.id !== p.id && j.nombre !== p.piloto);
                huboCambio = true;
                mostrarToast(`PILOTO ${p.piloto || ''} SE DESCONECTÓ DE LA SALA`);
            }
        }

        if (huboCambio && seccionModalActual === 'multijugador') {
            const pestanas = document.getElementById('modal-pestanas');
            const cuerpo = document.getElementById('modal-cuerpo');
            if (pestanas && cuerpo) {
                renderizarModalMultijugador(pestanas, cuerpo, 'online');
            }
        }
    }

    function calcularNivelJugador() {
        return 1 + Math.floor(Math.sqrt(Math.max(0, estado.xp) / 120));
    }

    function obtenerRangoAmigable(nivel) {
        const rangos = ['Piloto Novato', 'Cadete del Aire', 'Teniente Cóndor', 'Capitán de Escuadrón', 'Comandante Dorado', 'Leyenda de los Cielos'];
        return rangos[Math.min(rangos.length - 1, nivel - 1)];
    }

    function obtenerColorSkinActual() {
        const item = catalogoArticulos.find(a => a.id === estado.skinEquipada);
        return item ? item.color : 0x00dcff;
    }

    // ========================================================================
    // REFRESCAR DATOS DEL MENÚ PRINCIPAL Y HANGAR 3D
    // ========================================================================
    function refrescarMenuPrincipal() {
        const niv = calcularNivelJugador();
        const xpBase = (niv - 1) * (niv - 1) * 120;
        const xpMeta = niv * niv * 120;
        const pctXp = Math.max(5, Math.min(100, Math.round(((estado.xp - xpBase) / Math.max(1, xpMeta - xpBase)) * 100)));

        document.getElementById('ui-nombre-piloto').textContent = estado.nombrePiloto;
        document.getElementById('ui-rango-piloto').textContent = 'NIVEL ' + niv + ' · ' + obtenerRangoAmigable(niv).toUpperCase();
        document.getElementById('ui-barra-xp').style.width = pctXp + '%';
        document.getElementById('ui-monedas-top').textContent = estado.monedas.toLocaleString();

        const naveInfo = catalogoArticulos.find(a => a.id === estado.aeronaveEquipada) || catalogoArticulos[0];
        const skinInfo = catalogoArticulos.find(a => a.id === estado.skinEquipada) || catalogoArticulos[3];
        const poderInfo = catalogoArticulos.find(a => a.id === estado.poderEquipado) || catalogoArticulos[13];

        const codigosSerie = {
            'NAVE_CONDOR_X': 'CT-01 // STEALTH',
            'NAVE_F35': 'LM-F35 // MULTIRROL',
            'NAVE_F22': 'LM-F22 // DOMINIO AÉREO',
            'NAVE_B2': 'NG-B2 // FURTIVO'
        };
        const elCodigoSerie = document.getElementById('hangar-codigo-serie');
        if (elCodigoSerie) elCodigoSerie.textContent = codigosSerie[estado.aeronaveEquipada] || 'CT-01 // STEALTH';

        document.getElementById('hangar-nombre-nave').textContent = naveInfo.nombre.toUpperCase();
        document.getElementById('hangar-sub-nave').textContent = 'Pintura: ' + skinInfo.nombre + ' · Poder: ' + poderInfo.nombre;

        const nBlindaje = estado.mejoras.MEJORA_BLINDAJE || 0;
        const nEscudo = estado.mejoras.MEJORA_ESCUDO || 0;
        const nMotor = estado.mejoras.MEJORA_MOTOR || 0;
        const nCanon = estado.mejoras.MEJORA_CANON || 0;

        const vidaTotal = 100 + nBlindaje * 20 + (estado.aeronaveEquipada === 'NAVE_CONDOR_X' ? 20 : (estado.aeronaveEquipada === 'NAVE_F22' ? 12 : 0));
        const escudoTotal = 50 + nEscudo * 15 + (estado.aeronaveEquipada === 'NAVE_B2' ? 25 : (estado.aeronaveEquipada === 'NAVE_F22' ? 15 : 0));

        document.getElementById('stat-txt-vida').textContent = vidaTotal + ' PTS';
        document.getElementById('stat-bar-vida').style.width = Math.min(100, (vidaTotal / 220) * 100) + '%';

        document.getElementById('stat-txt-escudo').textContent = escudoTotal + ' PTS';
        document.getElementById('stat-bar-escudo').style.width = Math.min(100, (escudoTotal / 145) * 100) + '%';

        document.getElementById('stat-txt-vel').textContent = (100 + nMotor * 10) + '%';
        document.getElementById('stat-bar-vel').style.width = (50 + nMotor * 10) + '%';

        document.getElementById('stat-txt-fuego').textContent = (100 + nCanon * 15) + '%';
        document.getElementById('stat-bar-fuego').style.width = (50 + nCanon * 10) + '%';

        if (window.Motor3D) {
            window.Motor3D.actualizarNaveHangar3D(estado.aeronaveEquipada, obtenerColorSkinActual());
            window.Motor3D.configurarPreferencias(
                (estado.config.volumenEfectos || 80) / 100,
                !!estado.config.audioSilenciado,
                !!estado.config.controlRaton,
                {
                    dificultad: estado.config.dificultad || 'normal',
                    calidadGrafica: estado.config.calidadGrafica || 'alta',
                    intensidadSacudida: typeof estado.config.intensidadSacudida === 'number' ? estado.config.intensidadSacudida : 1.0
                }
            );
        }
    }

    function ico(nombre, clase = 'ico-inline') {
        return window.IconosJuego ? window.IconosJuego.obtener(nombre, clase) : '';
    }

    function renderizarEstrellasSvg(cantidad) {
        let html = '<div class="estrellas-mision">';
        for (let i = 1; i <= 3; i++) {
            html += window.IconosJuego
                ? window.IconosJuego.obtener(i <= cantidad ? 'estrellaLlena' : 'estrellaVacia')
                : '';
        }
        html += '</div>';
        return html;
    }

    function cambiarNaveRapidoHangar() {
        window.Motor3D.sonarEfecto('click');
        const navesDisponibles = ['NAVE_CONDOR_X', 'NAVE_F35', 'NAVE_F22', 'NAVE_B2'].filter(id => estado.comprados.includes(id));
        const idx = navesDisponibles.indexOf(estado.aeronaveEquipada);
        estado.aeronaveEquipada = navesDisponibles[(idx + 1) % navesDisponibles.length];
        guardarEstadoLocal();
        refrescarMenuPrincipal();
        const naveInfo = catalogoArticulos.find(a => a.id === estado.aeronaveEquipada);
        mostrarToast('AVIÓN EQUIPADO: ' + (naveInfo ? naveInfo.nombre.toUpperCase() : ''));
    }

    function reclamarRecompensaDiaria() {
        window.Motor3D.sonarEfecto('item');
        const hoy = new Date().toISOString().slice(0, 10);
        if (estado.ultimoDiaRecompensa === hoy) {
            mostrarToast('YA RECIBISTE TU REGALO DE HOY · VUELVE MAÑANA POR MÁS MONEDAS');
            return;
        }
        estado.ultimoDiaRecompensa = hoy;
        estado.rachaDias = (estado.rachaDias || 0) + 1;
        const premio = 200 + Math.min(300, (estado.rachaDias - 1) * 50);
        estado.monedas += premio;
        estado.xp += 100;
        guardarEstadoLocal();
        refrescarMenuPrincipal();
        mostrarToast('¡REGALO DIARIO DÍA #' + estado.rachaDias + '! GANASTE +' + premio + ' MONEDAS Y +100 XP');
    }

    // ========================================================================
    // CONSTRUCCIÓN DE LAS VENTANAS MODALES TÁCTICAS (CERO EMOJIS)
    // ========================================================================
    function abrirModal(seccion) {
        window.Motor3D.sonarEfecto('click');
        seccionModalActual = seccion;
        const contenedor = document.getElementById('contenedor-modal');
        const titulo = document.getElementById('modal-titulo');
        const pestanas = document.getElementById('modal-pestanas');
        const cuerpo = document.getElementById('modal-cuerpo');

        contenedor.classList.remove('oculta');
        pestanas.innerHTML = '';
        cuerpo.innerHTML = '';

        if (seccion === 'casual') {
            titulo.innerHTML = `${ico('campana', '')} <span>MODO CASUAL · CAMPAÑA Y ENTRENAMIENTO</span>`;
            renderizarModalCasual(pestanas, cuerpo);
        } else if (seccion === 'multijugador') {
            titulo.innerHTML = `${ico('multijugador', '')} <span>JUGAR CON AMIGOS · RED LOCAL E INTERNET</span>`;
            renderizarModalMultijugador(pestanas, cuerpo);
        } else if (seccion === 'tienda') {
            titulo.innerHTML = `${ico('tienda', '')} <span>TIENDA DE AVIONES 3D, PINTURAS Y MEJORAS</span>`;
            renderizarModalTienda(pestanas, cuerpo, 'todos');
        } else if (seccion === 'inventario') {
            titulo.innerHTML = `${ico('hangar', '')} <span>MI HANGAR · EQUIPA TU AVIÓN 3D</span>`;
            renderizarModalInventario(pestanas, cuerpo);
        } else if (seccion === 'progreso') {
            titulo.innerHTML = `${ico('progreso', '')} <span>PROGRESIÓN DE PILOTO Y RECOMPENSAS</span>`;
            renderizarModalProgreso(pestanas, cuerpo);
        } else if (seccion === 'logros') {
            titulo.innerHTML = `${ico('logros', '')} <span>ESTADÍSTICAS Y LOGROS DE COMBATE</span>`;
            renderizarModalLogros(pestanas, cuerpo);
        } else if (seccion === 'ajustes') {
            titulo.innerHTML = `${ico('ajustes', '')} <span>CONFIGURACIÓN DEL JUEGO</span>`;
            renderizarModalAjustes(pestanas, cuerpo);
        }
    }

    function cerrarModal() {
        window.Motor3D.sonarEfecto('click');
        seccionModalActual = null;
        document.getElementById('contenedor-modal').classList.add('oculta');
    }

    // 1. MODAL MODO CASUAL (5 MUNDOS 3D, TUTORIAL, PARTIDA RÁPIDA)
    function renderizarModalCasual(pestanas, cuerpo) {
        const btnContinuar = document.createElement('button');
        btnContinuar.className = 'tab-btn activo';
        btnContinuar.innerHTML = `${ico('jugar', '')} <span>CONTINUAR EN MISIÓN ${estado.nivelMaxCampana}</span>`;
        btnContinuar.onclick = () => lanzarPartida('CampanaCasual', estado.nivelMaxCampana);

        const btnTutorial = document.createElement('button');
        btnTutorial.className = 'tab-btn';
        btnTutorial.innerHTML = `${ico('progreso', '')} <span>TUTORIAL PASO A PASO (APRENDE A JUGAR)</span>`;
        btnTutorial.onclick = () => lanzarPartida('TutorialInteractivo', 1);

        const btnLibre = document.createElement('button');
        btnLibre.className = 'tab-btn';
        btnLibre.innerHTML = `${ico('velocidad', '')} <span>PARTIDA RÁPIDA DE ACCIÓN</span>`;
        btnLibre.onclick = () => lanzarPartida('BatallaLibre', 1);

        pestanas.append(btnContinuar, btnTutorial, btnLibre);

        const grid = document.createElement('div');
        grid.className = 'grilla-tarjetas';

        misionesCampana.forEach(m => {
            const desbloqueado = m.nivel <= estado.nivelMaxCampana;
            const est = estado.estrellasNivel[m.nivel] || 0;

            const card = document.createElement('div');
            card.className = 'tarjeta-item';
            card.innerHTML = `
                <div>
                    <div class="tarjeta-cabecera-tactica">
                        <span class="tarjeta-badge">${m.lugar}</span>
                        <span class="tarjeta-codigo-serie">SEC-0${m.nivel} // ${desbloqueado ? 'ACTIVO' : 'BLOQUEADO'}</span>
                    </div>
                    <h3 class="tarjeta-titulo" style="margin-top:4px;">${m.titulo}</h3>
                    ${renderizarEstrellasSvg(est)}
                    <p class="tarjeta-desc" style="margin-top:6px;">${m.desc}</p>
                    <div style="margin-top:10px; font-size:12.5px; color:#ffd54f; font-weight:700; display:flex; flex-direction:column; gap:4px; background:rgba(3,9,20,0.55); padding:8px 10px; border-left:2px solid #ffb800;">
                        <span>${ico('velocidad')} <strong>Duración:</strong> ${m.duracion || '~2 min 30 s'}</span>
                        <span>${ico('jefe')} <strong>Jefe Final:</strong> ${m.jefe}</span>
                        <span>${ico('moneda')} <strong>Recompensa:</strong> +${m.recompensa} Monedas</span>
                    </div>
                </div>
                <button class="btn-accion-tarjeta ${desbloqueado ? '' : 'deshabilitado'}">
                    ${desbloqueado
                        ? `${ico('jugar', '')} <span>DESPEGAR EN MISIÓN ${m.nivel}</span>`
                        : `${ico('candado', '')} <span>SUPERA LA MISIÓN ${m.nivel - 1} PRIMERO</span>`}
                </button>
            `;
            if (desbloqueado) {
                card.querySelector('button').onclick = () => lanzarPartida('CampanaCasual', m.nivel);
            }
            grid.appendChild(card);
        });

        cuerpo.appendChild(grid);
    }

    function conectarASalaMultijugador(codigo, modalidad, password, pestanas, cuerpo) {
        if (!codigo) {
            mostrarToast('⚠️ CÓDIGO DE SALA INVÁLIDO');
            return;
        }
        const req = (modalidad === 'Multijugador_1v1') ? 2 : ((modalidad === 'Multijugador_2v2') ? 4 : 2);
        salaMultijugador = {
            codigo: codigo,
            modalidad: modalidad || 'Multijugador_1v1',
            esHost: false,
            password: password || '',
            jugadoresRequeridos: req,
            jugadores: [
                {
                    id: 'CLIENTE',
                    nombre: estado.nombrePiloto + ' (Tú)',
                    aeronave: (catalogoArticulos.find(a => a.id === estado.aeronaveEquipada) || {}).nombre || 'Cóndor Dorado X',
                    equipo: 2,
                    listo: false,
                    ping: 4
                },
                {
                    id: 'HOST_REMOTO',
                    nombre: 'Anfitrión de la Sala',
                    aeronave: 'Caza F-35 Relámpago',
                    equipo: 1,
                    listo: true,
                    ping: 4
                }
            ]
        };

        enviarComandoNativo('unirseConCodigo', { codigo: codigo, password: password || '', equipo: 2 });
        window.Motor3D.sonarEfecto('click');
        mostrarToast(`CONECTANDO A SALA ${codigo}...`);
        renderizarModalMultijugador(pestanas, cuerpo, 'explorador');
    }

    // 2. MODAL MULTIJUGADOR (SALAS DE ESPERA REALES Y VALIDACIÓN ESTRICTA DE JUGADORES)
    function renderizarModalMultijugador(pestanas, cuerpo, modoRed = 'lan') {
        pestanas.innerHTML = '';
        cuerpo.innerHTML = '';

        if (!salaMultijugador) {
            // VISTA 1: EXPLORADOR DE SALAS ACTIVAS, CREACIÓN DE SALAS CON/SIN CONTRASEÑA, Y CÓDIGO DIRECTO
            const pestanaActual = (modoRed === 'crear' || modoRed === 'codigo') ? modoRed : 'explorador';

            const btnExplorador = document.createElement('button');
            const totalSalas = (estado.red.salasLan && Array.isArray(estado.red.salasLan)) ? estado.red.salasLan.length : 0;
            btnExplorador.className = 'tab-btn ' + (pestanaActual === 'explorador' ? 'activo' : '');
            btnExplorador.innerHTML = `${ico('wifi', '')} <span>SALAS ACTIVAS DISPONIBLES (${totalSalas})</span>`;
            btnExplorador.onclick = () => renderizarModalMultijugador(pestanas, cuerpo, 'explorador');

            const btnCrear = document.createElement('button');
            btnCrear.className = 'tab-btn ' + (pestanaActual === 'crear' ? 'activo' : '');
            btnCrear.innerHTML = `${ico('multijugador', '')} <span>CREAR SALA DE COMBATE</span>`;
            btnCrear.onclick = () => renderizarModalMultijugador(pestanas, cuerpo, 'crear');

            const btnCodigo = document.createElement('button');
            btnCodigo.className = 'tab-btn ' + (pestanaActual === 'codigo' ? 'activo' : '');
            btnCodigo.innerHTML = `${ico('copiar', '')} <span>UNIRSE POR CÓDIGO DIRECTO</span>`;
            btnCodigo.onclick = () => renderizarModalMultijugador(pestanas, cuerpo, 'codigo');

            pestanas.append(btnExplorador, btnCrear, btnCodigo);

            if (pestanaActual === 'explorador') {
                // SUB-VISTA A: EXPLORADOR DE SALAS EN VIVO
                const salas = (estado.red.salasLan && Array.isArray(estado.red.salasLan)) ? estado.red.salasLan : [];
                let listaSalasHtml = '';

                if (salas.length > 0) {
                    listaSalasHtml = `
                        <div class="grilla-tarjetas" style="grid-template-columns: repeat(auto-fill, minmax(320px, 1fr)); gap:14px; margin-top:14px;">
                            ${salas.map(s => {
                                const modAmigable = s.modalidad === 'Multijugador_1v1'
                                    ? '1 vs 1 · Duelo Directo'
                                    : (s.modalidad === 'Multijugador_2v2' ? '2 vs 2 · Batalla en Parejas' : 'Cooperativo · Escuadrón Aliado');
                                return `
                                    <div class="tarjeta-item" style="border-color:${s.requierePassword ? 'rgba(255,213,79,0.5)' : 'rgba(0,230,118,0.5)'}; background:rgba(10,24,45,0.7);">
                                        <div>
                                            <div style="display:flex; justify-content:space-between; align-items:center;">
                                                <span class="tarjeta-badge" style="color:${s.requierePassword ? '#ffd54f' : '#00e676'}; border-color:${s.requierePassword ? 'rgba(255,213,79,0.35)' : 'rgba(0,230,118,0.35)'};">
                                                    ${s.requierePassword ? `${ico('candado')} CON CONTRASEÑA` : `${ico('check')} PÚBLICA`}
                                                </span>
                                                <span style="font-size:12px; font-weight:800; color:#00e5ff;">
                                                    ${s.jugadores} / ${s.maxJugadores} PILOTOS
                                                </span>
                                            </div>
                                            <h3 class="tarjeta-titulo" style="margin-top:10px; font-size:17px; color:#fff;">${s.nombre}</h3>
                                            <p class="tarjeta-desc" style="margin-top:6px; font-size:12.5px;">
                                                Anfitrión: <strong>${s.host}</strong><br/>
                                                Modalidad: <strong>${modAmigable}</strong><br/>
                                                Código: <code style="color:var(--cyan);">${s.codigo}</code>
                                            </p>
                                            <div id="mp-pass-box-${s.codigo}" class="oculta" style="margin-top:10px; background:rgba(0,0,0,0.4); padding:10px; border-radius:6px; border:1px solid rgba(255,213,79,0.4);">
                                                <label style="font-size:12px; font-weight:800; color:#ffd54f;">INGRESA LA CONTRASEÑA DE LA SALA:</label>
                                                <div style="display:flex; gap:8px; margin-top:6px;">
                                                    <input id="mp-pass-input-${s.codigo}" type="password" class="input-juego" placeholder="Clave secreta..." style="flex:1;" />
                                                    <button class="btn-confirm-pass tab-btn activo" data-codigo="${s.codigo}" data-modalidad="${s.modalidad}" style="padding:4px 10px;">${ico('check', '')} ENTRAR</button>
                                                </div>
                                            </div>
                                        </div>
                                        <div style="margin-top:14px;">
                                            <button class="btn-unirse-card btn-accion-tarjeta ${s.requierePassword ? 'equipar' : 'comprar'}" data-codigo="${s.codigo}" data-requiere-pass="${s.requierePassword ? '1' : '0'}" data-modalidad="${s.modalidad}">
                                                ${s.requierePassword ? `${ico('candado', '')} <span>INGRESAR CON CONTRASEÑA</span>` : `${ico('jugar', '')} <span>UNIRSE A LA SALA AHORA</span>`}
                                            </button>
                                        </div>
                                    </div>
                                `;
                            }).join('')}
                        </div>
                    `;
                } else {
                    listaSalasHtml = `
                        <div style="background:rgba(0,229,255,0.06); border:1px dashed rgba(0,229,255,0.35); border-radius:10px; padding:32px 20px; text-align:center; margin-top:14px;">
                            <div style="font-size:36px; margin-bottom:10px; color:#00e5ff;">${ico('wifi')}</div>
                            <h3 style="font-size:18px; font-weight:800; color:#fff;">BUSCANDO SALAS ACTIVAS EN LA RED...</h3>
                            <p style="font-size:13.5px; color:#8cb4d8; margin:8px auto; max-width:540px;">
                                El radar está escaneando tu red Wi-Fi / LAN mediante datagramas UDP. En cuanto un amigo cree una sala en cualquier dispositivo de tu red, se listará automáticamente aquí.
                            </p>
                            <div style="display:flex; justify-content:center; gap:12px; margin-top:18px;">
                                <button id="mp-btn-ir-crear" class="btn-accion-tarjeta equipar" style="width:auto; padding:8px 18px;">
                                    ${ico('multijugador', '')} <span>CREAR MI PROPIA SALA</span>
                                </button>
                                <button id="mp-btn-ir-codigo" class="tab-btn activo" style="padding:8px 18px;">
                                    ${ico('copiar', '')} <span>UNIRSE POR CÓDIGO</span>
                                </button>
                            </div>
                        </div>
                    `;
                }

                cuerpo.innerHTML = `
                    <div class="fila-ajuste" style="border-color:rgba(0,229,255,0.45); background:rgba(0,229,255,0.1); margin-bottom:12px; display:flex; justify-content:space-between; align-items:center; flex-wrap:wrap; gap:10px;">
                        <div style="font-size:13.5px; font-weight:600;">
                            ${ico('wifi')} <strong>RADAR MULTIJUGADOR:</strong> Descubre salas abiertas en tu red local o crea una para que tus amigos se conecten.
                        </div>
                        <button id="mp-btn-refrescar-salas" class="tab-btn activo" style="font-size:12px; padding:6px 14px;">
                            ${ico('rotar', '')} <span>ACTUALIZAR RADAR</span>
                        </button>
                    </div>
                    ${listaSalasHtml}
                `;

                document.getElementById('mp-btn-refrescar-salas').onclick = () => {
                    window.Motor3D.sonarEfecto('click');
                    renderizarModalMultijugador(pestanas, cuerpo, 'explorador');
                    mostrarToast('RADAR MULTIJUGADOR ACTUALIZADO');
                };

                const btnIrCrear = document.getElementById('mp-btn-ir-crear');
                if (btnIrCrear) btnIrCrear.onclick = () => renderizarModalMultijugador(pestanas, cuerpo, 'crear');

                const btnIrCod = document.getElementById('mp-btn-ir-codigo');
                if (btnIrCod) btnIrCod.onclick = () => renderizarModalMultijugador(pestanas, cuerpo, 'codigo');

                // Enlaces de unión desde tarjetas
                cuerpo.querySelectorAll('.btn-unirse-card').forEach(b => {
                    b.onclick = () => {
                        const cod = b.getAttribute('data-codigo');
                        const reqPass = b.getAttribute('data-requiere-pass') === '1';
                        const mod = b.getAttribute('data-modalidad') || 'Multijugador_1v1';
                        if (reqPass) {
                            const boxPass = document.getElementById(`mp-pass-box-${cod}`);
                            if (boxPass) {
                                boxPass.classList.toggle('oculta');
                                const inp = document.getElementById(`mp-pass-input-${cod}`);
                                if (inp) inp.focus();
                            }
                        } else {
                            conectarASalaMultijugador(cod, mod, '', pestanas, cuerpo);
                        }
                    };
                });

                cuerpo.querySelectorAll('.btn-confirm-pass').forEach(b => {
                    b.onclick = () => {
                        const cod = b.getAttribute('data-codigo');
                        const mod = b.getAttribute('data-modalidad') || 'Multijugador_1v1';
                        const inp = document.getElementById(`mp-pass-input-${cod}`);
                        const pass = inp ? inp.value.trim() : '';
                        conectarASalaMultijugador(cod, mod, pass, pestanas, cuerpo);
                    };
                });

            } else if (pestanaActual === 'crear') {
                // SUB-VISTA B: CREAR SALA (CON O SIN CONTRASEÑA)
                cuerpo.innerHTML = `
                    <div class="fila-ajuste" style="border-color:rgba(0,229,255,0.45); background:rgba(0,229,255,0.1); margin-bottom:14px;">
                        <div style="font-size:13.5px; font-weight:600;">
                            ${ico('multijugador')} <strong>CREAR SALA:</strong> Configura tu escuadrón. Puedes crear una sala pública o protegerla con contraseña para vuelos privados.
                        </div>
                    </div>
                    <div style="max-width:620px; margin:0 auto; background:rgba(10,22,40,0.7); border:1px solid rgba(0,229,255,0.3); border-radius:10px; padding:22px;">
                        <div style="display:flex; flex-direction:column; gap:14px;">
                            <div>
                                <label style="font-size:13px; font-weight:700; color:#8cb4d8;">NOMBRE DE LA SALA:</label>
                                <input id="mp-crear-nombre" class="input-juego" style="width:100%; margin-top:5px;" value="Escuadrón de ${estado.nombrePiloto}" />
                            </div>
                            <div>
                                <label style="font-size:13px; font-weight:700; color:#8cb4d8;">MODALIDAD DE COMBATE:</label>
                                <select id="mp-crear-modalidad" class="select-juego" style="width:100%; margin-top:5px;">
                                    <option value="Multijugador_1v1">1 VS. 1 · DUELO DIRECTO (REQUIERE 2 JUGADORES)</option>
                                    <option value="Multijugador_2v2">2 VS. 2 · BATALLA POR EQUIPOS (REQUIERE 4 JUGADORES)</option>
                                    <option value="Multijugador_Cooperativo">COOPERATIVO · ESCUADRÓN ALIADO (2 A 4 JUGADORES)</option>
                                </select>
                            </div>
                            <div>
                                <label style="font-size:13px; font-weight:700; color:#8cb4d8;">TU ESCUADRÓN INICIAL:</label>
                                <select id="mp-crear-equipo" class="select-juego" style="width:100%; margin-top:5px;">
                                    <option value="1">ESCUADRÓN CÓNDOR AZUL (EQUIPO ALFA)</option>
                                    <option value="2">ESCUADRÓN FÉNIX DORADO (EQUIPO BRAVO)</option>
                                </select>
                            </div>
                            <div style="background:rgba(0,0,0,0.3); padding:14px; border-radius:6px; border:1px solid rgba(255,255,255,0.12);">
                                <label style="display:flex; align-items:center; gap:10px; cursor:pointer; font-size:13.5px; font-weight:700; color:#fff;">
                                    <input id="mp-chk-password" type="checkbox" style="width:18px; height:18px; cursor:pointer;" />
                                    <span>${ico('candado')} Proteger esta sala con Contraseña</span>
                                </label>
                                <div id="mp-box-password" class="oculta" style="margin-top:10px;">
                                    <label style="font-size:12.5px; font-weight:700; color:#ffd54f;">CONTRASEÑA DE ACCESO:</label>
                                    <input id="mp-crear-password" type="password" class="input-juego" style="width:100%; margin-top:5px;" placeholder="Escribe la clave secreta aquí..." />
                                    <p style="font-size:12px; color:#8cb4d8; margin-top:5px;">Los jugadores deberán ingresar esta contraseña para poder conectarse a tu sala.</p>
                                </div>
                            </div>
                            <button id="mp-btn-confirmar-crear" class="btn-accion-tarjeta equipar" style="margin-top:8px;">
                                ${ico('multijugador', '')} <span>CREAR SALA Y ESPERAR JUGADORES</span>
                            </button>
                        </div>
                    </div>
                `;

                const chkPass = document.getElementById('mp-chk-password');
                const boxPass = document.getElementById('mp-box-password');
                chkPass.onchange = () => {
                    if (chkPass.checked) {
                        boxPass.classList.remove('oculta');
                        document.getElementById('mp-crear-password').focus();
                    } else {
                        boxPass.classList.add('oculta');
                    }
                };

                document.getElementById('mp-btn-confirmar-crear').onclick = () => {
                    const nom = document.getElementById('mp-crear-nombre').value.trim() || `Escuadrón de ${estado.nombrePiloto}`;
                    const mod = document.getElementById('mp-crear-modalidad').value;
                    const eq = parseInt(document.getElementById('mp-crear-equipo').value, 10);
                    const tienePass = chkPass.checked;
                    const pass = tienePass ? document.getElementById('mp-crear-password').value.trim() : '';

                    if (tienePass && !pass) {
                        mostrarToast('⚠️ ESCRIBE UNA CONTRASEÑA O DESMARCA LA PROTECCIÓN');
                        return;
                    }

                    const req = (mod === 'Multijugador_1v1') ? 2 : ((mod === 'Multijugador_2v2') ? 4 : 2);

                    salaMultijugador = {
                        codigo: estado.red.codigoSala,
                        nombreSala: nom,
                        modalidad: mod,
                        esHost: true,
                        requierePassword: tienePass,
                        password: pass,
                        jugadoresRequeridos: req,
                        jugadores: [
                            {
                                id: 'HOST',
                                nombre: estado.nombrePiloto + ' (Tú)',
                                aeronave: (catalogoArticulos.find(a => a.id === estado.aeronaveEquipada) || {}).nombre || 'Cóndor Dorado X',
                                equipo: eq,
                                listo: true,
                                ping: 1
                            }
                        ]
                    };

                    enviarComandoNativo('iniciarSalaRed', {
                        rol: 'host',
                        modalidad: mod,
                        equipo: eq,
                        nombreSala: nom,
                        password: pass
                    });

                    window.Motor3D.sonarEfecto('poder');
                    mostrarToast(tienePass ? '¡SALA PROTEGIDA CREADA! ESPERANDO JUGADORES...' : '¡SALA PÚBLICA CREADA! ESPERANDO JUGADORES...');
                    renderizarModalMultijugador(pestanas, cuerpo, 'explorador');
                };

            } else {
                // SUB-VISTA C: UNIRSE POR CÓDIGO DIRECTO
                cuerpo.innerHTML = `
                    <div class="fila-ajuste" style="border-color:rgba(0,229,255,0.45); background:rgba(0,229,255,0.1); margin-bottom:14px;">
                        <div style="font-size:13.5px; font-weight:600;">
                            ${ico('copiar')} <strong>UNIÓN POR CÓDIGO DIRECTO:</strong> Si tu amigo te envió un código de invitación (ej. <code>CT-7F000001-3ACA-1</code>), ingrésalo aquí para conectarte.
                        </div>
                    </div>
                    <div style="max-width:580px; margin:0 auto; background:rgba(10,22,40,0.7); border:1px solid rgba(0,229,255,0.3); border-radius:10px; padding:22px;">
                        <div style="display:flex; flex-direction:column; gap:14px;">
                            <div>
                                <label style="font-size:13px; font-weight:700; color:#8cb4d8;">CÓDIGO DE INVITACIÓN DE LA SALA:</label>
                                <div style="display:flex; gap:8px; margin-top:5px;">
                                    <input id="mp-input-codigo-directo" class="input-juego" style="flex:1;" placeholder="Ejemplo: CT-C0A80132-3ACA-1" value="${estado.red.codigoSala || ''}" />
                                    <button id="mp-btn-pegar-codigo" class="tab-btn activo">${ico('copiar', '')} <span>PEGAR</span></button>
                                </div>
                            </div>
                            <div>
                                <label style="font-size:13px; font-weight:700; color:#8cb4d8;">CONTRASEÑA (OPCIONAL, SI LA SALA ESTÁ PROTEGIDA):</label>
                                <input id="mp-input-password-directo" type="password" class="input-juego" style="width:100%; margin-top:5px;" placeholder="Dejar en blanco si la sala es pública" />
                            </div>
                            <button id="mp-btn-unirse-codigo-directo" class="btn-accion-tarjeta comprar" style="margin-top:8px;">
                                ${ico('check', '')} <span>UNIRSE A LA SALA CON ESTE CÓDIGO</span>
                            </button>
                        </div>
                    </div>
                `;

                document.getElementById('mp-btn-pegar-codigo').onclick = async () => {
                    try {
                        if (navigator.clipboard) {
                            const txt = await navigator.clipboard.readText();
                            if (txt) document.getElementById('mp-input-codigo-directo').value = txt.trim();
                        }
                    } catch (e) { }
                };

                document.getElementById('mp-btn-unirse-codigo-directo').onclick = () => {
                    const cod = document.getElementById('mp-input-codigo-directo').value.trim();
                    const pass = document.getElementById('mp-input-password-directo').value.trim();
                    if (!cod) {
                        mostrarToast('INGRESA UN CÓDIGO DE SALA VÁLIDO');
                        return;
                    }
                    conectarASalaMultijugador(cod, 'Multijugador_1v1', pass, pestanas, cuerpo);
                };
            }
        } else {
            // VISTA 2: SALA DE ESPERA INTERACTIVA (LOBBY EN VIVO CON SLOTS Y ESTADO DE LISTO)
            const titModalidad = salaMultijugador.modalidad === 'Multijugador_1v1'
                ? '1 VS. 1 · DUELO DIRECTO'
                : (salaMultijugador.modalidad === 'Multijugador_2v2' ? '2 VS. 2 · BATALLA POR EQUIPOS' : 'COOPERATIVO · ESCUADRÓN ALIADO');

            const tabLobby = document.createElement('button');
            tabLobby.className = 'tab-btn activo';
            tabLobby.innerHTML = `${ico('multijugador', '')} <span>SALA ACTIVA: ${titModalidad}</span>`;
            pestanas.appendChild(tabLobby);

            const conectados = salaMultijugador.jugadores.length;
            const requeridos = salaMultijugador.jugadoresRequeridos;
            const todosListos = salaMultijugador.jugadores.every(j => j.listo);
            const puedeIniciar = (conectados >= requeridos) && todosListos;

            let slotsHtml = '';
            for (let i = 0; i < requeridos; i++) {
                const jug = salaMultijugador.jugadores[i];
                if (jug) {
                    const badgeListo = jug.listo
                        ? `<span class="badge-estado-jugador listo">${ico('check')} LISTO PARA COMBATE</span>`
                        : `<span class="badge-estado-jugador esperando">${ico('rotar')} PREPARANDO...</span>`;

                    slotsHtml += `
                        <div class="lobby-slot-card ocupado ${jug.listo ? 'listo' : ''}">
                            <div>
                                <div style="display:flex; justify-content:space-between; align-items:center;">
                                    <span class="tarjeta-badge">SLOT ${i + 1} · ${jug.equipo === 1 ? 'ESCUADRÓN ALFA' : 'ESCUADRÓN BRAVO'}</span>
                                    <span style="font-size:11px; font-weight:700; color:#00e5ff;">PING: ${jug.ping} ms</span>
                                </div>
                                <h3 style="font-size:16px; font-weight:800; color:#fff; margin-top:8px;">${ico('usuario')} ${jug.nombre}</h3>
                                <p style="font-size:12.5px; color:#8cb4d8; margin-top:4px;">${ico('hangar')} Nave: <strong>${jug.aeronave}</strong></p>
                            </div>
                            <div style="margin-top:12px; display:flex; justify-content:space-between; align-items:center;">
                                ${badgeListo}
                            </div>
                        </div>
                    `;
                } else {
                    slotsHtml += `
                        <div class="lobby-slot-card vacio">
                            <div>
                                <span class="tarjeta-badge" style="color:#8cb4d8; border-color:rgba(255,255,255,0.2);">SLOT ${i + 1} · ESPERANDO JUGADOR</span>
                                <h3 style="font-size:15px; font-weight:700; color:rgba(255,255,255,0.5); margin-top:8px;">
                                    ${ico('candado')} ESPERANDO PILOTO...
                                </h3>
                                <p style="font-size:12px; color:rgba(255,255,255,0.4); margin-top:4px;">
                                    Comparte el código o espera conexión en LAN.
                                </p>
                            </div>
                            <div style="margin-top:12px;">
                                <button class="tab-btn btn-add-bot-slot" data-slot="${i}" style="width:100%; font-size:11.5px; padding:6px 10px;">
                                    ${ico('usuario', '')} <span>AÑADIR JUGADOR DE PRUEBA (IA)</span>
                                </button>
                            </div>
                        </div>
                    `;
                }
            }

            const jugadorLocal = salaMultijugador.jugadores.find(j => j.nombre.includes('(Tú)'));
            const estadoLocalListo = jugadorLocal ? jugadorLocal.listo : false;

            cuerpo.innerHTML = `
                <div class="lobby-contenedor">
                    <div class="lobby-cabecera-sala">
                        <div>
                            <span style="font-size:12px; font-weight:800; color:var(--cyan); letter-spacing:1px;">CÓDIGO DE ACCESO DIRECTO:</span>
                            <div style="font-family:var(--font-display); font-size:22px; font-weight:900; color:var(--gold-bright); letter-spacing:2px; margin-top:2px;">
                                ${salaMultijugador.codigo}
                            </div>
                        </div>
                        <div style="display:flex; gap:10px; align-items:center;">
                            <button id="lobby-btn-copiar" class="tab-btn activo">${ico('copiar', '')} <span>COPIAR CÓDIGO</span></button>
                            <button id="lobby-btn-salir" class="tab-btn" style="border-color:#ff1744; color:#ff8a80;">${ico('salir', '')} <span>ABANDONAR SALA</span></button>
                        </div>
                    </div>

                    <div style="display:flex; justify-content:space-between; align-items:center; background:rgba(0,0,0,0.3); padding:8px 14px; border-left:4px solid ${puedeIniciar ? '#00e676' : '#ffd54f'}; font-size:13px; font-weight:700;">
                        <span>${ico(puedeIniciar ? 'check' : 'rotar')} ESTADO DEL LOBBY: <strong>${conectados} de ${requeridos} jugadores</strong> conectados (${salaMultijugador.jugadores.filter(j => j.listo).length} listos)</span>
                        <span style="color:${puedeIniciar ? '#00e676' : '#ffd54f'}; font-weight:800;">
                            ${puedeIniciar ? '¡TODOS LISTOS PARA EL DESPEGUE!' : `FALTAN ${requeridos - conectados > 0 ? (requeridos - conectados) + ' JUGADORES' : 'PILOTOS POR MARCAR LISTO'}`}
                        </span>
                    </div>

                    <div class="lobby-slots-grilla">
                        ${slotsHtml}
                    </div>

                    <div style="display:flex; gap:14px; margin-top:10px; flex-wrap:wrap;">
                        <button id="lobby-btn-toggle-listo" class="btn-accion-tarjeta ${estadoLocalListo ? 'comprar' : 'equipar'}" style="flex:1;">
                            ${estadoLocalListo ? `${ico('check', '')} <span>CANCELAR LISTO (CAMBIAR A PREPARANDO)</span>` : `${ico('check', '')} <span>¡ESTOY LISTO PARA VOLAR!</span>`}
                        </button>
                        <button id="lobby-btn-iniciar-despegue" class="btn-accion-tarjeta ${puedeIniciar ? 'listo-para-despegue' : 'bloqueado'}" style="flex:2;">
                            ${puedeIniciar ? `${ico('jugar', '')} <span>¡DESPEGAR AL COMBATE MULTIJUGADOR (${conectados}/${requeridos} LISTOS)!</span>` : `${ico('candado', '')} <span>DESPEGUE BLOQUEADO: SE REQUIEREN ${requeridos} JUGADORES LISTOS (${salaMultijugador.jugadores.filter(j => j.listo).length}/${requeridos})</span>`}
                        </button>
                    </div>
                </div>
            `;

            document.getElementById('lobby-btn-copiar').onclick = () => {
                if (navigator.clipboard) navigator.clipboard.writeText(salaMultijugador.codigo);
                mostrarToast('CÓDIGO DE SALA COPIADO: ' + salaMultijugador.codigo);
            };

            document.getElementById('lobby-btn-salir').onclick = () => {
                salaMultijugador = null;
                mostrarToast('ABANDONASTE LA SALA DE ESPERA');
                renderizarModalMultijugador(pestanas, cuerpo, modoRed);
            };

            document.getElementById('lobby-btn-toggle-listo').onclick = () => {
                if (jugadorLocal) {
                    jugadorLocal.listo = !jugadorLocal.listo;
                    enviarComandoNativo('marcarListoRed', { listo: jugadorLocal.listo });
                    window.Motor3D.sonarEfecto('click');
                    renderizarModalMultijugador(pestanas, cuerpo, modoRed);
                }
            };

            cuerpo.querySelectorAll('.btn-add-bot-slot').forEach(b => {
                b.onclick = () => {
                    const slotNum = parseInt(b.getAttribute('data-slot'), 10) + 1;
                    salaMultijugador.jugadores.push({
                        id: 'BOT_' + slotNum,
                        nombre: 'Piloto Bravo ' + slotNum + ' [IA]',
                        aeronave: slotNum % 2 === 0 ? 'Caza F-35 Relámpago' : 'Bombardero B-2 Espíritu',
                        equipo: slotNum <= 2 ? 1 : 2,
                        listo: true,
                        ping: 1
                    });
                    window.Motor3D.sonarEfecto('item');
                    mostrarToast('PILOTO DE PRUEBA CONECTADO Y MARCADO COMO LISTO');
                    renderizarModalMultijugador(pestanas, cuerpo, modoRed);
                };
            });

            // REQUISITO ESTRICTO: "no se debe poder iniciar si es que no hay los jugadores requeridos, etc."
            document.getElementById('lobby-btn-iniciar-despegue').onclick = () => {
                if (!puedeIniciar) {
                    window.Motor3D.sonarEfecto('peligro');
                    mostrarToast(`⚠️ DESPEGUE BLOQUEADO: SE REQUIEREN ${requeridos} JUGADORES Y TODOS DEBEN MARCAR 'ESTOY LISTO'`);
                    return;
                }
                window.Motor3D.sonarEfecto('poder');
                enviarComandoNativo('iniciarCombateMultijugador');
                cerrarModal();
                lanzarPartida(salaMultijugador.modalidad, 1);
            };
        }
    }

    function actualizarListaLobbyRed() {
        const el = document.getElementById('mp-lista-salas');
        if (el && estado.red.salasLan && estado.red.salasLan.length > 0) {
            el.innerHTML = `${ico('wifi')} Salas activas en LAN: ` + estado.red.salasLan.map(s => typeof s === 'string' ? s : s.nombre).join(' | ');
        }
        if (seccionModalActual === 'multijugador' && !salaMultijugador) {
            const pestanas = document.getElementById('modal-pestanas');
            const btnExplorador = pestanas ? pestanas.querySelector('button.tab-btn.activo') : null;
            if (btnExplorador && btnExplorador.innerHTML.includes('SALAS ACTIVAS')) {
                const totalSalas = (estado.red.salasLan && Array.isArray(estado.red.salasLan)) ? estado.red.salasLan.length : 0;
                btnExplorador.innerHTML = `${ico('wifi', '')} <span>SALAS ACTIVAS DISPONIBLES (${totalSalas})</span>`;
            }
        }
    }

    // 3. MODAL TIENDA (AVIONES 3D, PINTURAS, MEJORAS Y SUPERPODERES)
    function renderizarModalTienda(pestanas, cuerpo, filtro) {
        pestanas.innerHTML = '';
        cuerpo.innerHTML = '';

        const categorias = [
            { id: 'todos', ico: 'tienda', txt: 'TODO EL CATÁLOGO' },
            { id: 'Aeronave', ico: 'hangar', txt: 'AVIONES 3D' },
            { id: 'Apariencia', ico: 'progreso', txt: 'PINTURAS' },
            { id: 'Mejora', ico: 'ajustes', txt: 'MEJORAS DEL AVIÓN' },
            { id: 'Poder', ico: 'poder', txt: 'SUPERPODERES [Q]' }
        ];

        categorias.forEach(c => {
            const b = document.createElement('button');
            b.className = 'tab-btn ' + (filtro === c.id ? 'activo' : '');
            b.innerHTML = `${ico(c.ico, '')} <span>${c.txt}</span>`;
            b.onclick = () => renderizarModalTienda(pestanas, cuerpo, c.id);
            pestanas.appendChild(b);
        });

        const grid = document.createElement('div');
        grid.className = 'grilla-tarjetas';

        const items = catalogoArticulos.filter(a => filtro === 'todos' || a.categoria === filtro);

        items.forEach(art => {
            const card = document.createElement('div');
            card.className = 'tarjeta-item';

            if (art.categoria === 'Mejora') {
                const nivelActual = estado.mejoras[art.id] || 0;
                const esMax = nivelActual >= art.nivelMax;
                const costo = art.precioBase * (nivelActual + 1);
                const pctNivel = Math.round((nivelActual / art.nivelMax) * 100);

                card.innerHTML = `
                    <div>
                        <span class="tarjeta-badge">MEJORA PERMANENTE</span>
                        <h3 class="tarjeta-titulo" style="margin-top:10px;">${art.nombre}</h3>
                        <p class="tarjeta-desc" style="margin-top:6px;">${art.desc}</p>
                        <div style="margin-top:12px;">
                            <div style="display:flex; justify-content:space-between; font-size:12px; font-weight:800; color:#00e5ff; margin-bottom:4px;">
                                <span>NIVEL DE MEJORA</span>
                                <span>${nivelActual} / ${art.nivelMax}</span>
                            </div>
                            <div class="barra-stat-fondo"><div class="barra-stat-fill oro" style="width:${pctNivel}%;"></div></div>
                        </div>
                    </div>
                    <button class="btn-accion-tarjeta ${esMax ? 'deshabilitado' : 'comprar'}">
                        ${esMax
                            ? `${ico('check', '')} <span>MEJORADO AL MÁXIMO</span>`
                            : `${ico('moneda', '')} <span>MEJORAR POR ${costo} MONEDAS</span>`}
                    </button>
                `;

                if (!esMax) {
                    card.querySelector('button').onclick = () => comprarMejora(art, costo);
                }
            } else {
                const adquirido = estado.comprados.includes(art.id);
                const equipado = (estado.aeronaveEquipada === art.id || estado.skinEquipada === art.id || estado.poderEquipado === art.id);

                card.innerHTML = `
                    <div>
                        <span class="tarjeta-badge">${art.categoria.toUpperCase()}</span>
                        <h3 class="tarjeta-titulo" style="margin-top:10px;">${art.nombre}</h3>
                        <p class="tarjeta-desc" style="margin-top:6px;">${art.desc}</p>
                        <div style="margin-top:10px; font-weight:800; font-size:13.5px; color:${adquirido ? '#00e676' : '#ffd54f'};">
                            ${adquirido ? `${ico('check')} Disponible en tu Hangar` : `${ico('moneda')} Precio: ${art.precio} Monedas`}
                        </div>
                    </div>
                    <button class="btn-accion-tarjeta ${equipado ? 'deshabilitado' : (adquirido ? 'equipar' : 'comprar')}">
                        ${equipado
                            ? `${ico('check', '')} <span>EQUIPADO ACTUALMENTE</span>`
                            : (adquirido
                                ? `${ico('hangar', '')} <span>EQUIPAR AHORA</span>`
                                : `${ico('moneda', '')} <span>COMPRAR POR ${art.precio}</span>`)}
                    </button>
                `;

                if (!equipado) {
                    card.querySelector('button').onclick = () => {
                        if (adquirido) equiparItem(art);
                        else comprarItem(art);
                    };
                }
            }

            grid.appendChild(card);
        });

        cuerpo.appendChild(grid);
    }

    function comprarMejora(art, costo) {
        if (estado.monedas < costo) {
            mostrarToast('MONEDAS INSUFICIENTES · SUPERA UNA MISIÓN PARA GANAR MÁS');
            return;
        }
        window.Motor3D.sonarEfecto('item');
        estado.monedas -= costo;
        estado.mejoras[art.id] = (estado.mejoras[art.id] || 0) + 1;
        verificarNuevosLogros();
        guardarEstadoLocal();
        refrescarMenuPrincipal();
        abrirModal('tienda');
        mostrarToast('¡' + art.nombre.toUpperCase() + ' SUBIÓ A NIVEL ' + estado.mejoras[art.id] + '!');
    }

    function comprarItem(art) {
        if (estado.monedas < art.precio) {
            mostrarToast('NO TIENES SUFICIENTES MONEDAS PARA COMPRAR ' + art.nombre.toUpperCase());
            return;
        }
        window.Motor3D.sonarEfecto('item');
        estado.monedas -= art.precio;
        if (!estado.comprados.includes(art.id)) estado.comprados.push(art.id);
        equiparItem(art, false);
        verificarNuevosLogros();
        guardarEstadoLocal();
        refrescarMenuPrincipal();
        abrirModal('tienda');
        mostrarToast('¡COMPRASTE Y EQUIPASTE ' + art.nombre.toUpperCase() + '!');
    }

    function equiparItem(art, mostrarAviso = true) {
        window.Motor3D.sonarEfecto('click');
        if (art.categoria === 'Aeronave') estado.aeronaveEquipada = art.id;
        if (art.categoria === 'Apariencia') estado.skinEquipada = art.id;
        if (art.categoria === 'Poder') estado.poderEquipado = art.id;
        guardarEstadoLocal();
        refrescarMenuPrincipal();
        if (mostrarAviso) {
            mostrarToast('EQUIPADO: ' + art.nombre.toUpperCase());
            abrirModal('inventario');
        }
    }

    // 4. MODAL INVENTARIO / MI HANGAR
    function renderizarModalInventario(pestanas, cuerpo) {
        const adquiridos = catalogoArticulos.filter(a => a.categoria !== 'Mejora' && estado.comprados.includes(a.id));
        const grid = document.createElement('div');
        grid.className = 'grilla-tarjetas';

        adquiridos.forEach(art => {
            const equipado = (estado.aeronaveEquipada === art.id || estado.skinEquipada === art.id || estado.poderEquipado === art.id);
            const card = document.createElement('div');
            card.className = 'tarjeta-item';
            card.innerHTML = `
                <div>
                    <span class="tarjeta-badge">${art.categoria.toUpperCase()} DESBLOQUEADO</span>
                    <h3 class="tarjeta-titulo" style="margin-top:10px;">${art.nombre}</h3>
                    <p class="tarjeta-desc" style="margin-top:6px;">${art.desc}</p>
                </div>
                <button class="btn-accion-tarjeta ${equipado ? 'deshabilitado' : 'equipar'}">
                    ${equipado
                        ? `${ico('check', '')} <span>EN USO AHORA</span>`
                        : `${ico('hangar', '')} <span>EQUIPAR EN MI AVIÓN</span>`}
                </button>
            `;
            if (!equipado) {
                card.querySelector('button').onclick = () => equiparItem(art, true);
            }
            grid.appendChild(card);
        });

        cuerpo.appendChild(grid);
    }

    // 5. MODAL PROGRESIÓN Y RECOMPENSAS
    function renderizarModalProgreso(pestanas, cuerpo) {
        const niv = calcularNivelJugador();
        const totalEstrellas = Object.values(estado.estrellasNivel).reduce((a, b) => a + b, 0);

        cuerpo.innerHTML = `
            <div class="grilla-tarjetas">
                <div class="tarjeta-item">
                    <div>
                        <span class="tarjeta-badge">RANGO DE PILOTO</span>
                        <h3 class="tarjeta-titulo" style="margin-top:10px; font-size:22px; color:#00e5ff;">Nivel ${niv} · ${obtenerRangoAmigable(niv)}</h3>
                        <p class="tarjeta-desc" style="margin-top:10px;">Experiencia Total Acumulada: <strong style="color:#fff;">${estado.xp.toLocaleString()} XP</strong></p>
                        <p class="tarjeta-desc" style="margin-top:6px;">Estrellas de Campaña: <strong style="color:#ffd54f;">${totalEstrellas} de 15</strong></p>
                    </div>
                    <button id="prog-btn-diario" class="btn-accion-tarjeta comprar">
                        ${ico('regalo', '')} <span>RECLAMAR REGALO DIARIO</span>
                    </button>
                </div>

                <div class="tarjeta-item">
                    <div>
                        <span class="tarjeta-badge">ESCALAFÓN OFICIAL</span>
                        <h3 class="tarjeta-titulo" style="margin-top:10px;">Jerarquía de Ascenso</h3>
                        <p class="tarjeta-desc" style="margin-top:10px; line-height:1.75;">
                            ◆ Nivel 1: Piloto Novato<br>
                            ◆ Nivel 2: Cadete del Aire<br>
                            ◆ Nivel 3: Teniente Cóndor<br>
                            ◆ Nivel 4: Capitán de Escuadrón<br>
                            ◆ Nivel 5: Comandante Dorado<br>
                            ◆ Nivel 6+: Leyenda de los Cielos
                        </p>
                    </div>
                </div>
            </div>
        `;

        document.getElementById('prog-btn-diario').onclick = reclamarRecompensaDiaria;
    }

    // 6. MODAL ESTADÍSTICAS Y LOGROS
    function renderizarModalLogros(pestanas, cuerpo) {
        const resumen = document.createElement('div');
        resumen.style.cssText = 'display:flex; gap:14px; flex-wrap:wrap; margin-bottom:18px;';
        resumen.innerHTML = `
            <div class="fila-ajuste" style="flex:1;"><span>PARTIDAS JUGADAS</span><strong>${estado.partidas}</strong></div>
            <div class="fila-ajuste" style="flex:1;"><span>VICTORIAS</span><strong style="color:#00e676;">${estado.victorias}</strong></div>
            <div class="fila-ajuste" style="flex:1;"><span>DERRIBOS TOTALES</span><strong style="color:#00e5ff;">${estado.bajas}</strong></div>
            <div class="fila-ajuste" style="flex:1;"><span>MEJOR COMBO</span><strong style="color:#ffd54f;">x${estado.comboMax}</strong></div>
        `;
        cuerpo.appendChild(resumen);

        const grid = document.createElement('div');
        grid.className = 'grilla-tarjetas';

        listaLogros.forEach(logro => {
            const completado = estado.logros.includes(logro.id);
            const card = document.createElement('div');
            card.className = 'tarjeta-item';
            card.innerHTML = `
                <div>
                    <span class="tarjeta-badge">${completado ? 'DESBLOQUEADO' : 'EN PROGRESO'}</span>
                    <h3 class="tarjeta-titulo" style="margin-top:10px;">${logro.titulo}</h3>
                    <p class="tarjeta-desc" style="margin-top:6px;">${logro.desc}</p>
                </div>
                <div style="font-weight:800; font-size:13.5px; color:${completado ? '#00e676' : '#ffd54f'};">
                    ${completado
                        ? `${ico('check')} Recompensa Cobrada (+${logro.premio})`
                        : `${ico('moneda')} Premio: +${logro.premio} Monedas`}
                </div>
            `;
            grid.appendChild(card);
        });

        cuerpo.appendChild(grid);
    }

    // 7. MODAL AJUSTES (CONTROLES FÁCILES, SONIDO, PANTALLA Y ACCESIBILIDAD)
    function renderizarModalAjustes(pestanas, cuerpo) {
        cuerpo.innerHTML = `
            <div style="max-width:720px; margin:0 auto;">
                <div class="fila-ajuste">
                    <div>
                        <strong style="font-size:15px;">NOMBRE DE TU PILOTO</strong>
                        <p class="tarjeta-desc">Aparecerá en la barra superior y en las salas multijugador.</p>
                    </div>
                    <input id="cfg-nombre" class="input-juego" value="${estado.nombrePiloto}" />
                </div>

                <div class="fila-ajuste">
                    <div>
                        <strong style="font-size:15px;">VOLUMEN DE EFECTOS DE SONIDO</strong>
                        <p class="tarjeta-desc">Ajusta la potencia de los disparos láser, turbinas y explosiones.</p>
                    </div>
                    <input id="cfg-volumen" type="range" min="0" max="100" value="${estado.config.volumenEfectos}" />
                </div>

                <div class="fila-ajuste">
                    <div>
                        <strong style="font-size:15px;">SILENCIAR AUDIO</strong>
                        <p class="tarjeta-desc">Desactiva todos los sonidos del juego.</p>
                    </div>
                    <input id="cfg-silencio" type="checkbox" style="width:22px; height:22px;" ${estado.config.audioSilenciado ? 'checked' : ''} />
                </div>

                <div class="fila-ajuste">
                    <div>
                        <strong style="font-size:15px;">MODO DE CONTROL DE VUELO</strong>
                        <p class="tarjeta-desc">Elige entre volar con el Teclado (Flechas / WASD) o siguiendo el Ratón.</p>
                    </div>
                    <select id="cfg-control" class="select-juego">
                        <option value="teclado" ${!estado.config.controlRaton ? 'selected' : ''}>TECLADO (FLECHAS / WASD + ESPACIO)</option>
                        <option value="raton" ${estado.config.controlRaton ? 'selected' : ''}>SEGUIR EL RATÓN (+ CLIC PARA DISPARAR)</option>
                    </select>
                </div>

                <div class="fila-ajuste">
                    <div>
                        <strong style="font-size:15px;">DIFICULTAD</strong>
                        <p class="tarjeta-desc">Fácil: menos daño y enemigos lentos al disparar. Difícil: apuntan a donde te mueves, pero das +30% de puntos.</p>
                    </div>
                    <select id="cfg-dificultad" class="select-juego">
                        <option value="facil" ${estado.config.dificultad === 'facil' ? 'selected' : ''}>FÁCIL (RELAJADO)</option>
                        <option value="normal" ${!estado.config.dificultad || estado.config.dificultad === 'normal' ? 'selected' : ''}>NORMAL (RECOMENDADO)</option>
                        <option value="dificil" ${estado.config.dificultad === 'dificil' ? 'selected' : ''}>DIFÍCIL (AS DEL AIRE)</option>
                    </select>
                </div>

                <div class="fila-ajuste">
                    <div>
                        <strong style="font-size:15px;">CALIDAD GRÁFICA</strong>
                        <p class="tarjeta-desc">Baja quita las sombras y efectos de velocidad para equipos modestos.</p>
                    </div>
                    <select id="cfg-calidad" class="select-juego">
                        <option value="alta" ${!estado.config.calidadGrafica || estado.config.calidadGrafica === 'alta' ? 'selected' : ''}>ALTA (SOMBRAS NÍTIDAS)</option>
                        <option value="media" ${estado.config.calidadGrafica === 'media' ? 'selected' : ''}>MEDIA (EQUILIBRADA)</option>
                        <option value="baja" ${estado.config.calidadGrafica === 'baja' ? 'selected' : ''}>BAJA (MÁXIMO RENDIMIENTO)</option>
                    </select>
                </div>

                <div class="fila-ajuste">
                    <div>
                        <strong style="font-size:15px;">VIBRACIÓN DE CÁMARA</strong>
                        <p class="tarjeta-desc">Intensidad del temblor en explosiones e impactos (0 = desactivado, útil si te mareas).</p>
                    </div>
                    <input id="cfg-sacudida" type="range" min="0" max="100" value="${Math.round((typeof estado.config.intensidadSacudida === 'number' ? estado.config.intensidadSacudida : 1) * 100)}" />
                </div>

                <div class="fila-ajuste">
                    <div>
                        <strong style="font-size:15px;">PANTALLA COMPLETA</strong>
                        <p class="tarjeta-desc">Amplía la ventana a toda la pantalla de tu monitor.</p>
                    </div>
                    <button id="cfg-btn-fullscreen" class="tab-btn activo">${ico('ajustes', '')} <span>ALTERNAR PANTALLA COMPLETA</span></button>
                </div>

                <div style="display:flex; gap:12px; margin-top:22px;">
                    <button id="cfg-btn-guardar" class="btn-accion-tarjeta">${ico('check', '')} <span>GUARDAR MIS AJUSTES</span></button>
                    <button id="cfg-btn-reset" class="btn-accion-tarjeta comprar">${ico('rotar', '')} <span>RESTAURAR VALORES ORIGINALES</span></button>
                </div>
            </div>
        `;

        document.getElementById('cfg-btn-fullscreen').onclick = () => {
            enviarComandoNativo('alternarPantallaCompleta');
            if (!document.fullscreenElement) {
                document.documentElement.requestFullscreen().catch(() => { });
            } else {
                document.exitFullscreen().catch(() => { });
            }
        };

        document.getElementById('cfg-btn-guardar').onclick = () => {
            estado.nombrePiloto = document.getElementById('cfg-nombre').value.trim() || 'CÓNDOR-01';
            estado.config.volumenEfectos = parseInt(document.getElementById('cfg-volumen').value, 10);
            estado.config.audioSilenciado = document.getElementById('cfg-silencio').checked;
            estado.config.controlRaton = document.getElementById('cfg-control').value === 'raton';
            estado.config.dificultad = document.getElementById('cfg-dificultad').value;
            estado.config.calidadGrafica = document.getElementById('cfg-calidad').value;
            estado.config.intensidadSacudida = parseInt(document.getElementById('cfg-sacudida').value, 10) / 100;
            guardarEstadoLocal();
            refrescarMenuPrincipal();
            cerrarModal();
            mostrarToast('TUS AJUSTES FUERON GUARDADOS CORRECTAMENTE');
        };

        document.getElementById('cfg-btn-reset').onclick = () => {
            estado.config = { volumenEfectos: 85, audioSilenciado: false, controlRaton: false, pantallaCompleta: false, idioma: 'ES', dificultad: 'normal', calidadGrafica: 'alta', intensidadSacudida: 1.0 };
            guardarEstadoLocal();
            refrescarMenuPrincipal();
            abrirModal('ajustes');
            mostrarToast('SE RESTAURARON LOS AJUSTES ORIGINALES');
        };
    }

    // ========================================================================
    // LANZAMIENTO DE PARTIDAS Y PANTALLA DE FIN DE MISIÓN
    // ========================================================================
    function lanzarPartida(modalidad, nivel) {
        cerrarModal();
        ultimaConfigPartida = { modalidad, nivel };

        document.getElementById('capa-menu-principal').classList.add('oculta');
        document.getElementById('overlay-resultado').classList.add('oculta');
        document.getElementById('hud-combate').classList.remove('oculta');

        const nBlindaje = estado.mejoras.MEJORA_BLINDAJE || 0;
        const nEscudo = estado.mejoras.MEJORA_ESCUDO || 0;
        const nMotor = estado.mejoras.MEJORA_MOTOR || 0;
        const nCanon = estado.mejoras.MEJORA_CANON || 0;

        window.Motor3D.iniciarPartida3D({
            modalidad: modalidad,
            nivel: nivel,
            modeloId: estado.aeronaveEquipada,
            colorSkin: obtenerColorSkinActual(),
            vidaMax: 100 + nBlindaje * 20 + (estado.aeronaveEquipada === 'NAVE_CONDOR_X' ? 20 : (estado.aeronaveEquipada === 'NAVE_F22' ? 12 : 0)),
            escudoMax: 50 + nEscudo * 15 + (estado.aeronaveEquipada === 'NAVE_B2' ? 25 : (estado.aeronaveEquipada === 'NAVE_F22' ? 15 : 0)),
            velocidad: 21 * (1 + nMotor * 0.1),
            cadenciaFactor: 1 + nCanon * 0.15
        });
    }

    function volverAlMenuPrincipal() {
        window.Motor3D.activarModoHangar();
        document.getElementById('hud-combate').classList.add('oculta');
        document.getElementById('overlay-resultado').classList.add('oculta');
        document.getElementById('capa-menu-principal').classList.remove('oculta');
        refrescarMenuPrincipal();
    }

    function mostrarOverlayPausa(pausado, tieneCheckpoint) {
        const overlay = document.getElementById('overlay-resultado');
        if (!pausado) {
            overlay.classList.add('oculta');
            return;
        }
        overlay.classList.remove('oculta');
        overlay.innerHTML = `
            <div class="caja-resultado">
                <h2 style="color:#00e5ff;">JUEGO EN PAUSA</h2>
                <p>Tómate un respiro. Todo tu progreso está guardado.</p>
                <div class="botones-resultado">
                    <button id="pausa-btn-reanudar" class="btn-accion-tarjeta">${ico('jugar', '')} <span>CONTINUAR JUGANDO</span></button>
                    ${tieneCheckpoint ? `<button id="pausa-btn-check" class="btn-accion-tarjeta equipar">${ico('jefe', '')} <span>REINICIAR DESDE EL JEFE FINAL</span></button>` : ''}
                    <button id="pausa-btn-reiniciar" class="btn-accion-tarjeta comprar">${ico('rotar', '')} <span>REINICIAR MISIÓN COMPLETA</span></button>
                    <button id="pausa-btn-menu" class="btn-accion-tarjeta deshabilitado">${ico('salir', '')} <span>VOLVER AL MENÚ PRINCIPAL</span></button>
                </div>
            </div>
        `;
        document.getElementById('pausa-btn-reanudar').onclick = () => window.Motor3D.alternarPausa();
        if (tieneCheckpoint) {
            document.getElementById('pausa-btn-check').onclick = () => window.Motor3D.reintentarMision(true);
        }
        document.getElementById('pausa-btn-reiniciar').onclick = () => window.Motor3D.reintentarMision(false);
        document.getElementById('pausa-btn-menu').onclick = volverAlMenuPrincipal;
    }

    function alTerminarPartida(res) {
        estado.partidas++;
        if (res.victoria) estado.victorias++;
        estado.bajas += res.bajas;
        if (res.jefeDerrotado) estado.jefes++;
        if (res.comboMax > estado.comboMax) estado.comboMax = res.comboMax;

        const bonoEco = 1 + (estado.mejoras.MEJORA_ECONOMIA || 0) * 0.2;
        const misionInfo = misionesCampana[res.nivelCampana - 1] || misionesCampana[0];
        const baseMonedas = (res.victoria ? misionInfo.recompensa : 60) + res.monedasExtra + res.bajas * 15;
        const monedasGanadas = Math.round(baseMonedas * bonoEco);
        const xpGanada = Math.round((res.victoria ? 180 : 50) * bonoEco);

        estado.monedas += monedasGanadas;
        estado.xp += xpGanada;

        if (res.victoria && res.modalidad === 'CampanaCasual') {
            if (res.nivelCampana >= estado.nivelMaxCampana && res.nivelCampana < 10) {
                estado.nivelMaxCampana = res.nivelCampana + 1;
            }
            const prevEst = estado.estrellasNivel[res.nivelCampana] || 0;
            if (res.estrellas > prevEst) estado.estrellasNivel[res.nivelCampana] = res.estrellas;
        }

        const nuevosLogros = verificarNuevosLogros();
        guardarEstadoLocal();
        enviarComandoNativo('registrarResultadoPartida', res);

        const overlay = document.getElementById('overlay-resultado');
        overlay.classList.remove('oculta');

        overlay.innerHTML = `
            <div class="caja-resultado">
                <h2 style="color:${res.victoria ? '#00e676' : '#ff1744'};">
                    ${res.victoria ? '¡MISIÓN COMPLETADA!' : (res.motivoDerrota === 'COLISION_METEORO' ? '💥 IMPACTO CRÍTICO CON METEORO' : 'AVIÓN DERRIBADO')}
                </h2>
                ${(!res.victoria && res.motivoDerrota === 'COLISION_METEORO') ? `
                    <div class="caja-alerta-derrota">
                        ${ico('meteoro')} <strong>¡COLISIÓN DIRECTA CON METEORO!</strong><br/>
                        El choque final contra un meteoro ardiente destruyó tu avión. Cada impacto con roca arrasa tu escudo y gran parte del casco: esquívalos con [A / D], atraviésalos con un tonel evasivo [SHIFT] o destrúyelos a distancia con tus cañones láser.
                    </div>
                ` : ''}
                <div style="display:flex; justify-content:center; margin:10px 0;">
                    ${renderizarEstrellasSvg(res.estrellas)}
                </div>
                <p style="font-size:15px; color:#fff; margin-bottom:12px;">
                    Puntos: <strong>${res.puntaje.toLocaleString()}</strong> · Derribos: <strong>${res.bajas}</strong> · Mejor Combo: <strong>x${res.comboMax}</strong>
                </p>
                <div style="background:rgba(255,184,0,0.14); border:1px solid rgba(255,184,0,0.45); padding:12px; font-weight:800; color:#ffd54f;">
                    ${ico('moneda')} +${monedasGanadas} Monedas &nbsp;|&nbsp; ${ico('progreso')} +${xpGanada} XP
                </div>
                ${nuevosLogros.length > 0 ? `<div style="margin-top:10px; color:#00e676; font-weight:800;">${ico('logros')} ¡Nuevo Logro: ${nuevosLogros.join(', ')}!</div>` : ''}
                <div class="botones-resultado">
                    ${(res.victoria && res.modalidad === 'CampanaCasual' && res.nivelCampana < 10)
                        ? `<button id="fin-btn-sig" class="btn-accion-tarjeta">${ico('jugar', '')} <span>JUGAR SIGUIENTE MISIÓN (MISIÓN ${res.nivelCampana + 1})</span></button>`
                        : ''}
                    <button id="fin-btn-reintentar" class="btn-accion-tarjeta equipar">${ico('rotar', '')} <span>VOLVER A JUGAR / REVANCHA</span></button>
                    <button id="fin-btn-tienda" class="btn-accion-tarjeta comprar">${ico('tienda', '')} <span>IR A LA TIENDA DE MEJORAS</span></button>
                    <button id="fin-btn-menu" class="btn-accion-tarjeta deshabilitado">${ico('salir', '')} <span>VOLVER AL MENÚ PRINCIPAL</span></button>
                </div>
            </div>
        `;

        const btnSig = document.getElementById('fin-btn-sig');
        if (btnSig) btnSig.onclick = () => window.Motor3D.avanzarSiguienteNivel();
        document.getElementById('fin-btn-reintentar').onclick = () => window.Motor3D.reintentarMision(false);
        document.getElementById('fin-btn-tienda').onclick = () => {
            volverAlMenuPrincipal();
            abrirModal('tienda');
        };
        document.getElementById('fin-btn-menu').onclick = volverAlMenuPrincipal;
    }

    function verificarNuevosLogros() {
        const recien = [];
        function evalLogro(id, cond) {
            if (!cond || estado.logros.includes(id)) return;
            estado.logros.push(id);
            const info = listaLogros.find(l => l.id === id);
            if (info) {
                estado.monedas += info.premio;
                recien.push(info.titulo);
            }
        }
        evalLogro('LOGRO_PRIMERA_VICTORIA', estado.victorias >= 1);
        evalLogro('LOGRO_CAZADOR_10', estado.bajas >= 10);
        evalLogro('LOGRO_CAZADOR_50', estado.bajas >= 50);
        evalLogro('LOGRO_COMBO_5', estado.comboMax >= 5);
        evalLogro('LOGRO_PRIMER_JEFE', estado.jefes >= 1);
        evalLogro('LOGRO_CAMPANA_COMPLETA', (estado.estrellasNivel[5] || 0) >= 1);
        evalLogro('LOGRO_PRIMERA_COMPRA', estado.comprados.length > 4 || Object.values(estado.mejoras).some(v => v > 0));
        evalLogro('LOGRO_MEJORA_MAX', Object.values(estado.mejoras).some(v => v >= 5));
        evalLogro('LOGRO_MILLONARIO', estado.monedas >= 2000);
        return recien;
    }

    function mostrarToast(texto) {
        const toast = document.getElementById('toast-notificacion');
        if (!toast) return;
        toast.textContent = texto;
        toast.classList.remove('oculta');
        clearTimeout(toast._timer);
        toast._timer = setTimeout(() => toast.classList.add('oculta'), 1600);
        toast.onclick = () => {
            clearTimeout(toast._timer);
            toast.classList.add('oculta');
        };
    }

    function alternarPantallaCompleta() {
        enviarComandoNativo('alternarPantallaCompleta');
        try {
            if (!document.fullscreenElement) {
                document.documentElement.requestFullscreen().catch(() => {});
            } else {
                document.exitFullscreen().catch(() => {});
            }
        } catch (e) { }
    }

    // Atajo global de teclado para F11
    window.addEventListener('keydown', (e) => {
        if (e.code === 'F11') {
            e.preventDefault();
            alternarPantallaCompleta();
        }
    });

    return {
        inicializarUI: function () {
            cargarEstadoInicial();
            refrescarMenuPrincipal();
        },
        abrirModal,
        cerrarModal,
        lanzarPartida,
        cambiarNaveRapidoHangar,
        reclamarRecompensaDiaria,
        mostrarOverlayPausa,
        alTerminarPartida,
        mostrarToast,
        alternarPantallaCompleta,
        obtenerPoderEquipado: () => estado.poderEquipado,
        obtenerNombrePiloto: () => (estado && estado.nombrePiloto) ? estado.nombrePiloto : 'PILOTO',
        relanzarMisionActual: function (forzarNivel) {
            lanzarPartida(ultimaConfigPartida.modalidad, forzarNivel || ultimaConfigPartida.nivel);
        },
        enviarEstadoRed: function (x, z, vida, disparo) {
            enviarComandoNativo('estadoCombateRed', { x, z, vida, disparo });
        },
        salirDelJuego: function () {
            guardarEstadoLocal();
            enviarComandoNativo('salirJuego');
        }
    };
})();
