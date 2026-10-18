// ==============================================================================
// PROYECTO: CÓNDOR TÁCTICO (SIMULADOR DE COMBATE AÉREO 2D)
// AUTORES:
//   - RONAL ABELIN LARICO CCASO
//   - CLEVER NANDOR MAMANI HUALLPA
//   - JHON YOSSEP HINOJOSA APAZA
//   - JHON ALEX HUARACALLO SUCARI
//   - HECTOR CARLOS TORRES FLORES
// INSTITUCION: UNIVERSIDAD NACIONAL DE JULIACA (UNAJ)
// VERSION: 4.0.0
// ==============================================================================

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace CondorTactico
{
    /// <summary>
    /// Modalidades principales de ejecución del combate táctico.
    /// </summary>
    public enum ModalidadPartida
    {
        CampanaCasual = 0,
        BatallaLibre = 1,
        TutorialInteractivo = 2,
        Multijugador_1v1 = 3,
        Multijugador_2v2 = 4,
        Multijugador_Cooperativo = 5
    }

    /// <summary>
    /// Categorías de artículos disponibles en la tienda y el inventario.
    /// </summary>
    public enum CategoriaArticulo
    {
        Aeronave = 0,
        Apariencia = 1,
        MejoraPermanente = 2,
        PoderEspecial = 3
    }

    /// <summary>
    /// Tipos de poderes especiales activables en vuelo con la tecla [Q] o [E].
    /// </summary>
    public enum TipoPoderEspecial
    {
        PulsoElectromagnetico_EMP = 0,
        EscudoCuanticoSobrecarga = 1,
        SobrecargaVulcan_Overdrive = 2,
        SalvaTermobarica = 3
    }

    /// <summary>
    /// Tipos de peligros ambientales dinámicos según el escenario del nivel.
    /// </summary>
    public enum TipoPeligroAmbiental
    {
        VientoCruzadoAndino = 0,
        TormentaElectrica = 1,
        GranizoCriogenico = 2,
        MeteoritosPiroclasticos = 3,
        LaserOrbitalSatelital = 4
    }

    /// <summary>
    /// Tipos de suministros recolectables (Power-Ups) que sueltan las naves derribadas.
    /// </summary>
    public enum TipoSuministro
    {
        ReparacionCasco = 0,
        RecargaEscudo = 1,
        EnergiaHabilidad = 2,
        CreditosExtra = 3,
        PotenciadorDano = 4
    }

    /// <summary>
    /// Configuración persistente de audio, video, controles, idioma y accesibilidad.
    /// </summary>
    public sealed class ConfiguracionJuego
    {
        // Audio
        public int VolumenGeneral { get; set; } = 85;
        public int VolumenMusica { get; set; } = 75;
        public int VolumenEfectos { get; set; } = 90;
        public bool AudioSilenciado { get; set; } = false;

        // Video y Gráficos
        public bool PantallaCompleta { get; set; } = false;
        public int IndiceResolucion { get; set; } = 1; // 0: 1020x680, 1: 1280x720, 2: 1440x810, 3: 1600x900
        public int CalidadGrafica { get; set; } = 2;   // 0: Baja, 1: Media, 2: Ultra 4x SSAA
        public bool MostrarHudMilitar { get; set; } = true;
        public bool MostrarTelemetria { get; set; } = false;

        // Controles
        public int EsquemaControl { get; set; } = 0;   // 0: Híbrido (WASD+Flechas), 1: Solo WASD, 2: Solo Flechas, 3: Seguimiento Ratón
        public int SensibilidadVuelo { get; set; } = 100; // 50% a 150%
        public Keys TeclaDisparo { get; set; } = Keys.Space;
        public Keys TeclaPoderEspecial { get; set; } = Keys.Q;
        public Keys TeclaPausa { get; set; } = Keys.P;

        // Accesibilidad e Idioma
        public string Idioma { get; set; } = "ES";     // "ES" o "EN"
        public int IntensidadSacudidaCamara { get; set; } = 100; // 0% a 100%
        public bool SubtitulosAlertasVisuales { get; set; } = true;
        public bool ModoAltoContraste { get; set; } = false;

        public void RestaurarValoresPredeterminados()
        {
            VolumenGeneral = 85;
            VolumenMusica = 75;
            VolumenEfectos = 90;
            AudioSilenciado = false;
            PantallaCompleta = false;
            IndiceResolucion = 1;
            CalidadGrafica = 2;
            MostrarHudMilitar = true;
            MostrarTelemetria = false;
            EsquemaControl = 0;
            SensibilidadVuelo = 100;
            TeclaDisparo = Keys.Space;
            TeclaPoderEspecial = Keys.Q;
            TeclaPausa = Keys.P;
            Idioma = "ES";
            IntensidadSacudidaCamara = 100;
            SubtitulosAlertasVisuales = true;
            ModoAltoContraste = false;
        }

        public Size ObtenerTamanoResolucion()
        {
            return IndiceResolucion switch
            {
                0 => new Size(1020, 680),
                1 => new Size(1280, 740),
                2 => new Size(1440, 820),
                3 => new Size(1600, 900),
                _ => new Size(1280, 740)
            };
        }
    }

    /// <summary>
    /// Definición inmutable de un artículo del catálogo de la tienda.
    /// </summary>
    public sealed class ArticuloCatalogo
    {
        public string Id { get; init; } = "";
        public string Nombre { get; init; } = "";
        public string Descripcion { get; init; } = "";
        public CategoriaArticulo Categoria { get; init; }
        public int PrecioBase { get; init; }
        public int NivelMaximo { get; init; } = 1;
        public Color ColorRepresentativo { get; init; } = Color.Cyan;
    }

    /// <summary>
    /// Definición de un logro verificable dentro del juego.
    /// </summary>
    public sealed class DefinicionLogro
    {
        public string Id { get; init; } = "";
        public string Titulo { get; init; } = "";
        public string Descripcion { get; init; } = "";
        public int RecompensaMonedas { get; init; }
        public int RecompensaXP { get; init; }
    }

    /// <summary>
    /// Definición de un nivel del Modo Casual (Campaña Progresiva).
    /// </summary>
    public sealed class DefinicionNivelCampana
    {
        public int NumeroNivel { get; init; }
        public string CodigoOperacion { get; init; } = "";
        public string NombreEscenario { get; init; } = "";
        public string DescripcionMision { get; init; } = "";
        public string ObjetivoPrincipal { get; init; } = "";
        public string ObjetivoSecundario { get; init; } = "";
        public string NombreJefeFinal { get; init; } = "";
        public TipoAeronave ModeloJefe { get; init; }
        public int VidaJefe { get; init; }
        public int CantidadOleadasPrevias { get; init; }
        public TipoPeligroAmbiental PeligroAmbiental { get; init; }
        public Color ColorCieloSuperior { get; init; }
        public Color ColorCieloInferior { get; init; }
        public int RecompensaMonedasBase { get; init; }
        public int RecompensaXPBase { get; init; }
    }

    /// <summary>
    /// Datos persistentes del perfil del jugador, economía, inventario, logros y estadísticas.
    /// </summary>
    public sealed class PerfilJugador
    {
        public string NombrePiloto { get; set; } = "CÓNDOR-01";
        public int Monedas { get; set; } = 500;
        public int ExperienciaTotal { get; set; } = 0;
        public int NivelMaximoCampanaDesbloqueado { get; set; } = 1;
        public bool TutorialCompletado { get; set; } = false;

        // Equipamiento activo en el Inventario
        public string AeronaveEquipadaId { get; set; } = "NAVE_F35";
        public string SkinEquipadaId { get; set; } = "SKIN_ESTANDAR";
        public string PoderEquipadoId { get; set; } = "PODER_EMP";

        // Colecciones de propiedad e inventario
        public HashSet<string> ArticulosComprados { get; set; } = new HashSet<string>
        {
            "NAVE_F35",
            "NAVE_B2",
            "SKIN_ESTANDAR",
            "PODER_EMP"
        };

        public Dictionary<string, int> NivelesMejoras { get; set; } = new Dictionary<string, int>
        {
            ["MEJORA_BLINDAJE"] = 0,
            ["MEJORA_ESCUDO"] = 0,
            ["MEJORA_MOTOR"] = 0,
            ["MEJORA_CANON"] = 0,
            ["MEJORA_ECONOMIA"] = 0
        };

        public Dictionary<int, int> EstrellasPorNivel { get; set; } = new Dictionary<int, int>();
        public Dictionary<int, int> MejorPuntajePorNivel { get; set; } = new Dictionary<int, int>();
        public HashSet<string> LogrosDesbloqueados { get; set; } = new HashSet<string>();

        // Recompensas diarias
        public string UltimaFechaRecompensaDiaria { get; set; } = "";
        public int RachaDiasRecompensa { get; set; } = 0;

        // Estadísticas históricas
        public int PartidasJugadas { get; set; } = 0;
        public int VictoriasTotales { get; set; } = 0;
        public int DerrotasTotales { get; set; } = 0;
        public int BajasTotales { get; set; } = 0;
        public int JefesDerrotados { get; set; } = 0;
        public int DisparosTotales { get; set; } = 0;
        public int ImpactosTotales { get; set; } = 0;
        public int ComboMaximoHistorico { get; set; } = 0;
        public int MonedasAcumuladasHistorico { get; set; } = 500;
        public int PartidasMultijugadorJugadas { get; set; } = 0;
        public int VictoriasMultijugador { get; set; } = 0;

        // Cálculos derivados de progresión
        public int CalcularNivelJugador()
        {
            return 1 + (int)Math.Floor(Math.Sqrt(Math.Max(0, ExperienciaTotal) / 120.0));
        }

        public int ObtenerXPInicioNivelActual()
        {
            int n = CalcularNivelJugador() - 1;
            return n * n * 120;
        }

        public int ObtenerXPParaSiguienteNivel()
        {
            int n = CalcularNivelJugador();
            return n * n * 120;
        }

        public string ObtenerNombreRangoMilitar()
        {
            int nivel = CalcularNivelJugador();
            return nivel switch
            {
                1 => "Cadete de Vuelo",
                2 => "Alférez Táctico",
                3 => "Teniente de Escuadrilla",
                4 => "Capitán Interceptor",
                5 => "Mayor de Combate",
                6 => "Comandante Aéreo",
                7 => "Coronel de los Andes",
                8 => "Brigadier Estratosférico",
                _ => "Gran Mariscal Cóndor"
            };
        }

        public int ObtenerNivelMejora(string idMejora)
        {
            return NivelesMejoras.TryGetValue(idMejora, out int niv) ? Math.Clamp(niv, 0, 5) : 0;
        }
    }

    /// <summary>
    /// Gestor centralizado (Singleton Thread-Safe) de economía, inventario, niveles, configuración y persistencia.
    /// </summary>
    public static class GestorEstadoGlobal
    {
        private static readonly object _candado = new object();
        private static readonly string _directorioDatos = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CondorTacticoUNAJ");
        private static readonly string _rutaPerfil = Path.Combine(_directorioDatos, "perfil_jugador.json");
        private static readonly string _rutaConfig = Path.Combine(_directorioDatos, "configuracion.json");

        public static PerfilJugador Perfil { get; private set; } = new PerfilJugador();
        public static ConfiguracionJuego Configuracion { get; private set; } = new ConfiguracionJuego();

        public static readonly IReadOnlyList<ArticuloCatalogo> CatalogoTienda = new List<ArticuloCatalogo>
        {
            // Aeronaves
            new ArticuloCatalogo
            {
                Id = "NAVE_F35",
                Nombre = "F-35A Lightning II",
                Descripcion = "Caza polivalente supersónico de 5ª generación con alta cadencia AIM-120 y agilidad superior.",
                Categoria = CategoriaArticulo.Aeronave,
                PrecioBase = 0,
                ColorRepresentativo = Color.FromArgb(0, 195, 255)
            },
            new ArticuloCatalogo
            {
                Id = "NAVE_B2",
                Nombre = "B-2 Spirit AF 82-1071",
                Descripcion = "Bombardero estratégico furtivo con doble salva simultánea AGM-158 y escudo de absorción RAM.",
                Categoria = CategoriaArticulo.Aeronave,
                PrecioBase = 0,
                ColorRepresentativo = Color.FromArgb(80, 220, 180)
            },
            new ArticuloCatalogo
            {
                Id = "NAVE_CONDOR_X",
                Nombre = "Prototipo Cóndor X-UNAJ",
                Descripcion = "Variante de élite andina: combina blindaje reforzado (+8 HP), postquemador y cañón acelerado.",
                Categoria = CategoriaArticulo.Aeronave,
                PrecioBase = 950,
                ColorRepresentativo = Color.FromArgb(255, 200, 50)
            },

            // Apariencias / Skins
            new ArticuloCatalogo
            {
                Id = "SKIN_ESTANDAR",
                Nombre = "Gris Táctico OTAN",
                Descripcion = "Recubrimiento militar estándar de baja observabilidad radar.",
                Categoria = CategoriaArticulo.Apariencia,
                PrecioBase = 0,
                ColorRepresentativo = Color.FromArgb(140, 160, 185)
            },
            new ArticuloCatalogo
            {
                Id = "SKIN_FURTIVO_NOCTURNO",
                Nombre = "Obsidiana Nocturna",
                Descripcion = "Polímero negro abisal con estelas de plasma violeta.",
                Categoria = CategoriaArticulo.Apariencia,
                PrecioBase = 300,
                ColorRepresentativo = Color.FromArgb(150, 80, 255)
            },
            new ArticuloCatalogo
            {
                Id = "SKIN_ARTICO_ANDINO",
                Nombre = "Glaciar Altiplánico",
                Descripcion = "Camuflaje criogénico blanco-cian inspirado en la cordillera nevada.",
                Categoria = CategoriaArticulo.Apariencia,
                PrecioBase = 400,
                ColorRepresentativo = Color.FromArgb(90, 240, 255)
            },
            new ArticuloCatalogo
            {
                Id = "SKIN_CARMESI",
                Nombre = "Escuadrón Fénix Carmesí",
                Descripcion = "Aleación anodizada roja de interceptores de alta velocidad.",
                Categoria = CategoriaArticulo.Apariencia,
                PrecioBase = 550,
                ColorRepresentativo = Color.FromArgb(255, 70, 85)
            },
            new ArticuloCatalogo
            {
                Id = "SKIN_ORO_IMPERIAL",
                Nombre = "Sol de Oro Inca",
                Descripcion = "Revestimiento dorado reflectante de prestigio para comandantes veteranos.",
                Categoria = CategoriaArticulo.Apariencia,
                PrecioBase = 1200,
                ColorRepresentativo = Color.FromArgb(255, 215, 0)
            },

            // Mejoras Permanentes (Niveles 1 a 5)
            new ArticuloCatalogo
            {
                Id = "MEJORA_BLINDAJE",
                Nombre = "Fuselaje de Titanio-Carbono",
                Descripcion = "Incrementa la salud máxima estructural de tu aeronave en +6 HP por nivel.",
                Categoria = CategoriaArticulo.MejoraPermanente,
                PrecioBase = 200,
                NivelMaximo = 5,
                ColorRepresentativo = Color.FromArgb(80, 230, 120)
            },
            new ArticuloCatalogo
            {
                Id = "MEJORA_ESCUDO",
                Nombre = "Celdas de Escudo RAM",
                Descripcion = "Aumenta la capacidad del escudo deflector en +4 puntos por nivel.",
                Categoria = CategoriaArticulo.MejoraPermanente,
                PrecioBase = 220,
                NivelMaximo = 5,
                ColorRepresentativo = Color.FromArgb(60, 180, 255)
            },
            new ArticuloCatalogo
            {
                Id = "MEJORA_MOTOR",
                Nombre = "Toberas Vectoriales Mach-3",
                Descripcion = "Aumenta la velocidad de maniobra y desplazamiento en +8% por nivel.",
                Categoria = CategoriaArticulo.MejoraPermanente,
                PrecioBase = 240,
                NivelMaximo = 5,
                ColorRepresentativo = Color.FromArgb(255, 165, 40)
            },
            new ArticuloCatalogo
            {
                Id = "MEJORA_CANON",
                Nombre = "Acelerador Balístico Vulcan",
                Descripcion = "Incrementa la velocidad de proyectiles y reduce el enfriamiento de disparo.",
                Categoria = CategoriaArticulo.MejoraPermanente,
                PrecioBase = 260,
                NivelMaximo = 5,
                ColorRepresentativo = Color.FromArgb(255, 95, 95)
            },
            new ArticuloCatalogo
            {
                Id = "MEJORA_ECONOMIA",
                Nombre = "Radar de Telemetría Financiera",
                Descripcion = "Otorga +15% adicional de Créditos Tácticos y XP al finalizar cada misión.",
                Categoria = CategoriaArticulo.MejoraPermanente,
                PrecioBase = 250,
                NivelMaximo = 5,
                ColorRepresentativo = Color.FromArgb(255, 225, 80)
            },

            // Poderes Especiales Activos (Tecla Q / E)
            new ArticuloCatalogo
            {
                Id = "PODER_EMP",
                Nombre = "Pulso Electromagnético (EMP)",
                Descripcion = "[Tecla Q] Desintegra todos los proyectiles enemigos en pantalla y paraliza rivales por 3.5 s.",
                Categoria = CategoriaArticulo.PoderEspecial,
                PrecioBase = 0,
                ColorRepresentativo = Color.FromArgb(0, 230, 255)
            },
            new ArticuloCatalogo
            {
                Id = "PODER_ESCUDO",
                Nombre = "Cúpula Cuántica Invulnerable",
                Descripcion = "[Tecla Q] Otorga invulnerabilidad total durante 4.5 s y regenera +8 puntos de escudo RAM.",
                Categoria = CategoriaArticulo.PoderEspecial,
                PrecioBase = 450,
                ColorRepresentativo = Color.FromArgb(100, 255, 180)
            },
            new ArticuloCatalogo
            {
                Id = "PODER_OVERDRIVE",
                Nombre = "Frenesí Hiper-Vulcan",
                Descripcion = "[Tecla Q] Duplica la cadencia de fuego y dispara salvas triples durante 5.5 s.",
                Categoria = CategoriaArticulo.PoderEspecial,
                PrecioBase = 650,
                ColorRepresentativo = Color.FromArgb(255, 130, 40)
            },
            new ArticuloCatalogo
            {
                Id = "PODER_TERMOBARICO",
                Nombre = "Onda Termobárica Andina",
                Descripcion = "[Tecla Q] Detona una onda expansiva que inflige 18 pts de daño directo a todos los hostiles.",
                Categoria = CategoriaArticulo.PoderEspecial,
                PrecioBase = 850,
                ColorRepresentativo = Color.FromArgb(255, 70, 120)
            }
        };

        public static readonly IReadOnlyList<DefinicionNivelCampana> NivelesCampana = new List<DefinicionNivelCampana>
        {
            // MAPA 1: VALLE DEL TITICACA (NIVELES 1 Y 2)
            new DefinicionNivelCampana
            {
                NumeroNivel = 1,
                CodigoOperacion = "OP-01: FRONTERA ALTIPLÁNICA",
                NombreEscenario = "Cumbres de Juliaca y Lago Titicaca",
                DescripcionMision = "Intercepta las patrullas de reconocimiento invasoras que han violado el espacio aéreo surandino.",
                ObjetivoPrincipal = "Derrota 2 oleadas de escoltas y al Comandante Kuntur-I.",
                ObjetivoSecundario = "Completa la misión conservando al menos el 50% de vida.",
                NombreJefeFinal = "COMANDANTE KUNTUR-I (F-35 ÉLITE)",
                ModeloJefe = TipoAeronave.F35_Lightning_II,
                VidaJefe = 45,
                CantidadOleadasPrevias = 2,
                PeligroAmbiental = TipoPeligroAmbiental.VientoCruzadoAndino,
                ColorCieloSuperior = Color.FromArgb(14, 38, 76),
                ColorCieloInferior = Color.FromArgb(32, 78, 128),
                RecompensaMonedasBase = 350,
                RecompensaXPBase = 180
            },
            new DefinicionNivelCampana
            {
                NumeroNivel = 2,
                CodigoOperacion = "OP-02: BASTIÓN FLUVIAL",
                NombreEscenario = "Cuenca del Lago y Cañones Ribereños",
                DescripcionMision = "La flota enemiga intenta tomar el estrecho fluvial. Corta su suministro y destruye el crucero fluvial de asalto.",
                ObjetivoPrincipal = "Supera las patrullas fluviales y abate al Crucero Titikaka.",
                ObjetivoSecundario = "Consigue un combo x3 sin recibir impactos.",
                NombreJefeFinal = "CRUCERO FLUVIAL TITIKAKA",
                ModeloJefe = TipoAeronave.B2_Spirit_Stealth,
                VidaJefe = 55,
                CantidadOleadasPrevias = 3,
                PeligroAmbiental = TipoPeligroAmbiental.VientoCruzadoAndino,
                ColorCieloSuperior = Color.FromArgb(14, 38, 76),
                ColorCieloInferior = Color.FromArgb(32, 78, 128),
                RecompensaMonedasBase = 480,
                RecompensaXPBase = 240
            },

            // MAPA 2: CAÑÓN DE LA TORMENTA (NIVELES 3 Y 4)
            new DefinicionNivelCampana
            {
                NumeroNivel = 3,
                CodigoOperacion = "OP-03: CAÑÓN DE LAS TORMENTAS - FASE I",
                NombreEscenario = "Cordillera Oriental bajo Tormenta Eléctrica",
                DescripcionMision = "Atraviesa un frente de cumulonimbos activos mientras esquivas descargas atmosféricas y cazas furtivos.",
                ObjetivoPrincipal = "Supera 3 oleadas en la tormenta y abate al Comandante Tempestad.",
                ObjetivoSecundario = "Alcanza una racha de Combo x4 o superior.",
                NombreJefeFinal = "COMANDANTE TEMPESTAD",
                ModeloJefe = TipoAeronave.F35_Lightning_II,
                VidaJefe = 68,
                CantidadOleadasPrevias = 3,
                PeligroAmbiental = TipoPeligroAmbiental.TormentaElectrica,
                ColorCieloSuperior = Color.FromArgb(18, 20, 44),
                ColorCieloInferior = Color.FromArgb(42, 48, 86),
                RecompensaMonedasBase = 650,
                RecompensaXPBase = 340
            },
            new DefinicionNivelCampana
            {
                NumeroNivel = 4,
                CodigoOperacion = "OP-04: OJO DEL CICLÓN",
                NombreEscenario = "Desfiladero Central y Garganta del Rayo",
                DescripcionMision = "Penetra en el centro de la tormenta eléctrica y derriba al bombardero furtivo pesado.",
                ObjetivoPrincipal = "Neutraliza a la escolta pesada y destruye al Escuadrón B-2.",
                ObjetivoSecundario = "Completa la misión sin agotar el escudo deflector.",
                NombreJefeFinal = "ESCUADRÓN FANTASMA B-2",
                ModeloJefe = TipoAeronave.B2_Spirit_Stealth,
                VidaJefe = 80,
                CantidadOleadasPrevias = 3,
                PeligroAmbiental = TipoPeligroAmbiental.TormentaElectrica,
                ColorCieloSuperior = Color.FromArgb(18, 20, 44),
                ColorCieloInferior = Color.FromArgb(42, 48, 86),
                RecompensaMonedasBase = 850,
                RecompensaXPBase = 460
            },

            // MAPA 3: GLACIAR AUSTRAL (NIVELES 5 Y 6)
            new DefinicionNivelCampana
            {
                NumeroNivel = 5,
                CodigoOperacion = "OP-05: GRIETAS ÁRTICAS",
                NombreEscenario = "Base Científica Sur bajo Aurora Austral",
                DescripcionMision = "Operación polar a -50°C. Evita las ventiscas criogénicas y elimina los puestos avanzados enemigos.",
                ObjetivoPrincipal = "Neutraliza 3 oleadas polares y destruye la Vanguardia Illimani.",
                ObjetivoSecundario = "Completa el nivel sin usar reapariciones de emergencia.",
                NombreJefeFinal = "VANGUARDIA ÁRTICA ILLIMANI",
                ModeloJefe = TipoAeronave.F35_Lightning_II,
                VidaJefe = 95,
                CantidadOleadasPrevias = 4,
                PeligroAmbiental = TipoPeligroAmbiental.GranizoCriogenico,
                ColorCieloSuperior = Color.FromArgb(6, 26, 38),
                ColorCieloInferior = Color.FromArgb(16, 68, 78),
                RecompensaMonedasBase = 1100,
                RecompensaXPBase = 600
            },
            new DefinicionNivelCampana
            {
                NumeroNivel = 6,
                CodigoOperacion = "OP-06: FORTALEZA HELADA",
                NombreEscenario = "Macizo de Hielo y Glaciar Profundo",
                DescripcionMision = "Asalta la fortaleza aérea congelada que custodia el paso andino hacia el sur y desactiva su reactor.",
                ObjetivoPrincipal = "Supera el cerco criogénico y destruye el Crucero Glaciar Illimani.",
                ObjetivoSecundario = "Logra al menos 4 impactos críticos en el motor central.",
                NombreJefeFinal = "CRUCERO GLACIAR ILLIMANI",
                ModeloJefe = TipoAeronave.B2_Spirit_Stealth,
                VidaJefe = 115,
                CantidadOleadasPrevias = 4,
                PeligroAmbiental = TipoPeligroAmbiental.GranizoCriogenico,
                ColorCieloSuperior = Color.FromArgb(6, 26, 38),
                ColorCieloInferior = Color.FromArgb(16, 68, 78),
                RecompensaMonedasBase = 1400,
                RecompensaXPBase = 780
            },

            // MAPA 4: VOLCÁN EN ERUPCIÓN (NIVELES 7 Y 8)
            new DefinicionNivelCampana
            {
                NumeroNivel = 7,
                CodigoOperacion = "OP-07: RÍO DE MAGMA",
                NombreEscenario = "Cráteres en Erupción del Sur Andino",
                DescripcionMision = "Vuela sobre el río de magma ardiente mientras esquivas lluvia de fragmentos incandescentes.",
                ObjetivoPrincipal = "Sobrevive a 4 oleadas volcánicas y derrota al Caza Ígneo Quimera.",
                ObjetivoSecundario = "Consigue un combo x5 durante la lluvia de asteroides.",
                NombreJefeFinal = "CAZA ÍGNEO QUIMERA",
                ModeloJefe = TipoAeronave.F35_Lightning_II,
                VidaJefe = 135,
                CantidadOleadasPrevias = 4,
                PeligroAmbiental = TipoPeligroAmbiental.MeteoritosPiroclasticos,
                ColorCieloSuperior = Color.FromArgb(46, 14, 18),
                ColorCieloInferior = Color.FromArgb(92, 34, 24),
                RecompensaMonedasBase = 1750,
                RecompensaXPBase = 980
            },
            new DefinicionNivelCampana
            {
                NumeroNivel = 8,
                CodigoOperacion = "OP-08: CALDERA DEL AVERNO",
                NombreEscenario = "Caldera Central del Volcán en Erupción",
                DescripcionMision = "Enfrenta a la descomunal nave insignia Valkiria en el corazón del cráter volcánico.",
                ObjetivoPrincipal = "Sobrevive a la erupción piroclástica y destruye a la Valkiria Volcánica X-9.",
                ObjetivoSecundario = "Logra al menos 6 impactos críticos en cabina.",
                NombreJefeFinal = "VALKIRIA VOLCÁNICA X-9",
                ModeloJefe = TipoAeronave.B2_Spirit_Stealth,
                VidaJefe = 160,
                CantidadOleadasPrevias = 4,
                PeligroAmbiental = TipoPeligroAmbiental.MeteoritosPiroclasticos,
                ColorCieloSuperior = Color.FromArgb(46, 14, 18),
                ColorCieloInferior = Color.FromArgb(92, 34, 24),
                RecompensaMonedasBase = 2200,
                RecompensaXPBase = 1250
            },

            // MAPA 5: FORTALEZA ESPACIAL (NIVELES 9 Y 10)
            new DefinicionNivelCampana
            {
                NumeroNivel = 9,
                CodigoOperacion = "OP-09: ÓRBITA BAJA Y CERCO ESPACIAL",
                NombreEscenario = "Frontera Exoatmosférica y Campo de Satélites",
                DescripcionMision = "Rompe el cerco orbital de satélites artillados antes de que la estación nodriza active su rayo aniquilador.",
                ObjetivoPrincipal = "Destruye la guardia orbital y abate al Dreadnought Centinela Apu.",
                ObjetivoSecundario = "Mantén una precisión de disparo superior al 65%.",
                NombreJefeFinal = "DREADNOUGHT CENTINELA APU",
                ModeloJefe = TipoAeronave.F35_Lightning_II,
                VidaJefe = 185,
                CantidadOleadasPrevias = 4,
                PeligroAmbiental = TipoPeligroAmbiental.LaserOrbitalSatelital,
                ColorCieloSuperior = Color.FromArgb(5, 8, 22),
                ColorCieloInferior = Color.FromArgb(20, 14, 48),
                RecompensaMonedasBase = 2800,
                RecompensaXPBase = 1600
            },
            new DefinicionNivelCampana
            {
                NumeroNivel = 10,
                CodigoOperacion = "OP-10: BATALLA FINAL ORBITAL",
                NombreEscenario = "Núcleo de la Estación Espacial Imperial",
                DescripcionMision = "La batalla definitiva por la supervivencia de la Tierra: destruye el núcleo del colosal acorazado Apu-Supay.",
                ObjetivoPrincipal = "Supera la guardia pretoriana imperial y destruye al Acorazado Orbital Apu-Supay.",
                ObjetivoSecundario = "Obtén calificación de 3 Estrellas en la misión final.",
                NombreJefeFinal = "ACORAZADO ORBITAL APU-SUPAY",
                ModeloJefe = TipoAeronave.B2_Spirit_Stealth,
                VidaJefe = 220,
                CantidadOleadasPrevias = 5,
                PeligroAmbiental = TipoPeligroAmbiental.LaserOrbitalSatelital,
                ColorCieloSuperior = Color.FromArgb(5, 8, 22),
                ColorCieloInferior = Color.FromArgb(20, 14, 48),
                RecompensaMonedasBase = 3800,
                RecompensaXPBase = 2200
            }
        };

        public static readonly IReadOnlyList<DefinicionLogro> CatalogoLogros = new List<DefinicionLogro>
        {
            new DefinicionLogro { Id = "LOGRO_PRIMERA_VICTORIA", Titulo = "Bautismo de Fuego", Descripcion = "Gana tu primera partida en cualquier modo de juego.", RecompensaMonedas = 150, RecompensaXP = 100 },
            new DefinicionLogro { Id = "LOGRO_CAZADOR_10", Titulo = "Centinela del Aire", Descripcion = "Acumula 10 aeronaves enemigas derribadas.", RecompensaMonedas = 250, RecompensaXP = 150 },
            new DefinicionLogro { Id = "LOGRO_CAZADOR_50", Titulo = "As de los Andes", Descripcion = "Acumula 50 aeronaves enemigas derribadas.", RecompensaMonedas = 600, RecompensaXP = 400 },
            new DefinicionLogro { Id = "LOGRO_COMBO_5", Titulo = "Precisión Quirúrgica", Descripcion = "Alcanza un multiplicador de Combo x5 o superior.", RecompensaMonedas = 300, RecompensaXP = 200 },
            new DefinicionLogro { Id = "LOGRO_PRIMER_JEFE", Titulo = "Matagigantes", Descripcion = "Derrota a tu primer Jefe Final en el Modo Casual.", RecompensaMonedas = 350, RecompensaXP = 250 },
            new DefinicionLogro { Id = "LOGRO_CAMPANA_COMPLETA", Titulo = "Héroe de la UNAJ", Descripcion = "Completa los 5 niveles del Modo Casual (Operación Cóndor Final).", RecompensaMonedas = 1500, RecompensaXP = 1000 },
            new DefinicionLogro { Id = "LOGRO_PRIMERA_COMPRA", Titulo = "Ingeniería Aeronáutica", Descripcion = "Adquiere un artículo o mejora en la Tienda Táctica.", RecompensaMonedas = 150, RecompensaXP = 100 },
            new DefinicionLogro { Id = "LOGRO_MEJORA_MAX", Titulo = "Tecnología de 6ª Generación", Descripcion = "Lleva cualquier mejora permanente al Nivel 5 máximo.", RecompensaMonedas = 700, RecompensaXP = 500 },
            new DefinicionLogro { Id = "LOGRO_MILLONARIO", Titulo = "Reserva Estratégica", Descripcion = "Acumula un saldo disponible de 2,000 Créditos Tácticos o más.", RecompensaMonedas = 400, RecompensaXP = 300 },
            new DefinicionLogro { Id = "LOGRO_MULTIJUGADOR", Titulo = "Enlace de Datos Táctico", Descripcion = "Juega una partida en el Modo Multijugador (LAN o Internet).", RecompensaMonedas = 300, RecompensaXP = 200 }
        };

        static GestorEstadoGlobal()
        {
            CargarTodo();
        }

        public static void CargarTodo()
        {
            lock (_candado)
            {
                try
                {
                    Directory.CreateDirectory(_directorioDatos);

                    if (File.Exists(_rutaPerfil))
                    {
                        string jsonPerfil = File.ReadAllText(_rutaPerfil);
                        var cargado = JsonSerializer.Deserialize<PerfilJugador>(jsonPerfil);
                        if (cargado != null)
                        {
                            Perfil = cargado;
                            ValidarIntegridadPerfil();
                        }
                    }

                    if (File.Exists(_rutaConfig))
                    {
                        string jsonConfig = File.ReadAllText(_rutaConfig);
                        var configCargada = JsonSerializer.Deserialize<ConfiguracionJuego>(jsonConfig);
                        if (configCargada != null)
                        {
                            Configuracion = configCargada;
                        }
                    }
                }
                catch
                {
                    // Si hay corrupción de archivo, mantener valores seguros por defecto
                    ValidarIntegridadPerfil();
                }
            }
        }

        public static void GuardarTodo()
        {
            lock (_candado)
            {
                try
                {
                    Directory.CreateDirectory(_directorioDatos);
                    var opciones = new JsonSerializerOptions { WriteIndented = true };
                    File.WriteAllText(_rutaPerfil, JsonSerializer.Serialize(Perfil, opciones));
                    File.WriteAllText(_rutaConfig, JsonSerializer.Serialize(Configuracion, opciones));
                }
                catch
                {
                    // Ignorar errores de E/S en entornos restringidos
                }
            }
        }

        private static void ValidarIntegridadPerfil()
        {
            if (Perfil.Monedas < 0) Perfil.Monedas = 0;
            if (Perfil.ExperienciaTotal < 0) Perfil.ExperienciaTotal = 0;
            if (Perfil.NivelMaximoCampanaDesbloqueado < 1) Perfil.NivelMaximoCampanaDesbloqueado = 1;
            if (Perfil.NivelMaximoCampanaDesbloqueado > 5) Perfil.NivelMaximoCampanaDesbloqueado = 5;

            Perfil.ArticulosComprados ??= new HashSet<string>();
            Perfil.ArticulosComprados.Add("NAVE_F35");
            Perfil.ArticulosComprados.Add("NAVE_B2");
            Perfil.ArticulosComprados.Add("SKIN_ESTANDAR");
            Perfil.ArticulosComprados.Add("PODER_EMP");

            Perfil.NivelesMejoras ??= new Dictionary<string, int>();
            Perfil.EstrellasPorNivel ??= new Dictionary<int, int>();
            Perfil.MejorPuntajePorNivel ??= new Dictionary<int, int>();
            Perfil.LogrosDesbloqueados ??= new HashSet<string>();
        }

        public static int CalcularPrecioActualArticulo(ArticuloCatalogo art)
        {
            if (art.Categoria == CategoriaArticulo.MejoraPermanente)
            {
                int nivelActual = Perfil.ObtenerNivelMejora(art.Id);
                return art.PrecioBase * (nivelActual + 1);
            }
            return art.PrecioBase;
        }

        public static bool IntentarComprarArticulo(string idArticulo, out string mensaje)
        {
            lock (_candado)
            {
                ArticuloCatalogo? art = CatalogoTienda.FirstOrDefault(a => a.Id == idArticulo);
                if (art == null)
                {
                    mensaje = "El artículo solicitado no existe en el catálogo.";
                    return false;
                }

                if (art.Categoria == CategoriaArticulo.MejoraPermanente)
                {
                    int nivelActual = Perfil.ObtenerNivelMejora(art.Id);
                    if (nivelActual >= art.NivelMaximo)
                    {
                        mensaje = $"La mejora '{art.Nombre}' ya alcanzó su Nivel Máximo ({art.NivelMaximo}).";
                        return false;
                    }

                    int costo = CalcularPrecioActualArticulo(art);
                    if (Perfil.Monedas < costo)
                    {
                        mensaje = $"Saldo insuficiente. Necesitas {costo} Créditos Tácticos (Tienes {Perfil.Monedas}).";
                        return false;
                    }

                    Perfil.Monedas -= costo;
                    Perfil.NivelesMejoras[art.Id] = nivelActual + 1;
                    VerificarLogrosInterno(out _);
                    GuardarTodo();
                    mensaje = $"¡Mejora instalada! '{art.Nombre}' subió a Nivel {nivelActual + 1}.";
                    return true;
                }
                else
                {
                    if (Perfil.ArticulosComprados.Contains(art.Id))
                    {
                        mensaje = $"Ya posees '{art.Nombre}' en tu inventario.";
                        return false;
                    }

                    if (Perfil.Monedas < art.PrecioBase)
                    {
                        mensaje = $"Saldo insuficiente. Necesitas {art.PrecioBase} Créditos Tácticos (Tienes {Perfil.Monedas}).";
                        return false;
                    }

                    Perfil.Monedas -= art.PrecioBase;
                    Perfil.ArticulosComprados.Add(art.Id);
                    EquiparArticulo(art.Id, out _);
                    VerificarLogrosInterno(out _);
                    GuardarTodo();
                    mensaje = $"¡Compra exitosa! '{art.Nombre}' fue añadido y equipado en tu hangar.";
                    return true;
                }
            }
        }

        public static bool EquiparArticulo(string idArticulo, out string mensaje)
        {
            lock (_candado)
            {
                ArticuloCatalogo? art = CatalogoTienda.FirstOrDefault(a => a.Id == idArticulo);
                if (art == null || !Perfil.ArticulosComprados.Contains(idArticulo))
                {
                    mensaje = "Debes adquirir este artículo antes de equiparlo.";
                    return false;
                }

                switch (art.Categoria)
                {
                    case CategoriaArticulo.Aeronave:
                        Perfil.AeronaveEquipadaId = art.Id;
                        break;
                    case CategoriaArticulo.Apariencia:
                        Perfil.SkinEquipadaId = art.Id;
                        break;
                    case CategoriaArticulo.PoderEspecial:
                        Perfil.PoderEquipadoId = art.Id;
                        break;
                    default:
                        mensaje = "Las mejoras permanentes están activas automáticamente.";
                        return true;
                }

                GuardarTodo();
                mensaje = $"'{art.Nombre}' equipado exitosamente para el próximo vuelo.";
                return true;
            }
        }

        public static bool ReclamarRecompensaDiaria(out int monedasGanadas, out int xpGanada, out string mensaje)
        {
            lock (_candado)
            {
                string hoy = DateTime.Now.ToString("yyyy-MM-dd");
                if (Perfil.UltimaFechaRecompensaDiaria == hoy)
                {
                    monedasGanadas = 0;
                    xpGanada = 0;
                    mensaje = "Ya reclamaste tu suministro diario de hoy. Vuelve mañana para continuar tu racha.";
                    return false;
                }

                string ayer = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                if (Perfil.UltimaFechaRecompensaDiaria == ayer)
                {
                    Perfil.RachaDiasRecompensa++;
                }
                else
                {
                    Perfil.RachaDiasRecompensa = 1;
                }

                Perfil.UltimaFechaRecompensaDiaria = hoy;
                monedasGanadas = 150 + Math.Min(350, (Perfil.RachaDiasRecompensa - 1) * 50);
                xpGanada = 80 + Math.Min(200, (Perfil.RachaDiasRecompensa - 1) * 25);

                Perfil.Monedas += monedasGanadas;
                Perfil.MonedasAcumuladasHistorico += monedasGanadas;
                Perfil.ExperienciaTotal += xpGanada;

                VerificarLogrosInterno(out _);
                GuardarTodo();

                mensaje = $"¡Suministro Diario (Día #{Perfil.RachaDiasRecompensa}) recibido!\n+{monedasGanadas} Créditos Tácticos y +{xpGanada} XP.";
                return true;
            }
        }

        public static List<DefinicionLogro> RegistrarResultadoPartida(
            ModalidadPartida modalidad,
            int nivelCampana,
            bool victoria,
            int bajas,
            bool jefeDerrotado,
            int disparos,
            int impactos,
            int comboMax,
            int puntaje,
            int estrellasObtenidas,
            out int monedasGanadas,
            out int xpGanada)
        {
            lock (_candado)
            {
                Perfil.PartidasJugadas++;
                if (victoria) Perfil.VictoriasTotales++;
                else Perfil.DerrotasTotales++;

                if (modalidad == ModalidadPartida.Multijugador_1v1 ||
                    modalidad == ModalidadPartida.Multijugador_2v2 ||
                    modalidad == ModalidadPartida.Multijugador_Cooperativo)
                {
                    Perfil.PartidasMultijugadorJugadas++;
                    if (victoria) Perfil.VictoriasMultijugador++;
                }

                Perfil.BajasTotales += Math.Max(0, bajas);
                if (jefeDerrotado) Perfil.JefesDerrotados++;
                Perfil.DisparosTotales += Math.Max(0, disparos);
                Perfil.ImpactosTotales += Math.Max(0, impactos);
                if (comboMax > Perfil.ComboMaximoHistorico)
                {
                    Perfil.ComboMaximoHistorico = comboMax;
                }

                // Calcular recompensas económicas equilibradas
                double bonoEconomia = 1.0 + (Perfil.ObtenerNivelMejora("MEJORA_ECONOMIA") * 0.15);
                int baseMonedas = (bajas * 25) + (jefeDerrotado ? 180 : 0) + (victoria ? 120 : 35);
                int baseXP = (bajas * 18) + (jefeDerrotado ? 140 : 0) + (victoria ? 100 : 30);

                if (modalidad == ModalidadPartida.CampanaCasual && nivelCampana >= 1 && nivelCampana <= NivelesCampana.Count)
                {
                    var defNivel = NivelesCampana[nivelCampana - 1];
                    if (victoria)
                    {
                        baseMonedas += defNivel.RecompensaMonedasBase;
                        baseXP += defNivel.RecompensaXPBase;

                        if (nivelCampana >= Perfil.NivelMaximoCampanaDesbloqueado && nivelCampana < NivelesCampana.Count)
                        {
                            Perfil.NivelMaximoCampanaDesbloqueado = nivelCampana + 1;
                        }

                        int estPrevias = Perfil.EstrellasPorNivel.TryGetValue(nivelCampana, out int ePrev) ? ePrev : 0;
                        if (estrellasObtenidas > estPrevias)
                        {
                            Perfil.EstrellasPorNivel[nivelCampana] = estrellasObtenidas;
                        }
                    }

                    int puntPrevio = Perfil.MejorPuntajePorNivel.TryGetValue(nivelCampana, out int pPrev) ? pPrev : 0;
                    if (puntaje > puntPrevio)
                    {
                        Perfil.MejorPuntajePorNivel[nivelCampana] = puntaje;
                    }
                }

                monedasGanadas = (int)Math.Round(baseMonedas * bonoEconomia);
                xpGanada = (int)Math.Round(baseXP * bonoEconomia);

                Perfil.Monedas += monedasGanadas;
                Perfil.MonedasAcumuladasHistorico += monedasGanadas;
                Perfil.ExperienciaTotal += xpGanada;

                VerificarLogrosInterno(out List<DefinicionLogro> nuevosLogros);
                GuardarTodo();
                return nuevosLogros;
            }
        }

        private static void VerificarLogrosInterno(out List<DefinicionLogro> recienDesbloqueados)
        {
            var desbloqueados = new List<DefinicionLogro>();

            void Evaluar(string id, bool condicion)
            {
                if (!condicion || Perfil.LogrosDesbloqueados.Contains(id)) return;
                Perfil.LogrosDesbloqueados.Add(id);
                var def = CatalogoLogros.FirstOrDefault(l => l.Id == id);
                if (def != null)
                {
                    Perfil.Monedas += def.RecompensaMonedas;
                    Perfil.MonedasAcumuladasHistorico += def.RecompensaMonedas;
                    Perfil.ExperienciaTotal += def.RecompensaXP;
                    desbloqueados.Add(def);
                }
            }

            Evaluar("LOGRO_PRIMERA_VICTORIA", Perfil.VictoriasTotales >= 1);
            Evaluar("LOGRO_CAZADOR_10", Perfil.BajasTotales >= 10);
            Evaluar("LOGRO_CAZADOR_50", Perfil.BajasTotales >= 50);
            Evaluar("LOGRO_COMBO_5", Perfil.ComboMaximoHistorico >= 5);
            Evaluar("LOGRO_PRIMER_JEFE", Perfil.JefesDerrotados >= 1);
            Evaluar("LOGRO_CAMPANA_COMPLETA", Perfil.EstrellasPorNivel.ContainsKey(5) && Perfil.EstrellasPorNivel[5] >= 1);
            Evaluar("LOGRO_PRIMERA_COMPRA", Perfil.ArticulosComprados.Count > 4 || Perfil.NivelesMejoras.Values.Any(v => v > 0));
            Evaluar("LOGRO_MEJORA_MAX", Perfil.NivelesMejoras.Values.Any(v => v >= 5));
            Evaluar("LOGRO_MILLONARIO", Perfil.Monedas >= 2000);
            Evaluar("LOGRO_MULTIJUGADOR", Perfil.PartidasMultijugadorJugadas >= 1);

            recienDesbloqueados = desbloqueados;
        }

        public static TipoAeronave ObtenerModeloAeronaveEquipada()
        {
            return Perfil.AeronaveEquipadaId == "NAVE_B2"
                ? TipoAeronave.B2_Spirit_Stealth
                : TipoAeronave.F35_Lightning_II;
        }

        public static TipoPoderEspecial ObtenerPoderEspecialEquipado()
        {
            return Perfil.PoderEquipadoId switch
            {
                "PODER_ESCUDO" => TipoPoderEspecial.EscudoCuanticoSobrecarga,
                "PODER_OVERDRIVE" => TipoPoderEspecial.SobrecargaVulcan_Overdrive,
                "PODER_TERMOBARICO" => TipoPoderEspecial.SalvaTermobarica,
                _ => TipoPoderEspecial.PulsoElectromagnetico_EMP
            };
        }

        public static Color ObtenerColorSkinEquipada()
        {
            var skin = CatalogoTienda.FirstOrDefault(a => a.Id == Perfil.SkinEquipadaId);
            return skin?.ColorRepresentativo ?? Color.FromArgb(0, 195, 255);
        }
    }
}
