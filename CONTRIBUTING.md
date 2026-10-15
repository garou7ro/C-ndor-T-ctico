# 🤝 Guía de Contribución al Proyecto
# Cóndor Táctico: Tactical Air Combat Simulator

¡Gracias por tu interés en contribuir al desarrollo de **Cóndor Táctico**! Este documento establece las directrices de ingeniería de software, estándares de codificación en C# y flujos de trabajo de Git necesarios para mantener la calidad y el rendimiento del proyecto.

---

## 1. Código de Conducta

Al participar en este proyecto, te comprometes a seguir las normas de convivencia profesional descritas en nuestro [`CODE_OF_CONDUCT.md`](./CODE_OF_CONDUCT.md).

---

## 2. Flujo de Trabajo con Git (Git Flow)

Adoptamos una estrategia simplificada basada en ramas:

- `main`: Rama de producción y versiones estables publicadas.
- `develop`: Rama principal de integración para el trabajo en progreso.
- `feature/<nombre-caracteristica>`: Ramas para nuevas aeronaves, armas o subsistemas.
- `bugfix/<nombre-error>`: Ramas para corrección de anomalías.

```bash
# 1. Crear una nueva rama desde develop
git checkout develop
git pull origin develop
git checkout -b feature/nuevo-caza-su57

# 2. Realizar cambios con commits descriptivos en presente imperativo
git commit -m "feat(entities): Implementa modelo vectorial del Su-57 Felon"

# 3. Empujar la rama y abrir Pull Request
git push origin feature/nuevo-caza-su57
```

---

## 3. Estándares y Convenciones de Código en C#

El proyecto utiliza **C# 12** sobre **.NET 8.0** con `Nullable` habilitado. Sigue las directrices oficiales de Microsoft:

### 3.1. Nomenclatura
- **Clases, Métodos, Propiedades, Enums y Structs:** `PascalCase`  
  *Ejemplo:* `BombarderoB2Spirit`, `ActualizarFisica()`, `VidaMaxima`.
- **Campos privados:** `_camelCase` con prefijo de guión bajo.  
  *Ejemplo:* `_grillaColisiones`, `_aeronavesRemotas`.
- **Argumentos de métodos y variables locales:** `camelCase`  
  *Ejemplo:* `deltaX`, `limitesMapa`.
- **Constantes:** `PascalCase`  
  *Ejemplo:* `ComplejidadCiclomaticaPromedio`.

### 3.2. Reglas Estrictas del Bucle Caliente de Juego (Hot Loop)
Para asegurar **60 FPS estables** y **0 recolecciones del Garbage Collector (Gen 0/1/2)** durante el combate:

1. ❌ **PROHIBIDO instanciar objetos GDI+ (`new Bitmap()`, `new Pen()`, `new Brush()`) dentro del método `RenderizarEscenaCompleta` o `Dibujar`:**
   - Todos los pinceles o fuentes deben ser cacheados estáticamente o pre-creados como atributos de clase.
2. ❌ **PROHIBIDO utilizar LINQ (`.Where()`, `.Select()`, `.ToList()`) en `ActualizarFisica` o `ImpactarTick`:**
   - Usa bucles `for` indexados indexando arrays o `List<T>` para evitar asignaciones de enumeradores (`IEnumerator<T>`).
3. ❌ **PROHIBIDO la concatenación dinámica de cadenas (`+`) dentro del renderizado repetitivo:**
   - Utiliza `StringBuilder` pre-asignado o formatee solo cuando cambie el valor numérico.
4. ✅ **USAR siempre la técnica *Swap-and-Pop* para remover elementos de listas:**
   - Evita la degradación $\mathcal{O}(N)$ de `List.RemoveAt()`.

---

## 4. Checklist para Pull Requests (PR)

Antes de solicitar la revisión de un Pull Request, verifica que cumples con la siguiente lista de control:

- [ ] La solución compila limpiamente con `dotnet build -c Release` con **0 errores y 0 advertencias**.
- [ ] No se rompió la compatibilidad con el modo multijugador UDP ni el protocolo `PaqueteRedAeronave`.
- [ ] La complejidad de las nuevas colisiones se mantiene en $\mathcal{O}(N+K)$ usando `GrillaEspacialColisiones`.
- [ ] Se ejecutó el simulador con `F3` y se verificó que el tiempo de CPU por frame es menor a $1.5\text{ ms}$.
- [ ] El índice de mantenibilidad general reportado por Visual Studio se mantiene superior a **80/100**.
- [ ] Los nuevos archivos `.cs` cuentan con comentarios XML de documentación (`/// <summary>`).

---

## 5. Reporte de Errores y Sugerencias

Si encuentras un error o deseas proponer una nueva funcionalidad, abre un *Issue* en GitHub proporcionando:
1. Pasos precisos para reproducir el fallo.
2. Comportamiento observado vs comportamiento esperado.
3. Especificaciones del sistema operativo y tarjeta gráfica.
4. Exportación del reporte de telemetría táctica (`F4`) si el error involucra rendimiento o red.
