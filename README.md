# vibranceGUI

**Este repositorio es un fork independiente de vibranceGUI, creado originalmente por juvlarN.** [Proyecto original en GitHub](https://github.com/juv/vibranceGUI) · [juvlarN en Twitter/X](https://twitter.com/juvlarN)

Ajusta automáticamente la saturación de Windows al jugar a Counter-Strike 2 u otros juegos. vibranceGUI detecta la aplicación en primer plano y aplica el perfil correspondiente mediante **Digital Vibrance** en NVIDIA o el control de **saturación** en AMD.

## Qué ha mejorado en este fork

- **Interfaz oscura con Krypton Toolkit.** Ventana redimensionable, tarjetas redondeadas, acentos lavanda y desplegables con flechas discretas. Los interruptores ocupan una sola fila para dejar más espacio al editor.
- **Porcentajes editables.** Escribe el valor de saturación del escritorio o del juego en el campo junto a su barra. Pulsa **Enter** o sal del campo para aplicarlo. En los perfiles, pulsa **Guardar** para conservar el cambio.
- **Pausa rápida de la saturación.** Pulsa **Activo** para volver al nivel neutro y **Inactivo** para recuperar la saturación configurada. Al cerrar vibranceGUI, la saturación vuelve al nivel neutro y se conservan tus ajustes.
- **Editor de perfiles integrado.** Al seleccionar un juego, puedes ajustar su saturación y resolución en el panel lateral. Añade un juego que esté abierto o busca su ejecutable. Guarda los cambios del perfil o cancela para recuperar los valores guardados.
- **Español, inglés y portugués.** El selector cambia el idioma al momento y recuerda la elección al volver a abrir el programa.
- **Cambios de pantalla temporales.** La resolución del perfil se aplica sin escribirla en el registro de Windows. vibranceGUI guarda el modo anterior de cada monitor y lo restaura al salir del perfil, incluso al pasar a otro monitor, o al cerrar el programa. En NVIDIA, el modo del perfil también se aplica si la saturación ya coincide.
- **Guardado más fiable.** Los ajustes del escritorio se guardan automáticamente. Los perfiles se escriben en un XML temporal y el archivo anterior solo se sustituye cuando el nuevo está completo. La interfaz muestra un aviso si falla el guardado.
- **Detección de GPU.** Windows indica qué adaptadores AMD y NVIDIA están presentes y operativos antes de que vibranceGUI compruebe sus controladores. Si ambos fabricantes están disponibles, la aplicación avisa de que esta versión solo puede usar uno a la vez.
- **Descarga en un solo EXE.** GitHub Actions compila para Windows x86 con .NET Framework 4.8, verifica el SHA-256 de la DLL de NVIDIA e incrusta las dependencias gestionadas con Costura.

## Descargar y abrir

1. Abre [Actions → Windows build](https://github.com/rekznoz/vibranceGUI/actions/workflows/build.yml).
2. Entra en una ejecución completada correctamente y descarga **vibranceGUI-SingleExe-x86** desde **Artifacts**.
3. Extrae **vibrance.GUI.exe** y muévelo a donde quieras, incluido el escritorio. Requiere .NET Framework 4.8 en Windows.

Si tienes acceso de escritura, puedes compilar desde **Run workflow**. Desmarca **Build the single executable** para generar el ZIP de respaldo **vibranceGUI-Release-x86**. Si usas ese ZIP, extrae todo y mantén `lib` y `vibrance.GUI.exe.config` junto al ejecutable.

## Configurar un perfil de CS2

1. Abre CS2 y pulsa **Añadir juego activo** en vibranceGUI. También puedes pulsar **Buscar…** y seleccionar el ejecutable de CS2.
2. Selecciona el perfil y ajusta **Saturación en el juego**. Si juegas en ventana o sin bordes, puedes configurar también un modo de pantalla.
3. Pulsa **Guardar** antes de seleccionar otro juego. **Mantener resolución** tiene prioridad sobre los modos de pantalla configurados en los perfiles.

La saturación del escritorio se configura por separado y se aplica cuando no hay un perfil activo. La aplicación admite NVIDIA y AMD mediante sus controladores. El soporte en portátiles NVIDIA depende de que el controlador exponga Digital Vibrance. Las GPU Intel quedan fuera del alcance de esta versión. La detección y la aplicación del perfil de CS2 se han probado en un equipo NVIDIA.

## Compilar en Windows

Instala Visual Studio 2022 o Build Tools 2022 con **Desarrollo de escritorio de .NET** y el **Developer Pack de .NET Framework 4.8**. Abre una consola de desarrollador de Visual Studio en la raíz del repositorio. [Developer Pack de Microsoft](https://learn.microsoft.com/en-us/dotnet/framework/install/guide-for-developers)

```bat
msbuild vibrance.GUI.sln /restore /m /p:Configuration=Release /p:Platform=x86 /p:SingleExe=true
```

El EXE único está en `vibrance.GUI\bin\x86\SingleExe\vibrance.GUI.exe`. Para compilar con las DLL separadas, omite `/p:SingleExe=true`. En ese caso, el resultado está en `vibrance.GUI\bin\x86\Release\`. La DLL de NVIDIA requiere un proceso x86, también en Windows de 64 bits.

### Procedencia de la DLL de NVIDIA

El ejecutable incorpora `vibrance.GUI/NVIDIA/vibranceDLL.dll`. El commit `5ee9adef0e8b04efbb1a00f53f231145cdfedefd` introdujo los bytes actuales y registra el uso de `/MT` para enlazar estáticamente el runtime de C++. El autor publicó el proyecto C/C++ por separado, con instrucciones para compilar en x86 y Unicode. [Código fuente de vibranceDLL](https://github.com/juv/vibranceDLL)

```powershell
Get-FileHash .\vibrance.GUI\NVIDIA\vibranceDLL.dll -Algorithm SHA256
```

El SHA-256 esperado es `0F229F79934F21617337C28915A9449F7B2395B20D7FB0E4A02D14277163CEA0`. La DLL carece de firma Authenticode. El hash identifica los bytes incluidos y sigue pendiente verificar la revisión del código fuente y la cadena de herramientas que los generó.

## Contribuir

Puedes abrir una incidencia o enviar un pull request en este fork. Si informas de un fallo de pantalla, incluye el modelo de GPU, la versión del controlador, los monitores conectados y los pasos para reproducirlo.

## Créditos del proyecto original

- **juvlarN**, creador de vibranceGUI y contacto original para NVIDIA. [GitHub](https://github.com/juv) · [Twitter/X](https://twitter.com/juvlarN)
- **juRiiir3**, contacto original para AMD. [Twitter/X](https://twitter.com/juRiiir3)
