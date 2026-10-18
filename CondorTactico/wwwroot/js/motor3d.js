// ============================================================================
// CÓNDOR TÁCTICO 3D - MOTOR GRÁFICO WEBGL 2.0 (THREE.JS PBR + FÍSICAS CCD + HUD 3D)
// Renderizado 3D de aeronaves, hangar interactivo, terrenos, jefes, barras de
// salud en espacio de mundo, físicas de proyectiles normalizadas y potenciadores
// ============================================================================

window.Motor3D = (function () {
    let scene, camera, renderer, clock;
    let modoEscena = 'HANGAR'; // 'HANGAR' o 'COMBATE'

    // Luces globales
    let luzAmbiente, luzSol, luzPuntoJugador, luzRellenoAeronaves;

    // Grupo del Hangar 3D (Menú Principal / Tienda / Inventario)
    let grupoHangar = null;
    let naveHangarMesh = null;
    let anilloHangar1 = null;
    let anilloHangar2 = null;
    let anguloCamaraHangar = 0;

    // Grupo del Mundo 3D de Combate (Relieve Procedural Continuo, Río y Estructuras)
    let grupoMundoCombate = null;
    let chunksTerreno = [];
    let mallaRio = null;
    let estructurasEscenario3D = [];
    let nubes3D = [];
    let ondasExpansivas3D = [];
    let nivelActivoCombate = 1;

    // Contenedor DOM para barras de salud flotantes de enemigos
    let contenedorBarrasEnemigosDOM = null;

    // ========================================================================
    // CONFIGURACIÓN CENTRALIZADA DE ARQUITECTURA, NIVELES (2M 30S) Y LÍMITES 3D
    // ========================================================================
    const CONFIG_MOTOR = {
        LIMITE_X_JUGADOR: 13.5,
        LIMITE_Z_MIN_JUGADOR: -6.0,
        LIMITE_Z_MAX_JUGADOR: 3.2,
        // Corredor de vuelo 100% despejado: en |x| <= 20.5 el terreno jamás supera Y = -4.8
        MITAD_CORREDOR_SEGURO_X: 20.5,
        TECHO_MAXIMO_VALLE_Y: -4.8,
        PROFUNDIDAD_LECHO_RIO_Y: -9.6,
        DURACION_OBJETIVO_NIVEL_SEG: 150, // 2 minutos y 30 segundos por nivel
        OLEADAS_POR_NIVEL_CAMPANA: 10,
        TIEMPO_TRANSICION_OLEADA_SEG: 1.45
    };

    const CONFIG_NIVELES_CAMPANA = {
        // MAPA 1: VALLE DEL TITICACA (NIVELES 1 Y 2)
        1: {
            nombreZona: 'Valle del Titicaca - Fase I',
            duracionObjetivo: 150,
            totalOleadas: 10,
            velocidadTerreno: 24,
            intervaloMeteoroBase: [6.8, 9.2],
            vidaBaseEnemigo: 38,
            escalaDanoEnemigo: 0.85,
            vidaJefe: 650,
            jefeNombre: 'COMANDANTE KUNTUR-I'
        },
        2: {
            nombreZona: 'Valle del Titicaca - Fase II',
            duracionObjetivo: 150,
            totalOleadas: 10,
            velocidadTerreno: 25,
            intervaloMeteoroBase: [6.0, 8.2],
            vidaBaseEnemigo: 48,
            escalaDanoEnemigo: 0.95,
            vidaJefe: 820,
            jefeNombre: 'CRUCERO FLUVIAL TITIKAKA'
        },

        // MAPA 2: CAÑÓN DE LA TORMENTA (NIVELES 3 Y 4)
        3: {
            nombreZona: 'Cañón de la Tormenta - Fase I',
            duracionObjetivo: 150,
            totalOleadas: 10,
            velocidadTerreno: 26,
            intervaloMeteoroBase: [5.4, 7.5],
            vidaBaseEnemigo: 58,
            escalaDanoEnemigo: 1.05,
            vidaJefe: 1020,
            jefeNombre: 'COMANDANTE TEMPESTAD'
        },
        4: {
            nombreZona: 'Cañón de la Tormenta - Fase II',
            duracionObjetivo: 150,
            totalOleadas: 10,
            velocidadTerreno: 28,
            intervaloMeteoroBase: [4.8, 6.8],
            vidaBaseEnemigo: 70,
            escalaDanoEnemigo: 1.15,
            vidaJefe: 1250,
            jefeNombre: 'ESCUADRÓN FANTASMA B-2'
        },

        // MAPA 3: GLACIAR AUSTRAL (NIVELES 5 Y 6)
        5: {
            nombreZona: 'Glaciar Austral - Fase I',
            duracionObjetivo: 150,
            totalOleadas: 10,
            velocidadTerreno: 29,
            intervaloMeteoroBase: [4.4, 6.2],
            vidaBaseEnemigo: 82,
            escalaDanoEnemigo: 1.25,
            vidaJefe: 1500,
            jefeNombre: 'VANGUARDIA ÁRTICA ILLIMANI'
        },
        6: {
            nombreZona: 'Glaciar Austral - Fase II',
            duracionObjetivo: 150,
            totalOleadas: 10,
            velocidadTerreno: 31,
            intervaloMeteoroBase: [4.0, 5.8],
            vidaBaseEnemigo: 96,
            escalaDanoEnemigo: 1.35,
            vidaJefe: 1780,
            jefeNombre: 'CRUCERO GLACIAR ILLIMANI'
        },

        // MAPA 4: VOLCÁN EN ERUPCIÓN (NIVELES 7 Y 8)
        7: {
            nombreZona: 'Volcán en Erupción - Fase I',
            duracionObjetivo: 150,
            totalOleadas: 10,
            velocidadTerreno: 32,
            intervaloMeteoroBase: [3.6, 5.2],
            vidaBaseEnemigo: 110,
            escalaDanoEnemigo: 1.48,
            vidaJefe: 2100,
            jefeNombre: 'CAZA ÍGNEO QUIMERA'
        },
        8: {
            nombreZona: 'Volcán en Erupción - Fase II',
            duracionObjetivo: 150,
            totalOleadas: 10,
            velocidadTerreno: 34,
            intervaloMeteoroBase: [3.2, 4.6],
            vidaBaseEnemigo: 124,
            escalaDanoEnemigo: 1.60,
            vidaJefe: 2500,
            jefeNombre: 'VALKIRIA VOLCÁNICA X-9'
        },

        // MAPA 5: FORTALEZA ESPACIAL (NIVELES 9 Y 10)
        9: {
            nombreZona: 'Fortaleza Espacial - Fase I',
            duracionObjetivo: 150,
            totalOleadas: 10,
            velocidadTerreno: 35,
            intervaloMeteoroBase: [2.8, 4.2],
            vidaBaseEnemigo: 140,
            escalaDanoEnemigo: 1.72,
            vidaJefe: 2950,
            jefeNombre: 'DREADNOUGHT CENTINELA APU'
        },
        10: {
            nombreZona: 'Fortaleza Espacial - Fase II',
            duracionObjetivo: 150,
            totalOleadas: 10,
            velocidadTerreno: 38,
            intervaloMeteoroBase: [2.4, 3.6],
            vidaBaseEnemigo: 158,
            escalaDanoEnemigo: 1.88,
            vidaJefe: 3500,
            jefeNombre: 'ACORAZADO ORBITAL APU-SUPAY'
        }
    };

    // Caché de geometrías y materiales compartidos en GPU (Cero fugas de memoria por frame)
    const recursosCompartidos3D = {
        inicializado: false,
        geomCilindroUnitario: null,
        geomEsferaUnitaria: null,
        geomHaloOrbe: null,
        geomAnilloOnda: null,
        geomOctaedroItem: null,
        geomToroItem: null,
        geomAnilloPoderFino: null,
        geomAnilloStasis: null,
        matBlancoPuro: null,
        matStasisEMP: null,
        cacheMaterialesGlow: {},
        cacheMaterialesItem: {},
        cacheTexturasTerreno: {}
    };

    function asegurarRecursosCompartidos3D() {
        if (recursosCompartidos3D.inicializado) return;
        recursosCompartidos3D.geomCilindroUnitario = new THREE.CylinderGeometry(1, 1, 1, 10);
        recursosCompartidos3D.geomCilindroUnitario.rotateX(Math.PI / 2);
        recursosCompartidos3D.geomEsferaUnitaria = new THREE.SphereGeometry(1, 14, 12);
        recursosCompartidos3D.geomHaloOrbe = new THREE.RingGeometry(0.82, 1.45, 20);
        recursosCompartidos3D.geomAnilloOnda = new THREE.RingGeometry(0.42, 0.88, 36);
        recursosCompartidos3D.geomOctaedroItem = new THREE.OctahedronGeometry(0.65, 0);
        recursosCompartidos3D.geomToroItem = new THREE.TorusGeometry(0.95, 0.06, 10, 28);
        recursosCompartidos3D.geomAnilloPoderFino = new THREE.TorusGeometry(2.15, 0.022, 10, 48);
        recursosCompartidos3D.geomAnilloStasis = new THREE.TorusGeometry(1.55, 0.055, 8, 28);
        recursosCompartidos3D.geomAnilloStasis.rotateX(Math.PI / 2);
        recursosCompartidos3D.matBlancoPuro = new THREE.MeshBasicMaterial({ color: 0xffffff });
        recursosCompartidos3D.matStasisEMP = new THREE.MeshBasicMaterial({
            color: 0x00e5ff,
            transparent: true,
            opacity: 0.82,
            blending: THREE.AdditiveBlending,
            depthWrite: false
        });
        recursosCompartidos3D.inicializado = true;
    }

    function obtenerMaterialGlowCompartido(colorHex, opacidad = 0.88) {
        asegurarRecursosCompartidos3D();
        const clave = `${colorHex}_${opacidad}`;
        if (!recursosCompartidos3D.cacheMaterialesGlow[clave]) {
            recursosCompartidos3D.cacheMaterialesGlow[clave] = new THREE.MeshBasicMaterial({
                color: colorHex,
                transparent: true,
                opacity: opacidad,
                blending: THREE.AdditiveBlending,
                depthWrite: false,
                side: THREE.DoubleSide
            });
        }
        return recursosCompartidos3D.cacheMaterialesGlow[clave];
    }

    function liberarMallaTemporal(obj, descartarMaterialClonado = false) {
        if (!obj) return;
        if (descartarMaterialClonado && obj.material && typeof obj.material.dispose === 'function') {
            obj.material.dispose();
        }
    }

    // Entidades de la partida activa
    let estadoJuego = {
        enCurso: false,
        pausado: false,
        terminado: false,
        modalidad: 'CampanaCasual',
        nivel: 1,
        oleada: 1,
        totalOleadas: 10,
        subEscalonOleada: 0,
        refuerzosPendientes: 0,
        temporizadorRefuerzos: 0,
        temporizadorTransicionOleada: 0,
        tiempoMision: 0,
        duracionObjetivoMision: 150,
        enFaseJefe: false,
        faseJefeActual: 1,
        checkpointJefe: false,
        vidasReserva: 1,
        usoReaparicion: false,
        puntos: 0,
        combo: 1,
        comboMax: 1,
        tiempoCombo: 0,
        monedasPartida: 0,
        bajas: 0,
        disparos: 0,
        impactos: 0,
        cargaPoder: 100,
        tiempoPoderActivo: 0,
        duracionMaxPoderActivo: 6.0,
        tiempoInvulnerable: 0,
        duracionMaxInvulnerable: 5.0,
        tiempoEnemigosParalizados: 0,
        duracionMaxParalizados: 4.0,
        tiempoEfectoReparacion: 0,
        colorEfectoReparacion: 0x00e676,
        sacudidaCamara: 0,
        pasoTutorial: 1,
        progresoTutorial: 0
    };

    let jugador = {
        mesh: null,
        x: 0,
        z: 8,
        vx: 0,
        vz: 0,
        vida: 100,
        vidaMax: 100,
        escudo: 50,
        escudoMax: 50,
        cooldownDisparo: 0,
        modeloId: 'NAVE_CONDOR_X',
        colorSkin: 0x00dcff,
        velocidadBase: 20,
        cadenciaFactor: 1.0,
        // Maniobra evasiva (tonel / barrel roll) con fotogramas de invulnerabilidad
        tiempoManiobra: 0,
        cooldownManiobra: 0,
        direccionManiobra: 1,
        perfil: null
    };

    // ========================================================================
    // PERFILES DE VUELO POR AERONAVE (CADA AVIÓN SE PILOTA DE FORMA DISTINTA)
    // aceleracion/frenado: respuesta exponencial (1/s); mayor = más ágil
    // ========================================================================
    const PERFILES_VUELO = {
        NAVE_CONDOR_X: { aceleracion: 7.5, frenado: 6.0, factorVelocidad: 1.00, factorCadencia: 1.00, rollMax: 0.62, respuestaRoll: 7.0, cooldownManiobra: 1.55 },
        NAVE_F35:      { aceleracion: 10.5, frenado: 8.5, factorVelocidad: 1.06, factorCadencia: 1.12, rollMax: 0.78, respuestaRoll: 10.0, cooldownManiobra: 1.20 },
        NAVE_F22:      { aceleracion: 8.5, frenado: 5.0, factorVelocidad: 1.14, factorCadencia: 1.00, rollMax: 0.70, respuestaRoll: 8.5, cooldownManiobra: 1.40 },
        NAVE_B2:       { aceleracion: 4.6, frenado: 3.4, factorVelocidad: 0.86, factorCadencia: 0.92, rollMax: 0.34, respuestaRoll: 4.5, cooldownManiobra: 2.10 }
    };
    const DURACION_MANIOBRA = 0.42;

    // Dificultad global (accesible pero con desafío real)
    const MODIFICADORES_DIFICULTAD = {
        facil:   { dano: 0.65, cadenciaEnemiga: 1.35, punteria: 0.0,  puntos: 0.8 },
        normal:  { dano: 1.00, cadenciaEnemiga: 1.00, punteria: 0.35, puntos: 1.0 },
        dificil: { dano: 1.30, cadenciaEnemiga: 0.80, punteria: 0.70, puntos: 1.3 }
    };
    let dificultadActual = 'normal';
    let intensidadSacudida = 1.0;
    let calidadGrafica = 'alta';
    let escalaTiempo = 1.0;
    let tiempoCamaraSlowMo = 0;

    function modDificultad() {
        return MODIFICADORES_DIFICULTAD[dificultadActual] || MODIFICADORES_DIFICULTAD.normal;
    }

    // Trauma de cámara: se acumula con impactos y decae; la sacudida es trauma² (curva suave)
    function agregarTrauma(cantidad) {
        estadoJuego.sacudidaCamara = Math.min(1.0, Math.max(estadoJuego.sacudidaCamara, 0) + cantidad);
    }

    let enemigos = [];
    let proyectiles = [];
    let suministros = [];
    let peligros = [];
    let meteoros3D = [];
    let temporizadorMeteoro = 4.0;
    let temporizadorSuministroMulti = 8.0;
    let remotosRed = {};
    let contadorIdEnemigo = 1;
    let tiempoUltimoEnvioRed = 0;

    // Entrada de usuario (Teclado + Ratón)
    const teclas = {};
    let ratonX = 0, ratonY = 0, ratonDisparando = false, usarRatonVuelo = false;

    // Mando (Gamepad API): stick analógico, gatillo de disparo y botones de acción
    const mando = { conectado: false, ejeX: 0, ejeY: 0, disparo: false, poder: false, maniobra: false, pausa: false };
    const mandoPrevio = { poder: false, maniobra: false, pausa: false };

    function aplicarZonaMuerta(v, zona = 0.18) {
        if (Math.abs(v) < zona) return 0;
        return Math.sign(v) * (Math.abs(v) - zona) / (1 - zona);
    }

    function leerMando() {
        mando.conectado = false;
        if (!navigator.getGamepads) return;
        const lista = navigator.getGamepads();
        for (let i = 0; i < lista.length; i++) {
            const gp = lista[i];
            if (!gp || !gp.connected) continue;
            const b = (idx) => !!(gp.buttons[idx] && (gp.buttons[idx].pressed || gp.buttons[idx].value > 0.4));
            mando.conectado = true;
            mando.ejeX = aplicarZonaMuerta(gp.axes[0] || 0);
            mando.ejeY = aplicarZonaMuerta(gp.axes[1] || 0);
            if (b(14)) mando.ejeX = -1;
            if (b(15)) mando.ejeX = 1;
            if (b(12)) mando.ejeY = -1;
            if (b(13)) mando.ejeY = 1;
            mando.disparo = b(0) || b(7);
            mando.poder = b(1) || b(5) || b(3);
            mando.maniobra = b(2) || b(4) || b(6);
            mando.pausa = b(9);
            return;
        }
    }

    // ========================================================================
    // AUDIO WEBAUDIO SINTETIZADO MULTICANAL (DIFERENCIADO POR ACCIÓN Y PODER)
    // ========================================================================
    let audioCtx = null;
    let volumenEfectos = 0.8;
    let audioSilenciado = false;

    // Bus maestro con compresor: evita saturación cuando coinciden muchas explosiones
    let busMaestro = null;
    let bufferRuido = null;
    let motorAudio = null;
    let temporizadorAlarma = 0;

    function inicializarAudio() {
        if (!audioCtx) {
            const AudioContextClass = window.AudioContext || window.webkitAudioContext;
            if (AudioContextClass) audioCtx = new AudioContextClass();
            if (audioCtx) {
                const compresor = audioCtx.createDynamicsCompressor();
                compresor.threshold.value = -16;
                compresor.knee.value = 12;
                compresor.ratio.value = 5;
                compresor.attack.value = 0.004;
                compresor.release.value = 0.22;
                busMaestro = audioCtx.createGain();
                busMaestro.gain.value = 1.0;
                busMaestro.connect(compresor);
                compresor.connect(audioCtx.destination);

                // Ruido blanco precalculado (1 s) para explosiones, impactos y viento
                bufferRuido = audioCtx.createBuffer(1, audioCtx.sampleRate, audioCtx.sampleRate);
                const datos = bufferRuido.getChannelData(0);
                for (let i = 0; i < datos.length; i++) datos[i] = Math.random() * 2 - 1;
            }
        }
        if (audioCtx && audioCtx.state === 'suspended') {
            audioCtx.resume();
        }
    }

    // Ráfaga de ruido filtrado con barrido de frecuencia (cuerpo de explosiones e impactos)
    function rafagaRuido(now, duracion, volumen, tipoFiltro, freqIni, freqFin, q = 0.8) {
        if (!bufferRuido) return;
        const src = audioCtx.createBufferSource();
        src.buffer = bufferRuido;
        src.playbackRate.value = 0.8 + Math.random() * 0.4;
        const filtro = audioCtx.createBiquadFilter();
        filtro.type = tipoFiltro;
        filtro.Q.value = q;
        filtro.frequency.setValueAtTime(freqIni, now);
        filtro.frequency.exponentialRampToValueAtTime(Math.max(20, freqFin), now + duracion);
        const g = audioCtx.createGain();
        g.gain.setValueAtTime(volumen, now);
        g.gain.exponentialRampToValueAtTime(0.0008, now + duracion);
        src.connect(filtro);
        filtro.connect(g);
        g.connect(busMaestro);
        src.start(now, Math.random() * 0.5);
        src.stop(now + duracion + 0.02);
    }

    // Zumbido continuo de turbinas: dos sierras desafinadas + ruido de viento, modulados por la velocidad
    function asegurarMotorAudio() {
        if (motorAudio || !audioCtx || !busMaestro) return;
        const salida = audioCtx.createGain();
        salida.gain.value = 0;
        const filtro = audioCtx.createBiquadFilter();
        filtro.type = 'lowpass';
        filtro.frequency.value = 420;
        filtro.Q.value = 1.4;
        const osc1 = audioCtx.createOscillator();
        const osc2 = audioCtx.createOscillator();
        osc1.type = 'sawtooth';
        osc2.type = 'sawtooth';
        osc1.frequency.value = 62;
        osc2.frequency.value = 62.9;
        const viento = audioCtx.createBufferSource();
        viento.buffer = bufferRuido;
        viento.loop = true;
        const filtroViento = audioCtx.createBiquadFilter();
        filtroViento.type = 'bandpass';
        filtroViento.frequency.value = 900;
        filtroViento.Q.value = 0.6;
        const gViento = audioCtx.createGain();
        gViento.gain.value = 0.35;
        osc1.connect(filtro);
        osc2.connect(filtro);
        filtro.connect(salida);
        viento.connect(filtroViento);
        filtroViento.connect(gViento);
        gViento.connect(salida);
        salida.connect(busMaestro);
        osc1.start();
        osc2.start();
        viento.start();
        motorAudio = { salida, filtro, osc1, osc2, filtroViento };
    }

    function actualizarMotorAudio(activo, factorVelocidad, enManiobra) {
        if (!audioCtx) return;
        if (activo && !audioSilenciado && volumenEfectos > 0.01) asegurarMotorAudio();
        if (!motorAudio) return;
        const now = audioCtx.currentTime;
        const objetivo = (activo && !audioSilenciado) ? Math.min(0.11, volumenEfectos * 0.075) * (0.75 + factorVelocidad * 0.35) : 0;
        motorAudio.salida.gain.setTargetAtTime(objetivo, now, 0.12);
        const tono = 58 + factorVelocidad * 22 + (enManiobra ? 18 : 0);
        motorAudio.osc1.frequency.setTargetAtTime(tono, now, 0.08);
        motorAudio.osc2.frequency.setTargetAtTime(tono * 1.013, now, 0.08);
        motorAudio.filtro.frequency.setTargetAtTime(380 + factorVelocidad * 520 + (enManiobra ? 600 : 0), now, 0.1);
        motorAudio.filtroViento.frequency.setTargetAtTime(700 + factorVelocidad * 900, now, 0.15);
    }

    function sonarEfecto(tipo) {
        if (audioSilenciado || volumenEfectos <= 0.01) return;
        try {
            inicializarAudio();
            if (!audioCtx || !busMaestro) return;
            const now = audioCtx.currentTime;
            const vol = Math.min(0.35, volumenEfectos * 0.3);

            // Capas de ruido: dan cuerpo físico a explosiones, impactos y maniobras
            if (tipo === 'explosion') {
                rafagaRuido(now, 0.55, vol * 1.25, 'lowpass', 2400, 90, 0.7);
            } else if (tipo === 'explosion_grande') {
                rafagaRuido(now, 1.3, vol * 1.6, 'lowpass', 1800, 40, 0.9);
                tipo = 'explosion';
            } else if (tipo === 'impacto') {
                rafagaRuido(now, 0.07, vol * 0.45, 'highpass', 3200, 1800, 0.6);
                return;
            } else if (tipo === 'dano_jugador') {
                rafagaRuido(now, 0.22, vol * 0.95, 'bandpass', 1400, 260, 1.2);
                tipo = 'peligro';
            } else if (tipo === 'maniobra') {
                rafagaRuido(now, 0.42, vol * 0.7, 'bandpass', 500, 2600, 1.6);
                return;
            } else if (tipo === 'laser_enemigo') {
                tipo = 'fin_poder';
            } else if (tipo === 'alarma') {
                tipo = 'alarma_tono';
            }

            const osc = audioCtx.createOscillator();
            const gain = audioCtx.createGain();
            osc.connect(gain);
            gain.connect(busMaestro);

            if (tipo === 'disparo') {
                osc.type = 'sawtooth';
                osc.frequency.setValueAtTime(560, now);
                osc.frequency.exponentialRampToValueAtTime(125, now + 0.085);
                gain.gain.setValueAtTime(vol * 0.55, now);
                gain.gain.exponentialRampToValueAtTime(0.001, now + 0.085);
                osc.start(now);
                osc.stop(now + 0.085);
            } else if (tipo === 'disparo_overdrive') {
                osc.type = 'triangle';
                osc.frequency.setValueAtTime(780, now);
                osc.frequency.exponentialRampToValueAtTime(190, now + 0.09);
                gain.gain.setValueAtTime(vol * 0.65, now);
                gain.gain.exponentialRampToValueAtTime(0.001, now + 0.09);
                osc.start(now);
                osc.stop(now + 0.09);
            } else if (tipo === 'explosion') {
                osc.type = 'triangle';
                osc.frequency.setValueAtTime(135, now);
                osc.frequency.exponentialRampToValueAtTime(26, now + 0.34);
                gain.gain.setValueAtTime(vol, now);
                gain.gain.exponentialRampToValueAtTime(0.001, now + 0.34);
                osc.start(now);
                osc.stop(now + 0.34);
            } else if (tipo === 'item' || tipo === 'item_vida') {
                osc.type = 'sine';
                osc.frequency.setValueAtTime(523.25, now);
                osc.frequency.setValueAtTime(659.25, now + 0.07);
                osc.frequency.exponentialRampToValueAtTime(1046.5, now + 0.22);
                gain.gain.setValueAtTime(vol * 0.85, now);
                gain.gain.exponentialRampToValueAtTime(0.001, now + 0.22);
                osc.start(now);
                osc.stop(now + 0.22);
            } else if (tipo === 'item_escudo') {
                osc.type = 'triangle';
                osc.frequency.setValueAtTime(440, now);
                osc.frequency.exponentialRampToValueAtTime(880, now + 0.24);
                gain.gain.setValueAtTime(vol * 0.85, now);
                gain.gain.exponentialRampToValueAtTime(0.001, now + 0.24);
                osc.start(now);
                osc.stop(now + 0.24);
            } else if (tipo === 'item_fuego' || tipo === 'poder_overdrive') {
                osc.type = 'sawtooth';
                osc.frequency.setValueAtTime(293.66, now);
                osc.frequency.exponentialRampToValueAtTime(880, now + 0.32);
                gain.gain.setValueAtTime(vol * 0.9, now);
                gain.gain.exponentialRampToValueAtTime(0.001, now + 0.32);
                osc.start(now);
                osc.stop(now + 0.32);
            } else if (tipo === 'poder_emp') {
                osc.type = 'sine';
                osc.frequency.setValueAtTime(920, now);
                osc.frequency.exponentialRampToValueAtTime(140, now + 0.42);
                gain.gain.setValueAtTime(vol, now);
                gain.gain.exponentialRampToValueAtTime(0.001, now + 0.42);
                osc.start(now);
                osc.stop(now + 0.42);
            } else if (tipo === 'poder_escudo') {
                osc.type = 'sine';
                osc.frequency.setValueAtTime(392, now);
                osc.frequency.setValueAtTime(523.25, now + 0.1);
                osc.frequency.exponentialRampToValueAtTime(783.99, now + 0.38);
                gain.gain.setValueAtTime(vol * 0.95, now);
                gain.gain.exponentialRampToValueAtTime(0.001, now + 0.38);
                osc.start(now);
                osc.stop(now + 0.38);
            } else if (tipo === 'poder' || tipo === 'poder_solar') {
                osc.type = 'sine';
                osc.frequency.setValueAtTime(220, now);
                osc.frequency.exponentialRampToValueAtTime(880, now + 0.38);
                gain.gain.setValueAtTime(vol, now);
                gain.gain.exponentialRampToValueAtTime(0.001, now + 0.38);
                osc.start(now);
                osc.stop(now + 0.38);
            } else if (tipo === 'fin_poder') {
                osc.type = 'sine';
                osc.frequency.setValueAtTime(580, now);
                osc.frequency.exponentialRampToValueAtTime(280, now + 0.16);
                gain.gain.setValueAtTime(vol * 0.45, now);
                gain.gain.exponentialRampToValueAtTime(0.001, now + 0.16);
                osc.start(now);
                osc.stop(now + 0.16);
            } else if (tipo === 'click') {
                osc.type = 'sine';
                osc.frequency.setValueAtTime(680, now);
                osc.frequency.exponentialRampToValueAtTime(880, now + 0.05);
                gain.gain.setValueAtTime(vol * 0.4, now);
                gain.gain.exponentialRampToValueAtTime(0.001, now + 0.05);
                osc.start(now);
                osc.stop(now + 0.05);
            } else if (tipo === 'peligro') {
                osc.type = 'sine';
                osc.frequency.setValueAtTime(540, now);
                osc.frequency.exponentialRampToValueAtTime(320, now + 0.12);
                gain.gain.setValueAtTime(vol * 0.45, now);
                gain.gain.exponentialRampToValueAtTime(0.001, now + 0.12);
                osc.start(now);
                osc.stop(now + 0.12);
            } else if (tipo === 'alarma_tono') {
                osc.type = 'square';
                osc.frequency.setValueAtTime(880, now);
                osc.frequency.setValueAtTime(660, now + 0.09);
                gain.gain.setValueAtTime(vol * 0.22, now);
                gain.gain.exponentialRampToValueAtTime(0.001, now + 0.18);
                osc.start(now);
                osc.stop(now + 0.18);
            } else {
                osc.disconnect();
                gain.disconnect();
            }
        } catch (e) { }
    }

    // ========================================================================
    // RECURSOS DE TEXTURAS PBR, ENTORNO HDR PROCEDURAL Y RELIEVE GEOLÓGICO
    // ========================================================================
    const cargadorTexturas = new THREE.TextureLoader();
    const texturasJuego = {
        meteoroRoca: cargadorTexturas.load('assets/texturas/textura_meteoro_roca.png'),
        meteoroNormal: cargadorTexturas.load('assets/texturas/textura_meteoro_normal.png'),
        camuflajeF22: cargadorTexturas.load('assets/texturas/textura_camuflaje_f22.png'),
        camuflajeB2: cargadorTexturas.load('assets/texturas/textura_camuflaje_b2.png'),
        particulaHumo: cargadorTexturas.load('assets/texturas/particula_humo.png'),
        particulaChispa: cargadorTexturas.load('assets/texturas/particula_chispa.png')
    };
    texturasJuego.meteoroRoca.wrapS = THREE.RepeatWrapping;
    texturasJuego.meteoroRoca.wrapT = THREE.RepeatWrapping;
    texturasJuego.meteoroNormal.wrapS = THREE.RepeatWrapping;
    texturasJuego.meteoroNormal.wrapT = THREE.RepeatWrapping;

    // Generador de Textura Geológica de Alta Definición para el Terreno (Sin estiramiento ni ruido visual, 100% Seamless C^∞)
    function obtenerTexturaGeologicaBioma(nivel) {
        const bioma = Math.max(1, Math.min(5, Math.ceil(nivel / 2)));
        if (recursosCompartidos3D.cacheTexturasTerreno[bioma]) {
            return recursosCompartidos3D.cacheTexturasTerreno[bioma];
        }
        const canvas = document.createElement('canvas');
        canvas.width = 512;
        canvas.height = 512;
        const ctx = canvas.getContext('2d');

        // Fondo base equilibrado para multiplicar con los Vertex Colors sin sobreexposición
        ctx.fillStyle = '#8494a6';
        ctx.fillRect(0, 0, 512, 512);

        const OMEGA_TEX = (Math.PI * 2.0) / 512.0;

        // Estratos geológicos periódicos continuos de alto contraste (sin costuras ni saltos de fase en 0 y 512)
        for (let y = 0; y < 512; y += 8) {
            const variacion = Math.sin(y * OMEGA_TEX * 4.0) * 7.0 + Math.cos(y * OMEGA_TEX * 8.0) * 3.5;
            ctx.strokeStyle = (y % 32 === 0)
                ? 'rgba(10, 18, 30, 0.34)'
                : 'rgba(22, 34, 48, 0.18)';
            ctx.lineWidth = (y % 32 === 0) ? 1.6 : 1.0;
            ctx.beginPath();
            for (let x = 0; x <= 512; x += 16) {
                const offset = Math.sin(x * OMEGA_TEX * 3.0 + y * OMEGA_TEX * 2.0) * 5.0
                    + Math.cos(x * OMEGA_TEX * 6.0 - y * OMEGA_TEX * 4.0) * 2.5 + variacion;
                if (x === 0) ctx.moveTo(x, y + offset);
                else ctx.lineTo(x, y + offset);
            }
            ctx.stroke();
        }

        // Micro-fracturas de roca y facetas minerales periódicas (toroide de 512x512)
        for (let i = 0; i < 240; i++) {
            const rx = (i * 73) % 512;
            const ry = (i * 137) % 512;
            const rw = 8 + (i % 18);
            const rh = 4 + (i % 10);
            ctx.fillStyle = (i % 2 === 0) ? 'rgba(255, 255, 255, 0.07)' : 'rgba(8, 14, 24, 0.18)';
            ctx.fillRect(rx, ry, rw, rh);
            if (rx + rw > 512) ctx.fillRect(rx - 512, ry, rw, rh);
            if (ry + rh > 512) ctx.fillRect(rx, ry - 512, rw, rh);
            if (rx + rw > 512 && ry + rh > 512) ctx.fillRect(rx - 512, ry - 512, rw, rh);
        }

        // Detalle táctico sutil según bioma (100% armónico C^∞)
        if (bioma === 4) {
            // Venas volcánicas incandescentes que se cierran armónicamente sin discontinuidad en los bordes
            ctx.strokeStyle = 'rgba(255, 110, 30, 0.28)';
            ctx.lineWidth = 2.4;
            for (let v = 0; v < 12; v++) {
                ctx.beginPath();
                const vx0 = (v * 42.66) % 512;
                const faseV = v * 1.618;
                for (let vy = 0; vy <= 512; vy += 16) {
                    const despX = Math.sin(vy * OMEGA_TEX * 2.0 + faseV) * 14.0
                        + Math.sin(vy * OMEGA_TEX * 4.0 + faseV * 2.0) * 6.0;
                    if (vy === 0) ctx.moveTo(vx0 + despX, vy);
                    else ctx.lineTo(vx0 + despX, vy);
                }
                ctx.stroke();
            }
        } else if (bioma === 5) {
            // Retícula de fortaleza orbital
            ctx.strokeStyle = 'rgba(0, 220, 255, 0.18)';
            ctx.lineWidth = 1;
            for (let g = 0; g < 512; g += 64) {
                ctx.strokeRect(g, 0, 64, 512);
            }
        }

        const tex = new THREE.CanvasTexture(canvas);
        tex.wrapS = THREE.RepeatWrapping;
        tex.wrapT = THREE.RepeatWrapping;
        tex.repeat.set(6, 6);
        recursosCompartidos3D.cacheTexturasTerreno[bioma] = tex;
        return tex;
    }

    // Generador de Mapa de Entorno PBR (PMREM) calibrado para reflejos metálicos nítidos sin lavar los colores base
    function configurarEntornoReflejosPBR() {
        if (!renderer || typeof THREE.PMREMGenerator === 'undefined') return;
        try {
            const pmremGenerator = new THREE.PMREMGenerator(renderer);
            pmremGenerator.compileEquirectangularShader();

            const envScene = new THREE.Scene();
            envScene.background = new THREE.Color(0x050b16);

            // Domo atmosférico controlado
            const hemi = new THREE.HemisphereLight(0x6fa8dc, 0x08101c, 0.52);
            envScene.add(hemi);

            // Bandas especulares compactas para reflejos definidos en cúpulas y bordes de ataque
            const matKey = new THREE.MeshBasicMaterial({ color: 0xffe8c8 });
            const panelKey = new THREE.Mesh(new THREE.PlaneGeometry(12, 8), matKey);
            panelKey.position.set(32, 38, 24);
            panelKey.lookAt(0, 0, 0);
            envScene.add(panelKey);

            const matRim = new THREE.MeshBasicMaterial({ color: 0x00dcff });
            const panelRim = new THREE.Mesh(new THREE.PlaneGeometry(14, 7), matRim);
            panelRim.position.set(-34, 24, -28);
            panelRim.lookAt(0, 0, 0);
            envScene.add(panelRim);

            const matGold = new THREE.MeshBasicMaterial({ color: 0xffb800 });
            const panelGold = new THREE.Mesh(new THREE.PlaneGeometry(10, 6), matGold);
            panelGold.position.set(0, -28, 30);
            panelGold.lookAt(0, 0, 0);
            envScene.add(panelGold);

            const renderTarget = pmremGenerator.fromScene(envScene, 0.04);
            scene.environment = renderTarget.texture;
            pmremGenerator.dispose();
        } catch (e) { }
    }

    // ========================================================================
    // SHADERS GLSL PERSONALIZADOS (PLASMA, ESCUDO FRESNEL Y PROPULSORES)
    // ========================================================================
    const uniformsGlobalesShader = {
        uTime: { value: 0 }
    };

    function crearMaterialPlasmaGLSL(esEnemigo) {
        return new THREE.ShaderMaterial({
            uniforms: {
                uTime: uniformsGlobalesShader.uTime,
                uColorCore: { value: new THREE.Color(0xffffff) },
                uColorEdge: { value: new THREE.Color(esEnemigo ? 0xff1744 : 0x00e5ff) }
            },
            vertexShader: `
                varying vec2 vUv;
                void main() {
                    vUv = uv;
                    gl_Position = projectionMatrix * modelViewMatrix * vec4(position, 1.0);
                }
            `,
            fragmentShader: `
                uniform float uTime;
                uniform vec3 uColorCore;
                uniform vec3 uColorEdge;
                varying vec2 vUv;
                void main() {
                    float diamond = abs(sin((vUv.y * 14.0) - uTime * 28.0));
                    float core = smoothstep(0.0, 0.85, 1.0 - vUv.y) * (0.65 + 0.35 * diamond);
                    vec3 col = mix(uColorEdge, uColorCore, core);
                    float alpha = (1.0 - vUv.y) * (0.75 + 0.25 * sin(uTime * 35.0));
                    gl_FragColor = vec4(col * 1.4, alpha);
                }
            `,
            transparent: true,
            blending: THREE.AdditiveBlending,
            depthWrite: false
        });
    }

    function crearMaterialEscudoFresnelGLSL(colorHex, intensidadBase = 0.24) {
        return new THREE.ShaderMaterial({
            uniforms: {
                uTime: uniformsGlobalesShader.uTime,
                uColor: { value: new THREE.Color(colorHex) },
                uIntensity: { value: intensidadBase }
            },
            vertexShader: `
                varying vec3 vNormal;
                varying vec3 vViewPosition;
                void main() {
                    vNormal = normalize(normalMatrix * normal);
                    vec4 mvPosition = modelViewMatrix * vec4(position, 1.0);
                    vViewPosition = -mvPosition.xyz;
                    gl_Position = projectionMatrix * mvPosition;
                }
            `,
            fragmentShader: `
                uniform float uTime;
                uniform vec3 uColor;
                uniform float uIntensity;
                varying vec3 vNormal;
                varying vec3 vViewPosition;
                void main() {
                    vec3 normal = normalize(vNormal);
                    vec3 viewDir = normalize(vViewPosition);
                    float fresnel = pow(1.0 - abs(dot(normal, viewDir)), 2.8);
                    float pulse = 0.85 + 0.15 * sin(uTime * 7.0);
                    gl_FragColor = vec4(uColor * 1.6, fresnel * uIntensity * pulse);
                }
            `,
            transparent: true,
            blending: THREE.AdditiveBlending,
            depthWrite: false,
            side: THREE.DoubleSide
        });
    }

    function crearCampoParticulas3D(cantidad, radio, colorHex, tamano = 0.14) {
        const geom = new THREE.BufferGeometry();
        const pos = new Float32Array(cantidad * 3);
        for (let i = 0; i < cantidad; i++) {
            pos[i * 3] = (Math.random() - 0.5) * radio * 2;
            pos[i * 3 + 1] = (Math.random() - 0.2) * radio * 0.8;
            pos[i * 3 + 2] = (Math.random() - 0.5) * radio * 2;
        }
        geom.setAttribute('position', new THREE.BufferAttribute(pos, 3));
        const mat = new THREE.PointsMaterial({
            color: colorHex,
            size: tamano,
            transparent: true,
            opacity: 0.72,
            blending: THREE.AdditiveBlending
        });
        return new THREE.Points(geom, mat);
    }

    // ========================================================================
    // POOL DE PARTÍCULAS REUTILIZABLES (CERO ALOCACIONES NI COMPILACIÓN DE
    // MATERIALES DURANTE EL COMBATE: CHISPAS ADITIVAS + HUMO VOLUMÉTRICO)
    // ========================================================================
    let grupoParticulas = null;
    const poolChispas = { items: [], cursor: 0 };
    const poolHumo = { items: [], cursor: 0 };
    const TAM_POOL_CHISPAS = 420;
    const TAM_POOL_HUMO = 180;

    function crearPoolParticulas() {
        grupoParticulas = new THREE.Group();
        grupoParticulas.name = 'poolParticulas';
        const llenar = (pool, cantidad, esHumo) => {
            for (let i = 0; i < cantidad; i++) {
                const mat = new THREE.SpriteMaterial({
                    map: esHumo ? texturasJuego.particulaHumo : texturasJuego.particulaChispa,
                    color: 0xffffff,
                    transparent: true,
                    opacity: 0,
                    blending: esHumo ? THREE.NormalBlending : THREE.AdditiveBlending,
                    depthWrite: false
                });
                const sprite = new THREE.Sprite(mat);
                sprite.visible = false;
                sprite.renderOrder = esHumo ? 4 : 5;
                grupoParticulas.add(sprite);
                pool.items.push({ sprite, activo: false, vx: 0, vy: 0, vz: 0, vida: 0, vidaMax: 1, tamIni: 1, tamFin: 1, opacidad: 1, arrastre: 0, gravedad: 0, rotVel: 0 });
            }
        };
        llenar(poolChispas, TAM_POOL_CHISPAS, false);
        llenar(poolHumo, TAM_POOL_HUMO, true);
        scene.add(grupoParticulas);
    }

    function emitirParticula(esHumo, x, y, z, vx, vy, vz, vida, tamIni, tamFin, colorHex, opacidad = 0.95, arrastre = 1.5, gravedad = 0) {
        const pool = esHumo ? poolHumo : poolChispas;
        if (!pool.items.length) return;
        const p = pool.items[pool.cursor];
        pool.cursor = (pool.cursor + 1) % pool.items.length;
        p.activo = true;
        p.vx = vx; p.vy = vy; p.vz = vz;
        p.vida = vida; p.vidaMax = vida;
        p.tamIni = tamIni; p.tamFin = tamFin;
        p.opacidad = opacidad;
        p.arrastre = arrastre;
        p.gravedad = gravedad;
        p.sprite.visible = true;
        p.sprite.position.set(x, y, z);
        p.sprite.scale.set(tamIni, tamIni, 1);
        p.sprite.material.color.setHex(colorHex);
        p.sprite.material.opacity = opacidad;
        p.sprite.material.rotation = Math.random() * Math.PI * 2;
        p.rotVel = (Math.random() - 0.5) * 2.0;
    }

    function actualizarPoolParticulas(dt, desplazamientoMundoZ) {
        const procesar = (pool, esHumo) => {
            for (let i = 0; i < pool.items.length; i++) {
                const p = pool.items[i];
                if (!p.activo) continue;
                p.vida -= dt;
                if (p.vida <= 0) {
                    p.activo = false;
                    p.sprite.visible = false;
                    continue;
                }
                const amort = Math.exp(-p.arrastre * dt);
                p.vx *= amort; p.vy = p.vy * amort - p.gravedad * dt; p.vz *= amort;
                const pos = p.sprite.position;
                pos.x += p.vx * dt;
                pos.y += p.vy * dt;
                // El humo queda anclado al mundo y se desplaza con el terreno (sensación de avance)
                pos.z += p.vz * dt + (esHumo ? desplazamientoMundoZ * 0.55 : 0);
                const t = 1 - p.vida / p.vidaMax;
                const tam = p.tamIni + (p.tamFin - p.tamIni) * t;
                p.sprite.scale.set(tam, tam, 1);
                p.sprite.material.rotation += p.rotVel * dt;
                // Chispas: apagado rápido; humo: aparición suave y desvanecido largo
                p.sprite.material.opacity = esHumo
                    ? p.opacidad * Math.min(1, t * 6) * (1 - t)
                    : p.opacidad * (1 - t * t);
            }
        };
        procesar(poolChispas, false);
        procesar(poolHumo, true);
    }

    function vaciarPoolParticulas() {
        [poolChispas, poolHumo].forEach(pool => pool.items.forEach(p => {
            p.activo = false;
            p.sprite.visible = false;
        }));
    }

    // ========================================================================
    // ESTELAS DE CONDENSACIÓN (CONTRAILS) EN PUNTAS DE ALA: CINTA DINÁMICA
    // ========================================================================
    const PUNTOS_ESTELA = 26;

    function crearEstela(colorHex) {
        const geom = new THREE.BufferGeometry();
        const posiciones = new Float32Array(PUNTOS_ESTELA * 2 * 3);
        const colores = new Float32Array(PUNTOS_ESTELA * 2 * 3);
        const indices = [];
        for (let i = 0; i < PUNTOS_ESTELA - 1; i++) {
            const a = i * 2, b = i * 2 + 1, c = (i + 1) * 2, d = (i + 1) * 2 + 1;
            indices.push(a, b, c, b, d, c);
        }
        geom.setIndex(indices);
        geom.setAttribute('position', new THREE.BufferAttribute(posiciones, 3));
        geom.setAttribute('color', new THREE.BufferAttribute(colores, 3));
        const mat = new THREE.MeshBasicMaterial({
            vertexColors: true,
            transparent: true,
            opacity: 1.0,
            blending: THREE.AdditiveBlending,
            depthWrite: false,
            side: THREE.DoubleSide
        });
        const mesh = new THREE.Mesh(geom, mat);
        mesh.frustumCulled = false;
        mesh.renderOrder = 6;
        return { mesh, puntos: [], color: new THREE.Color(colorHex), intensidad: 0 };
    }

    function actualizarEstela(estela, x, y, z, ancho, intensidadObjetivo, dt, desplazamientoMundoZ) {
        estela.intensidad += (intensidadObjetivo - estela.intensidad) * Math.min(1, dt * 6);
        // Los puntos antiguos se desplazan con el mundo (el aire "fluye" hacia atrás)
        for (let i = 0; i < estela.puntos.length; i++) {
            estela.puntos[i].z += desplazamientoMundoZ;
        }
        estela.puntos.unshift({ x, y, z });
        if (estela.puntos.length > PUNTOS_ESTELA) estela.puntos.length = PUNTOS_ESTELA;

        const pos = estela.mesh.geometry.attributes.position.array;
        const col = estela.mesh.geometry.attributes.color.array;
        const n = estela.puntos.length;
        for (let i = 0; i < PUNTOS_ESTELA; i++) {
            const p = estela.puntos[Math.min(i, n - 1)];
            const t = i / (PUNTOS_ESTELA - 1);
            const w = ancho * (1 - t);
            const k = i * 6;
            pos[k] = p.x - w; pos[k + 1] = p.y; pos[k + 2] = p.z;
            pos[k + 3] = p.x + w; pos[k + 4] = p.y; pos[k + 5] = p.z;
            const brillo = (i < n ? (1 - t) * (1 - t) : 0) * estela.intensidad;
            col[k] = col[k + 3] = estela.color.r * brillo;
            col[k + 1] = col[k + 4] = estela.color.g * brillo;
            col[k + 2] = col[k + 5] = estela.color.b * brillo;
        }
        estela.mesh.geometry.attributes.position.needsUpdate = true;
        estela.mesh.geometry.attributes.color.needsUpdate = true;
    }

    // ========================================================================
    // LÍNEAS DE VELOCIDAD ATMOSFÉRICAS (PERCEPCIÓN DE MOVIMIENTO A ALTA VELOCIDAD)
    // ========================================================================
    let lineasVelocidad = null;
    const CANT_LINEAS_VELOCIDAD = 60;

    function crearLineasVelocidad() {
        const geom = new THREE.BufferGeometry();
        const pos = new Float32Array(CANT_LINEAS_VELOCIDAD * 2 * 3);
        const datos = [];
        for (let i = 0; i < CANT_LINEAS_VELOCIDAD; i++) {
            datos.push({
                x: (Math.random() - 0.5) * 46,
                y: -2.5 + Math.random() * 9,
                z: -70 + Math.random() * 95,
                largo: 1.8 + Math.random() * 3.5,
                vel: 0.9 + Math.random() * 0.5
            });
        }
        geom.setAttribute('position', new THREE.BufferAttribute(pos, 3));
        const mat = new THREE.LineBasicMaterial({
            color: 0xcfefff,
            transparent: true,
            opacity: 0.16,
            blending: THREE.AdditiveBlending,
            depthWrite: false
        });
        const lineas = new THREE.LineSegments(geom, mat);
        lineas.frustumCulled = false;
        lineas.userData.datos = datos;
        return lineas;
    }

    function actualizarLineasVelocidad(dt, velMundo, factorExtra) {
        if (!lineasVelocidad) return;
        const datos = lineasVelocidad.userData.datos;
        const pos = lineasVelocidad.geometry.attributes.position.array;
        for (let i = 0; i < datos.length; i++) {
            const d = datos[i];
            d.z += velMundo * d.vel * 1.6 * (1 + factorExtra) * dt;
            if (d.z > 26) {
                d.z = -70 - Math.random() * 20;
                d.x = (Math.random() - 0.5) * 46;
            }
            const largo = d.largo * (1 + factorExtra * 1.8);
            const k = i * 6;
            pos[k] = d.x; pos[k + 1] = d.y; pos[k + 2] = d.z;
            pos[k + 3] = d.x; pos[k + 4] = d.y; pos[k + 5] = d.z - largo;
        }
        lineasVelocidad.geometry.attributes.position.needsUpdate = true;
        lineasVelocidad.material.opacity = 0.05 + factorExtra * 0.16;
    }

    // ========================================================================
    // TEXTURA DE CORRIENTE DEL RÍO (FLUJO ANIMADO SIN COSTURAS)
    // ========================================================================
    let texturaCorrienteRio = null;

    function obtenerTexturaCorrienteRio() {
        if (texturaCorrienteRio) return texturaCorrienteRio;
        const c = document.createElement('canvas');
        c.width = 128;
        c.height = 256;
        const ctx = c.getContext('2d');
        ctx.fillStyle = '#c4d4e0';
        ctx.fillRect(0, 0, 128, 256);
        // Ondas alargadas periódicas (se repiten sin cortes en ambos ejes)
        for (let i = 0; i < 70; i++) {
            const x = (i * 37) % 128;
            const y = (i * 91) % 256;
            const largo = 18 + (i % 5) * 9;
            ctx.strokeStyle = (i % 3 === 0) ? 'rgba(255,255,255,0.32)' : 'rgba(40,70,100,0.14)';
            ctx.lineWidth = (i % 4 === 0) ? 2 : 1;
            for (let dx = -128; dx <= 128; dx += 128) {
                for (let dy = -256; dy <= 256; dy += 256) {
                    ctx.beginPath();
                    ctx.moveTo(x + dx, y + dy);
                    ctx.quadraticCurveTo(x + dx + 4, y + dy + largo * 0.5, x + dx, y + dy + largo);
                    ctx.stroke();
                }
            }
        }
        texturaCorrienteRio = new THREE.CanvasTexture(c);
        texturaCorrienteRio.wrapS = THREE.RepeatWrapping;
        texturaCorrienteRio.wrapT = THREE.RepeatWrapping;
        texturaCorrienteRio.repeat.set(2, 18);
        return texturaCorrienteRio;
    }

    // ========================================================================
    // RETROALIMENTACIÓN DE IMPACTO EN AERONAVES (DESTELLO EMISIVO) Y CACHÉ DE NODOS
    // ========================================================================
    function obtenerNodosCacheados(root) {
        if (root.userData.nodosCache) return root.userData.nodosCache;
        const cache = { materiales: [], propulsor: null, nucleoJefe: null, haloEMP: null, cargaDisparo: null };
        root.traverse(node => {
            if (node.name === 'propulsor' && !cache.propulsor) cache.propulsor = node;
            if (node.name === 'nucleoJefe' && !cache.nucleoJefe) cache.nucleoJefe = node;
            if (node.name === 'haloEMP') cache.haloEMP = node;
            if (node.name === 'cargaDisparo') cache.cargaDisparo = node;
            if (node.isMesh && node.material && node.material.emissive && !node.material.isShaderMaterial) {
                cache.materiales.push({ mat: node.material, emisivoBase: node.material.emissive.getHex(), intensidadBase: node.material.emissiveIntensity || 0 });
            }
        });
        root.userData.nodosCache = cache;
        return cache;
    }

    // Los nodos de efectos (escudos, anillos) son hijos directos del root y sobreviven al reemplazo del GLB
    function obtenerNodoNombrado(root, nombre) {
        const mapa = root.userData.nodosEfecto || (root.userData.nodosEfecto = {});
        if (!(nombre in mapa)) mapa[nombre] = root.getObjectByName(nombre) || null;
        return mapa[nombre];
    }

    function aplicarDestelloImpacto(root, intensidad) {
        const cache = obtenerNodosCacheados(root);
        for (let i = 0; i < cache.materiales.length; i++) {
            const m = cache.materiales[i];
            if (intensidad > 0.01) {
                m.mat.emissive.setRGB(1, 0.92, 0.85);
                m.mat.emissiveIntensity = m.intensidadBase + intensidad * 1.6;
            } else {
                m.mat.emissive.setHex(m.emisivoBase);
                m.mat.emissiveIntensity = m.intensidadBase;
            }
        }
    }

    // ========================================================================
    // GESTOR DE MODELOS 3D GLTF / GLB DE ALTA FIDELIDAD (BLENDER 5.2 BMESH LOFTED)
    // ========================================================================
    const cargadorGLTF = typeof THREE.GLTFLoader !== 'undefined' ? new THREE.GLTFLoader() : null;
    const cacheModelosGLTF = {};
    const rutasModelosGLB = {
        'NAVE_CONDOR_X': 'assets/modelos/Condor_X.glb',
        'NAVE_F35': 'assets/modelos/F35_Lightning_II.glb',
        'NAVE_B2': 'assets/modelos/B2_Spirit.glb',
        'NAVE_F22': 'assets/modelos/F22_Raptor.glb',
        'NAVE_ENEMIGO_LIGERO': 'assets/modelos/Enemigo_Ligero.glb',
        'NAVE_ENEMIGO_ATAQUE': 'assets/modelos/Enemigo_Ataque.glb',
        'NAVE_ENEMIGO_PESADO': 'assets/modelos/Enemigo_Pesado.glb',
        'NAVE_JEFE_VALKIRIA': 'assets/modelos/Valkiria_Boss.glb',
        'ASTEROIDE_VOLCANICO': 'assets/modelos/Asteroide_Volcanico.glb',
        'ITEM_ESCUDO': 'assets/modelos/Item_Escudo.glb',
        'ITEM_REPARACION': 'assets/modelos/Item_Reparacion.glb',
        'MISIL_TACTICO': 'assets/modelos/Misil_Tactico.glb'
    };

    const ESCALAS_MODELOS_GLB = {
        'NAVE_CONDOR_X': 0.78,
        'NAVE_F35': 0.74,
        'NAVE_B2': 0.62,
        'NAVE_F22': 0.72,
        'NAVE_ENEMIGO_LIGERO': 1.02,
        'NAVE_ENEMIGO_ATAQUE': 1.12,
        'NAVE_ENEMIGO_PESADO': 1.30,
        'NAVE_JEFE_VALKIRIA': 1.45
    };

    function calibrarMaterialesInstanciaGLB(instancia, modeloId, colorSkinHex, esEnemigo, esElite) {
        instancia.traverse((node) => {
            if (node.isMesh && node.material) {
                node.castShadow = true;
                node.receiveShadow = true;
                node.material = node.material.clone();
                const matName = (node.material.name || '').toLowerCase();
                const esCristal = matName.includes('canopy') || matName.includes('glass') || matName.includes('visor');
                const esOro = matName.includes('gold') || matName.includes('oro');

                if (node.material.map) {
                    node.material.map.wrapS = THREE.RepeatWrapping;
                    node.material.map.wrapT = THREE.RepeatWrapping;
                    node.material.map.anisotropy = 4;
                    node.material.map.needsUpdate = true;
                }
                if (node.material.normalMap) {
                    node.material.normalMap.wrapS = THREE.RepeatWrapping;
                    node.material.normalMap.wrapT = THREE.RepeatWrapping;
                    node.material.normalMap.anisotropy = 4;
                    if (node.material.normalScale) {
                        node.material.normalScale.set(1.15, 1.15);
                    }
                    node.material.normalMap.needsUpdate = true;
                }

                if (node.material.emissiveMap) {
                    node.material.emissiveMap.wrapS = THREE.RepeatWrapping;
                    node.material.emissiveMap.wrapT = THREE.RepeatWrapping;
                    node.material.emissiveMap.anisotropy = 4;
                    node.material.emissiveMap.needsUpdate = true;
                    if (node.material.emissiveIntensity !== undefined) {
                        node.material.emissiveIntensity = 1.45;
                    }
                }

                if (esCristal) {
                    node.material.metalness = 0.88;
                    node.material.roughness = 0.04;
                    if (node.material.envMapIntensity !== undefined) node.material.envMapIntensity = 1.45;
                } else if (esOro) {
                    node.material.metalness = 0.92;
                    node.material.roughness = 0.16;
                    if (node.material.envMapIntensity !== undefined) node.material.envMapIntensity = 1.25;
                } else {
                    if (node.material.metalness !== undefined) {
                        node.material.metalness = Math.max(0.18, Math.min(node.material.metalness, 0.85));
                    }
                    if (node.material.roughness !== undefined) {
                        node.material.roughness = Math.max(0.15, Math.min(node.material.roughness, 0.55));
                    }
                    if (node.material.envMapIntensity !== undefined) {
                        node.material.envMapIntensity = 0.85;
                    }
                }
                // Personalizar acento de pintura en naves del jugador si eligió otra skin
                if (!esEnemigo && colorSkinHex && (matName.includes('plasma') || matName.includes('cyan') || matName.includes('trim'))) {
                    if (colorSkinHex !== 0x00dcff && colorSkinHex !== 0x00e5ff) {
                        node.material.color.setHex(colorSkinHex);
                        if (node.material.emissive) {
                            node.material.emissive.setHex(colorSkinHex);
                        }
                    }
                }
                // Si es un comandante Élite, realzar sus franjas con fulgor dorado imperial
                if (esEnemigo && esElite && (matName.includes('crimson') || matName.includes('amber') || matName.includes('trim'))) {
                    node.material.color.setHex(0xffb800);
                    if (node.material.emissive) {
                        node.material.emissive.setHex(0xff9100);
                        node.material.emissiveIntensity = 1.2;
                    }
                }
                if (node.name.toLowerCase().includes('flame') || node.name.toLowerCase().includes('thruster')) {
                    node.name = 'propulsor';
                }
                if (node.name.toLowerCase().includes('core')) {
                    node.name = 'nucleoJefe';
                }
            }
        });
    }

    function adjuntarNodosEfectosAeronave(root, colorAcento, esEnemigo, esJefe, modeloId) {
        asegurarRecursosCompartidos3D();
        if (!esEnemigo) {
            // 1. Escudo energético conformal Fresnel (sutil para no lavar el detalle PBR del caza)
            const escudoConformal = new THREE.Mesh(
                new THREE.SphereGeometry(2.25, 22, 16),
                crearMaterialEscudoFresnelGLSL(colorAcento, 0.06)
            );
            escudoConformal.scale.set(1.06, 0.28, 1.02);
            escudoConformal.name = 'burbujaEscudo';
            root.add(escudoConformal);

            // 2. Anillo orbital perimetral fino de Escudo Invencible (bajo el plano alar para no tapar la nave)
            const matAnilloEscudo = obtenerMaterialGlowCompartido(0x00e676, 0.42);
            const anilloEscudoActivo = new THREE.Mesh(recursosCompartidos3D.geomAnilloPoderFino, matAnilloEscudo);
            anilloEscudoActivo.scale.set(1.08, 1.08, 1.0);
            anilloEscudoActivo.position.y = -0.22;
            anilloEscudoActivo.rotation.x = Math.PI / 2;
            anilloEscudoActivo.visible = false;
            anilloEscudoActivo.name = 'anilloEscudoActivo';
            root.add(anilloEscudoActivo);

            // 3. Anillo fino y nodos de Sobrecarga de Armas (visible al activar PODER_OVERDRIVE / Fuego Triple)
            const grupoOverdrive = new THREE.Group();
            grupoOverdrive.name = 'grupoOverdriveArmas';
            grupoOverdrive.position.y = -0.20;
            grupoOverdrive.visible = false;

            const matOroGlow = obtenerMaterialGlowCompartido(0xffb800, 0.45);
            const haloOverdrive = new THREE.Mesh(recursosCompartidos3D.geomAnilloPoderFino, matOroGlow);
            haloOverdrive.scale.set(0.92, 0.92, 1.0);
            haloOverdrive.rotation.x = Math.PI / 2;
            grupoOverdrive.add(haloOverdrive);

            [-1.45, 1.45].forEach(wx => {
                const orbeArma = new THREE.Mesh(recursosCompartidos3D.geomEsferaUnitaria, matOroGlow);
                orbeArma.scale.set(0.12, 0.12, 0.28);
                orbeArma.position.set(wx, 0.18, -0.75);
                grupoOverdrive.add(orbeArma);
            });
            root.add(grupoOverdrive);

            // 4. Anillo holográfico fino de Reparación / Recarga al recoger suministros
            const matReparacion = obtenerMaterialGlowCompartido(0x00e676, 0.50);
            const anilloReparacion = new THREE.Mesh(recursosCompartidos3D.geomAnilloPoderFino, matReparacion);
            anilloReparacion.scale.set(0.95, 0.95, 1.0);
            anilloReparacion.rotation.x = Math.PI / 2;
            anilloReparacion.visible = false;
            anilloReparacion.name = 'anilloReparacion';
            root.add(anilloReparacion);
        } else {
            // Anillo electromagnético de parálisis EMP (visible cuando PODER_EMP congela enemigos)
            const haloEMP = new THREE.Mesh(recursosCompartidos3D.geomAnilloStasis, recursosCompartidos3D.matStasisEMP);
            const escalaEMP = esJefe ? 2.6 : (modeloId === 'NAVE_ENEMIGO_PESADO' ? 1.35 : 0.95);
            haloEMP.scale.set(escalaEMP, escalaEMP, escalaEMP);
            haloEMP.position.y = 0.15;
            haloEMP.visible = false;
            haloEMP.name = 'haloEMP';
            root.add(haloEMP);

            // Indicador de carga de disparo (telegrafía el ataque para permitir esquivarlo con justicia)
            const cargaDisparo = new THREE.Mesh(recursosCompartidos3D.geomEsferaUnitaria, obtenerMaterialGlowCompartido(colorAcento, 0.9));
            cargaDisparo.position.set(0, 0.35, esJefe ? 2.4 : 1.55);
            cargaDisparo.scale.set(0.001, 0.001, 0.001);
            cargaDisparo.visible = false;
            cargaDisparo.name = 'cargaDisparo';
            root.add(cargaDisparo);

            if (esJefe || modeloId === 'NAVE_JEFE_VALKIRIA') {
                const escudoJefe = new THREE.Mesh(
                    new THREE.SphereGeometry(4.8, 24, 18),
                    crearMaterialEscudoFresnelGLSL(0xff1744, 0.24)
                );
                escudoJefe.scale.set(1.1, 0.35, 1.0);
                escudoJefe.name = 'escudoJefe';
                root.add(escudoJefe);
            }
        }
    }

    function precargarModelosGLB() {
        if (!cargadorGLTF) {
            console.warn('[GLTF Loader] THREE.GLTFLoader no está definido en tiempo de ejecución.');
            return;
        }
        Object.keys(rutasModelosGLB).forEach(id => {
            const ruta = rutasModelosGLB[id];
            cargadorGLTF.load(ruta, (gltf) => {
                const modelo = gltf.scene;
                modelo.traverse((node) => {
                    if (node.isMesh) {
                        node.castShadow = true;
                        node.receiveShadow = true;
                        if (node.name.toLowerCase().includes('flame') || node.name.toLowerCase().includes('thruster')) {
                            node.name = 'propulsor';
                        }
                        if (node.name.toLowerCase().includes('core')) {
                            node.name = 'nucleoJefe';
                        }
                    }
                });
                cacheModelosGLTF[id] = modelo;
                if (id === jugador.modeloId && modoEscena === 'HANGAR') {
                    actualizarNaveHangar3D(jugador.modeloId, jugador.colorSkin);
                }
            }, undefined, (err) => {
                console.warn('[GLTF Loader] Error al cargar ' + ruta + ':', err);
            });
        });
    }

    // ========================================================================
    // CONSTRUCTOR DE MODELOS 3D DE ALTA FIDELIDAD (B-2, F-35, F-22, CÓNDOR, FLOTA Y JEFE)
    // ========================================================================
    function crearModeloAeronave3D(modeloId, colorHex, esEnemigo = false, esJefe = false, esElite = false) {
        const root = new THREE.Group();

        const colorPrimario = esEnemigo
            ? (esJefe ? 0x252a36 : 0x364052)
            : 0x182436;
        const colorAcento = esEnemigo
            ? (esJefe ? 0xff1744 : (esElite ? 0xffb800 : 0xff2d55))
            : (colorHex || 0x00dcff);
        const colorOro = 0xffb800;

        // Si el modelo GLTF reconstruido en Blender ya está en caché, instanciarlo con PBR completo
        if (cacheModelosGLTF[modeloId]) {
            const instancia = cacheModelosGLTF[modeloId].clone(true);
            const s = ESCALAS_MODELOS_GLB[modeloId] || 0.78;
            instancia.scale.set(s, s, s);
            calibrarMaterialesInstanciaGLB(instancia, modeloId, colorHex, esEnemigo, esElite);

            if (esEnemigo) {
                instancia.rotation.y = Math.PI;
            }
            root.add(instancia);
            adjuntarNodosEfectosAeronave(root, colorAcento, esEnemigo, esJefe, modeloId);
            return root;
        }

        // Si aún está cargando desde disco, conectar callback para reemplazar malla provisional al terminar
        if (!cacheModelosGLTF[modeloId] && cargadorGLTF && rutasModelosGLB[modeloId]) {
            cargadorGLTF.load(rutasModelosGLB[modeloId], (gltf) => {
                const modelo = gltf.scene;
                cacheModelosGLTF[modeloId] = modelo;
                if (root && root.parent) {
                    root.userData.nodosCache = null;
                    const nombresPreservar = ['burbujaEscudo', 'escudoJefe', 'anilloEscudoActivo', 'grupoOverdriveArmas', 'anilloReparacion', 'haloEMP', 'cargaDisparo'];
                    const hijosARemover = root.children.filter(c => !nombresPreservar.includes(c.name));
                    hijosARemover.forEach(h => root.remove(h));

                    const nuevaInst = modelo.clone(true);
                    const sc = ESCALAS_MODELOS_GLB[modeloId] || 0.78;
                    nuevaInst.scale.set(sc, sc, sc);
                    calibrarMaterialesInstanciaGLB(nuevaInst, modeloId, colorHex, esEnemigo, esElite);
                    if (esEnemigo) {
                        nuevaInst.rotation.y = Math.PI;
                    }
                    root.add(nuevaInst);
                }
            });
        }

        // Respaldo procedural inmediato mientras termina la lectura asíncrona del primer frame
        const matCasco = new THREE.MeshStandardMaterial({
            color: colorPrimario,
            metalness: 0.45,
            roughness: 0.35
        });
        const matAcento = new THREE.MeshStandardMaterial({
            color: colorAcento,
            emissive: colorAcento,
            emissiveIntensity: 0.75,
            metalness: 0.65,
            roughness: 0.2
        });
        const matOro = new THREE.MeshStandardMaterial({
            color: colorOro,
            emissive: 0xff9100,
            emissiveIntensity: 0.4,
            metalness: 0.9,
            roughness: 0.15
        });
        const matCabina = new THREE.MeshPhysicalMaterial({
            color: esEnemigo ? 0xff1744 : 0xffb300,
            emissive: esEnemigo ? 0xaa0022 : 0x774400,
            emissiveIntensity: 0.45,
            metalness: 0.9,
            roughness: 0.08,
            clearcoat: 1.0
        });
        const matPlasma = crearMaterialPlasmaGLSL(esEnemigo);

        const geomAguja = new THREE.ConeGeometry(0.38, 3.6, 10);
        geomAguja.rotateX(-Math.PI / 2);
        const aguja = new THREE.Mesh(geomAguja, matCasco);
        root.add(aguja);

        const geomAlas = new THREE.BoxGeometry(3.8, 0.1, 1.4);
        const alas = new THREE.Mesh(geomAlas, matAcento);
        root.add(alas);

        const cabina = new THREE.Mesh(new THREE.SphereGeometry(0.28, 14, 10), matCabina);
        cabina.scale.set(0.8, 0.6, 1.8);
        cabina.position.set(0, 0.24, -0.5);
        root.add(cabina);

        const llama = new THREE.Mesh(new THREE.ConeGeometry(0.2, 1.1, 10), matPlasma);
        llama.rotateX(Math.PI / 2);
        llama.position.set(0, 0, 1.8);
        llama.name = 'propulsor';
        root.add(llama);

        if (esEnemigo && !esJefe) {
            root.rotation.y = Math.PI;
        }
        adjuntarNodosEfectosAeronave(root, colorAcento, esEnemigo, esJefe, modeloId);
        return root;
    }

    // ========================================================================
    // MODELO Y GENERACIÓN DE METEOROS / ASTEROIDES SÓLIDOS 3D (SIN HUECOS)
    // ========================================================================
    function crearMallaMeteoro3D(radio) {
        const grupo = new THREE.Group();

        if (cacheModelosGLTF['ASTEROIDE_VOLCANICO']) {
            const ast = cacheModelosGLTF['ASTEROIDE_VOLCANICO'].clone(true);
            ast.scale.set(radio * 0.95, radio * 0.95, radio * 0.95);
            ast.traverse(node => {
                if (node.isMesh && node.material) {
                    node.castShadow = true;
                    node.receiveShadow = true;
                    node.material = node.material.clone();
                    node.material.metalness = Math.min(node.material.metalness, 0.12);
                    node.material.roughness = Math.max(node.material.roughness, 0.82);
                    if (node.material.emissive) {
                        node.material.emissive.setHex(0xff5500);
                        node.material.emissiveIntensity = 0.0;
                    }
                }
            });
            ast.name = 'rocaMeteoro';
            grupo.add(ast);
            return grupo;
        }

        // Fallback procedural si aún no cargó el modelo GLB
        const geom = new THREE.IcosahedronGeometry(radio, 3);
        const pos = geom.attributes.position;
        const count = pos.count;
        const v = new THREE.Vector3();

        for (let i = 0; i < count; i++) {
            v.fromBufferAttribute(pos, i);
            const normDir = v.clone().normalize();
            const deform = Math.sin(normDir.x * 3.2 + normDir.z * 2.0) * 0.12 +
                           Math.cos(normDir.y * 3.0 - normDir.x * 2.2) * 0.08;
            v.multiplyScalar(1.0 + deform);
            pos.setXYZ(i, v.x, v.y, v.z);
        }
        geom.computeVertexNormals();

        const matRoca = new THREE.MeshStandardMaterial({
            map: texturasJuego.meteoroRoca,
            normalMap: texturasJuego.meteoroNormal,
            normalScale: new THREE.Vector2(1.8, 1.8),
            roughness: 0.88,
            metalness: 0.05,
            emissive: 0xff5500,
            emissiveIntensity: 0.0
        });

        const roca = new THREE.Mesh(geom, matRoca);
        roca.name = 'rocaMeteoro';
        grupo.add(roca);
        return grupo;
    }

    function desplegarMeteoro3D(posXForzado = null, posZForzado = null) {
        if (!grupoMundoCombate) return;
        const radio = 1.45 + Math.random() * 0.95;
        const mesh = crearMallaMeteoro3D(radio);
        const esMulti = estadoJuego.modalidad && estadoJuego.modalidad.startsWith('Multijugador');

        let posX, posZ, vx, vz;

        if (esMulti && posXForzado === null) {
            // EN MODO MULTIJUGADOR: Los meteoros/obstáculos entran lateralmente desde los flancos
            const vieneDeLaIzquierda = Math.random() < 0.5;
            posX = vieneDeLaIzquierda ? -26.0 : 26.0;
            posZ = -22.0 + Math.random() * 24.0; // En la zona activa de dogfight [-22, +2]
            vx = vieneDeLaIzquierda ? (13.5 + Math.random() * 7.5) : -(13.5 + Math.random() * 7.5);
            vz = (Math.random() - 0.5) * 3.6;
        } else {
            posX = (typeof posXForzado === 'number') ? posXForzado : ((Math.random() - 0.5) * 22);
            posZ = (typeof posZForzado === 'number') ? posZForzado : -70;
            vx = (Math.random() - 0.5) * 2.4;
            vz = 15.5 + Math.random() * 3.5;
        }

        mesh.position.set(posX, 0, posZ);
        grupoMundoCombate.add(mesh);

        const vidaMeteoro = Math.round(180 + radio * 90);
        meteoros3D.push({
            mesh: mesh,
            x: posX,
            z: posZ,
            vx: vx,
            vz: vz,
            rotX: (Math.random() - 0.5) * 2.0,
            rotY: (Math.random() - 0.5) * 2.0,
            radio: radio,
            vida: vidaMeteoro,
            vidaMax: vidaMeteoro,
            tiempoImpactoFlash: 0
        });

        const elAlerta = document.getElementById('hud-alerta-peligro');
        if (elAlerta) {
            elAlerta.classList.remove('oculta');
            clearTimeout(elAlerta._timer);
            elAlerta._timer = setTimeout(() => elAlerta.classList.add('oculta'), 1600);
        }
        sonarEfecto('peligro');
    }

    // ========================================================================
    // INICIALIZACIÓN DEL MOTOR THREE.JS Y ESCENAS 3D
    // ========================================================================
    function inicializar(canvasElement) {
        clock = new THREE.Clock();
        scene = new THREE.Scene();
        scene.background = new THREE.Color(0x040914);
        scene.fog = new THREE.FogExp2(0x061022, 0.014);

        camera = new THREE.PerspectiveCamera(50, window.innerWidth / window.innerHeight, 0.1, 280);

        renderer = new THREE.WebGLRenderer({
            canvas: canvasElement,
            antialias: true,
            alpha: false,
            powerPreference: 'high-performance',
            precision: 'highp',
            depth: true,
            stencil: false,
            preserveDrawingBuffer: false
        });
        renderer.setSize(window.innerWidth, window.innerHeight);
        renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, 2));
        renderer.toneMapping = THREE.ACESFilmicToneMapping;
        renderer.toneMappingExposure = 0.96;
        renderer.shadowMap.enabled = true;
        renderer.shadowMap.type = THREE.PCFSoftShadowMap;

        try {
            const gl = renderer.getContext();
            const dbg = gl.getExtension('WEBGL_debug_renderer_info');
            if (dbg) {
                const gpu = gl.getParameter(dbg.UNMASKED_RENDERER_WEBGL);
                console.log('[Cóndor Táctico 3D] Tarjeta Gráfica Activa (GPU):', gpu);
            }
        } catch (e) { }

        configurarEntornoReflejosPBR();

        luzAmbiente = new THREE.HemisphereLight(0xd8ecff, 0x0e1826, 0.85);
        scene.add(luzAmbiente);

        luzSol = new THREE.DirectionalLight(0xfff6e8, 1.38);
        luzSol.position.set(22, 42, 20);
        // Sombras proyectadas de las aeronaves sobre el terreno: referencia clave de profundidad en vista cenital
        luzSol.castShadow = true;
        luzSol.shadow.mapSize.set(2048, 2048);
        luzSol.shadow.camera.left = -34;
        luzSol.shadow.camera.right = 34;
        luzSol.shadow.camera.top = 46;
        luzSol.shadow.camera.bottom = -46;
        luzSol.shadow.camera.near = 5;
        luzSol.shadow.camera.far = 110;
        luzSol.shadow.bias = -0.0006;
        luzSol.shadow.normalBias = 0.04;
        scene.add(luzSol);

        luzPuntoJugador = new THREE.PointLight(0x00e5ff, 1.15, 20);
        luzPuntoJugador.position.set(0, 3, 8);
        scene.add(luzPuntoJugador);

        luzRellenoAeronaves = new THREE.DirectionalLight(0xaad4ff, 0.65);
        luzRellenoAeronaves.position.set(-18, 45, 12);
        scene.add(luzRellenoAeronaves);

        precargarModelosGLB();

        crearPoolParticulas();
        lineasVelocidad = crearLineasVelocidad();
        lineasVelocidad.visible = false;
        scene.add(lineasVelocidad);
        aplicarCalidadGrafica();

        construirEscenaHangar3D();
        construirEscenaCombate3D(1);
        activarModoHangar();

        configurarEventosEntrada();

        window.addEventListener('resize', () => {
            camera.aspect = window.innerWidth / window.innerHeight;
            camera.updateProjectionMatrix();
            renderer.setSize(window.innerWidth, window.innerHeight);
        });

        bucleRenderizado();
    }

    // Calidad gráfica: Alta (sombras 2048 + DPR hasta 2), Media (sombras 1024 + DPR 1.25), Baja (sin sombras, DPR 1)
    function aplicarCalidadGrafica() {
        if (!renderer) return;
        const dpr = window.devicePixelRatio || 1;
        let ratio, sombras, tamSombra;
        if (calidadGrafica === 'baja') {
            ratio = 1; sombras = false; tamSombra = 512;
        } else if (calidadGrafica === 'media') {
            ratio = Math.min(dpr, 1.25); sombras = true; tamSombra = 1024;
        } else {
            ratio = Math.min(dpr, 2); sombras = true; tamSombra = 2048;
        }
        renderer.setPixelRatio(ratio);
        renderer.setSize(window.innerWidth, window.innerHeight);
        if (renderer.shadowMap.enabled !== sombras) {
            renderer.shadowMap.enabled = sombras;
            // Forzar recompilación de materiales para activar/desactivar el muestreo de sombras
            scene.traverse(n => { if (n.material) { (Array.isArray(n.material) ? n.material : [n.material]).forEach(m => { m.needsUpdate = true; }); } });
        }
        if (luzSol && luzSol.shadow.mapSize.x !== tamSombra) {
            luzSol.shadow.mapSize.set(tamSombra, tamSombra);
            if (luzSol.shadow.map) {
                luzSol.shadow.map.dispose();
                luzSol.shadow.map = null;
            }
        }
        if (lineasVelocidad) lineasVelocidad.userData.habilitado = calidadGrafica !== 'baja';
    }

    // ========================================================================
    // ESCENA 1: BAHÍA DE HANGAR 3D AAA (VISTA DESPEJADA SIN OBSTRUCCIÓN DE PILARES)
    // ========================================================================
    function construirEscenaHangar3D() {
        if (grupoHangar) scene.remove(grupoHangar);
        grupoHangar = new THREE.Group();

        // Plataforma circular metálica multinivel del hangar
        const matPlat = new THREE.MeshStandardMaterial({
            color: 0x091324,
            metalness: 0.88,
            roughness: 0.22
        });
        const plataforma = new THREE.Mesh(new THREE.CylinderGeometry(6.2, 7.0, 0.5, 64), matPlat);
        plataforma.position.y = -1.25;
        grupoHangar.add(plataforma);

        const plataformaInterior = new THREE.Mesh(
            new THREE.CylinderGeometry(4.1, 4.4, 0.22, 48),
            new THREE.MeshStandardMaterial({ color: 0x11223c, metalness: 0.85, roughness: 0.18 })
        );
        plataformaInterior.position.y = -0.98;
        grupoHangar.add(plataformaInterior);

        // Anillos luminosos concéntricos dorados y cian
        const matAnilloCian = new THREE.MeshBasicMaterial({ color: 0x00e5ff });
        anilloHangar1 = new THREE.Mesh(new THREE.TorusGeometry(5.3, 0.055, 14, 72), matAnilloCian);
        anilloHangar1.rotation.x = Math.PI / 2;
        anilloHangar1.position.y = -0.94;
        grupoHangar.add(anilloHangar1);

        const matAnilloOro = new THREE.MeshBasicMaterial({ color: 0xffb800 });
        anilloHangar2 = new THREE.Mesh(new THREE.TorusGeometry(3.85, 0.045, 14, 64), matAnilloOro);
        anilloHangar2.rotation.x = Math.PI / 2;
        anilloHangar2.position.y = -0.86;
        grupoHangar.add(anilloHangar2);

        // Arcos estructurales superiores del Hangar Espacial (amplio radio 13.5)
        const matArco = new THREE.MeshStandardMaterial({
            color: 0x102038,
            metalness: 0.85,
            roughness: 0.25,
            emissive: 0x003366,
            emissiveIntensity: 0.35
        });
        [-7.5, -1.5, 4.5].forEach(zPos => {
            const arco = new THREE.Mesh(new THREE.TorusGeometry(13.5, 0.30, 10, 48, Math.PI), matArco);
            arco.position.set(0, -1.4, zPos);
            grupoHangar.add(arco);

            const luzArco = new THREE.Mesh(
                new THREE.TorusGeometry(13.15, 0.06, 8, 48, Math.PI),
                new THREE.MeshBasicMaterial({ color: zPos === -1.5 ? 0xffb800 : 0x00e5ff })
            );
            luzArco.position.set(0, -1.4, zPos);
            grupoHangar.add(luzArco);
        });

        // Haz volumétrico de luz sobre la aeronave
        const matHaz = new THREE.MeshBasicMaterial({
            color: 0x00e5ff,
            transparent: true,
            opacity: 0.06,
            blending: THREE.AdditiveBlending,
            depthWrite: false,
            side: THREE.DoubleSide
        });
        const hazCentral = new THREE.Mesh(new THREE.ConeGeometry(5.2, 10.0, 32, 1, true), matHaz);
        hazCentral.position.set(0.35, 3.8, 0);
        grupoHangar.add(hazCentral);

        // Balizas perimetrales bajas en los flancos laterales del hangar (sin cortar el fondo detrás del caza)
        [-11.5, 11.5].forEach(px => {
            [-6.0, 0.0, 6.0].forEach(pz => {
                const pilar = new THREE.Mesh(
                    new THREE.BoxGeometry(0.36, 2.4, 0.36),
                    new THREE.MeshStandardMaterial({ color: 0x162946, emissive: 0x0088cc, emissiveIntensity: 0.45 })
                );
                pilar.position.set(px, -0.2, pz);
                grupoHangar.add(pilar);
            });
        });

        // Luces guía de pista en el suelo de la bahía
        [-2.4, 2.4].forEach(lx => {
            for (let lz = -5; lz <= 5; lz += 2.5) {
                const baliza = new THREE.Mesh(
                    new THREE.BoxGeometry(0.18, 0.05, 0.45),
                    new THREE.MeshBasicMaterial({ color: lz === 0 ? 0xffb800 : 0x00e5ff })
                );
                baliza.position.set(lx + 0.35, -1.0, lz);
                grupoHangar.add(baliza);
            }
        });

        const chispasHangar = crearCampoParticulas3D(320, 14, 0xffd54f, 0.10);
        grupoHangar.add(chispasHangar);

        const grid = new THREE.GridHelper(56, 36, 0x00dcff, 0x0d2240);
        grid.position.y = -1.48;
        grid.material.transparent = true;
        grid.material.opacity = 0.38;
        grupoHangar.add(grid);

        actualizarNaveHangar3D('NAVE_CONDOR_X', 0x00dcff);
        scene.add(grupoHangar);
    }

    function actualizarNaveHangar3D(modeloId, colorSkinHex) {
        if (!grupoHangar) return;
        if (naveHangarMesh) {
            grupoHangar.remove(naveHangarMesh);
        }
        jugador.modeloId = modeloId || 'NAVE_CONDOR_X';
        jugador.colorSkin = colorSkinHex || 0x00dcff;

        naveHangarMesh = crearModeloAeronave3D(jugador.modeloId, jugador.colorSkin, false, false, false);
        naveHangarMesh.scale.set(1.08, 1.08, 1.08);
        naveHangarMesh.position.set(0.05, 0.35, 0.0);
        naveHangarMesh.rotation.y = Math.PI * 0.78;
        grupoHangar.add(naveHangarMesh);
    }

    function activarModoHangar() {
        modoEscena = 'HANGAR';
        estadoJuego.enCurso = false;
        limpiarBarrasSaludEnemigosDOM();
        actualizarChipsPotenciadoresDOM();
        vaciarPoolParticulas();
        if (lineasVelocidad) lineasVelocidad.visible = false;
        if (jugador.estelas) jugador.estelas.forEach(e => { e.mesh.visible = false; });
        actualizarMotorAudio(false, 0, false);
        actualizarOverlayDanoDOM(0, 1);
        escalaTiempo = 1.0;
        tiempoCamaraSlowMo = 0;
        if (camera && camera.fov !== 50) {
            camera.fov = 50;
            camera.updateProjectionMatrix();
        }
        if (grupoHangar) grupoHangar.visible = true;
        if (grupoMundoCombate) grupoMundoCombate.visible = false;
        scene.background = new THREE.Color(0x040915);
        scene.fog = new THREE.FogExp2(0x040915, 0.020);
        if (luzAmbiente) {
            luzAmbiente.color.setHex(0xd8ecff);
            luzAmbiente.groundColor.setHex(0x0a1424);
            luzAmbiente.intensity = 0.82;
        }
        if (luzSol) {
            luzSol.color.setHex(0xfff6e8);
            luzSol.intensity = 1.35;
            luzSol.position.set(18, 36, 22);
        }
        if (luzRellenoAeronaves) {
            luzRellenoAeronaves.intensity = 0.60;
        }
    }

    // ========================================================================
    // ESCENA 2: MUNDO 3D DE COMBATE (RELIEVE ESTRATIFICADO, RÍO Y ESTRUCTURAS 3D)
    // ========================================================================
    const temasBiomas = {
        1: {
            cielo: 0x0f2847,
            niebla: 0x18385c,
            agua: 0x0088cc,
            aguaEmisiva: 0x003355,
            ribera: [0.10, 0.24, 0.20],
            valle: [0.12, 0.26, 0.18],
            ladera: [0.34, 0.30, 0.25],
            cumbre: [0.85, 0.92, 0.98],
            luz: 0xfff6e0,
            colorEstructura: 0x26384c,
            acentoEstructura: 0x00e5ff
        },
        2: {
            cielo: 0x0c152c,
            niebla: 0x142240,
            agua: 0x00a8cc,
            aguaEmisiva: 0x003852,
            ribera: [0.10, 0.16, 0.26],
            valle: [0.13, 0.19, 0.30],
            ladera: [0.26, 0.33, 0.46],
            cumbre: [0.68, 0.78, 0.92],
            luz: 0xb8d4ff,
            colorEstructura: 0x1e293b,
            acentoEstructura: 0x00e5ff
        },
        3: {
            cielo: 0x072236,
            niebla: 0x0e3854,
            agua: 0x00d8ff,
            aguaEmisiva: 0x005577,
            ribera: [0.08, 0.24, 0.34],
            valle: [0.10, 0.28, 0.38],
            ladera: [0.24, 0.48, 0.62],
            cumbre: [0.92, 0.98, 1.00],
            luz: 0xa0ffff,
            colorEstructura: 0x204860,
            acentoEstructura: 0x80f8ff
        },
        4: {
            cielo: 0x260909,
            niebla: 0x3e1210,
            agua: 0xff4500,
            aguaEmisiva: 0xcc2900,
            ribera: [0.28, 0.10, 0.06],
            valle: [0.16, 0.09, 0.09],
            ladera: [0.32, 0.16, 0.14],
            cumbre: [0.95, 0.52, 0.18],
            luz: 0xffa058,
            colorEstructura: 0x281515,
            acentoEstructura: 0xff5722
        },
        5: {
            cielo: 0x050718,
            niebla: 0x0d1130,
            agua: 0x7928ca,
            aguaEmisiva: 0x4a148c,
            ribera: [0.14, 0.08, 0.26],
            valle: [0.10, 0.09, 0.22],
            ladera: [0.22, 0.18, 0.42],
            cumbre: [0.72, 0.58, 0.98],
            luz: 0xeea8ff,
            colorEstructura: 0x1a1638,
            acentoEstructura: 0xd500f9
        }
    };

    const PERIODO_MAPA_Z = 160.0;
    const OMEGA_MAPA_Z = (Math.PI * 2.0) / PERIODO_MAPA_Z;

    function calcularCanyonCenter(z) {
        // Armónicos exactos del periodo L = 160 para continuidad C^∞ sin costuras entre bloques
        return Math.sin(z * OMEGA_MAPA_Z) * 2.6 + Math.cos(z * OMEGA_MAPA_Z * 2.0) * 1.15;
    }

    function calcularElevacionMundo(x, z, nivel) {
        const centroRio = calcularCanyonCenter(z);
        const distRio = Math.abs(x - centroRio);
        const absX = Math.abs(x);

        // Todas las frecuencias en Z son múltiplos enteros exactos de OMEGA_MAPA_Z (2π / 160)
        const r1 = Math.sin(x * 0.07 + z * OMEGA_MAPA_Z) * 1.6 + Math.cos(x * 0.12 - z * OMEGA_MAPA_Z * 2.0) * 1.1;
        const r2 = Math.sin(x * 0.24 + z * OMEGA_MAPA_Z * 4.0) * 0.55 + Math.cos(x * 0.38 - z * OMEGA_MAPA_Z * 6.0) * 0.3;
        const ruido = r1 + r2;

        // 1. CORREDOR DE VUELO SEGURO GARANTIZADO (|x| <= 20.5):
        // En todo |x| <= 20.5 el terreno permanece entre Y = -9.6 y Y = -4.8
        if (absX <= CONFIG_MOTOR.MITAD_CORREDOR_SEGURO_X) {
            const factorCauce = Math.min(1.0, distRio / 8.5);
            const factorTerraza = absX / CONFIG_MOTOR.MITAD_CORREDOR_SEGURO_X;
            const elevValle = CONFIG_MOTOR.PROFUNDIDAD_LECHO_RIO_Y
                + (factorCauce * factorCauce) * 2.2
                + (factorTerraza * factorTerraza) * 1.8
                + ruido * 0.25;
            return Math.min(CONFIG_MOTOR.TECHO_MAXIMO_VALLE_Y, elevValle);
        }

        // 2. CORDILLERAS EXTERIORES EN LOS FLANCOS (|x| > 20.5)
        const distFlanco = absX - CONFIG_MOTOR.MITAD_CORREDOR_SEGURO_X;
        const factorMontana = Math.min(1.0, distFlanco / 38.0);
        const bioma = Math.max(1, Math.min(5, Math.ceil(nivel / 2)));
        const escalaBioma = (bioma === 4) ? 1.08 : ((bioma === 3) ? 1.04 : 1.0);
        const cresta = Math.pow(factorMontana, 1.35) * (10.5 + Math.sin(z * OMEGA_MAPA_Z + x * 0.03) * 2.6) * escalaBioma;
        const elevFlanco = CONFIG_MOTOR.TECHO_MAXIMO_VALLE_Y + cresta + ruido * 0.48;

        return Math.min(7.8, elevFlanco);
    }

    function aplicarRelieveYColoresAChunk(geom, chunkOffsetZ, nivel) {
        const bioma = Math.max(1, Math.min(5, Math.ceil(nivel / 2)));
        const tema = temasBiomas[bioma] || temasBiomas[1];
        const pos = geom.attributes.position;
        const count = pos.count;

        if (!geom.attributes.color) {
            geom.setAttribute('color', new THREE.BufferAttribute(new Float32Array(count * 3), 3));
        }
        if (!geom.attributes.normal) {
            geom.setAttribute('normal', new THREE.BufferAttribute(new Float32Array(count * 3), 3));
        }
        const colAttr = geom.attributes.color;
        const normAttr = geom.attributes.normal;
        const eps = 0.25;

        for (let i = 0; i < count; i++) {
            const localX = pos.getX(i);
            const localZ = pos.getZ(i);
            const sampleZ = localZ; // Función 100% periódica de periodo 160 centrada en [-80, +80]

            const elev = calcularElevacionMundo(localX, sampleZ, nivel);
            pos.setY(i, elev);

            // Normales continuas analíticas por diferencias centrales (sin costura en los bordes ±80)
            const hL = calcularElevacionMundo(localX - eps, sampleZ, nivel);
            const hR = calcularElevacionMundo(localX + eps, sampleZ, nivel);
            const hD = calcularElevacionMundo(localX, sampleZ - eps, nivel);
            const hU = calcularElevacionMundo(localX, sampleZ + eps, nivel);
            const nx = hL - hR;
            const ny = 2.0 * eps;
            const nz = hD - hU;
            const invLen = 1.0 / Math.sqrt(nx * nx + ny * ny + nz * nz);
            normAttr.setXYZ(i, nx * invLen, ny * invLen, nz * invLen);

            // Coloreado estratificado periódico por elevación y distancia al cauce
            const centroRio = calcularCanyonCenter(sampleZ);
            const distRio = Math.abs(localX - centroRio);
            const variacion = Math.sin(localX * 0.15 + sampleZ * OMEGA_MAPA_Z * 3.0) * 0.04;

            let r, g, b;
            if (distRio < 5.2 && Math.abs(localX) < 12.0) {
                const t = distRio / 5.2;
                r = tema.ribera[0] * (1 - t) + tema.valle[0] * t;
                g = tema.ribera[1] * (1 - t) + tema.valle[1] * t;
                b = tema.ribera[2] * (1 - t) + tema.valle[2] * t;
            } else if (elev <= CONFIG_MOTOR.TECHO_MAXIMO_VALLE_Y + 0.35) {
                const t = Math.max(0, Math.min(1, (elev - CONFIG_MOTOR.PROFUNDIDAD_LECHO_RIO_Y) / 4.8));
                r = tema.valle[0] * (1 - t * 0.35) + tema.ladera[0] * (t * 0.35);
                g = tema.valle[1] * (1 - t * 0.35) + tema.ladera[1] * (t * 0.35);
                b = tema.valle[2] * (1 - t * 0.35) + tema.ladera[2] * (t * 0.35);
            } else if (elev < 1.8) {
                const t = Math.max(0, Math.min(1, (elev - CONFIG_MOTOR.TECHO_MAXIMO_VALLE_Y) / 6.6));
                r = tema.valle[0] * (1 - t) + tema.ladera[0] * t;
                g = tema.valle[1] * (1 - t) + tema.ladera[1] * t;
                b = tema.valle[2] * (1 - t) + tema.ladera[2] * t;
            } else {
                const t = Math.max(0, Math.min(1, (elev - 1.8) / 5.2));
                r = tema.ladera[0] * (1 - t) + tema.cumbre[0] * t;
                g = tema.ladera[1] * (1 - t) + tema.cumbre[1] * t;
                b = tema.ladera[2] * (1 - t) + tema.cumbre[2] * t;
            }

            colAttr.setXYZ(
                i,
                Math.max(0, Math.min(1, r + variacion)),
                Math.max(0, Math.min(1, g + variacion)),
                Math.max(0, Math.min(1, b + variacion))
            );
        }
        pos.needsUpdate = true;
        colAttr.needsUpdate = true;
        normAttr.needsUpdate = true;
    }

    // Construye hitos arquitectónicos y naturales 3D en el fondo del cañón y flancos exteriores
    // (100% fuera del corredor de vuelo |x| <= 20.5 a Y = 0 para no interferir jamás con la lectura de juego)
    function crearHitoEscenario3D(nivel, indice, localZ = 0) {
        const bioma = Math.max(1, Math.min(5, Math.ceil(nivel / 2)));
        const tema = temasBiomas[bioma] || temasBiomas[1];
        const grupo = new THREE.Group();

        const matBase = new THREE.MeshStandardMaterial({
            color: tema.colorEstructura,
            metalness: 0.65,
            roughness: 0.35
        });
        const matLuz = new THREE.MeshBasicMaterial({
            color: tema.acentoEstructura
        });

        if (indice % 2 === 0) {
            // Esclusas / Pilonos Hidroeléctricos Gemelos en las riberas del río (sin cruzar X=0 para máxima claridad)
            const cCenter = calcularCanyonCenter(localZ);
            [-8.4, 8.4].forEach(px => {
                const signo = Math.sign(px);
                const elevSuelo = calcularElevacionMundo(cCenter + px, localZ, nivel);
                const pilar = new THREE.Mesh(new THREE.BoxGeometry(1.4, 3.2, 1.8), matBase);
                pilar.position.set(px, elevSuelo + 0.6, 0);
                grupo.add(pilar);

                const contrafuerte = new THREE.Mesh(new THREE.BoxGeometry(2.2, 0.45, 1.1), matBase);
                contrafuerte.position.set(px - signo * 0.9, elevSuelo + 1.6, 0);
                contrafuerte.rotation.z = -signo * 0.22;
                grupo.add(contrafuerte);

                const baliza = new THREE.Mesh(new THREE.OctahedronGeometry(0.32, 0), matLuz);
                baliza.position.set(px - signo * 1.6, elevSuelo + 2.0, 0);
                grupo.add(baliza);
            });
            grupo.userData.esCauceCentral = true;
        } else {
            // Torreta de Radar / Aguja Cristalina / Pilono Energético en los flancos exteriores (|x| >= 24)
            const lado = (indice % 4 === 1) ? -1 : 1;
            const posX = lado * (24.5 + (indice % 3) * 2.8);
            const elevSuelo = calcularElevacionMundo(posX, localZ, nivel);
            const baseY = elevSuelo;

            if (bioma === 3 || bioma === 4) {
                // Aguja de hielo glaciar o monolito basáltico volcánico
                const aguja = new THREE.Mesh(new THREE.ConeGeometry(1.8, 7.5, 6), matBase);
                aguja.position.set(posX, baseY + 3.4, 0);
                aguja.rotation.z = -lado * 0.12;
                grupo.add(aguja);

                const cristal = new THREE.Mesh(new THREE.OctahedronGeometry(0.65, 0), matLuz);
                cristal.position.set(posX, baseY + 7.2, 0);
                grupo.add(cristal);
            } else {
                // Torre de comunicaciones / Pilono defensivo andino
                const torre = new THREE.Mesh(new THREE.CylinderGeometry(0.55, 1.1, 6.2, 8), matBase);
                torre.position.set(posX, baseY + 2.8, 0);
                grupo.add(torre);

                const anillo = new THREE.Mesh(new THREE.TorusGeometry(1.15, 0.12, 8, 20), matLuz);
                anillo.rotation.x = Math.PI / 2;
                anillo.position.set(posX, baseY + 5.2, 0);
                grupo.add(anillo);
            }
            grupo.userData.esCauceCentral = false;
        }
        return grupo;
    }

    function construirEscenaCombate3D(nivel) {
        asegurarRecursosCompartidos3D();
        if (grupoMundoCombate) {
            chunksTerreno.forEach(c => {
                c.traverse(node => {
                    if (node.isMesh && node.geometry) node.geometry.dispose();
                });
            });
            nubes3D.forEach(n => {
                if (n.material) n.material.dispose();
            });
            scene.remove(grupoMundoCombate);
        }
        grupoMundoCombate = new THREE.Group();
        chunksTerreno = [];
        estructurasEscenario3D = [];
        nubes3D = [];
        mallaRio = null;
        nivelActivoCombate = nivel;

        const bioma = Math.max(1, Math.min(5, Math.ceil(nivel / 2)));
        const tema = temasBiomas[bioma] || temasBiomas[1];
        scene.background = new THREE.Color(tema.cielo);
        scene.fog = new THREE.FogExp2(tema.niebla, 0.0058);
        luzSol.color.setHex(tema.luz);

        if (bioma === 4) {
            luzAmbiente.color.setHex(0xffb888);
            luzAmbiente.groundColor.setHex(0x28120a);
            luzAmbiente.intensity = 0.85;
            luzSol.intensity = 1.38;
        } else if (bioma === 3) {
            luzAmbiente.color.setHex(0xd4f4ff);
            luzAmbiente.groundColor.setHex(0x162838);
            luzAmbiente.intensity = 0.88;
            luzSol.intensity = 1.40;
        } else if (bioma === 5) {
            luzAmbiente.color.setHex(0xcca0ff);
            luzAmbiente.groundColor.setHex(0x140d24);
            luzAmbiente.intensity = 0.82;
            luzSol.intensity = 1.35;
        } else if (bioma === 2) {
            luzAmbiente.color.setHex(0xaec8ea);
            luzAmbiente.groundColor.setHex(0x111c2e);
            luzAmbiente.intensity = 0.84;
            luzSol.intensity = 1.38;
        } else {
            luzAmbiente.color.setHex(0xdceeff);
            luzAmbiente.groundColor.setHex(0x182622);
            luzAmbiente.intensity = 0.86;
            luzSol.intensity = 1.42;
        }

        // Sol alto y ligeramente detrás de la cámara: la sombra del avión cae por delante y a un lado (siempre legible)
        luzSol.position.set(16, 46, 12);
        luzSol.target.position.set(0, 0, -16);
        if (!luzSol.target.parent) scene.add(luzSol.target);

        if (luzRellenoAeronaves) {
            luzRellenoAeronaves.intensity = 0.62;
            luzRellenoAeronaves.position.set(-16, 40, 20);
            luzRellenoAeronaves.target.position.set(0, 0, 5);
            if (!luzRellenoAeronaves.target.parent) scene.add(luzRellenoAeronaves.target);
        }

        // 1. Bloques Maestros Periódicos (Terreno + Río + Estructuras solidarios en cada bloque de 160u)
        const texGeologica = obtenerTexturaGeologicaBioma(bioma);
        const matTerreno = new THREE.MeshStandardMaterial({
            map: texGeologica,
            vertexColors: true,
            roughness: 0.90,
            metalness: 0.04,
            envMapIntensity: 0.10
        });

        const matRio = new THREE.MeshStandardMaterial({
            map: obtenerTexturaCorrienteRio(),
            color: tema.agua,
            emissive: tema.aguaEmisiva,
            emissiveIntensity: bioma === 4 ? 1.05 : (bioma === 5 ? 0.85 : 0.32),
            metalness: bioma === 4 ? 0.20 : 0.68,
            roughness: bioma === 4 ? 0.42 : 0.22,
            envMapIntensity: 0.28
        });
        mallaRio = { material: matRio };

        const tamanoChunk = PERIODO_MAPA_Z; // 160.0 exacto
        const segmentosGrid = 96;

        // Geometría periódica maestra de terreno y río (compartida con empalme C^∞ exacto en z = ±80)
        const geomChunkMaestro = new THREE.PlaneGeometry(190, tamanoChunk, segmentosGrid, segmentosGrid);
        geomChunkMaestro.rotateX(-Math.PI / 2);
        aplicarRelieveYColoresAChunk(geomChunkMaestro, 0, nivel);

        const geomRioMaestro = new THREE.PlaneGeometry(13.5, tamanoChunk, 24, 96);
        geomRioMaestro.rotateX(-Math.PI / 2);
        const posRio = geomRioMaestro.attributes.position;
        for (let i = 0; i < posRio.count; i++) {
            const rz = posRio.getZ(i);
            const rx = posRio.getX(i);
            posRio.setX(i, rx + calcularCanyonCenter(rz));
        }
        geomRioMaestro.computeVertexNormals();

        // 5 sectores periódicos consecutivos (total 800u: horizonte infinito sin corte visible ni pop)
        [0, -160, -320, -480, -640].forEach(posZ => {
            const grupoSector = new THREE.Group();
            grupoSector.position.set(0, 0, posZ);

            const meshTerreno = new THREE.Mesh(geomChunkMaestro, matTerreno);
            meshTerreno.renderOrder = 0;
            meshTerreno.receiveShadow = true;
            grupoSector.add(meshTerreno);

            const meshRio = new THREE.Mesh(geomRioMaestro, matRio);
            meshRio.receiveShadow = true;
            meshRio.position.set(0, -8.45, 0);
            meshRio.renderOrder = 0;
            grupoSector.add(meshRio);

            // 4 hitos ambientales por sector de 160u (espaciados cada 40u en localZ = -60, -20, +20, +60)
            [-60, -20, 20, 60].forEach((localZ, idx) => {
                const hito = crearHitoEscenario3D(nivel, idx, localZ);
                const xHito = hito.userData.esCauceCentral ? calcularCanyonCenter(localZ) : 0;
                hito.position.set(xHito, 0, localZ);
                grupoSector.add(hito);
            });

            grupoMundoCombate.add(grupoSector);
            chunksTerreno.push(grupoSector);
        });

        // 4. Nubes Volumétricas con disciplina estricta de altitud y flanco
        for (let i = 0; i < 18; i++) {
            const matNubeSprite = new THREE.SpriteMaterial({
                map: texturasJuego.particulaHumo,
                color: nivel === 4 ? 0xff7043 : (nivel === 3 ? 0xcceeff : 0xd8e6f5),
                transparent: true,
                opacity: 0.15,
                blending: THREE.NormalBlending,
                depthWrite: false
            });
            const spriteNube = new THREE.Sprite(matNubeSprite);
            const escalaNube = 16.0 + Math.random() * 14.0;
            spriteNube.scale.set(escalaNube, escalaNube * 0.40, 1);

            const enValleProfundo = (i % 2 === 0);
            const alturaNube = enValleProfundo
                ? (-7.0 + (i % 3) * 0.8)
                : (1.2 + (i % 3) * 1.4);
            const lado = (i % 4 < 2) ? -1 : 1;
            const posX = enValleProfundo
                ? ((Math.random() - 0.5) * 30)
                : (lado * (32 + Math.random() * 22));
            const posZ = -180 + i * 18;

            spriteNube.position.set(posX, alturaNube, posZ);
            spriteNube.renderOrder = 1;
            grupoMundoCombate.add(spriteNube);
            nubes3D.push(spriteNube);
        }

        scene.add(grupoMundoCombate);
    }

    // ========================================================================
    // GESTIÓN DE BARRAS DE SALUD DE ENEMIGOS EN ESPACIO DE MUNDO (DOM PROYECTADO)
    // ========================================================================
    function asegurarContenedorBarrasEnemigos() {
        if (!contenedorBarrasEnemigosDOM) {
            contenedorBarrasEnemigosDOM = document.getElementById('hud-barras-enemigos-mundo');
        }
        return contenedorBarrasEnemigosDOM;
    }

    function eliminarBarraSaludEnemigoDOM(en) {
        if (en && en.elBarraDOM) {
            if (en.elBarraDOM.parentNode) {
                en.elBarraDOM.parentNode.removeChild(en.elBarraDOM);
            }
            en.elBarraDOM = null;
        }
    }

    function limpiarBarrasSaludEnemigosDOM() {
        enemigos.forEach(en => eliminarBarraSaludEnemigoDOM(en));
        const cont = asegurarContenedorBarrasEnemigos();
        if (cont) cont.innerHTML = '';
    }

    function crearOActualizarElementoBarraEnemigo(en) {
        const cont = asegurarContenedorBarrasEnemigos();
        if (!cont) return null;

        if (!en.elBarraDOM) {
            const el = document.createElement('div');
            let modClase = '';
            if (en.esJefe) modClase = 'es-jefe';
            else if (en.esElite) modClase = 'elite';
            else if (en.modeloId === 'NAVE_ENEMIGO_PESADO') modClase = 'clase-pesado';
            else if (en.modeloId === 'NAVE_ENEMIGO_ATAQUE') modClase = 'clase-ataque';

            el.className = `barra-enemigo-mundo ${modClase}`.trim();
            const etiquetaCorta = en.esJefe
                ? 'JEFE VALKIRIA'
                : (en.esElite ? 'ÉLITE GÓLEM' : (en.modeloId === 'NAVE_ENEMIGO_PESADO' ? 'GÓLEM' : (en.modeloId === 'NAVE_ENEMIGO_ATAQUE' ? 'VÍBORA' : 'HALCÓN')));

            el.innerHTML = `
                <div class="barra-enemigo-etiqueta">
                    <span class="lbl-clase">${etiquetaCorta}</span>
                    <span class="lbl-hp">${Math.max(0, Math.ceil(en.vida))}</span>
                </div>
                <div class="barra-enemigo-pista">
                    <div class="barra-enemigo-lag" style="width:100%;"></div>
                    <div class="barra-enemigo-fill" style="width:100%;"></div>
                </div>
            `;
            cont.appendChild(el);
            en.elBarraDOM = el;
            en.elFillDOM = el.querySelector('.barra-enemigo-fill');
            en.elLagDOM = el.querySelector('.barra-enemigo-lag');
            en.elHpTxtDOM = el.querySelector('.lbl-hp');
            en.elClaseTxtDOM = el.querySelector('.lbl-clase');
        }
        return en.elBarraDOM;
    }

    const vecProyeccionTemp = new THREE.Vector3();

    function actualizarBarrasSaludEnemigosDOM(dt) {
        const cont = asegurarContenedorBarrasEnemigos();
        if (!cont || !camera) return;

        camera.updateMatrixWorld();
        const anchoPantalla = window.innerWidth;
        const altoPantalla = window.innerHeight;
        const paralizados = estadoJuego.tiempoEnemigosParalizados > 0;

        // 1. Calcular coordenadas 2D proyectadas para cada enemigo activo
        const proyectados = [];
        for (let i = 0; i < enemigos.length; i++) {
            const en = enemigos[i];
            if (en.vida <= 0 || !en.mesh) {
                eliminarBarraSaludEnemigoDOM(en);
                continue;
            }

            const el = crearOActualizarElementoBarraEnemigo(en);
            if (!el) continue;

            // Actualizar lag de daño blanco suave
            if (typeof en.vidaVisualLag !== 'number') en.vidaVisualLag = en.vida;
            if (en.vidaVisualLag > en.vida) {
                en.vidaVisualLag = Math.max(en.vida, en.vidaVisualLag - (en.vidaMax * 0.55) * dt);
            } else {
                en.vidaVisualLag = en.vida;
            }

            if (en.tiempoImpactoReciente > 0) {
                en.tiempoImpactoReciente -= dt;
                el.classList.add('impacto-reciente');
            } else {
                el.classList.remove('impacto-reciente');
            }

            if (paralizados) {
                el.classList.add('paralizado');
            } else {
                el.classList.remove('paralizado');
            }

            const offsetAltura = en.esJefe ? 2.45 : (en.modeloId === 'NAVE_ENEMIGO_PESADO' ? 1.55 : 1.25);
            vecProyeccionTemp.set(en.x, (en.mesh.position.y || 0.3) + offsetAltura, en.z);
            vecProyeccionTemp.project(camera);

            // Verificar que esté dentro del frustum visible frente a la cámara
            if (vecProyeccionTemp.z > 1.0 || vecProyeccionTemp.z < -1.0 || en.z < -92 || en.z > 12) {
                el.style.opacity = '0';
                continue;
            }

            const sx = (vecProyeccionTemp.x * 0.5 + 0.5) * anchoPantalla;
            const sy = (-vecProyeccionTemp.y * 0.5 + 0.5) * altoPantalla;

            const distCam = Math.hypot(camera.position.x - en.x, camera.position.z - en.z);
            const escalaDist = Math.max(0.76, Math.min(1.15, 36.0 / Math.max(20.0, distCam)));

            proyectados.push({
                en,
                el,
                sx,
                sy,
                escala: escalaDist
            });
        }

        // 2. Separación suave en espacio de pantalla 2D para evitar superposición ilegible
        for (let iter = 0; iter < 2; iter++) {
            for (let a = 0; a < proyectados.length; a++) {
                for (let b = a + 1; b < proyectados.length; b++) {
                    const pa = proyectados[a];
                    const pb = proyectados[b];
                    const dx = pb.sx - pa.sx;
                    const dy = pb.sy - pa.sy;
                    if (Math.abs(dx) < 82 && Math.abs(dy) < 24) {
                        const empujeY = (24 - Math.abs(dy)) * 0.5;
                        if (pa.sy <= pb.sy) {
                            pa.sy -= empujeY;
                            pb.sy += empujeY;
                        } else {
                            pa.sy += empujeY;
                            pb.sy -= empujeY;
                        }
                    }
                }
            }
        }

        // 3. Aplicar posición, escala y porcentaje de salud en tiempo real
        for (let i = 0; i < proyectados.length; i++) {
            const item = proyectados[i];
            const en = item.en;
            const el = item.el;

            const pct = Math.max(0, Math.min(100, (en.vida / Math.max(1, en.vidaMax)) * 100));
            const pctLag = Math.max(pct, Math.min(100, (en.vidaVisualLag / Math.max(1, en.vidaMax)) * 100));

            if (en.elFillDOM) {
                en.elFillDOM.style.width = `${pct.toFixed(1)}%`;
                if (pct <= 28) {
                    en.elFillDOM.style.background = 'linear-gradient(90deg, #d50000, #ff1744)';
                } else if (pct <= 55 && !en.esElite) {
                    en.elFillDOM.style.background = 'linear-gradient(90deg, #ff6d00, #ffd54f)';
                } else {
                    en.elFillDOM.style.background = '';
                }
            }
            if (en.elLagDOM) {
                en.elLagDOM.style.width = `${pctLag.toFixed(1)}%`;
            }
            if (en.elHpTxtDOM) {
                en.elHpTxtDOM.textContent = paralizados ? `EMP · ${Math.max(0, Math.ceil(en.vida))}` : `${Math.max(0, Math.ceil(en.vida))}`;
            }

            const clampedX = Math.max(48, Math.min(anchoPantalla - 48, item.sx));
            const clampedY = Math.max(54, Math.min(altoPantalla - 60, item.sy));

            el.style.opacity = '1';
            el.style.transform = `translate3d(${clampedX.toFixed(1)}px, ${clampedY.toFixed(1)}px, 0) translate(-50%, -100%) scale(${item.escala.toFixed(2)})`;
        }
    }

    // ========================================================================
    // CHIPS DE POTENCIADORES ACTIVOS EN EL HUD CON TEMPORIZADOR EN VIVO
    // ========================================================================
    function actualizarChipsPotenciadoresDOM() {
        const cont = document.getElementById('hud-potenciadores-activos');
        if (!cont) return;

        if (!estadoJuego.enCurso || estadoJuego.terminado) {
            cont.innerHTML = '';
            return;
        }

        const activos = [];
        if (estadoJuego.tiempoPoderActivo > 0) {
            activos.push({
                id: 'overdrive',
                clase: 'chip-overdrive',
                icono: 'fuego',
                nombre: 'FUEGO TRIPLE OVERDRIVE',
                restante: estadoJuego.tiempoPoderActivo,
                max: estadoJuego.duracionMaxPoderActivo || 6.0
            });
        }
        if (estadoJuego.tiempoInvulnerable > 0.15) {
            activos.push({
                id: 'escudo',
                clase: 'chip-escudo',
                icono: 'escudo',
                nombre: 'ESCUDO INVENCIBLE',
                restante: estadoJuego.tiempoInvulnerable,
                max: estadoJuego.duracionMaxInvulnerable || 5.0
            });
        }
        if (estadoJuego.tiempoEnemigosParalizados > 0) {
            activos.push({
                id: 'emp',
                clase: 'chip-emp',
                icono: 'emp',
                nombre: 'ONDA EMP · PARÁLISIS',
                restante: estadoJuego.tiempoEnemigosParalizados,
                max: estadoJuego.duracionMaxParalizados || 4.0
            });
        }

        if (activos.length === 0) {
            if (cont.innerHTML !== '') cont.innerHTML = '';
            return;
        }

        cont.innerHTML = activos.map(a => {
            const pct = Math.max(0, Math.min(100, (a.restante / a.max) * 100));
            const expirando = a.restante <= 1.35 ? 'expirando' : '';
            const svgIco = window.IconosJuego ? window.IconosJuego.obtener(a.icono, '') : '';
            return `
                <div class="chip-potenciador-activo ${a.clase} ${expirando}">
                    <div class="chip-potenciador-cabecera">
                        <span class="chip-potenciador-nombre">${svgIco} <span>${a.nombre}</span></span>
                        <span class="chip-potenciador-tiempo">${a.restante.toFixed(1)}s</span>
                    </div>
                    <div class="chip-potenciador-barra-fondo">
                        <div class="chip-potenciador-barra-fill" style="width:${pct.toFixed(1)}%;"></div>
                    </div>
                </div>
            `;
        }).join('');
    }

    // ========================================================================
    // INICIO Y GESTIÓN DE PARTIDA 3D (CAMPAÑA 2M 30S, TUTORIAL, LIBRE Y MP)
    // ========================================================================
    function iniciarPartida3D(config) {
        modoEscena = 'COMBATE';
        if (grupoHangar) grupoHangar.visible = false;
        limpiarBarrasSaludEnemigosDOM();

        const nivelCfg = CONFIG_NIVELES_CAMPANA[config.nivel || 1] || CONFIG_NIVELES_CAMPANA[1];

        estadoJuego.modalidad = config.modalidad || 'CampanaCasual';
        estadoJuego.nivel = config.nivel || 1;
        estadoJuego.oleada = 1;
        estadoJuego.totalOleadas = estadoJuego.modalidad === 'CampanaCasual'
            ? nivelCfg.totalOleadas
            : (estadoJuego.modalidad === 'BatallaLibre' ? 8 : 3);
        estadoJuego.subEscalonOleada = 0;
        estadoJuego.refuerzosPendientes = 0;
        estadoJuego.temporizadorRefuerzos = 0;
        estadoJuego.temporizadorTransicionOleada = 0;
        estadoJuego.tiempoMision = 0;
        estadoJuego.duracionObjetivoMision = nivelCfg.duracionObjetivo || CONFIG_MOTOR.DURACION_OBJETIVO_NIVEL_SEG;
        estadoJuego.enFaseJefe = false;
        estadoJuego.faseJefeActual = 1;
        estadoJuego.checkpointJefe = !!config.forzarJefe;

        const elAlertaJefe = document.getElementById('hud-alerta-jefe');
        if (elAlertaJefe) elAlertaJefe.classList.add('oculta');

        estadoJuego.enCurso = true;
        estadoJuego.pausado = false;
        estadoJuego.terminado = false;
        estadoJuego.vidasReserva = 1;
        estadoJuego.usoReaparicion = false;
        estadoJuego.puntos = 0;
        estadoJuego.combo = 1;
        estadoJuego.comboMax = 1;
        estadoJuego.monedasPartida = 0;
        estadoJuego.bajas = 0;
        estadoJuego.disparos = 0;
        estadoJuego.impactos = 0;
        estadoJuego.cargaPoder = 100;
        estadoJuego.tiempoPoderActivo = (config.cadenciaFactor && config.cadenciaFactor >= 1.5) ? 3.6 : 0;
        estadoJuego.duracionMaxPoderActivo = (config.cadenciaFactor && config.cadenciaFactor >= 1.5) ? 3.6 : 6.0;
        estadoJuego.tiempoInvulnerable = 2.5;
        estadoJuego.duracionMaxInvulnerable = 2.5;
        estadoJuego.tiempoEnemigosParalizados = 0;
        estadoJuego.duracionMaxParalizados = 4.0;
        estadoJuego.tiempoEfectoReparacion = 0;
        estadoJuego.pasoTutorial = 1;
        estadoJuego.progresoTutorial = 0;

        construirEscenaCombate3D(estadoJuego.nivel);
        grupoMundoCombate.visible = true;
        camera.position.set(0, 17.2, 16.0);
        camera.lookAt(0, 0.0, -12.0);

        enemigos = [];
        proyectiles = [];
        suministros = [];
        peligros = [];
        meteoros3D = [];
        temporizadorMeteoro = 4.5;
        ondasExpansivas3D = [];
        remotosRed = {};

        jugador.x = 0;
        jugador.z = 2.0;
        jugador.vx = 0;
        jugador.vz = 0;
        jugador.vidaMax = config.vidaMax || 100;
        jugador.vida = jugador.vidaMax;
        jugador.escudoMax = config.escudoMax || 50;
        jugador.escudo = jugador.escudoMax;
        jugador.modeloId = config.modeloId || 'NAVE_CONDOR_X';
        jugador.colorSkin = config.colorSkin || 0x00dcff;
        jugador.perfil = PERFILES_VUELO[jugador.modeloId] || PERFILES_VUELO.NAVE_CONDOR_X;
        jugador.velocidadBase = (config.velocidad || 21) * jugador.perfil.factorVelocidad;
        jugador.cadenciaFactor = (config.cadenciaFactor || 1.0) * jugador.perfil.factorCadencia;
        jugador.tiempoManiobra = 0;
        jugador.cooldownManiobra = 0;
        jugador.rollVisual = 0;

        jugador.mesh = crearModeloAeronave3D(jugador.modeloId, jugador.colorSkin, false, false, false);
        jugador.mesh.position.set(jugador.x, 0, jugador.z);
        jugador.mesh.renderOrder = 10;
        grupoMundoCombate.add(jugador.mesh);

        // Estelas de punta de ala (se reconstruyen con el color de la pintura equipada)
        if (jugador.estelas) jugador.estelas.forEach(e => { scene.remove(e.mesh); e.mesh.geometry.dispose(); e.mesh.material.dispose(); });
        jugador.estelas = [crearEstela(jugador.colorSkin), crearEstela(jugador.colorSkin)];
        jugador.estelas.forEach(e => scene.add(e.mesh));
        vaciarPoolParticulas();
        if (lineasVelocidad) lineasVelocidad.visible = lineasVelocidad.userData.habilitado !== false;
        escalaTiempo = 1.0;
        tiempoCamaraSlowMo = 0;
        estadoJuego.sacudidaCamara = 0;
        estadoJuego.tiempoFlashDano = 0;
        estadoJuego.dificultad = dificultadActual;
        estadoJuego.temporizadorCierreVictoria = null;
        reiniciarCamaraCombate();

        if (estadoJuego.modalidad === 'TutorialInteractivo') {
            generarSuministro3D(0, 1.5, 'energia');
        } else if (estadoJuego.modalidad === 'Multijugador_1v1' || estadoJuego.modalidad === 'Multijugador_2v2') {
            desplegarDueloMultijugador3D(estadoJuego.modalidad === 'Multijugador_2v2' ? 2 : 1);
        } else if (estadoJuego.modalidad === 'Multijugador_Cooperativo') {
            desplegarCooperativo3D();
        } else if ((estadoJuego.checkpointJefe || config.forzarJefe) && estadoJuego.modalidad === 'CampanaCasual') {
            desplegarJefeFinal3D();
        } else {
            desplegarOleadaEnemiga3D(1, false);
        }

        actualizarHudDOM();
        actualizarChipsPotenciadoresDOM();
    }

    function desplegarDueloMultijugador3D(cantidadRivales = 1) {
        estadoJuego.oleada = 1;
        estadoJuego.totalOleadas = 1;
        estadoJuego.enFaseJefe = false;

        for (let i = 0; i < cantidadRivales; i++) {
            const modelo = (i === 0) ? 'NAVE_F35' : 'NAVE_B2';
            const meshRival = crearModeloAeronave3D(modelo, 0xff1744, true, false, false);
            const posX = (i - (cantidadRivales - 1) / 2) * 7.5;
            const posZ = -98 - i * 5.5;
            meshRival.position.set(posX, 0.4, posZ);
            grupoMundoCombate.add(meshRival);

            enemigos.push({
                id: contadorIdEnemigo++,
                mesh: meshRival,
                modeloId: modelo,
                x: posX,
                z: posZ,
                prevX: posX,
                prevZ: posZ,
                yaw: 0,
                xMeta: posX,
                zMeta: -28.0 - i * 5.0,
                estado: 'ENTRADA',
                tiempoEstado: 0,
                tiempoPatrulla: i * 1.5,
                indice: i,
                vxRepulsion: 0,
                vzRepulsion: 0,
                vida: 240,
                vidaMax: 240,
                vidaVisualLag: 240,
                esJefe: false,
                esRivalMultijugador: true,
                cooldown: 2.5 + i * 0.5,
                nombre: cantidadRivales === 1 ? 'Rival en Línea: Escuadrón Fénix' : `Rival Bravo ${i + 1}`
            });
        }
        actualizarHudDOM();
    }

    function desplegarCooperativo3D() {
        estadoJuego.oleada = 1;
        estadoJuego.totalOleadas = 6;
        estadoJuego.enFaseJefe = false;

        const meshAliado = crearModeloAeronave3D('NAVE_CONDOR_X', 0x00e676, false, false, false);
        meshAliado.position.set(3.8, 0, 2.0);
        grupoMundoCombate.add(meshAliado);

        remotosRed['aliado_coop'] = {
            mesh: meshAliado,
            x: 3.8,
            z: 2.0,
            vida: 120,
            nombre: 'Compañero Aliado: Cóndor-02',
            cooldownDisparo: 0.25
        };

        desplegarOleadaEnemiga3D(1, false);
    }

    // Diseño de progresión táctica para 10 oleadas variadas (~2 min 30 seg por nivel)
    function obtenerPlanOleadaTactica(nivel, numOleada, esRefuerzo) {
        const cfgNivel = CONFIG_NIVELES_CAMPANA[nivel] || CONFIG_NIVELES_CAMPANA[1];
        const vidaBase = cfgNivel.vidaBaseEnemigo + numOleada * 5;

        const arquetipos = {
            1: { titulo: 'Vanguardia de Exploración', formacion: 0, naves: ['LIGERO', 'LIGERO', 'ATAQUE'], refuerzos: 2, meteoros: 0 },
            2: { titulo: 'Patrulla de Intercepción', formacion: 1, naves: ['LIGERO', 'ATAQUE', 'LIGERO'], refuerzos: 2, meteoros: 0 },
            3: { titulo: 'Escuadrón de Asalto Víbora', formacion: 2, naves: ['ATAQUE', 'LIGERO', 'ATAQUE', 'LIGERO'], refuerzos: 2, meteoros: 1 },
            4: { titulo: 'Corredor de Tormenta de Meteoros', formacion: 0, naves: ['LIGERO', 'ATAQUE', 'LIGERO'], refuerzos: 2, meteoros: 2, suministroExtra: 'escudo' },
            5: { titulo: 'Comandante de Sub-Sector (Élite)', formacion: 3, naves: ['ATAQUE', 'PESADO_ELITE', 'ATAQUE'], refuerzos: 2, meteoros: 1, suministroExtra: 'energia' },
            6: { titulo: 'Pinza Táctica de Flanco', formacion: 2, naves: ['LIGERO', 'ATAQUE', 'ATAQUE', 'LIGERO'], refuerzos: 3, meteoros: 0 },
            7: { titulo: 'División Acorazada Gólem', formacion: 3, naves: ['PESADO', 'LIGERO', 'ATAQUE', 'PESADO'], refuerzos: 2, meteoros: 1 },
            8: { titulo: 'Cinturón Sísmico de Asteroides', formacion: 1, naves: ['ATAQUE', 'LIGERO', 'ATAQUE'], refuerzos: 3, meteoros: 2, suministroExtra: 'vida' },
            9: { titulo: 'Escuadrón de Élite Pretoriano', formacion: 0, naves: ['ATAQUE', 'PESADO_ELITE', 'LIGERO', 'ATAQUE'], refuerzos: 3, meteoros: 1 },
            10: { titulo: 'Guardia Imperial del Sector', formacion: 3, naves: ['PESADO', 'ATAQUE', 'PESADO_ELITE', 'ATAQUE'], refuerzos: 2, meteoros: 1, suministroExtra: 'fuego' }
        };

        const plan = arquetipos[numOleada] || arquetipos[3];
        const listaClases = esRefuerzo
            ? (numOleada >= 6 ? ['LIGERO', 'ATAQUE', 'LIGERO'] : ['LIGERO', 'ATAQUE'])
            : plan.naves;

        return {
            titulo: plan.titulo,
            formacion: (plan.formacion + (esRefuerzo ? 1 : 0)) % 4,
            clases: listaClases,
            vidaBase: vidaBase,
            tieneRefuerzos: !esRefuerzo && plan.refuerzos > 0 && estadoJuego.modalidad !== 'TutorialInteractivo',
            meteorosEvento: esRefuerzo ? 0 : plan.meteoros,
            suministroExtra: esRefuerzo ? null : plan.suministroExtra
        };
    }

    function desplegarOleadaEnemiga3D(numOleada, esRefuerzo = false) {
        estadoJuego.oleada = numOleada;
        estadoJuego.enFaseJefe = false;

        const plan = obtenerPlanOleadaTactica(estadoJuego.nivel, numOleada, esRefuerzo);
        const cantidad = plan.clases.length;

        if (!esRefuerzo) {
            estadoJuego.subEscalonOleada = 1;
            estadoJuego.refuerzosPendientes = plan.tieneRefuerzos ? 1 : 0;
            estadoJuego.temporizadorRefuerzos = 5.4;

            if (plan.meteorosEvento > 0 && estadoJuego.modalidad !== 'TutorialInteractivo') {
                for (let m = 0; m < plan.meteorosEvento; m++) {
                    const posXMet = (m === 0) ? -7.5 + Math.random() * 5.0 : 4.5 + Math.random() * 5.0;
                    desplegarMeteoro3D(posXMet, -68 - m * 16);
                }
            }
            if (plan.suministroExtra) {
                generarSuministro3D((Math.random() - 0.5) * 12, -24, plan.suministroExtra);
            }
        } else {
            estadoJuego.subEscalonOleada = 2;
        }

        for (let i = 0; i < cantidad; i++) {
            const claveClase = plan.clases[i];
            let modelo = 'NAVE_ENEMIGO_ATAQUE';
            let nombreClase = 'Caza Táctico Víbora';
            let factorVida = 1.05;
            let offsetZClase = -28.5;
            let colorTinte = 0xff6d00;
            let esElite = false;

            if (claveClase === 'LIGERO') {
                modelo = 'NAVE_ENEMIGO_LIGERO';
                nombreClase = 'Interceptor Halcón';
                factorVida = 0.80;
                offsetZClase = -24.5;
                colorTinte = 0xff1744;
            } else if (claveClase === 'PESADO') {
                modelo = 'NAVE_ENEMIGO_PESADO';
                nombreClase = 'Crucero Blindado Gólem';
                factorVida = 1.75;
                offsetZClase = -34.5;
                colorTinte = 0xd500f9;
            } else if (claveClase === 'PESADO_ELITE') {
                modelo = 'NAVE_ENEMIGO_PESADO';
                nombreClase = 'Comandante Gólem [ÉLITE]';
                factorVida = 2.45;
                offsetZClase = -33.0;
                colorTinte = 0xffb800;
                esElite = true;
            }

            let xSpawn = 0;
            let zSpawn = -96;
            const spanX = Math.min(20, Math.max(10, (cantidad - 1) * 5.6));
            let xMeta = cantidad === 1 ? 0 : (-spanX / 2 + (i / (cantidad - 1)) * spanX);
            let zMeta = offsetZClase - (i % 2) * 3.2;

            if (plan.formacion === 0) {
                const dCentro = Math.abs(i - (cantidad - 1) / 2);
                xSpawn = xMeta * 1.15;
                zSpawn = -92 - dCentro * 5.0;
            } else if (plan.formacion === 1) {
                xSpawn = -13 + i * (26 / Math.max(1, cantidad - 1));
                zSpawn = -92 - i * 4.5;
            } else if (plan.formacion === 2) {
                const lado = (i % 2 === 0) ? -1 : 1;
                xSpawn = lado * (12.5 + i * 1.8);
                zSpawn = -94 - i * 3.8;
            } else {
                xSpawn = xMeta;
                zSpawn = -95 - (i % 2) * 6.0;
            }

            const meshEnemigo = crearModeloAeronave3D(modelo, colorTinte, true, false, esElite);
            if (esElite) {
                meshEnemigo.scale.multiplyScalar(1.16);
            }
            meshEnemigo.position.set(xSpawn, 0.4, zSpawn);
            grupoMundoCombate.add(meshEnemigo);

            const vidaFinal = Math.round(plan.vidaBase * factorVida);
            enemigos.push({
                id: contadorIdEnemigo++,
                mesh: meshEnemigo,
                modeloId: modelo,
                x: xSpawn,
                z: zSpawn,
                prevX: xSpawn,
                prevZ: zSpawn,
                yaw: 0,
                xMeta: Math.max(-12.0, Math.min(12.0, xMeta)),
                zMeta: zMeta,
                estado: 'ENTRADA',
                tiempoEstado: 0,
                tiempoPatrulla: i * 1.25,
                indice: enemigos.length + i,
                vxRepulsion: 0,
                vzRepulsion: 0,
                vida: vidaFinal,
                vidaMax: vidaFinal,
                vidaVisualLag: vidaFinal,
                esJefe: false,
                esElite: esElite,
                cooldown: 0.28 + i * 0.24,
                nombre: nombreClase
            });
        }
        actualizarHudDOM();
    }

    const nombresJefes = {
        1: 'COMANDANTE KUNTUR-I',
        2: 'ESCUADRÓN FANTASMA B-2',
        3: 'CRUCERO GLACIAR ILLIMANI',
        4: 'VALKIRIA VOLCÁNICA X-9',
        5: 'ACORAZADO ORBITAL APU-SUPAY'
    };

    function desplegarJefeFinal3D() {
        estadoJuego.enFaseJefe = true;
        estadoJuego.faseJefeActual = 1;
        estadoJuego.checkpointJefe = true;
        estadoJuego.refuerzosPendientes = 0;
        // Al iniciar desde el checkpoint del jefe la oleada quedaba en 1 y, tras derrotarlo,
        // la misión reanudaba las oleadas en lugar de declarar la victoria
        estadoJuego.oleada = estadoJuego.totalOleadas;
        sonarEfecto('poder');

        const cfgNivel = CONFIG_NIVELES_CAMPANA[estadoJuego.nivel] || CONFIG_NIVELES_CAMPANA[1];
        const vidaJefe = cfgNivel.vidaJefe || (520 + estadoJuego.nivel * 180);
        const nombre = cfgNivel.jefeNombre || nombresJefes[estadoJuego.nivel] || 'VALKIRIA VOLCÁNICA X-9';
        const meshJefe = crearModeloAeronave3D('NAVE_JEFE_VALKIRIA', 0xff1744, true, true, false);

        meshJefe.position.set(0, 0.8, -105);
        grupoMundoCombate.add(meshJefe);

        enemigos.push({
            id: contadorIdEnemigo++,
            mesh: meshJefe,
            modeloId: 'NAVE_JEFE_VALKIRIA',
            x: 0,
            z: -105,
            prevX: 0,
            prevZ: -105,
            yaw: 0,
            xMeta: 0,
            zMeta: -30.0,
            estado: 'ENTRADA',
            tiempoEstado: 0,
            tiempoPatrulla: 0,
            indice: 0,
            vxRepulsion: 0,
            vzRepulsion: 0,
            vida: vidaJefe,
            vidaMax: vidaJefe,
            vidaVisualLag: vidaJefe,
            esJefe: true,
            escoltaDesplegada: false,
            cooldown: 0.35,
            patronDisparo: 0,
            nombre: nombre
        });

        crearOndaExpansiva3D(0, -6.5, 0xff1744, 26);
        actualizarHudDOM();
    }

    // ========================================================================
    // GENERADORES DE PROYECTILES Y FÍSICAS VECTORIALES NORMALIZADAS + CCD
    // ========================================================================
    function crearMallaRayoPlasma(colorHex, longitud = 1.8, radioCore = 0.085, radioGlow = 0.23, conPunta = false) {
        asegurarRecursosCompartidos3D();
        const grupo = new THREE.Group();

        const core = new THREE.Mesh(recursosCompartidos3D.geomCilindroUnitario, recursosCompartidos3D.matBlancoPuro);
        core.scale.set(radioCore, radioCore, longitud);
        grupo.add(core);

        const matGlow = obtenerMaterialGlowCompartido(colorHex, 0.90);
        const glow = new THREE.Mesh(recursosCompartidos3D.geomCilindroUnitario, matGlow);
        glow.scale.set(radioGlow, radioGlow, longitud * 1.15);
        grupo.add(glow);

        if (conPunta) {
            const flare = new THREE.Mesh(recursosCompartidos3D.geomEsferaUnitaria, matGlow);
            flare.scale.set(radioGlow * 1.35, radioGlow * 1.35, radioGlow * 1.8);
            flare.position.set(0, 0, longitud * 0.42);
            grupo.add(flare);
        }

        return grupo;
    }

    function crearMallaOrbePlasma(colorHex, radio = 0.40) {
        asegurarRecursosCompartidos3D();
        const grupo = new THREE.Group();

        const core = new THREE.Mesh(recursosCompartidos3D.geomEsferaUnitaria, recursosCompartidos3D.matBlancoPuro);
        const rCore = radio * 0.58;
        core.scale.set(rCore, rCore, rCore);
        grupo.add(core);

        const matGlow = obtenerMaterialGlowCompartido(colorHex, 0.92);
        const glow = new THREE.Mesh(recursosCompartidos3D.geomEsferaUnitaria, matGlow);
        glow.scale.set(radio, radio, radio);
        grupo.add(glow);

        const halo = new THREE.Mesh(recursosCompartidos3D.geomHaloOrbe, matGlow);
        halo.scale.set(radio * 1.15, radio * 1.15, 1);
        halo.rotation.x = -Math.PI / 2;
        grupo.add(halo);

        return grupo;
    }

    // Calcula un vector de velocidad 100% normalizado con rapidez constante ||v|| = velocidadTotal
    function calcularVectorDisparoNormalizado(origenX, origenZ, destinoX, destinoZ, velocidadTotal, anguloSpreadRad = 0, conPrediccion = true) {
        // Puntería predictiva parcial (según dificultad y nivel): obliga a cambiar de rumbo, no solo a quedarse quieto
        if (conPrediccion) {
            const punteria = Math.min(0.85, modDificultad().punteria + (estadoJuego.nivel - 1) * 0.03);
            if (punteria > 0) {
                const tVuelo = Math.hypot(destinoX - origenX, destinoZ - origenZ) / Math.max(1, velocidadTotal);
                destinoX += jugador.vx * tVuelo * punteria;
                destinoZ += jugador.vz * tVuelo * punteria;
            }
        }
        let dx = destinoX - origenX;
        let dz = destinoZ - origenZ;
        const len = Math.hypot(dx, dz);
        if (len < 0.001) {
            dx = 0;
            dz = 1;
        } else {
            dx /= len;
            dz /= len;
        }
        // Garantizar siempre avance frontal hacia el corredor del jugador (+Z)
        if (dz < 0.28) {
            dz = 0.28;
            const norm = Math.hypot(dx, dz);
            dx /= norm;
            dz /= norm;
        }
        const angulo = Math.atan2(dx, dz) + anguloSpreadRad;
        return {
            vx: Math.sin(angulo) * velocidadTotal,
            vz: Math.cos(angulo) * velocidadTotal,
            yaw: angulo
        };
    }

    // Transforma el offset local del cañón (ala/proa) según la orientación yaw actual del enemigo
    function transformarOffsetBoquilla(en, localX, localZ) {
        const yaw = en.yaw || 0;
        const cosY = Math.cos(yaw);
        const sinY = Math.sin(yaw);
        return {
            x: en.x + localX * cosY + localZ * sinY,
            z: en.z - localX * sinY + localZ * cosY
        };
    }

    // Detección de Colisión Continua (CCD): distancia mínima de un punto (cx, cz) al segmento (ax,az)->(bx,bz)
    function distanciaPuntoASegmento(ax, az, bx, bz, cx, cz) {
        const abx = bx - ax;
        const abz = bz - az;
        const lenSq = abx * abx + abz * abz;
        if (lenSq < 0.000001) return Math.hypot(cx - ax, cz - az);
        const t = Math.max(0, Math.min(1, ((cx - ax) * abx + (cz - az) * abz) / lenSq));
        const projX = ax + t * abx;
        const projZ = az + t * abz;
        return Math.hypot(cx - projX, cz - projZ);
    }

    function crearDestelloBoquilla3D(x, z, colorHex) {
        emitirParticula(false, x, 0.5, z, 0, 0, 4.0, 0.09, 0.95, 0.55, colorHex, 0.92, 0, 0);
    }

    // ========================================================================
    // FÍSICA, DISPAROS 3D, POTENCIADORES Y EFECTOS DE PARTÍCULAS
    // ========================================================================
    function dispararJugador3D() {
        if (!estadoJuego.enCurso || estadoJuego.pausado || estadoJuego.terminado) return;
        if (jugador.cooldownDisparo > 0) return;

        const sobrecarga = estadoJuego.tiempoPoderActivo > 0;
        jugador.cooldownDisparo = (0.16 / jugador.cadenciaFactor) * (sobrecarga ? 0.55 : 1.0);
        estadoJuego.disparos++;
        sonarEfecto(sobrecarga ? 'disparo_overdrive' : 'disparo');

        if (estadoJuego.modalidad === 'TutorialInteractivo' && estadoJuego.pasoTutorial === 2) {
            estadoJuego.progresoTutorial++;
            if (estadoJuego.progresoTutorial >= 5) {
                estadoJuego.pasoTutorial = 3;
                estadoJuego.progresoTutorial = 0;
            }
        }

        const colorLaser = sobrecarga ? 0xffd54f : jugador.colorSkin;
        const offsets = (jugador.modeloId === 'NAVE_B2' || sobrecarga)
            ? [-1.1, 0, 1.1]
            : [-0.85, 0.85];

        const velLaser = 60.0;
        offsets.forEach((ox, idx) => {
            const angSpread = (sobrecarga && offsets.length === 3) ? (idx - 1) * 0.065 : 0;
            const vx = Math.sin(angSpread) * velLaser;
            const vz = -Math.cos(angSpread) * velLaser;
            const spawnX = jugador.x + ox;
            const spawnZ = jugador.z - 1.45;

            const laser = crearMallaRayoPlasma(colorLaser, sobrecarga ? 2.25 : 2.0, 0.088, sobrecarga ? 0.28 : 0.24, true);
            laser.position.set(spawnX, 0.50, spawnZ);
            laser.rotation.y = Math.atan2(vx, vz);
            grupoMundoCombate.add(laser);

            proyectiles.push({
                mesh: laser,
                x: spawnX,
                z: spawnZ,
                prevX: spawnX,
                prevZ: spawnZ,
                vx: vx,
                vz: vz,
                radioColision: 0.24,
                tiempoVida: 0,
                tiempoVidaMax: 2.2,
                activo: true,
                esEnemigo: false,
                dano: sobrecarga ? 18 : 13
            });
        });

        if (window.InterfazJuego && window.InterfazJuego.enviarEstadoRed) {
            window.InterfazJuego.enviarEstadoRed(jugador.x, jugador.z, jugador.vida, true);
        }
    }

    function activarSuperpoderJugador() {
        if (!estadoJuego.enCurso || estadoJuego.pausado || estadoJuego.terminado) return;
        if (estadoJuego.cargaPoder < 100) return;

        estadoJuego.cargaPoder = 0;
        agregarTrauma(0.5);

        if (estadoJuego.modalidad === 'TutorialInteractivo' && estadoJuego.pasoTutorial === 4) {
            estadoJuego.pasoTutorial = 5;
            desplegarOleadaEnemiga3D(1, false);
        }

        const poderId = (window.InterfazJuego && window.InterfazJuego.obtenerPoderEquipado()) || 'PODER_EMP';

        if (poderId === 'PODER_EMP') {
            sonarEfecto('poder_emp');
            for (let i = proyectiles.length - 1; i >= 0; i--) {
                if (proyectiles[i].esEnemigo) {
                    crearExplosion3D(proyectiles[i].x, proyectiles[i].z, 0x00e5ff, 4);
                    grupoMundoCombate.remove(proyectiles[i].mesh);
                    proyectiles.splice(i, 1);
                }
            }
            estadoJuego.duracionMaxParalizados = 4.0;
            estadoJuego.tiempoEnemigosParalizados = 4.0;
            crearOndaExpansiva3D(jugador.x, jugador.z, 0x00e5ff, 28);
            if (window.InterfazJuego) window.InterfazJuego.mostrarToast('ONDA EMP ACTIVA: PROYECTILES ANULADOS Y ENEMIGOS CONGELADOS [4.0s]');
        } else if (poderId === 'PODER_ESCUDO') {
            sonarEfecto('poder_escudo');
            estadoJuego.duracionMaxInvulnerable = 5.0;
            estadoJuego.tiempoInvulnerable = 5.0;
            jugador.escudo = Math.min(jugador.escudoMax, jugador.escudo + 35);
            crearOndaExpansiva3D(jugador.x, jugador.z, 0x00e676, 20);
            if (window.InterfazJuego) window.InterfazJuego.mostrarToast('ESCUDO INVENCIBLE ACTIVO: INMUNIDAD TOTAL Y +35 ESCUDO [5.0s]');
        } else if (poderId === 'PODER_OVERDRIVE') {
            sonarEfecto('poder_overdrive');
            estadoJuego.duracionMaxPoderActivo = 6.0;
            estadoJuego.tiempoPoderActivo = 6.0;
            crearOndaExpansiva3D(jugador.x, jugador.z, 0xffb800, 20);
            if (window.InterfazJuego) window.InterfazJuego.mostrarToast('SOBRECARGA DE ARMAS: FUEGO TRIPLE RÁPIDO ACTIVO [6.0s]');
        } else {
            sonarEfecto('poder_solar');
            crearOndaExpansiva3D(0, -10, 0xff3d57, 34);
            enemigos.forEach(en => {
                en.vida -= 75;
                en.tiempoImpactoReciente = 0.25;
                crearExplosion3D(en.x, en.z, 0xff9100, 16);
            });
            if (window.InterfazJuego) window.InterfazJuego.mostrarToast('EXPLOSIÓN SOLAR TERMOBÁRICA: IMPACTO MASIVO EN EL SECTOR');
        }

        actualizarHudDOM();
        actualizarChipsPotenciadoresDOM();
    }

    // Explosión en capas: destello central + bola de fuego + chispas balísticas + humo que se expande y queda atrás
    function crearExplosion3D(x, z, colorHex, cantidad = 20) {
        const escala = Math.min(1.6, 0.45 + cantidad / 18);
        const y = 0.4;
        emitirParticula(false, x, y, z, 0, 0, 0, 0.14, 1.2 * escala, 4.2 * escala, 0xffffff, 0.95, 0, 0);
        if (cantidad >= 8) {
            emitirParticula(false, x, y, z, 0, 0, 0, 0.32, 1.6 * escala, 3.4 * escala, 0xffb74d, 0.85, 0, 0);
        }
        const chispas = Math.min(cantidad, 26);
        for (let i = 0; i < chispas; i++) {
            const ang = Math.random() * Math.PI * 2;
            const vel = (5 + Math.random() * 16) * (0.6 + escala * 0.4);
            emitirParticula(false, x, y, z,
                Math.cos(ang) * vel, 2 + Math.random() * 6, Math.sin(ang) * vel,
                0.35 + Math.random() * 0.45, 0.32 + Math.random() * 0.3, 0.05, colorHex, 0.95, 2.4, 9.0);
        }
        const humos = cantidad >= 8 ? Math.min(10, Math.round(cantidad / 3)) : 1;
        for (let i = 0; i < humos; i++) {
            const ang = Math.random() * Math.PI * 2;
            const vel = 1 + Math.random() * 3.5 * escala;
            emitirParticula(true, x + Math.cos(ang) * 0.4, y, z + Math.sin(ang) * 0.4,
                Math.cos(ang) * vel, 0.8 + Math.random() * 1.2, Math.sin(ang) * vel,
                0.9 + Math.random() * 0.9, 0.8 * escala, (2.6 + Math.random() * 1.6) * escala, 0x55606e, 0.55, 1.2, -0.4);
        }
    }

    function crearOndaExpansiva3D(x, z, colorHex, radioMax = 14) {
        asegurarRecursosCompartidos3D();
        const mat = new THREE.MeshBasicMaterial({
            color: colorHex,
            side: THREE.DoubleSide,
            transparent: true,
            opacity: 0.85,
            depthWrite: false
        });
        const anillo = new THREE.Mesh(recursosCompartidos3D.geomAnilloOnda, mat);
        anillo.rotation.x = -Math.PI / 2;
        anillo.position.set(x, 0.2, z);
        grupoMundoCombate.add(anillo);

        ondasExpansivas3D.push({
            mesh: anillo,
            escala: 1,
            escalaMax: radioMax,
            vida: 0.55
        });
    }

    function generarSuministro3D(x = null, z = null, tipoForzado = null, vxForzado = null, vzForzado = null) {
        asegurarRecursosCompartidos3D();
        const tipos = ['vida', 'escudo', 'energia', 'monedas', 'fuego'];
        const tipo = tipoForzado || tipos[Math.floor(Math.random() * tipos.length)];
        const colores = {
            vida: 0x00e676,
            escudo: 0x00e5ff,
            energia: 0xd500f9,
            monedas: 0xffd54f,
            fuego: 0xff6d00
        };

        const esMulti = estadoJuego.modalidad && estadoJuego.modalidad.startsWith('Multijugador');
        let posX, posZ, vx, vz;

        if (esMulti && x === null) {
            // EN MODO MULTIJUGADOR: Las mejoras/suministros entran lateralmente desde los flancos
            const vieneDeLaIzquierda = Math.random() < 0.5;
            posX = vieneDeLaIzquierda ? -26.0 : 26.0;
            posZ = -18.0 + Math.random() * 20.0;
            vx = vieneDeLaIzquierda ? (7.5 + Math.random() * 4.5) : -(7.5 + Math.random() * 4.5);
            vz = (Math.random() - 0.5) * 2.5;
        } else {
            posX = (typeof x === 'number') ? x : ((Math.random() - 0.5) * 18.0);
            posZ = (typeof z === 'number') ? z : -55.0;
            vx = (typeof vxForzado === 'number') ? vxForzado : 0;
            vz = (typeof vzForzado === 'number') ? vzForzado : 4.5;
        }

        const grupoItem = new THREE.Group();
        if (tipo === 'escudo' && cacheModelosGLTF['ITEM_ESCUDO']) {
            const itemInst = cacheModelosGLTF['ITEM_ESCUDO'].clone(true);
            itemInst.scale.set(0.75, 0.75, 0.75);
            grupoItem.add(itemInst);
        } else if (tipo === 'vida' && cacheModelosGLTF['ITEM_REPARACION']) {
            const itemInst = cacheModelosGLTF['ITEM_REPARACION'].clone(true);
            itemInst.scale.set(0.75, 0.75, 0.75);
            grupoItem.add(itemInst);
        } else {
            if (!recursosCompartidos3D.cacheMaterialesItem[tipo]) {
                recursosCompartidos3D.cacheMaterialesItem[tipo] = new THREE.MeshStandardMaterial({
                    color: colores[tipo],
                    emissive: colores[tipo],
                    emissiveIntensity: 0.65,
                    metalness: 0.8,
                    roughness: 0.2
                });
            }
            const cubo = new THREE.Mesh(recursosCompartidos3D.geomOctaedroItem, recursosCompartidos3D.cacheMaterialesItem[tipo]);
            grupoItem.add(cubo);

            const halo = new THREE.Mesh(recursosCompartidos3D.geomToroItem, recursosCompartidos3D.matBlancoPuro);
            halo.rotation.x = Math.PI / 2;
            grupoItem.add(halo);
        }

        grupoItem.position.set(posX, 0.2, posZ);
        grupoMundoCombate.add(grupoItem);

        suministros.push({
            mesh: grupoItem,
            x: posX,
            z: posZ,
            vx: vx,
            vz: vz,
            tipo: tipo
        });
    }

    // ========================================================================
    // ACTUALIZACIÓN DEL GAME LOOP 3D A 60 FPS
    // ========================================================================
    let fpsPromedio = 60;

    function bucleRenderizado() {
        requestAnimationFrame(bucleRenderizado);
        const dtCrudo = clock.getDelta();
        if (dtCrudo > 0) fpsPromedio += ((1 / dtCrudo) - fpsPromedio) * 0.05;
        const dtReal = Math.min(dtCrudo, 0.05);
        const elapsed = clock.getElapsedTime();
        uniformsGlobalesShader.uTime.value = elapsed;

        if (modoEscena === 'HANGAR') {
            actualizarEscenaHangar(dtReal, elapsed);
        } else if (modoEscena === 'COMBATE') {
            leerMando();
            procesarBotonesMando();
            // Cámara lenta cinemática (derribo de jefe): recupera la velocidad normal suavemente
            if (tiempoCamaraSlowMo > 0) {
                tiempoCamaraSlowMo -= dtReal;
                escalaTiempo = 0.28;
            } else {
                escalaTiempo += (1.0 - escalaTiempo) * Math.min(1, dtReal * 4.0);
            }
            actualizarEscenaCombate(dtReal * escalaTiempo, elapsed);
            const enVuelo = estadoJuego.enCurso && !estadoJuego.pausado && !estadoJuego.terminado;
            const factorVel = jugador.velocidadBase > 0 ? Math.min(1.4, Math.hypot(jugador.vx, jugador.vz) / jugador.velocidadBase) : 0;
            actualizarMotorAudio(enVuelo, factorVel, jugador.tiempoManiobra > 0);
        }

        renderer.render(scene, camera);
    }

    function actualizarEscenaHangar(dt, elapsed) {
        // Vista de estudio 3/4 superior-frontal centrada entre los paneles de interfaz
        const anguloShowcase = Math.sin(elapsed * 0.38) * 0.22 + 0.08;
        camera.position.set(
            Math.sin(anguloShowcase) * 8.4 + 0.15,
            4.65 + Math.sin(elapsed * 0.70) * 0.16,
            Math.cos(anguloShowcase) * 8.4 + 0.95
        );
        camera.lookAt(0.05, 0.18, 0.0);

        if (naveHangarMesh) {
            naveHangarMesh.position.y = 0.38 + Math.sin(elapsed * 2.0) * 0.10;
            naveHangarMesh.rotation.y = Math.PI * 0.78 + Math.sin(elapsed * 0.45) * 0.14;
            naveHangarMesh.rotation.z = Math.sin(elapsed * 1.2) * 0.08;
            naveHangarMesh.rotation.x = Math.cos(elapsed * 1.0) * 0.04;
        }
        if (anilloHangar1) anilloHangar1.rotation.z = elapsed * 0.5;
        if (anilloHangar2) anilloHangar2.rotation.z = -elapsed * 0.8;
    }

    // ========================================================================
    // CÁMARA DE COMBATE (RESORTE CRÍTICAMENTE AMORTIGUADO + TRAUMA + FOV DINÁMICO)
    // ========================================================================
    const camEstado = { x: 0, y: 17.2, z: 16.0, vx: 0, vy: 0, vz: 0, lookX: 0, roll: 0, fov: 50 };

    function reiniciarCamaraCombate() {
        camEstado.x = jugador.x * 0.3; camEstado.y = 17.2; camEstado.z = 16.0;
        camEstado.vx = camEstado.vy = camEstado.vz = 0;
        camEstado.lookX = jugador.x * 0.2;
        camEstado.roll = 0;
        camEstado.fov = 50;
        if (camera) {
            camera.fov = 50;
            camera.updateProjectionMatrix();
            camera.position.set(camEstado.x, camEstado.y, camEstado.z);
            camera.lookAt(camEstado.lookX, 0.0, -12.0);
        }
    }

    function resorteCamara(eje, objetivo, omega, dt) {
        const v = 'v' + eje;
        const a = omega * omega * (objetivo - camEstado[eje]) - 2 * omega * camEstado[v];
        camEstado[v] += a * dt;
        camEstado[eje] += camEstado[v] * dt;
    }

    function actualizarCamaraCombate(dt, elapsed) {
        const velMax = Math.max(1, jugador.velocidadBase);
        const latNorm = jugador.vx / velMax;
        const lonNorm = jugador.vz / velMax;

        // Anticipación: la cámara se adelanta un poco hacia donde vira el avión
        resorteCamara('x', jugador.x * 0.30 + latNorm * 1.2, 5.5, dt);
        resorteCamara('y', 17.2 + jugador.z * 0.05, 4.5, dt);
        resorteCamara('z', 16.0 + jugador.z * 0.16, 4.5, dt);
        camEstado.lookX += (jugador.x * 0.2 + latNorm * 0.8 - camEstado.lookX) * (1 - Math.exp(-5 * dt));

        const trauma = Math.max(0, estadoJuego.sacudidaCamara || 0);
        const sacudida = trauma * trauma * intensidadSacudida;
        estadoJuego.sacudidaCamara = Math.max(0, trauma - dt * 1.4);
        const t = elapsed;
        const nx = (Math.sin(t * 41.3) + Math.sin(t * 23.9 + 1.7) * 0.6) * 0.62 * sacudida * 1.3;
        const ny = (Math.sin(t * 37.1 + 0.6) + Math.sin(t * 19.3 + 2.4) * 0.6) * 0.62 * sacudida * 1.3;
        const nRoll = (Math.sin(t * 29.7 + 3.1)) * sacudida * 0.035;

        camera.position.set(camEstado.x + nx, camEstado.y + ny, camEstado.z);
        camera.lookAt(camEstado.lookX + nx * 0.4, 0.0, -10.5 + jugador.z * 0.25);

        const rollObjetivo = -latNorm * 0.035 * intensidadSacudida + (jugador.tiempoManiobra > 0 ? -jugador.direccionManiobra * 0.03 : 0);
        camEstado.roll += (rollObjetivo - camEstado.roll) * (1 - Math.exp(-4 * dt));
        camera.rotateZ(camEstado.roll + nRoll);

        // FOV: se abre al avanzar o al ejecutar el tonel (sensación de velocidad sin perder visibilidad)
        const fovObjetivo = 50 + (lonNorm < 0 ? -lonNorm * 2.5 : 0) + (jugador.tiempoManiobra > 0 ? 4.5 : 0);
        camEstado.fov += (fovObjetivo - camEstado.fov) * (1 - Math.exp(-5 * dt));
        if (Math.abs(camera.fov - camEstado.fov) > 0.01) {
            camera.fov = camEstado.fov;
            camera.updateProjectionMatrix();
        }
    }

    // ========================================================================
    // ALERTAS DEL PILOTO: VIÑETA DE DAÑO, ALARMA DE CASCO CRÍTICO E INDICADOR DE TONEL
    // ========================================================================
    const domHud = {};
    function elHud(id) {
        if (!domHud[id] || !domHud[id].isConnected) domHud[id] = document.getElementById(id);
        return domHud[id];
    }

    function actualizarOverlayDanoDOM(flash, pctVida) {
        const el = elHud('hud-vineta-dano');
        if (!el) return;
        const critico = pctVida < 0.3 && modoEscena === 'COMBATE' && estadoJuego.enCurso;
        const pulso = critico ? (0.25 + 0.2 * Math.sin(performance.now() * 0.008)) * (1 - pctVida / 0.3) : 0;
        el.style.opacity = Math.min(1, flash * 0.9 + pulso).toFixed(3);
    }

    function actualizarAlertasJugador(dt) {
        estadoJuego.tiempoFlashDano = Math.max(0, (estadoJuego.tiempoFlashDano || 0) - dt * 2.2);
        const pctVida = jugador.vida / Math.max(1, jugador.vidaMax);
        actualizarOverlayDanoDOM(estadoJuego.tiempoFlashDano, pctVida);

        if (pctVida < 0.25 && jugador.vida > 0) {
            temporizadorAlarma -= dt;
            if (temporizadorAlarma <= 0) {
                temporizadorAlarma = 0.85;
                sonarEfecto('alarma');
            }
        } else {
            temporizadorAlarma = 0;
        }

        const elMan = elHud('hud-maniobra');
        if (elMan) {
            const cd = (jugador.perfil && jugador.perfil.cooldownManiobra) || 1.5;
            const listo = jugador.cooldownManiobra <= 0;
            const pct = listo ? 100 : (1 - jugador.cooldownManiobra / cd) * 100;
            const fill = elHud('hud-fill-maniobra');
            if (fill) fill.style.width = pct.toFixed(0) + '%';
            elMan.classList.toggle('lista', listo);
        }
    }

    function ejecutarManiobraEvasiva() {
        if (modoEscena !== 'COMBATE' || !estadoJuego.enCurso || estadoJuego.pausado || estadoJuego.terminado) return;
        if (jugador.cooldownManiobra > 0 || jugador.tiempoManiobra > 0) return;
        let dir = 0;
        if (teclas['ArrowLeft'] || teclas['KeyA']) dir -= 1;
        if (teclas['ArrowRight'] || teclas['KeyD']) dir += 1;
        if (mando.conectado && mando.ejeX !== 0) dir = Math.sign(mando.ejeX);
        if (dir === 0) dir = Math.abs(jugador.vx) > 1 ? Math.sign(jugador.vx) : (jugador.x > 0 ? -1 : 1);
        jugador.direccionManiobra = dir;
        jugador.tiempoManiobra = DURACION_MANIOBRA;
        jugador.cooldownManiobra = ((jugador.perfil && jugador.perfil.cooldownManiobra) || 1.5) + DURACION_MANIOBRA;
        sonarEfecto('maniobra');
    }

    // Intangible: invulnerabilidad por poder/reaparición o durante los fotogramas del tonel
    function jugadorIntangible() {
        return estadoJuego.tiempoInvulnerable > 0 || jugador.tiempoManiobra > 0;
    }

    function actualizarEscenaCombate(dt, elapsed) {
        if (!estadoJuego.enCurso || estadoJuego.pausado || estadoJuego.terminado) {
            // Tras el fin de la misión las explosiones terminan de disiparse detrás del informe
            if (estadoJuego.terminado && !estadoJuego.pausado) actualizarPoolParticulas(dt, 0);
            return;
        }

        const prevSegundo = Math.floor(estadoJuego.tiempoMision || 0);
        const prevDecima = Math.floor((estadoJuego.tiempoMision || 0) * 10);
        estadoJuego.tiempoMision = (estadoJuego.tiempoMision || 0) + dt;
        const nuevoSegundo = Math.floor(estadoJuego.tiempoMision);
        const nuevaDecima = Math.floor(estadoJuego.tiempoMision * 10);

        // 1. Desplazar sectores periódicos maestros (Terreno + Río + Estructuras solidarios, empalme C^∞ exacto)
        const cfgNivel = CONFIG_NIVELES_CAMPANA[nivelActivoCombate] || CONFIG_NIVELES_CAMPANA[1];
        const velTerreno = cfgNivel.velocidadTerreno || 25;
        const cicloTotalZ = PERIODO_MAPA_Z * chunksTerreno.length; // 3 * 160 = 480.0 exacto

        const desplazamientoMundoZ = dt * velTerreno;
        chunksTerreno.forEach(sector => {
            sector.position.z += desplazamientoMundoZ;
            if (sector.position.z >= PERIODO_MAPA_Z) {
                sector.position.z -= cicloTotalZ;
            }
        });
        if (texturaCorrienteRio) {
            // Corriente del río algo más rápida que el terreno: el agua fluye con vida propia
            texturaCorrienteRio.offset.y = (texturaCorrienteRio.offset.y + dt * 0.55) % 1;
        }

        if (mallaRio && mallaRio.material && nivelActivoCombate === 4) {
            mallaRio.material.emissiveIntensity = 0.95 + 0.35 * Math.sin(elapsed * 4.0);
        }

        nubes3D.forEach(nube => {
            nube.position.z += dt * 34;
            if (nube.position.z > 35) {
                nube.position.z = -240;
                if (nube.position.y < -3.0) {
                    nube.position.x = (Math.random() - 0.5) * 30;
                } else {
                    const lado = Math.random() < 0.5 ? -1 : 1;
                    nube.position.x = lado * (32 + Math.random() * 22);
                }
            }
        });

        // 2. Temporizadores de potenciadores activos con aviso auditivo y visual de expiración
        if (estadoJuego.tiempoInvulnerable > 0) {
            const antes = estadoJuego.tiempoInvulnerable;
            estadoJuego.tiempoInvulnerable = Math.max(0, estadoJuego.tiempoInvulnerable - dt);
            if (antes > 0 && estadoJuego.tiempoInvulnerable === 0 && estadoJuego.duracionMaxInvulnerable >= 4.0) {
                sonarEfecto('fin_poder');
            }
        }
        if (estadoJuego.tiempoPoderActivo > 0) {
            const antes = estadoJuego.tiempoPoderActivo;
            estadoJuego.tiempoPoderActivo = Math.max(0, estadoJuego.tiempoPoderActivo - dt);
            if (antes > 0 && estadoJuego.tiempoPoderActivo === 0) {
                sonarEfecto('fin_poder');
            }
        }
        if (estadoJuego.tiempoEnemigosParalizados > 0) {
            const antes = estadoJuego.tiempoEnemigosParalizados;
            estadoJuego.tiempoEnemigosParalizados = Math.max(0, estadoJuego.tiempoEnemigosParalizados - dt);
            if (antes > 0 && estadoJuego.tiempoEnemigosParalizados === 0) {
                sonarEfecto('fin_poder');
            }
        }
        if (estadoJuego.tiempoEfectoReparacion > 0) {
            estadoJuego.tiempoEfectoReparacion = Math.max(0, estadoJuego.tiempoEfectoReparacion - dt);
        }

        if (estadoJuego.tiempoCombo > 0) {
            estadoJuego.tiempoCombo -= dt;
            if (estadoJuego.tiempoCombo <= 0) {
                estadoJuego.combo = 1;
                actualizarHudDOM();
            }
        }
        if (jugador.cooldownDisparo > 0) jugador.cooldownDisparo -= dt;

        // Actualizar chips de potenciadores cada décima de segundo
        if (nuevaDecima !== prevDecima) {
            actualizarChipsPotenciadoresDOM();
        }

        // Despliegue dinámico del segundo escalón (refuerzos tácticos de la oleada)
        if (estadoJuego.refuerzosPendientes > 0 && !estadoJuego.enFaseJefe) {
            estadoJuego.temporizadorRefuerzos -= dt;
            if (estadoJuego.temporizadorRefuerzos <= 0 || enemigos.length <= 1) {
                estadoJuego.refuerzosPendientes--;
                desplegarOleadaEnemiga3D(estadoJuego.oleada, true);
            }
        }

        if (nuevoSegundo !== prevSegundo) {
            actualizarHudDOM();
        }

        // 3. Movimiento 3D suave de la aeronave del jugador con alabeo y efectos visuales de potenciadores
        // Entrada analógica unificada: teclado (digital), mando (stick) o ratón (proporcional a la distancia)
        let dirX = 0, dirZ = 0;
        if (teclas['ArrowLeft'] || teclas['KeyA']) dirX -= 1;
        if (teclas['ArrowRight'] || teclas['KeyD']) dirX += 1;
        if (teclas['ArrowUp'] || teclas['KeyW']) dirZ -= 1;
        if (teclas['ArrowDown'] || teclas['KeyS']) dirZ += 1;

        if (mando.conectado && (mando.ejeX !== 0 || mando.ejeY !== 0)) {
            dirX = mando.ejeX;
            dirZ = mando.ejeY;
        }

        if (usarRatonVuelo) {
            // Llegada suave: la intensidad decrece al acercarse al cursor (sin oscilación de "todo o nada")
            const metaX = ratonX * 14;
            const metaZ = 4 + ratonY * 8;
            dirX = Math.max(-1, Math.min(1, (metaX - jugador.x) / 2.6));
            dirZ = Math.max(-1, Math.min(1, (metaZ - jugador.z) / 2.6));
        }

        const magEntrada = Math.hypot(dirX, dirZ);
        if (magEntrada > 1) {
            dirX /= magEntrada;
            dirZ /= magEntrada;
        }

        // FÍSICA DE VUELO ARCADE CON INERCIA: aceleración y frenado exponenciales según el perfil del avión
        const perfil = jugador.perfil || PERFILES_VUELO.NAVE_CONDOR_X;
        const velMax = jugador.velocidadBase;
        const tasaEje = (entrada, vel, objetivo) => {
            if (entrada === 0) return perfil.frenado;
            // Contravolante: invertir la dirección muerde más rápido para que el control se sienta preciso
            return (Math.sign(objetivo) !== Math.sign(vel) && Math.abs(vel) > 0.5) ? perfil.aceleracion * 1.45 : perfil.aceleracion;
        };
        const objVx = dirX * velMax;
        const objVz = dirZ * velMax;
        jugador.vx += (objVx - jugador.vx) * (1 - Math.exp(-tasaEje(dirX, jugador.vx, objVx) * dt));
        jugador.vz += (objVz - jugador.vz) * (1 - Math.exp(-tasaEje(dirZ, jugador.vz, objVz) * dt));

        // Maniobra evasiva (tonel): impulso lateral breve con intangibilidad
        if (jugador.cooldownManiobra > 0) jugador.cooldownManiobra = Math.max(0, jugador.cooldownManiobra - dt);
        let progresoManiobra = 0;
        if (jugador.tiempoManiobra > 0) {
            jugador.tiempoManiobra = Math.max(0, jugador.tiempoManiobra - dt);
            progresoManiobra = 1 - jugador.tiempoManiobra / DURACION_MANIOBRA;
            const empuje = velMax * 1.85 * (1 - progresoManiobra * 0.6);
            jugador.vx += (jugador.direccionManiobra * empuje - jugador.vx) * (1 - Math.exp(-16 * dt));
            if (Math.random() < 0.6) {
                emitirParticula(false, jugador.x, 0.3, jugador.z + 0.8, 0, 0, 6, 0.25, 0.7, 0.1, jugador.colorSkin, 0.6, 2, 0);
            }
        }

        jugador.x += jugador.vx * dt;
        jugador.z += jugador.vz * dt;

        // Límites blandos: el avión "apoya" contra el borde sin rebotar y pierde la velocidad hacia afuera
        if (jugador.x > CONFIG_MOTOR.LIMITE_X_JUGADOR) { jugador.x = CONFIG_MOTOR.LIMITE_X_JUGADOR; if (jugador.vx > 0) jugador.vx *= 0.2; }
        if (jugador.x < -CONFIG_MOTOR.LIMITE_X_JUGADOR) { jugador.x = -CONFIG_MOTOR.LIMITE_X_JUGADOR; if (jugador.vx < 0) jugador.vx *= 0.2; }
        if (jugador.z > CONFIG_MOTOR.LIMITE_Z_MAX_JUGADOR) { jugador.z = CONFIG_MOTOR.LIMITE_Z_MAX_JUGADOR; if (jugador.vz > 0) jugador.vz *= 0.2; }
        if (jugador.z < CONFIG_MOTOR.LIMITE_Z_MIN_JUGADOR) { jugador.z = CONFIG_MOTOR.LIMITE_Z_MIN_JUGADOR; if (jugador.vz < 0) jugador.vz *= 0.2; }

        if (jugador.mesh) {
            // Alabeo, cabeceo y guiñada derivados de la velocidad real (no de la tecla): peso y continuidad
            const latNorm = jugador.vx / Math.max(1, velMax);
            const lonNorm = jugador.vz / Math.max(1, velMax);
            const rollObjetivo = -Math.max(-1.25, Math.min(1.25, latNorm)) * perfil.rollMax;
            jugador.rollVisual = (jugador.rollVisual || 0) + (rollObjetivo - (jugador.rollVisual || 0)) * (1 - Math.exp(-perfil.respuestaRoll * dt));

            // Turbulencia visual (no altera la posición jugable): más intensa en el Cañón de la Tormenta
            const biomaAct = Math.max(1, Math.min(5, Math.ceil(nivelActivoCombate / 2)));
            const turb = biomaAct === 2 ? 2.2 : (biomaAct === 5 ? 0.6 : 1.0);
            const turbRoll = (Math.sin(elapsed * 1.7) * 0.022 + Math.sin(elapsed * 3.3 + 1.1) * 0.012) * turb;
            const turbY = (Math.sin(elapsed * 2.1) * 0.05 + Math.sin(elapsed * 4.7 + 0.4) * 0.02) * turb;

            let rollManiobra = 0;
            if (jugador.tiempoManiobra > 0) {
                const suave = progresoManiobra * progresoManiobra * (3 - 2 * progresoManiobra);
                rollManiobra = -jugador.direccionManiobra * suave * Math.PI * 2;
            }

            jugador.mesh.position.set(jugador.x, turbY, jugador.z);
            jugador.mesh.rotation.z = jugador.rollVisual + turbRoll + rollManiobra;
            const pitchObjetivo = -lonNorm * 0.17;
            jugador.mesh.rotation.x += (pitchObjetivo - jugador.mesh.rotation.x) * (1 - Math.exp(-8 * dt));
            const yawObjetivo = -latNorm * 0.10;
            jugador.mesh.rotation.y += (yawObjetivo - jugador.mesh.rotation.y) * (1 - Math.exp(-6 * dt));

            // Estelas de condensación en las puntas de ala: más intensas al virar fuerte o al hacer el tonel
            if (jugador.estelas) {
                const envergadura = jugador.modeloId === 'NAVE_B2' ? 2.6 : 1.85;
                const cosR = Math.cos(jugador.mesh.rotation.z);
                const sinR = Math.sin(jugador.mesh.rotation.z);
                const intensidadEstela = Math.min(1, 0.28 + Math.abs(latNorm) * 0.6 + (jugador.tiempoManiobra > 0 ? 0.6 : 0));
                [-1, 1].forEach((lado, idx) => {
                    const e = jugador.estelas[idx];
                    e.mesh.visible = true;
                    actualizarEstela(e, jugador.x + lado * envergadura * cosR, turbY + lado * envergadura * sinR * 0.5 + 0.05, jugador.z + 0.9,
                        0.07, intensidadEstela * 0.55, dt, desplazamientoMundoZ);
                });
            }

            // Humo de daño: el avión muestra visualmente su estado crítico
            if (jugador.vida < jugador.vidaMax * 0.3 && Math.random() < dt * 22) {
                emitirParticula(true, jugador.x + (Math.random() - 0.5) * 0.8, 0.3, jugador.z + 1.0, 0, 0.6, 2.0, 0.9, 0.5, 1.8, 0x3a3f47, 0.5, 1.0, -0.3);
                if (Math.random() < 0.35) emitirParticula(false, jugador.x, 0.35, jugador.z + 0.9, (Math.random() - 0.5) * 2, 1, 3, 0.25, 0.4, 0.1, 0xff7a1a, 0.9, 1.5, 0);
            }

            // Transmisión continua de telemetría de red a 20Hz (50ms) en combates multijugador
            const esPartidaRed = estadoJuego.modalidad && estadoJuego.modalidad.startsWith('Multijugador_');
            if (esPartidaRed && estadoJuego.enCurso && !estadoJuego.pausado) {
                tiempoUltimoEnvioRed += dt;
                if (tiempoUltimoEnvioRed >= 0.05) {
                    tiempoUltimoEnvioRed = 0;
                    if (window.InterfazJuego && window.InterfazJuego.enviarEstadoRed) {
                        window.InterfazJuego.enviarEstadoRed(jugador.x, jugador.z, Math.max(0, Math.round(jugador.vida)), false);
                    }
                }
            }

            // Actualizar efectos visuales 3D activos en la aeronave del jugador
            const burbujaEscudo = obtenerNodoNombrado(jugador.mesh, 'burbujaEscudo');
            if (burbujaEscudo && burbujaEscudo.material && burbujaEscudo.material.uniforms) {
                const invul = estadoJuego.tiempoInvulnerable > 0.15;
                burbujaEscudo.material.uniforms.uIntensity.value = invul ? 0.22 : (jugador.escudo > 0 ? 0.06 : 0.02);
                burbujaEscudo.material.uniforms.uColor.value.setHex(invul ? 0x00e676 : jugador.colorSkin);
            }

            const anilloEscudoActivo = obtenerNodoNombrado(jugador.mesh, 'anilloEscudoActivo');
            if (anilloEscudoActivo) {
                anilloEscudoActivo.visible = estadoJuego.tiempoInvulnerable > 0.15;
                if (anilloEscudoActivo.visible) {
                    anilloEscudoActivo.rotation.z = elapsed * 3.5;
                }
            }

            const grupoOverdrive = obtenerNodoNombrado(jugador.mesh, 'grupoOverdriveArmas');
            if (grupoOverdrive) {
                grupoOverdrive.visible = estadoJuego.tiempoPoderActivo > 0;
                if (grupoOverdrive.visible) {
                    grupoOverdrive.rotation.y = elapsed * 4.5;
                }
            }

            const anilloReparacion = obtenerNodoNombrado(jugador.mesh, 'anilloReparacion');
            if (anilloReparacion) {
                anilloReparacion.visible = estadoJuego.tiempoEfectoReparacion > 0;
                if (anilloReparacion.visible) {
                    const progreso = 1.0 - (estadoJuego.tiempoEfectoReparacion / 1.1);
                    anilloReparacion.position.y = -0.35 + progreso * 0.9;
                    if (anilloReparacion.material && anilloReparacion.material.color) {
                        anilloReparacion.material.color.setHex(estadoJuego.colorEfectoReparacion || 0x00e676);
                    }
                }
            }
        }

        luzPuntoJugador.position.set(jugador.x, 2.5, jugador.z);
        luzPuntoJugador.color.setHex(estadoJuego.tiempoPoderActivo > 0 ? 0xffb800 : (estadoJuego.tiempoInvulnerable > 0.2 ? 0x00e676 : 0x00e5ff));

        // Actualizar cinta de brújula de aviónica en el HUD
        const elRumbo = document.getElementById('hud-brujula-rumbo');
        const elMarcas = document.getElementById('hud-brujula-marcas');
        if (elRumbo) {
            const hdg = (360 + Math.round(jugador.x * 1.5)) % 360;
            elRumbo.textContent = `HDG ${hdg === 0 ? 360 : hdg}° // SEC-0${estadoJuego.nivel}`;
        }
        if (elMarcas) {
            elMarcas.style.transform = `translateX(${(-jugador.x * 2.8).toFixed(1)}px)`;
        }

        if (estadoJuego.modalidad === 'TutorialInteractivo' && estadoJuego.pasoTutorial === 1 && (dirX !== 0 || dirZ !== 0)) {
            estadoJuego.progresoTutorial += dt;
            if (estadoJuego.progresoTutorial >= 1.5) {
                estadoJuego.pasoTutorial = 2;
                estadoJuego.progresoTutorial = 0;
                actualizarHudDOM();
            }
        }

        if (teclas['Space'] || teclas['Enter'] || ratonDisparando || mando.disparo) {
            dispararJugador3D();
        }

        // 4. Cámara de persecución con resorte amortiguado, anticipación lateral, inclinación en virajes,
        //    FOV dinámico y sacudida suave basada en trauma (ruido continuo, no saltos aleatorios por frame)
        actualizarCamaraCombate(dt, elapsed);
        actualizarLineasVelocidad(dt, velTerreno, jugador.tiempoManiobra > 0 ? 1 : Math.min(1, Math.abs(jugador.vz) / Math.max(1, jugador.velocidadBase) * (jugador.vz < 0 ? 0.6 : 0)));
        actualizarPoolParticulas(dt, desplazamientoMundoZ);
        actualizarAlertasJugador(dt);

        // 5. Actualizar enemigos y Jefes Finales 3D con Inteligencia Táctica y Orientación Yaw
        const paralizadosEMP = estadoJuego.tiempoEnemigosParalizados > 0;
        for (let i = enemigos.length - 1; i >= 0; i--) {
            const en = enemigos[i];

            // Mostrar/ocultar halo electromagnético EMP en el modelo 3D del enemigo
            if (en.mesh) {
                const haloEMP = obtenerNodosCacheados(en.mesh).haloEMP;
                if (haloEMP) {
                    haloEMP.visible = paralizadosEMP;
                    if (paralizadosEMP) {
                        haloEMP.rotation.z = elapsed * 5.0;
                    }
                }
            }

            if (en.esRemotoNet) {
                en.x += (en.xMeta - en.x) * Math.min(1.0, dt * 18.0);
                en.z += (en.zMeta - en.z) * Math.min(1.0, dt * 18.0);
                en.mesh.position.set(en.x, 0.4, en.z);
                en.mesh.rotation.y = Math.PI;
                const rollTarget = Math.max(-0.45, Math.min(0.45, -(en.x - (en.prevX || en.x)) * 14.0));
                en.mesh.rotation.z += (rollTarget - en.mesh.rotation.z) * 8.0 * dt;
                en.prevX = en.x;
                en.prevZ = en.z;
                continue;
            }

            if (!paralizadosEMP) {
                en.tiempoEstado = (en.tiempoEstado || 0) + dt;
                en.tiempoPatrulla = (en.tiempoPatrulla || 0) + dt;

                const distSeguridadMinima = en.esJefe ? 26.5 : (en.modeloId === 'NAVE_ENEMIGO_PESADO' ? 30.5 : 23.0);
                const zLimiteMaximo = jugador.z - distSeguridadMinima;

                let targetX = en.x;
                let targetZ = en.z;

                if (en.estado === 'ENTRADA') {
                    targetX = en.xMeta;
                    targetZ = en.zMeta;
                    en.z += (targetZ - en.z) * 4.8 * dt;
                    en.x += (targetX - en.x) * 3.4 * dt;

                    if (Math.abs(en.z - targetZ) < 1.4) {
                        en.estado = 'PATRULLA';
                        en.tiempoEstado = 0;
                    }
                } else if (en.estado === 'RETIRADA') {
                    const zRefugio = en.zMeta - 5.0;
                    en.z += (zRefugio - en.z) * 4.6 * dt;
                    const dirEscape = (en.indice % 2 === 0 ? 1 : -1);
                    en.x += dirEscape * 5.6 * dt;
                    if (en.tiempoEstado > 1.2 || Math.abs(en.z - zRefugio) < 1.2) {
                        en.estado = 'PATRULLA';
                        en.tiempoEstado = 0;
                    }
                } else if (en.estado === 'ATAQUE') {
                    const zAtaque = Math.min(en.zMeta + 3.0, zLimiteMaximo);
                    en.z += (zAtaque - en.z) * 3.2 * dt;
                    en.x += (jugador.x - en.x) * 1.5 * dt;
                    if (en.tiempoEstado > 1.2) {
                        en.estado = 'RETIRADA';
                        en.tiempoEstado = 0;
                    }
                } else {
                    if (en.esJefe) {
                        targetX = Math.sin(en.tiempoPatrulla * 0.85) * 8.8;
                        targetZ = en.zMeta + Math.cos(en.tiempoPatrulla * 0.45) * 2.0;
                    } else if (en.modeloId === 'NAVE_ENEMIGO_LIGERO') {
                        targetX = en.xMeta + Math.sin(en.tiempoPatrulla * 1.8 + en.indice * 1.3) * 5.8;
                        targetZ = en.zMeta + Math.cos(en.tiempoPatrulla * 1.2 + en.indice) * 1.8;
                    } else if (en.modeloId === 'NAVE_ENEMIGO_PESADO') {
                        targetX = en.xMeta + Math.sin(en.tiempoPatrulla * 0.7 + en.indice) * 3.2;
                        targetZ = en.zMeta + Math.cos(en.tiempoPatrulla * 0.5) * 1.2;
                    } else {
                        targetX = en.xMeta + Math.sin(en.tiempoPatrulla * 1.3 + en.indice * 1.4) * 4.8;
                        targetZ = en.zMeta + Math.sin(en.tiempoPatrulla * 0.8) * 1.5;
                    }

                    en.x += (targetX - en.x) * 3.5 * dt;
                    en.z += (targetZ - en.z) * 2.8 * dt;

                    if (!en.esJefe && en.cooldown < 0.35 && Math.random() < 0.02) {
                        en.estado = 'ATAQUE';
                        en.tiempoEstado = 0;
                    }
                }

                // Reacción Evasiva Proactiva
                const distanciaAlJugadorZ = jugador.z - en.z;
                if (distanciaAlJugadorZ < distSeguridadMinima) {
                    const factorCompresion = distSeguridadMinima - distanciaAlJugadorZ;
                    en.z -= (factorCompresion + 2.0) * 8.5 * dt;
                    const dirEscapeX = Math.sign(en.x - jugador.x) || (en.indice % 2 === 0 ? 1 : -1);
                    en.x += dirEscapeX * 7.0 * dt;
                    en.estado = 'RETIRADA';
                    en.tiempoEstado = 0;
                }

                // Separación Mutua (Boids Repulsion)
                let repX = 0;
                let repZ = 0;
                const radioSep = (en.esJefe || en.modeloId === 'NAVE_ENEMIGO_PESADO') ? 6.2 : 3.8;
                for (let j = 0; j < enemigos.length; j++) {
                    if (i === j) continue;
                    const otro = enemigos[j];
                    const dx = en.x - otro.x;
                    const dz = en.z - otro.z;
                    const d = Math.hypot(dx, dz);
                    if (d < radioSep && d > 0.05) {
                        const fuerza = (radioSep - d) / radioSep;
                        repX += (dx / d) * fuerza * 9.5;
                        repZ += (dz / d) * fuerza * 6.0;
                    }
                }
                en.vxRepulsion = (en.vxRepulsion || 0) + (repX - (en.vxRepulsion || 0)) * 6.0 * dt;
                en.vzRepulsion = (en.vzRepulsion || 0) + (repZ - (en.vzRepulsion || 0)) * 6.0 * dt;

                en.x += en.vxRepulsion * dt;
                en.z += en.vzRepulsion * dt;

                en.z = Math.min(en.z, zLimiteMaximo);
                en.x = Math.max(-CONFIG_MOTOR.LIMITE_X_JUGADOR, Math.min(CONFIG_MOTOR.LIMITE_X_JUGADOR, en.x));

                // Inclinación 3D Dinámica (Roll, Pitch & Yaw orientado hacia el jugador)
                const velX = (en.x - (en.prevX || en.x)) / Math.max(0.001, dt);
                const velZ = (en.z - (en.prevZ || en.z)) / Math.max(0.001, dt);
                en.prevX = en.x;
                en.prevZ = en.z;

                const yawHaciaJugador = Math.atan2(jugador.x - en.x, Math.max(8.0, jugador.z - en.z));
                const maxYaw = en.esJefe ? 0.16 : (en.modeloId === 'NAVE_ENEMIGO_PESADO' ? 0.24 : 0.38);
                const yawTarget = Math.max(-maxYaw, Math.min(maxYaw, yawHaciaJugador * 0.65));
                en.yaw = (en.yaw || 0) + (yawTarget - (en.yaw || 0)) * 5.0 * dt;

                en.mesh.position.set(en.x, en.esJefe ? 0.8 : (0.2 + (en.indice % 2) * 0.25), en.z);
                const factorRoll = en.esJefe ? 0.08 : 0.28;
                const rollTarget = Math.max(-0.35, Math.min(0.35, -velX * factorRoll));
                const pitchTarget = en.estado === 'ENTRADA' ? 0.08 : (en.estado === 'RETIRADA' ? -0.12 : Math.max(-0.15, Math.min(0.15, velZ * 0.03)));
                en.mesh.rotation.z += (rollTarget - en.mesh.rotation.z) * 6.0 * dt;
                en.mesh.rotation.x += (pitchTarget - en.mesh.rotation.x) * 5.0 * dt;
                en.mesh.rotation.y = en.yaw;

                const prop = obtenerNodosCacheados(en.mesh).propulsor;
                if (prop) {
                    const escalaFuego = (en.estado === 'ENTRADA' || en.estado === 'RETIRADA') ? 1.45 : 1.0;
                    prop.scale.set(1, escalaFuego + Math.sin(en.tiempoPatrulla * 25) * 0.15, 1);
                }

                const nucleo = obtenerNodosCacheados(en.mesh).nucleoJefe;
                if (nucleo) {
                    nucleo.rotation.y += dt * 3;
                    nucleo.rotation.x += dt * 2;
                }

                // Disparo enemigo (Solo si ya completó su entrada)
                const nodosEn = obtenerNodosCacheados(en.mesh);
                if (en.estado !== 'ENTRADA') {
                    en.cooldown -= dt;
                    if (en.cooldown <= 0) {
                        const cadencia = modDificultad().cadenciaEnemiga;
                        en.cooldown = (en.esJefe
                            ? Math.max(0.85, 1.55 - estadoJuego.nivel * 0.09)
                            : (1.75 + Math.random() * 1.25)) * cadencia;
                        dispararEnemigo3D(en);
                    }
                }

                // Telegrafía del disparo: el cañón brilla y crece durante los 0.4 s previos
                if (nodosEn.cargaDisparo) {
                    const ventana = 0.4;
                    const cargando = en.estado !== 'ENTRADA' && en.cooldown < ventana;
                    nodosEn.cargaDisparo.visible = cargando;
                    if (cargando) {
                        const k = 1 - Math.max(0, en.cooldown) / ventana;
                        const s = (en.esJefe ? 0.75 : 0.32) * (0.25 + k * 0.75) * (1 + Math.sin(elapsed * 40) * 0.08);
                        nodosEn.cargaDisparo.scale.set(s, s, s);
                    }
                }

                const distJugador = Math.hypot(jugador.x - en.x, jugador.z - en.z);
                if (distJugador < 2.8) {
                    // Choque con impulso físico (sin teletransporte): ambos se repelen
                    const nx = (jugador.x - en.x) / Math.max(0.01, distJugador);
                    const nz = (jugador.z - en.z) / Math.max(0.01, distJugador);
                    jugador.vx += nx * 26;
                    jugador.vz += nz * 26;
                    en.z -= 2.5;
                    if (!jugadorIntangible()) {
                        recibirDanoJugador(15);
                        crearExplosion3D(jugador.x, jugador.z, 0xff9800, 10);
                    }
                }
            }

            // Destello emisivo al recibir impacto + humo cuando el enemigo está gravemente dañado
            if (en.mesh && !en.esRemotoNet) {
                if (en.tiempoDestello > 0) {
                    en.tiempoDestello = Math.max(0, en.tiempoDestello - dt);
                    aplicarDestelloImpacto(en.mesh, en.tiempoDestello / 0.09);
                }
                if (en.vida > 0 && en.vida < en.vidaMax * 0.35 && Math.random() < dt * (en.esJefe ? 30 : 12)) {
                    const r = en.esJefe ? 2.2 : 0.8;
                    emitirParticula(true, en.x + (Math.random() - 0.5) * r, 0.6, en.z - 0.5, 0, 0.8, -1.5, 1.0, 0.6, 2.2, 0x2e3238, 0.55, 0.8, -0.3);
                    if (Math.random() < 0.4) emitirParticula(false, en.x + (Math.random() - 0.5) * r, 0.6, en.z, 0, 1.2, -2, 0.3, 0.5, 0.1, 0xff6d00, 0.9, 1.2, 0);
                }
            }

            // Verificar si el enemigo fue destruido
            if (en.vida <= 0) {
                sonarEfecto(en.esJefe || en.esElite ? 'explosion_grande' : 'explosion');
                crearExplosion3D(en.x, en.z, en.esJefe ? 0xffb800 : 0xff5252, en.esJefe ? 28 : 18);
                crearOndaExpansiva3D(en.x, en.z, en.esJefe ? 0xffd54f : 0xff5252, en.esJefe ? 22 : 10);
                agregarTrauma(en.esJefe ? 0.95 : (en.esElite ? 0.55 : 0.32));
                if (en.esJefe) {
                    // Secuencia cinemática del derribo: cámara lenta + explosiones encadenadas
                    tiempoCamaraSlowMo = 0.9;
                    for (let k = 0; k < 4; k++) {
                        crearExplosion3D(en.x + (Math.random() - 0.5) * 6, en.z + (Math.random() - 0.5) * 4, k % 2 ? 0xff6d00 : 0xffd54f, 22);
                    }
                }

                estadoJuego.bajas++;
                estadoJuego.monedasPartida += en.esJefe ? 150 : (en.esElite ? 50 : 20);
                estadoJuego.puntos += Math.round((en.esJefe ? 1500 : (en.esElite ? 450 : 200)) * estadoJuego.combo * modDificultad().puntos);

                if (en.esJefe || en.esElite || Math.random() < 0.45) {
                    generarSuministro3D(en.x, en.z, en.esElite ? 'escudo' : null);
                }

                eliminarBarraSaludEnemigoDOM(en);
                grupoMundoCombate.remove(en.mesh);
                enemigos.splice(i, 1);
                if (en.idRed && remotosRed[en.idRed]) {
                    delete remotosRed[en.idRed];
                }
                actualizarHudDOM();

                if (estadoJuego.modalidad === 'Multijugador_1v1' || estadoJuego.modalidad === 'Multijugador_2v2') {
                    if (enemigos.length === 0) {
                        finalizarMision(true);
                        return;
                    }
                }
            }
        }

        // 6. Actualizar proyectiles 3D con Detección Continua de Colisiones (CCD Segmento-Círculo)
        for (let i = proyectiles.length - 1; i >= 0; i--) {
            const p = proyectiles[i];
            if (!p.activo) {
                grupoMundoCombate.remove(p.mesh);
                proyectiles.splice(i, 1);
                continue;
            }

            p.prevX = p.x;
            p.prevZ = p.z;
            p.x += p.vx * dt;
            p.z += p.vz * dt;
            p.tiempoVida = (p.tiempoVida || 0) + dt;

            p.mesh.position.set(p.x, 0.50, p.z);
            p.mesh.rotation.y = Math.atan2(p.vx, p.vz);

            // Ciclo de vida estricto y límites espaciales
            if (p.tiempoVida >= (p.tiempoVidaMax || 4.8) || p.z < -85 || p.z > 24 || Math.abs(p.x) > 26) {
                p.activo = false;
                grupoMundoCombate.remove(p.mesh);
                proyectiles.splice(i, 1);
                continue;
            }

            if (!p.esEnemigo) {
                // A. Proyectil del Jugador -> Colisión continua contra Enemigos
                let impacto = false;
                for (let j = 0; j < enemigos.length; j++) {
                    const en = enemigos[j];
                    const radioHit = en.esJefe ? 2.75 : (en.esElite ? 1.85 : (en.modeloId === 'NAVE_ENEMIGO_PESADO' ? 1.70 : 1.45));
                    const distSeg = distanciaPuntoASegmento(p.prevX, p.prevZ, p.x, p.z, en.x, en.z);
                    if (distSeg <= radioHit + (p.radioColision || 0.22)) {
                        en.vida -= p.dano;
                        en.tiempoImpactoReciente = 0.16;
                        en.tiempoDestello = 0.09;
                        sonarEfecto('impacto');
                        estadoJuego.impactos++;
                        estadoJuego.tiempoCombo = 3.2;
                        if (estadoJuego.impactos % 2 === 0 && estadoJuego.combo < 8) {
                            estadoJuego.combo++;
                            if (estadoJuego.combo > estadoJuego.comboMax) estadoJuego.comboMax = estadoJuego.combo;
                        }
                        estadoJuego.cargaPoder = Math.min(100, estadoJuego.cargaPoder + 5);
                        estadoJuego.puntos += 25 * estadoJuego.combo;
                        crearExplosion3D(p.x, p.z, 0x00e5ff, 6);
                        impacto = true;
                        actualizarHudDOM();
                        break;
                    }
                }
                if (impacto) {
                    p.activo = false;
                    grupoMundoCombate.remove(p.mesh);
                    proyectiles.splice(i, 1);
                    continue;
                }

                // B. Proyectil del Jugador -> Colisión continua contra Meteoros 3D
                for (let mIdx = meteoros3D.length - 1; mIdx >= 0; mIdx--) {
                    const met = meteoros3D[mIdx];
                    const distMet = distanciaPuntoASegmento(p.prevX, p.prevZ, p.x, p.z, met.x, met.z);
                    if (distMet <= met.radio + 0.55) {
                        met.vida -= p.dano;
                        met.tiempoImpactoFlash = 0.12;
                        crearExplosion3D(p.x, p.z, 0xff7043, 6);

                        met.mesh.traverse(child => {
                            if (child.isMesh && child.material && child.material.emissiveIntensity !== undefined) {
                                child.material.emissiveIntensity = 0.38;
                            }
                        });

                        if (met.vida <= 0) {
                            sonarEfecto('explosion_grande');
                            crearExplosion3D(met.x, met.z, 0xff5722, 26);
                            crearOndaExpansiva3D(met.x, met.z, 0xff9100, 18);
                            agregarTrauma(0.45);
                            estadoJuego.puntos += Math.round(350 * estadoJuego.combo * modDificultad().puntos);

                            if (Math.random() < 0.68) {
                                generarSuministro3D(met.x, met.z, Math.random() < 0.5 ? 'monedas' : 'energia');
                            }

                            grupoMundoCombate.remove(met.mesh);
                            meteoros3D.splice(mIdx, 1);
                        }
                        impacto = true;
                        actualizarHudDOM();
                        break;
                    }
                }
                if (impacto) {
                    p.activo = false;
                    grupoMundoCombate.remove(p.mesh);
                    proyectiles.splice(i, 1);
                }
            } else {
                // C. Proyectil Enemigo -> Bloqueo físico contra asteroides sólidos (no atraviesa roca sólida)
                let bloqueadoPorRoca = false;
                for (let mIdx = 0; mIdx < meteoros3D.length; mIdx++) {
                    const met = meteoros3D[mIdx];
                    const distRoca = distanciaPuntoASegmento(p.prevX, p.prevZ, p.x, p.z, met.x, met.z);
                    if (distRoca <= met.radio * 0.92) {
                        crearExplosion3D(p.x, p.z, 0xff5252, 4);
                        bloqueadoPorRoca = true;
                        break;
                    }
                }
                if (bloqueadoPorRoca) {
                    p.activo = false;
                    grupoMundoCombate.remove(p.mesh);
                    proyectiles.splice(i, 1);
                    continue;
                }

                // D. Proyectil Enemigo -> Colisión continua contra el Avión del Jugador
                const distJug = distanciaPuntoASegmento(p.prevX, p.prevZ, p.x, p.z, jugador.x, jugador.z);
                const radioHitJugador = 1.15 + (p.radioColision || 0.18);
                if (jugador.tiempoManiobra > 0 && distJug <= radioHitJugador + 0.8) {
                    // Esquiva en tonel: el proyectil atraviesa la estela; roce premiado con carga de poder
                    if (!p.rozado) {
                        p.rozado = true;
                        estadoJuego.cargaPoder = Math.min(100, estadoJuego.cargaPoder + 4);
                        estadoJuego.puntos += 15;
                        emitirParticula(false, p.x, 0.5, p.z, 0, 0, 0, 0.18, 0.4, 1.4, 0xffffff, 0.7, 0, 0);
                    }
                } else if (distJug <= radioHitJugador) {
                    if (estadoJuego.tiempoInvulnerable <= 0) {
                        recibirDanoJugador(p.dano);
                    } else {
                        // Rebote energético sobre el escudo invencible
                        crearExplosion3D(p.x, p.z, 0x00e676, 5);
                    }
                    p.activo = false;
                    grupoMundoCombate.remove(p.mesh);
                    proyectiles.splice(i, 1);
                }
            }
        }

        // 7. Actualizar Obstáculos y Meteoros 3D
        temporizadorMeteoro -= dt;
        if (temporizadorMeteoro <= 0 && estadoJuego.modalidad !== 'TutorialInteractivo') {
            const rangoMet = cfgNivel.intervaloMeteoroBase || [4.8, 7.2];
            temporizadorMeteoro = rangoMet[0] + Math.random() * (rangoMet[1] - rangoMet[0]);
            desplegarMeteoro3D();
        }

        for (let i = meteoros3D.length - 1; i >= 0; i--) {
            const m = meteoros3D[i];
            m.z += m.vz * dt;
            m.x += m.vx * dt;
            m.mesh.position.set(m.x, 0, m.z);
            m.mesh.rotation.x += m.rotX * dt;
            m.mesh.rotation.y += m.rotY * dt;

            if (m.tiempoImpactoFlash > 0) {
                m.tiempoImpactoFlash -= dt;
                if (m.tiempoImpactoFlash <= 0) {
                    m.mesh.traverse(child => {
                        if (child.isMesh && child.material && child.material.emissiveIntensity !== undefined) {
                            child.material.emissiveIntensity = 0.0;
                        }
                    });
                }
            }

            m.tiempoRastro = (m.tiempoRastro || 0) + dt;
            if (m.tiempoRastro > 0.045) {
                m.tiempoRastro = 0;
                // Estela incandescente + humo de la roca en llamas (pool reutilizable)
                emitirParticula(false,
                    m.x + (Math.random() - 0.5) * m.radio * 0.45, (Math.random() - 0.5) * 0.35, m.z - m.radio * 0.8,
                    (Math.random() - 0.5) * 1.5, (Math.random() - 0.2) * 1.0, -m.vz * 0.16,
                    0.32 + Math.random() * 0.22, m.radio * (0.45 + Math.random() * 0.45), m.radio * 0.15,
                    Math.random() > 0.35 ? 0xff5722 : 0xffb300, 0.88, 0.5, 0);
                emitirParticula(true, m.x, 0.2, m.z - m.radio * 0.6, 0, 0.4, -m.vz * 0.35, 1.1, m.radio * 0.5, m.radio * 1.6, 0x4a3a33, 0.4, 0.6, -0.2);
            }

            const distJugador = Math.hypot(jugador.x - m.x, jugador.z - m.z);
            if (distJugador < m.radio + 1.25 && jugador.tiempoManiobra <= 0) {
                // Colisión con roca: daño masivo coherente (respeta escudo, invulnerabilidad y nave de reserva)
                // en lugar de la derrota instantánea anterior que ignoraba el Escudo Invencible
                sonarEfecto('explosion_grande');
                crearExplosion3D(m.x, m.z, 0xff3d00, 28);
                crearOndaExpansiva3D(m.x, m.z, 0xff1744, 25);
                const nx = (jugador.x - m.x) / Math.max(0.01, distJugador);
                const nz = (jugador.z - m.z) / Math.max(0.01, distJugador);
                jugador.vx += nx * 34;
                jugador.vz += nz * 34;
                grupoMundoCombate.remove(m.mesh);
                meteoros3D.splice(i, 1);
                if (estadoJuego.tiempoInvulnerable <= 0) {
                    agregarTrauma(1.0);
                    const danoRoca = Math.round(jugador.vidaMax * 0.55 + jugador.escudoMax * 0.5);
                    recibirDanoJugador(danoRoca, 'COLISION_METEORO');
                    if (estadoJuego.terminado) return;
                } else {
                    agregarTrauma(0.5);
                }
                continue;
            }

            if (m.z > 22 || Math.abs(m.x) > 28) {
                grupoMundoCombate.remove(m.mesh);
                meteoros3D.splice(i, 1);
            }
        }

        // 7. Vuelo de Aliados Cooperativos y de Escuadrón (Red + IA)
        for (let rKey in remotosRed) {
            const al = remotosRed[rKey];
            if (!al || !al.mesh) continue;

            if (al.esAliado || rKey === 'aliado_coop') {
                if (rKey === 'aliado_coop' && !al.esRemotoNet) {
                    al.x = jugador.x + 3.8 + Math.sin(elapsed * 2.2) * 0.45;
                    al.z = jugador.z + Math.cos(elapsed * 1.8) * 0.35;
                    al.mesh.position.set(al.x, 0, al.z);
                    al.mesh.rotation.z = jugador.mesh ? jugador.mesh.rotation.z * 0.85 : 0;

                    al.cooldownDisparo -= dt;
                    if (al.cooldownDisparo <= 0 && enemigos.length > 0) {
                        al.cooldownDisparo = 0.32;
                        const laser = crearMallaRayoPlasma(0x00e676, 1.5, 0.07, 0.18, true);
                        laser.position.set(al.x, 0.45, al.z - 1.2);
                        laser.rotation.y = Math.PI;
                        grupoMundoCombate.add(laser);
                        proyectiles.push({
                            mesh: laser,
                            x: al.x,
                            z: al.z - 1.2,
                            prevX: al.x,
                            prevZ: al.z - 1.2,
                            vx: 0,
                            vz: -42,
                            radioColision: 0.20,
                            tiempoVida: 0,
                            tiempoVidaMax: 2.4,
                            activo: true,
                            esEnemigo: false,
                            dano: 10
                        });
                    }
                } else if (al.esRemotoNet) {
                    al.x += (al.xMeta - al.x) * Math.min(1.0, dt * 18.0);
                    al.z += (al.zMeta - al.z) * Math.min(1.0, dt * 18.0);
                    al.mesh.position.set(al.x, 0.0, al.z);
                    al.mesh.rotation.y = 0;
                    const rollTarget = Math.max(-0.45, Math.min(0.45, -(al.x - (al.prevX || al.x)) * 14.0));
                    al.mesh.rotation.z += (rollTarget - al.mesh.rotation.z) * 8.0 * dt;
                    al.prevX = al.x;
                    al.prevZ = al.z;
                }
            }
        }

        // 8. Actualizar suministros recolectables 3D
        if (estadoJuego.modalidad && estadoJuego.modalidad.startsWith('Multijugador')) {
            temporizadorSuministroMulti -= dt;
            if (temporizadorSuministroMulti <= 0) {
                temporizadorSuministroMulti = 12.0 + Math.random() * 8.0;
                generarSuministro3D();
            }
        }

        for (let i = suministros.length - 1; i >= 0; i--) {
            const s = suministros[i];
            if (estadoJuego.modalidad !== 'TutorialInteractivo') {
                s.x += (s.vx || 0) * dt;
                s.z += (s.vz !== undefined ? s.vz : 4.5) * dt;
            }
            s.mesh.position.set(s.x, 0.2 + Math.sin(elapsed * 3.5 + i) * 0.12, s.z);
            s.mesh.rotation.y += dt * 3.2;

            if (Math.hypot(s.x - jugador.x, s.z - jugador.z) < 1.95) {
                aplicarSuministro(s.tipo);
                grupoMundoCombate.remove(s.mesh);
                suministros.splice(i, 1);
            } else if (s.z > 24 || s.z < -80 || Math.abs(s.x) > 32) {
                grupoMundoCombate.remove(s.mesh);
                suministros.splice(i, 1);
            }
        }

        // 9. Actualizar partículas y ondas expansivas 3D
        for (let i = ondasExpansivas3D.length - 1; i >= 0; i--) {
            const onda = ondasExpansivas3D[i];
            onda.vida -= dt;
            onda.escala += (onda.escalaMax - onda.escala) * 6 * dt;
            onda.mesh.scale.set(onda.escala, onda.escala, 1);
            onda.mesh.material.opacity = Math.max(0, onda.vida * 1.6);
            if (onda.vida <= 0) {
                grupoMundoCombate.remove(onda.mesh);
                liberarMallaTemporal(onda.mesh, true);
                ondasExpansivas3D.splice(i, 1);
            }
        }

        // 10. Proyectar y actualizar todas las barras de salud de enemigos en espacio de mundo
        actualizarBarrasSaludEnemigosDOM(dt);

        // 11. Verificar avance de oleada, Jefe Final o Victoria
        verificarProgresoMision(dt);
    }

    // ========================================================================
    // SISTEMA DE DISPARO ENEMIGO CON TRAYECTORIAS NORMALIZADAS Y ORIENTACIÓN
    // ========================================================================
    function dispararEnemigo3D(en) {
        const cfgNivel = CONFIG_NIVELES_CAMPANA[estadoJuego.nivel] || CONFIG_NIVELES_CAMPANA[1];
        const multDano = cfgNivel.escalaDanoEnemigo || 1.0;

        if (en.esJefe) {
            const pctVida = en.vida / Math.max(1, en.vidaMax);
            en.patronDisparo = (en.patronDisparo || 0) + 1;

            if (pctVida <= 0.35) {
                // FASE 3: MODO FURIA VOLCÁNICA (< 35% vida) — Abanico radial simétrico de velocidad constante
                estadoJuego.faseJefeActual = 3;
                const nucleo = obtenerNodosCacheados(en.mesh).nucleoJefe;
                if (nucleo && nucleo.material && nucleo.material.color) {
                    nucleo.material.color.setHex(0xff0044);
                }

                if (en.patronDisparo % 2 === 0) {
                    const numOrbes = 7;
                    const velOrbe = 21.0;
                    const maxAngulo = 0.95; // ~54 grados a cada lado hacia el jugador
                    const offsetRot = Math.sin(clock.getElapsedTime() * 1.6) * 0.12;
                    const posNucleo = transformarOffsetBoquilla(en, 0, 1.5);
                    crearDestelloBoquilla3D(posNucleo.x, posNucleo.z, 0xff0055);

                    for (let b = 0; b < numOrbes; b++) {
                        const fraccion = (b / (numOrbes - 1)) * 2 - 1; // [-1, +1]
                        const angulo = fraccion * maxAngulo + offsetRot;
                        const vx = Math.sin(angulo) * velOrbe;
                        const vz = Math.cos(angulo) * velOrbe;
                        crearProyectilEnemigo3D(posNucleo.x, posNucleo.z, vx, vz, Math.round(14 * multDano), 'orbe_plasma');
                    }
                } else {
                    const velHaz = 26.0;
                    [-2.8, 2.8].forEach(ox => {
                        const posTorreta = transformarOffsetBoquilla(en, ox, 2.0);
                        const vec = calcularVectorDisparoNormalizado(posTorreta.x, posTorreta.z, jugador.x, jugador.z, velHaz, 0);
                        crearDestelloBoquilla3D(posTorreta.x, posTorreta.z, 0xff3d00);
                        crearProyectilEnemigo3D(posTorreta.x, posTorreta.z, vec.vx, vec.vz, Math.round(16 * multDano), 'haz_pesado');
                    });
                }
            } else if (pctVida <= 0.68) {
                // FASE 2: ASALTO DE ESCOLTA Y TORRETAS OUTRIGGER (68% - 35% vida)
                estadoJuego.faseJefeActual = 2;
                if (!en.escoltaDesplegada && enemigos.length < 3) {
                    en.escoltaDesplegada = true;
                    [-8.5, 8.5].forEach((posXEsc, idxEsc) => {
                        const meshEsc = crearModeloAeronave3D('NAVE_ENEMIGO_LIGERO', 0xff5252, true, false, false);
                        meshEsc.position.set(posXEsc, 0.4, -88);
                        grupoMundoCombate.add(meshEsc);
                        enemigos.push({
                            id: contadorIdEnemigo++,
                            mesh: meshEsc,
                            modeloId: 'NAVE_ENEMIGO_LIGERO',
                            x: posXEsc,
                            z: -88,
                            prevX: posXEsc,
                            prevZ: -88,
                            yaw: 0,
                            xMeta: posXEsc,
                            zMeta: -24.0,
                            estado: 'ENTRADA',
                            tiempoEstado: 0,
                            tiempoPatrulla: idxEsc * 1.5,
                            indice: idxEsc + 10,
                            vxRepulsion: 0,
                            vzRepulsion: 0,
                            vida: 55,
                            vidaMax: 55,
                            vidaVisualLag: 55,
                            esJefe: false,
                            cooldown: 1.8,
                            nombre: 'Escolta Real Halcón'
                        });
                    });
                }

                if (en.patronDisparo % 2 === 0) {
                    const velHaz = 23.5;
                    [-2.6, 0, 2.6].forEach(ox => {
                        const posT = transformarOffsetBoquilla(en, ox, 2.0);
                        const vec = calcularVectorDisparoNormalizado(posT.x, posT.z, jugador.x, jugador.z, velHaz, 0);
                        crearDestelloBoquilla3D(posT.x, posT.z, 0xff3d00);
                        crearProyectilEnemigo3D(posT.x, posT.z, vec.vx, vec.vz, Math.round(13 * multDano), 'haz_pesado');
                    });
                } else {
                    const velOrbe = 20.0;
                    const posCentro = transformarOffsetBoquilla(en, 0, 1.8);
                    crearDestelloBoquilla3D(posCentro.x, posCentro.z, 0xff0055);
                    for (let p = 0; p < 5; p++) {
                        const spread = (p - 2) * 0.19;
                        const vec = calcularVectorDisparoNormalizado(posCentro.x, posCentro.z, jugador.x, jugador.z, velOrbe, spread);
                        crearProyectilEnemigo3D(posCentro.x, posCentro.z, vec.vx, vec.vz, Math.round(12 * multDano), 'orbe_plasma');
                    }
                }
            } else {
                // FASE 1: ATAQUE TÁCTICO INICIAL (100% - 68% vida)
                estadoJuego.faseJefeActual = 1;
                if (en.patronDisparo % 2 === 0) {
                    const velAbanico = 20.5;
                    const posProa = transformarOffsetBoquilla(en, 0, 2.0);
                    crearDestelloBoquilla3D(posProa.x, posProa.z, 0xff1744);
                    for (let p = 0; p < 5; p++) {
                        const ang = (p - 2) * 0.18;
                        const vx = Math.sin(ang) * velAbanico;
                        const vz = Math.cos(ang) * velAbanico;
                        crearProyectilEnemigo3D(posProa.x, posProa.z, vx, vz, Math.round(11 * multDano), 'laser_abanico');
                    }
                } else {
                    const velDir = 22.5;
                    [-1.2, 1.2].forEach(tx => {
                        const posT = transformarOffsetBoquilla(en, tx, 2.2);
                        const vec = calcularVectorDisparoNormalizado(posT.x, posT.z, jugador.x, jugador.z, velDir, 0);
                        crearDestelloBoquilla3D(posT.x, posT.z, 0xff1744);
                        crearProyectilEnemigo3D(posT.x, posT.z, vec.vx, vec.vz, Math.round(12 * multDano), 'laser_dirigido');
                    });
                }
            }
        } else {
            if (en.modeloId === 'NAVE_ENEMIGO_PESADO') {
                // Crucero Blindado Gólem: Cañones pesados frontales o abanico triple dirigido (si es Élite)
                const velPesado = 21.5;
                if (en.esElite) {
                    [-1.25, 0, 1.25].forEach((cx, idx) => {
                        const posB = transformarOffsetBoquilla(en, cx, 1.6);
                        const spread = (idx - 1) * 0.14;
                        const vec = calcularVectorDisparoNormalizado(posB.x, posB.z, jugador.x, jugador.z, velPesado, spread);
                        crearDestelloBoquilla3D(posB.x, posB.z, 0xffb800);
                        crearProyectilEnemigo3D(posB.x, posB.z, vec.vx, vec.vz, Math.round(11 * multDano), 'haz_pesado');
                    });
                } else {
                    [-1.15, 1.15].forEach(cx => {
                        const posB = transformarOffsetBoquilla(en, cx, 1.55);
                        const vec = calcularVectorDisparoNormalizado(posB.x, posB.z, jugador.x, jugador.z, velPesado, 0);
                        crearDestelloBoquilla3D(posB.x, posB.z, 0xff0055);
                        crearProyectilEnemigo3D(posB.x, posB.z, vec.vx, vec.vz, Math.round(11 * multDano), 'laser_pesado');
                    });
                }
            } else if (en.modeloId === 'NAVE_ENEMIGO_LIGERO') {
                // Interceptor Halcón Carmesí: Disparo rápido de precisión normalizado hacia el avión del jugador
                const velLigero = 24.5;
                const posProa = transformarOffsetBoquilla(en, 0, 1.35);
                const vec = calcularVectorDisparoNormalizado(posProa.x, posProa.z, jugador.x, jugador.z, velLigero, 0);
                crearDestelloBoquilla3D(posProa.x, posProa.z, 0xff1744);
                crearProyectilEnemigo3D(posProa.x, posProa.z, vec.vx, vec.vz, Math.round(8 * multDano), 'laser_ligero');
            } else {
                // Caza Táctico Víbora: Ráfaga doble desde las alas orientadas hacia el corredor del jugador
                const velAtaque = 21.0;
                [-0.65, 0.65].forEach(ox => {
                    const posAla = transformarOffsetBoquilla(en, ox, 1.35);
                    const vec = calcularVectorDisparoNormalizado(posAla.x, posAla.z, jugador.x + ox * 0.8, jugador.z, velAtaque, 0);
                    crearDestelloBoquilla3D(posAla.x, posAla.z, 0xff6d00);
                    crearProyectilEnemigo3D(posAla.x, posAla.z, vec.vx, vec.vz, Math.round(9 * multDano), 'laser_estandar');
                });
            }
        }
    }

    function crearProyectilEnemigo3D(x, z, vx, vz, dano, tipo = 'laser') {
        const esOrbe = tipo === 'orbe_plasma';
        let colorProy = 0xff1744;
        if (esOrbe || tipo === 'laser_pesado') colorProy = 0xff0055;
        else if (tipo === 'haz_pesado') colorProy = 0xff3d00;
        else if (tipo === 'laser_estandar') colorProy = 0xff6d00;

        const mesh = esOrbe
            ? crearMallaOrbePlasma(colorProy, 0.42)
            : crearMallaRayoPlasma(colorProy, tipo === 'haz_pesado' ? 2.4 : 1.75, 0.09, 0.24, true);

        mesh.position.set(x, 0.50, z);
        mesh.rotation.y = Math.atan2(vx, vz);
        grupoMundoCombate.add(mesh);

        proyectiles.push({
            mesh: mesh,
            x: x,
            z: z,
            prevX: x,
            prevZ: z,
            vx: vx,
            vz: vz,
            radioColision: esOrbe ? 0.36 : 0.20,
            tiempoVida: 0,
            tiempoVidaMax: 4.8,
            activo: true,
            esEnemigo: true,
            dano: dano
        });
    }

    function recibirDanoJugador(dano, motivoDerrota = null) {
        dano = Math.max(1, Math.round(dano * modDificultad().dano));
        estadoJuego.combo = 1;
        agregarTrauma(Math.min(0.6, 0.22 + dano / 120));
        estadoJuego.tiempoFlashDano = Math.min(1, 0.45 + dano / 60);
        sonarEfecto('dano_jugador');
        crearExplosion3D(jugador.x, jugador.z, 0xff3d57, 10);

        if (jugador.escudo > 0) {
            const absorbido = Math.min(jugador.escudo, Math.ceil(dano * 0.7));
            jugador.escudo -= absorbido;
            jugador.vida -= (dano - absorbido);
        } else {
            jugador.vida -= dano;
        }

        if (jugador.vida <= 0) {
            if (estadoJuego.vidasReserva > 0) {
                estadoJuego.vidasReserva--;
                estadoJuego.usoReaparicion = true;
                jugador.vida = jugador.vidaMax;
                jugador.escudo = jugador.escudoMax;
                estadoJuego.duracionMaxInvulnerable = 3.0;
                estadoJuego.tiempoInvulnerable = 3.0;
                crearOndaExpansiva3D(jugador.x, jugador.z, 0x00e676, 16);
                if (window.InterfazJuego) {
                    window.InterfazJuego.mostrarToast('¡NAVE DE RESERVA DESPLEGADA! ESCUDO AL 100%');
                }
            } else {
                jugador.vida = 0;
                sonarEfecto('explosion_grande');
                crearExplosion3D(jugador.x, jugador.z, 0xff3d00, 28);
                finalizarMision(false, motivoDerrota);
            }
        }
        actualizarHudDOM();
    }

    function aplicarSuministro(tipo) {
        if (estadoJuego.modalidad === 'TutorialInteractivo' && estadoJuego.pasoTutorial === 3) {
            estadoJuego.pasoTutorial = 4;
            estadoJuego.cargaPoder = 100;
        }

        if (tipo === 'vida') {
            sonarEfecto('item_vida');
            jugador.vida = Math.min(jugador.vidaMax, jugador.vida + 30);
            estadoJuego.tiempoEfectoReparacion = 1.1;
            estadoJuego.colorEfectoReparacion = 0x00e676;
            crearOndaExpansiva3D(jugador.x, jugador.z, 0x00e676, 9);
            if (window.InterfazJuego) window.InterfazJuego.mostrarToast('NANORREPARACIÓN: +30 INTEGRIDAD DE CASCO');
        } else if (tipo === 'escudo') {
            sonarEfecto('item_escudo');
            jugador.escudo = Math.min(jugador.escudoMax, jugador.escudo + 25);
            estadoJuego.tiempoEfectoReparacion = 1.1;
            estadoJuego.colorEfectoReparacion = 0x00e5ff;
            crearOndaExpansiva3D(jugador.x, jugador.z, 0x00e5ff, 9);
            if (window.InterfazJuego) window.InterfazJuego.mostrarToast('CELDA DE ESCUDO: +25 BARRERA DEFLECTORA');
        } else if (tipo === 'energia') {
            sonarEfecto('poder_escudo');
            estadoJuego.cargaPoder = 100;
            estadoJuego.tiempoEfectoReparacion = 1.1;
            estadoJuego.colorEfectoReparacion = 0xd500f9;
            crearOndaExpansiva3D(jugador.x, jugador.z, 0xd500f9, 10);
            if (window.InterfazJuego) window.InterfazJuego.mostrarToast('REACTOR AL 100%: ¡SUPERPODER LISTO [TECLA Q]!');
        } else if (tipo === 'monedas') {
            sonarEfecto('item');
            estadoJuego.monedasPartida += 50;
            crearOndaExpansiva3D(jugador.x, jugador.z, 0xffd54f, 8);
            if (window.InterfazJuego) window.InterfazJuego.mostrarToast('SUMINISTRO DE ORO: +50 MONEDAS TÁCTICAS');
        } else if (tipo === 'fuego') {
            sonarEfecto('item_fuego');
            estadoJuego.duracionMaxPoderActivo = 5.0;
            estadoJuego.tiempoPoderActivo = 5.0;
            crearOndaExpansiva3D(jugador.x, jugador.z, 0xffb800, 11);
            if (window.InterfazJuego) window.InterfazJuego.mostrarToast('POTENCIADOR DE CAÑONES: FUEGO TRIPLE ACTIVO [5.0s]');
        }

        actualizarHudDOM();
        actualizarChipsPotenciadoresDOM();
    }

    function verificarProgresoMision(dt = 0.016) {
        if (enemigos.length > 0 || estadoJuego.refuerzosPendientes > 0) {
            estadoJuego.temporizadorTransicionOleada = CONFIG_MOTOR.TIEMPO_TRANSICION_OLEADA_SEG;
            return;
        }

        if (estadoJuego.modalidad === 'TutorialInteractivo') {
            if (estadoJuego.pasoTutorial >= 5) {
                finalizarMision(true);
            }
            return;
        }

        if (estadoJuego.modalidad === 'CampanaCasual' || estadoJuego.modalidad === 'BatallaLibre' || estadoJuego.modalidad === 'Multijugador_Cooperativo') {
            if (estadoJuego.oleada < estadoJuego.totalOleadas || (!estadoJuego.enFaseJefe && estadoJuego.modalidad === 'CampanaCasual')) {
                if (estadoJuego.temporizadorTransicionOleada > 0) {
                    estadoJuego.temporizadorTransicionOleada -= dt;
                    return;
                }
                estadoJuego.temporizadorTransicionOleada = CONFIG_MOTOR.TIEMPO_TRANSICION_OLEADA_SEG;

                if (estadoJuego.oleada < estadoJuego.totalOleadas) {
                    desplegarOleadaEnemiga3D(estadoJuego.oleada + 1, false);
                    return;
                } else if (!estadoJuego.enFaseJefe && estadoJuego.modalidad === 'CampanaCasual') {
                    desplegarJefeFinal3D();
                    return;
                }
            }
        }

        // Breve pausa cinemática antes del informe de victoria (deja ver la explosión final)
        if (estadoJuego.temporizadorCierreVictoria == null) {
            estadoJuego.temporizadorCierreVictoria = 1.6;
            // Sector asegurado: los proyectiles hostiles restantes se desintegran (sin derrotas injustas tras ganar)
            for (let i = proyectiles.length - 1; i >= 0; i--) {
                if (!proyectiles[i].esEnemigo) continue;
                emitirParticula(false, proyectiles[i].x, 0.5, proyectiles[i].z, 0, 0, 0, 0.2, 0.6, 0.1, 0xff5252, 0.8, 0, 0);
                grupoMundoCombate.remove(proyectiles[i].mesh);
                proyectiles.splice(i, 1);
            }
            meteoros3D.forEach(m => grupoMundoCombate.remove(m.mesh));
            meteoros3D = [];
            return;
        }
        estadoJuego.temporizadorCierreVictoria -= dt;
        if (estadoJuego.temporizadorCierreVictoria > 0) return;
        finalizarMision(true);
    }

    function finalizarMision(victoria, motivoDerrota = null) {
        if (estadoJuego.terminado) return;
        estadoJuego.terminado = true;
        estadoJuego.enCurso = false;
        limpiarBarrasSaludEnemigosDOM();
        actualizarChipsPotenciadoresDOM();

        let estrellas = 0;
        if (victoria) {
            estrellas = 1;
            if (jugador.vida >= jugador.vidaMax * 0.5) estrellas++;
            if (!estadoJuego.usoReaparicion && estadoJuego.comboMax >= 3) estrellas++;
        }

        if (window.InterfazJuego && window.InterfazJuego.alTerminarPartida) {
            window.InterfazJuego.alTerminarPartida({
                modalidad: estadoJuego.modalidad,
                nivelCampana: estadoJuego.nivel,
                victoria: victoria,
                motivoDerrota: motivoDerrota,
                bajas: estadoJuego.bajas,
                jefeDerrotado: victoria && estadoJuego.enFaseJefe,
                disparos: estadoJuego.disparos,
                impactos: estadoJuego.impactos,
                comboMax: estadoJuego.comboMax,
                puntaje: estadoJuego.puntos,
                estrellas: estrellas,
                monedasExtra: estadoJuego.monedasPartida,
                tiempoMisionSegundos: Math.round(estadoJuego.tiempoMision || 0)
            });
        }
    }

    function formatearTiempoMMSS(segundosTotales) {
        const seg = Math.max(0, Math.floor(segundosTotales || 0));
        const m = Math.floor(seg / 60);
        const s = seg % 60;
        return `${m < 10 ? '0' + m : m}:${s < 10 ? '0' + s : s}`;
    }

    // ========================================================================
    // SINCRONIZACIÓN DEL HUD Y ENTRADA DE USUARIO
    // ========================================================================
    function actualizarHudDOM() {
        const elVida = document.getElementById('hud-fill-vida');
        const elTxtVida = document.getElementById('hud-txt-vida');
        const elEscudo = document.getElementById('hud-fill-escudo');
        const elTxtEscudo = document.getElementById('hud-txt-escudo');
        const elVidasExtra = document.getElementById('hud-txt-vidas-extra');
        const elPuntos = document.getElementById('hud-txt-puntos');
        const elCombo = document.getElementById('hud-txt-combo');
        const elMonedas = document.getElementById('hud-txt-monedas');
        const elMision = document.getElementById('hud-badge-mision');
        const elPoderFill = document.getElementById('hud-fill-poder');
        const elPoderTxt = document.getElementById('hud-txt-poder');
        const elCajaJefe = document.getElementById('hud-caja-jefe');
        const elJefeNombre = document.getElementById('hud-nombre-jefe');
        const elJefeFill = document.getElementById('hud-fill-jefe');
        const elBannerTut = document.getElementById('hud-banner-tutorial');

        if (!elVida) return;

        const pctVida = Math.max(0, Math.min(100, Math.round((jugador.vida / jugador.vidaMax) * 100)));
        const pctEsc = Math.max(0, Math.min(100, Math.round((jugador.escudo / jugador.escudoMax) * 100)));
        elVida.style.width = pctVida + '%';
        elTxtVida.textContent = pctVida + '%';
        elEscudo.style.width = pctEsc + '%';
        elTxtEscudo.textContent = pctEsc + '%';
        elVidasExtra.textContent = 'RESERVA: ' + estadoJuego.vidasReserva;

        elPuntos.textContent = estadoJuego.puntos.toLocaleString();
        elCombo.textContent = 'x' + estadoJuego.combo;
        const icoMon = window.IconosJuego ? window.IconosJuego.obtener('moneda', 'ico-inline') : '';
        elMonedas.innerHTML = `${icoMon} +${estadoJuego.monedasPartida}`;

        elPoderFill.style.width = Math.round(estadoJuego.cargaPoder) + '%';
        const icoPod = window.IconosJuego ? window.IconosJuego.obtener('poder', 'ico-inline') : '';
        elPoderTxt.innerHTML = estadoJuego.cargaPoder >= 100
            ? `${icoPod} ¡SUPERPODER LISTO! [TECLA Q]`
            : `${icoPod} CARGANDO PODER: ${Math.round(estadoJuego.cargaPoder)}%`;

        const txtReloj = formatearTiempoMMSS(estadoJuego.tiempoMision);

        if (estadoJuego.modalidad === 'TutorialInteractivo') {
            elMision.textContent = 'TUTORIAL PASO ' + estadoJuego.pasoTutorial + ' DE 5';
            elBannerTut.classList.remove('oculta');
            const mensajesTut = {
                1: 'Paso 1: Usa las Flechas o las teclas W, A, S, D para pilotar tu avión 3D.',
                2: 'Paso 2: Presiona ESPACIO o Haz Clic Izquierdo 5 veces para disparar tus cañones.',
                3: 'Paso 3: Vuela hacia el cristal brillante en el centro para recoger suministros.',
                4: 'Paso 4: ¡Tu Superpoder está al 100%! Presiona la tecla Q o Clic Derecho para activarlo.',
                5: 'Paso 5: ¡Excelente! Derriba las naves de prueba. Truco: SHIFT hace un tonel que esquiva los disparos.'
            };
            elBannerTut.textContent = mensajesTut[estadoJuego.pasoTutorial] || '';
        } else if (estadoJuego.modalidad === 'Multijugador_1v1' || estadoJuego.modalidad === 'Multijugador_2v2') {
            elBannerTut.classList.add('oculta');
            elMision.textContent = `DUELO MULTIJUGADOR 3D · ${txtReloj}`;
        } else if (estadoJuego.modalidad === 'Multijugador_Cooperativo') {
            elBannerTut.classList.add('oculta');
            elMision.textContent = `COOPERATIVO 3D · OLEADA ${estadoJuego.oleada}/${estadoJuego.totalOleadas} · ${txtReloj}`;
        } else {
            elBannerTut.classList.add('oculta');
            if (estadoJuego.enFaseJefe) {
                elMision.textContent = `MISIÓN ${estadoJuego.nivel} · ¡ALERTA DE JEFE FINAL (FASE ${estadoJuego.faseJefeActual})! · ${txtReloj}`;
            } else {
                elMision.textContent = `MISIÓN ${estadoJuego.nivel} · OLEADA ${estadoJuego.oleada} DE ${estadoJuego.totalOleadas} · ${txtReloj}`;
            }
        }

        const jefe = enemigos.find(e => e.esJefe || e.esRivalMultijugador);
        if (jefe) {
            elCajaJefe.classList.remove('oculta');
            const icoJefe = window.IconosJuego ? window.IconosJuego.obtener(jefe.esRivalMultijugador ? 'multijugador' : 'jefe', 'ico-inline') : '';
            elJefeNombre.innerHTML = `${icoJefe} ${jefe.nombre}`;
            elJefeFill.style.width = Math.max(0, Math.min(100, (jefe.vida / jefe.vidaMax) * 100)) + '%';
        } else {
            elCajaJefe.classList.add('oculta');
        }
    }

    function configurarEventosEntrada() {
        window.addEventListener('keydown', e => {
            teclas[e.code] = true;
            if (e.repeat) return;
            if (e.code === 'KeyQ' || e.code === 'KeyE') {
                activarSuperpoderJugador();
            } else if (e.code === 'ShiftLeft' || e.code === 'ShiftRight' || e.code === 'KeyF') {
                ejecutarManiobraEvasiva();
            } else if (e.code === 'KeyP' || e.code === 'Escape') {
                if (modoEscena === 'COMBATE' && !estadoJuego.terminado) {
                    alternarPausa();
                }
            }
            // Evitar que Espacio/flechas desplacen la página o activen botones enfocados durante el combate
            if (modoEscena === 'COMBATE' && estadoJuego.enCurso && ['Space', 'ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight'].includes(e.code)) {
                e.preventDefault();
            }
        });

        window.addEventListener('keyup', e => {
            teclas[e.code] = false;
        });

        // Pausa automática al perder el foco (Alt+Tab, minimizar): estándar de juegos comerciales
        const pausarPorFoco = () => {
            Object.keys(teclas).forEach(k => { teclas[k] = false; });
            ratonDisparando = false;
            if (modoEscena === 'COMBATE' && estadoJuego.enCurso && !estadoJuego.pausado && !estadoJuego.terminado) {
                alternarPausa();
            }
        };
        window.addEventListener('blur', pausarPorFoco);
        document.addEventListener('visibilitychange', () => { if (document.hidden) pausarPorFoco(); });

        window.addEventListener('mousemove', e => {
            ratonX = (e.clientX / window.innerWidth) * 2 - 1;
            ratonY = (e.clientY / window.innerHeight) * 2 - 1;
        });

        window.addEventListener('mousedown', e => {
            if (modoEscena !== 'COMBATE' || estadoJuego.pausado || estadoJuego.terminado) return;
            if (e.target && e.target.tagName === 'CANVAS') {
                if (e.button === 0) ratonDisparando = true;
                if (e.button === 2) activarSuperpoderJugador();
            }
        });

        window.addEventListener('mouseup', () => {
            ratonDisparando = false;
        });

        window.addEventListener('contextmenu', e => e.preventDefault());
    }

    // Botones del mando con detección de flanco (una acción por pulsación)
    function procesarBotonesMando() {
        if (!mando.conectado) {
            mandoPrevio.poder = mandoPrevio.maniobra = mandoPrevio.pausa = false;
            return;
        }
        if (mando.poder && !mandoPrevio.poder) activarSuperpoderJugador();
        if (mando.maniobra && !mandoPrevio.maniobra) ejecutarManiobraEvasiva();
        if (mando.pausa && !mandoPrevio.pausa && !estadoJuego.terminado) alternarPausa();
        mandoPrevio.poder = mando.poder;
        mandoPrevio.maniobra = mando.maniobra;
        mandoPrevio.pausa = mando.pausa;
    }

    function alternarPausa() {
        estadoJuego.pausado = !estadoJuego.pausado;
        if (window.InterfazJuego && window.InterfazJuego.mostrarOverlayPausa) {
            window.InterfazJuego.mostrarOverlayPausa(estadoJuego.pausado, estadoJuego.checkpointJefe);
        }
    }

    function sincronizarRed(paquetes, modalidad) {
        if (!paquetes || !paquetes.length || !estadoJuego.enCurso) return;
        const nombreLocal = (window.InterfazJuego && window.InterfazJuego.obtenerNombrePiloto) ? window.InterfazJuego.obtenerNombrePiloto() : '';
        const modActual = modalidad || estadoJuego.modalidad || 'Multijugador_1v1';
        const esDuelo = (modActual === 'Multijugador_1v1' || modActual === 'Multijugador_2v2');

        for (let i = 0; i < paquetes.length; i++) {
            const p = paquetes[i];
            if (!p || !p.id) continue;
            if (p.piloto === nombreLocal) continue;

            const esRival = esDuelo || (p.equipo && p.equipo !== 1);
            const posX = esRival ? -p.x : p.x;
            const posZ = esRival ? (-p.z - 26.0) : p.z;

            let rem = remotosRed[p.id];
            if (!rem) {
                const modelo = esRival ? 'NAVE_F35' : 'NAVE_CONDOR_X';
                const color = esRival ? 0xff1744 : 0x00e676;
                const mesh = crearModeloAeronave3D(modelo, color, esRival, false, false);
                mesh.position.set(posX, esRival ? 0.4 : 0.0, posZ);
                if (esRival) {
                    mesh.rotation.y = Math.PI;
                }
                grupoMundoCombate.add(mesh);

                rem = {
                    id: contadorIdEnemigo++,
                    idRed: p.id,
                    nombre: p.piloto || (esRival ? 'Rival en Red' : 'Aliado en Red'),
                    mesh: mesh,
                    modeloId: modelo,
                    x: posX,
                    z: posZ,
                    prevX: posX,
                    prevZ: posZ,
                    xMeta: posX,
                    zMeta: posZ,
                    yaw: esRival ? Math.PI : 0,
                    vida: p.vida || 100,
                    vidaMax: 100,
                    vidaVisualLag: p.vida || 100,
                    esRival: esRival,
                    esRemotoNet: true,
                    esJefe: false,
                    esAliado: !esRival,
                    cooldown: 0
                };
                remotosRed[p.id] = rem;

                if (esRival) {
                    enemigos.push(rem);
                    sonarEfecto('peligro');
                } else {
                    sonarEfecto('item');
                }
            } else {
                rem.xMeta = posX;
                rem.zMeta = posZ;
                rem.vida = p.vida;
            }

            if (p.disparo || p.tipo === 'FIRE') {
                const dirVz = esRival ? 42 : -42;
                const laser = crearMallaRayoPlasma(esRival ? 0xff1744 : 0x00e676, 1.5, 0.07, 0.18, true);
                laser.position.set(rem.x, 0.45, rem.z + (esRival ? 1.2 : -1.2));
                laser.rotation.y = esRival ? 0 : Math.PI;
                grupoMundoCombate.add(laser);

                proyectiles.push({
                    mesh: laser,
                    x: rem.x,
                    z: rem.z + (esRival ? 1.2 : -1.2),
                    prevX: rem.x,
                    prevZ: rem.z + (esRival ? 1.2 : -1.2),
                    vx: 0,
                    vz: dirVz,
                    radioColision: 0.22,
                    tiempoVida: 0,
                    tiempoVidaMax: 2.5,
                    activo: true,
                    esEnemigo: esRival,
                    dano: 12
                });
                sonarEfecto('laser_enemigo');
            }

            if (p.tipo === 'DISCONNECT') {
                if (rem.mesh) grupoMundoCombate.remove(rem.mesh);
                if (rem.elBarraDOM) rem.elBarraDOM.remove();
                delete remotosRed[p.id];
                enemigos = enemigos.filter(e => e.idRed !== p.id);
            }
        }
    }

    return {
        inicializar,
        activarModoHangar,
        actualizarNaveHangar3D,
        iniciarPartida3D,
        sincronizarRed,
        desplegarMeteoro3D,
        activarSuperpoderJugador,
        alternarPausa,
        sonarEfecto,
        posicionarJugador3D: function (x, z = 2.0) {
            jugador.x = Math.max(-CONFIG_MOTOR.LIMITE_X_JUGADOR, Math.min(CONFIG_MOTOR.LIMITE_X_JUGADOR, x));
            jugador.z = Math.max(CONFIG_MOTOR.LIMITE_Z_MIN_JUGADOR, Math.min(CONFIG_MOTOR.LIMITE_Z_MAX_JUGADOR, z));
            if (jugador.mesh) {
                jugador.mesh.position.set(jugador.x, 0, jugador.z);
            }
            jugador.vx = 0;
            jugador.vz = 0;
            reiniciarCamaraCombate();
            actualizarBarrasSaludEnemigosDOM(0.016);
        },
        obtenerTelemetriaCombate: function () {
            const elevDebajo = calcularElevacionMundo(jugador.x, jugador.z, estadoJuego.nivel);
            return {
                nivel: estadoJuego.nivel,
                oleada: estadoJuego.oleada,
                totalOleadas: estadoJuego.totalOleadas,
                duracionObjetivoSegundos: estadoJuego.duracionObjetivoMision,
                enemigosActivos: enemigos.length,
                barrasSaludRenderizadas: enemigos.filter(e => !!e.elBarraDOM).length,
                proyectilesActivos: proyectiles.length,
                particulasActivas: poolChispas.items.filter(p => p.activo).length + poolHumo.items.filter(p => p.activo).length,
                fps: Math.round(fpsPromedio),
                jugadorVx: Number(jugador.vx.toFixed(2)),
                jugadorVz: Number(jugador.vz.toFixed(2)),
                perfilVuelo: jugador.modeloId,
                dificultad: dificultadActual,
                calidadGrafica: calidadGrafica,
                llamadasDibujo: renderer ? renderer.info.render.calls : 0,
                jugadorX: jugador.x,
                jugadorY: 0.0,
                jugadorZ: jugador.z,
                elevacionTerrenoBajoAvion: Number(elevDebajo.toFixed(3)),
                margenVerticalLibre: Number((0.0 - elevDebajo).toFixed(3)),
                avionVisibleSinSuperposicion: elevDebajo <= CONFIG_MOTOR.TECHO_MAXIMO_VALLE_Y
            };
        },
        configurarPreferencias: function (volEfectos, silenciado, controlRaton, extras) {
            volumenEfectos = volEfectos;
            audioSilenciado = silenciado;
            usarRatonVuelo = controlRaton;
            if (extras) {
                if (MODIFICADORES_DIFICULTAD[extras.dificultad]) dificultadActual = extras.dificultad;
                if (typeof extras.intensidadSacudida === 'number') intensidadSacudida = Math.max(0, Math.min(1.5, extras.intensidadSacudida));
                if (['alta', 'media', 'baja'].includes(extras.calidadGrafica) && extras.calidadGrafica !== calidadGrafica) {
                    calidadGrafica = extras.calidadGrafica;
                    aplicarCalidadGrafica();
                }
            }
            if (audioSilenciado) actualizarMotorAudio(false, 0, false);
        },
        ejecutarManiobraEvasiva,
        reintentarMision: function (desdeCheckpoint) {
            if (!desdeCheckpoint) estadoJuego.checkpointJefe = false;
            if (window.InterfazJuego) window.InterfazJuego.relanzarMisionActual();
        },
        avanzarSiguienteNivel: function () {
            estadoJuego.checkpointJefe = false;
            estadoJuego.nivel = Math.min(10, estadoJuego.nivel + 1);
            if (window.InterfazJuego) window.InterfazJuego.relanzarMisionActual(estadoJuego.nivel);
        },
        obtenerEstadoModelosGLTF: function () {
            return {
                cargadorExiste: !!cargadorGLTF,
                modelosEnCache: Object.keys(cacheModelosGLTF),
                rutas: rutasModelosGLB
            };
        }
    };
})();
