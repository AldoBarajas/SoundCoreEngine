# SoundCore Engine

**SoundCore Engine** es una aplicación de escritorio desarrollada en **C# con Windows Forms y .NET 8**. Simula una cola de reproducción para DJ y permite estudiar, de manera práctica, cómo cambian las operaciones al utilizar distintas estructuras de datos.

El programa reproduce archivos mediante el motor multimedia VLC incluido por NuGet. No utiliza Windows Media Player y el selector permite elegir cualquier extensión; el archivo se reproducirá cuando VLC sea compatible con su contenido.

## Objetivo del proyecto

El objetivo es comparar el comportamiento de tres formas de almacenar una cola de pistas:

- Una lista simple enlazada genérica creada desde cero.
- LinkedList<T>, la lista enlazada incluida en .NET.
- List<T>, la lista basada en arreglo dinámico incluida en .NET.

La aplicación muestra que la elección de una estructura afecta el costo de insertar, eliminar y ordenar datos. Incluye un benchmark con Stopwatch para observar dicha diferencia en milisegundos.

## Características

- Selección de cualquier archivo desde el equipo.
- Reproducción, pausa y reanudación usando VLC.
- Registro editable de título, artista, BPM y duración estimada.
- Tres colas independientes, una por cada estructura de datos.
- Vista de la cola activa con posición, identificador, título, BPM, duración y formato.
- Inserción al final y operación “Reproducir siguiente”.
- Avance y reproducción de la primera pista.
- Inversión, ordenamiento por BPM y purga de duplicados.
- Benchmark de inserciones intermedias sobre las tres estructuras.

## Estructuras implementadas

### ListaSimpleEnlazada<T>

Esta es la estructura principal del proyecto. Se implementó desde cero con Nodo<T>, sin usar arreglos ni List<T> internamente. Cada nodo contiene el valor almacenado y una referencia Siguiente al nodo posterior.

Operaciones disponibles:

- AgregarAlFinal(T): agrega un nodo al final. Complejidad O(n).
- ReproducirSiguiente(T): inserta después de la cabeza. Complejidad O(1).
- AvanzarPista(): elimina y devuelve la cabeza. Complejidad O(1).
- Invertir(): invierte referencias de nodos in-place. Complejidad O(n) y memoria auxiliar O(1).
- InsertarOrdenado(T, Comparison<T>): inserta según un criterio.
- OrdenarEnSitio(Comparison<T>): ordena nodos existentes sin crear otra lista.
- DepurarDuplicados(...): elimina duplicados sin estructuras auxiliares. Complejidad O(n²).

### LinkedList<T>

Es la colección enlazada nativa de .NET. Se utiliza para comparar las operaciones de nodos de la lista propia con una implementación de biblioteca.

### List<T>

Es un arreglo dinámico de .NET. Es muy útil para acceso por índice, pero una inserción en medio normalmente desplaza los elementos posteriores; por ello tiene costo O(n).

## Cómo funciona la aplicación

1. Pulsa **Elegir cualquier archivo** y selecciona una canción u otro archivo multimedia.
2. Revisa o modifica el título, artista, BPM y duración.
3. En **Modo de estructura**, selecciona una de las tres opciones con los botones circulares.
4. Usa **Encolar al final** para añadir la pista a la cola seleccionada.
5. Usa **Reproducir siguiente** para colocarla en la segunda posición.
6. Pulsa **Avanzar y reproducir** para retirar la primera pista de la cola y reproducirla.
7. Usa las acciones de invertir, ordenar o purgar sobre la cola activa.
8. Cambia de modo para observar la cola independiente de cada estructura.
9. Ejecuta **Iniciar prueba** para comparar el rendimiento de las tres implementaciones.

> Encolar una pista no cambia inmediatamente el texto “Sonando”. Para comenzar o cambiar la reproducción se debe usar **Avanzar y reproducir**.

## Reproductor multimedia

La reproducción se implementó con estos paquetes NuGet:

- LibVLCSharp.WinForms, versión 3.9.4.
- VideoLAN.LibVLC.Windows, versión 3.0.21.

LibVLCSharp es la integración de .NET para el motor LibVLC. El proyecto crea un MediaPlayer, abre el archivo seleccionado como Media y controla acciones como reproducir, pausar, detener y detectar el final de la pista.

## Tecnologías utilizadas

- C#
- .NET 8
- Windows Forms
- LibVLCSharp / LibVLC para reproducción multimedia
- Colecciones genéricas de .NET
- Stopwatch para medición de rendimiento

## Requisitos

- Windows 10 o Windows 11.
- Visual Studio 2022 con la carga de trabajo **Desarrollo de escritorio de .NET**.
- SDK de .NET 8.
- Acceso a NuGet.org durante la primera restauración de paquetes.

## Instalación y ejecución

1. Abre SoundCoreEngine.sln con Visual Studio 2022.
2. Si Visual Studio solicita recargar el proyecto, acepta.
3. Haz clic derecho sobre la solución y elige **Restaurar paquetes NuGet**.
4. Compila con Ctrl + Shift + B.
5. Ejecuta con F5.

Si la restauración conserva errores antiguos, cierra Visual Studio, elimina las carpetas bin y obj del proyecto, abre de nuevo la solución y restaura los paquetes.

## Benchmark

La prueba realiza muchas inserciones justo después del primer elemento:

- La lista propia reconecta referencias entre nodos.
- LinkedList<T> inserta después de un nodo conocido.
- List<T> mueve los elementos posteriores para abrir espacio.

Por esa razón, las dos listas enlazadas suelen requerir menos tiempo para este caso específico. Los resultados dependen del equipo, por lo que se deben interpretar como una medición experimental y no como valores fijos.

## Organización del código

    SoundCoreEngine/
    ├── EstructurasPropias/
    │   ├── Nodo.cs
    │   └── ListaSimpleEnlazada.cs
    ├── Modelos/
    │   └── Pista.cs
    ├── Servicios/
    │   └── ReproductorVlc.cs
    ├── MainForm.cs
    ├── Program.cs
    └── SoundCoreEngine.csproj

- **EstructurasPropias**: contiene los nodos y la lista implementada manualmente.
- **Modelos**: define la pista y sus metadatos.
- **Servicios**: encapsula la reproducción con LibVLC.
- **MainForm**: crea la interfaz y conecta los botones con cada estructura.
- **Program**: inicia la aplicación Windows Forms.

## Demostración sugerida

1. Agrega tres pistas con BPM distintos en la lista propia.
2. Inserta una pista mediante “Reproducir siguiente”.
3. Invierte, ordena y elimina un duplicado.
4. Avanza la cola para reproducir una pista.
5. Cambia a las otras dos estructuras y agrega una pista en cada una.
6. Ejecuta el benchmark y explica por qué List<T> desplaza elementos durante una inserción intermedia.

## Autoría

Completar antes de la entrega:

- Integrante(s):
- Número(s) de control:
- Materia: Estructura de Datos.
- Unidad: Estructuras de Datos Lineales.
- Fecha:
