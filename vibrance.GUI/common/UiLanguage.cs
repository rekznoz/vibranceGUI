using System;
using System.Collections.Generic;

namespace vibrance.GUI.common
{
    internal static class UiLanguage
    {
        private static readonly Dictionary<string, string[]> Translations = new Dictionary<string, string[]>
        {
            { "Updates", new[] { "Novedades", "Novidades" } },
            { "Exit", new[] { "Salir", "Sair" } },
            { "Desktop color and game profiles", new[] { "Color del escritorio y perfiles de juego", "Cor do ambiente de trabalho e perfis de jogo" } },
            { "Starting", new[] { "Iniciando", "A iniciar" } },
            { "●  Active", new[] { "●  Activo", "●  Ativo" } },
            { "NVAPI unloaded", new[] { "NVAPI desactivada", "NVAPI desativada" } },
            { "Closing...", new[] { "Cerrando…", "A fechar…" } },
            { "Desktop vibrance", new[] { "Saturación del escritorio", "Saturação do ambiente de trabalho" } },
            { "Color level when no game profile is active.", new[] { "Nivel de color sin un perfil de juego activo.", "Nível de cor sem um perfil de jogo ativo." } },
            { "Game profiles", new[] { "Perfiles de juego", "Perfis de jogo" } },
            { "Set a separate color level and display mode for each game.", new[] { "Configura el color y la resolución de cada juego.", "Define a cor e a resolução de cada jogo." } },
            { "Add running game", new[] { "Añadir juego activo", "Adicionar jogo ativo" } },
            { "Browse…", new[] { "Buscar…", "Procurar…" } },
            { "Cancel", new[] { "Cancelar", "Cancelar" } },
            { "Remove", new[] { "Eliminar", "Remover" } },
            { "Select a game to edit its profile.", new[] { "Selecciona un juego para editar su perfil.", "Seleciona um jogo para editar o perfil." } },
            { "No game profiles yet\r\nAdd a running game or browse for an executable.",
                new[] { "Aún no hay perfiles de juego\r\nAñade un juego activo o busca un ejecutable.",
                    "Ainda não há perfis de jogo\r\nAdiciona um jogo ativo ou procura um executável." } },
            { "Start with Windows", new[] { "Iniciar con Windows", "Iniciar com o Windows" } },
            { "Primary monitor only", new[] { "Solo monitor principal", "Só o monitor principal" } },
            { "Keep Windows resolution", new[] { "Mantener resolución", "Manter resolução" } },
            { "Limit vibrance changes to one monitor.", new[] { "Limita los cambios de saturación a un monitor.", "Limita as alterações de saturação a um monitor." } },
            { "Prevent game profiles from changing the display resolution.", new[] { "Impide que los perfiles cambien la resolución de pantalla.", "Impede os perfis de alterarem a resolução do ecrã." } },
            { "Desktop settings autosave", new[] { "Ajustes del escritorio con guardado automático", "Definições do ambiente de trabalho com gravação automática" } },
            { "Language", new[] { "Idioma", "Idioma" } },
            { "Donate", new[] { "Donar", "Doar" } },
            { "Your settings could not be saved. Check that the settings folder is writable.", new[] { "No se pudo guardar la configuración. Comprueba que puedes escribir en la carpeta de configuración.", "Não foi possível guardar as definições. Verifica se podes escrever na pasta de configuração." } },
            { "Registered to Autostart!", new[] { "Inicio automático activado.", "Arranque automático ativado." } },
            { "Registering to Autostart failed!", new[] { "No se pudo activar el inicio automático.", "Não foi possível ativar o arranque automático." } },
            { "Updated Autostart Path!", new[] { "Ruta de inicio automático actualizada.", "Caminho de arranque automático atualizado." } },
            { "Updating Autostart Path failed!", new[] { "No se pudo actualizar la ruta de inicio automático.", "Não foi possível atualizar o caminho de arranque automático." } },
            { "Unregistered from Autostart!", new[] { "Inicio automático desactivado.", "Arranque automático desativado." } },
            { "Unregistering from Autostart failed!", new[] { "No se pudo desactivar el inicio automático.", "Não foi possível desativar o arranque automático." } },
            { "Select a game executable", new[] { "Seleccionar un ejecutable de juego", "Selecionar um executável de jogo" } },
            { "In-game vibrance", new[] { "Saturación en el juego", "Saturação no jogo" } },
            { "In-game resolution", new[] { "Resolución en el juego", "Resolução no jogo" } },
            { "Windowed or borderless mode only", new[] { "Solo en ventana o sin bordes", "Apenas em janela ou sem bordas" } },
            { "Change resolution in-game", new[] { "Cambiar la resolución en el juego", "Alterar a resolução no jogo" } },
            { "Settings for “{0}”", new[] { "Ajustes de «{0}»", "Definições de «{0}»" } },
            { "Save", new[] { "Guardar", "Guardar" } },
            { "Reload processes", new[] { "Recargar procesos", "Recarregar processos" } },
            { "Process explorer", new[] { "Explorador de procesos", "Explorador de processos" } },
            { "Programs", new[] { "Programas", "Programas" } },
            { "Full path", new[] { "Ruta completa", "Caminho completo" } },
            { "vibranceGUI Error", new[] { "Error de vibranceGUI", "Erro do vibranceGUI" } },
            { "You can run vibranceGUI only once at a time!", new[] { "vibranceGUI ya está en ejecución.", "O vibranceGUI já está em execução." } },
            { "No supported AMD or NVIDIA graphics adapter with an available driver was detected. Press Yes to open the support profile in your browser.", new[] { "No se detectó una GPU AMD o NVIDIA compatible con un controlador disponible. Pulsa Sí para abrir el perfil de soporte.", "Não foi detetada uma GPU AMD ou NVIDIA compatível com um controlador disponível. Seleciona Sim para abrir o perfil de suporte." } },
            { "Windows reports both an AMD and an NVIDIA graphics adapter with available drivers. This version of vibranceGUI can use only one graphics provider at a time.", new[] { "Windows detectó GPU AMD y NVIDIA con controladores disponibles. Esta versión admite un fabricante a la vez.", "O Windows detetou GPU AMD e NVIDIA com controladores disponíveis. Esta versão suporta um fabricante de cada vez." } },
            { "The graphics driver could not be initialized. Press OK to open the vibranceGUI guide.", new[] { "No se pudo inicializar el controlador gráfico. Pulsa Aceptar para abrir la guía de vibranceGUI.", "Não foi possível inicializar o controlador gráfico. Seleciona OK para abrir o guia do vibranceGUI." } },
            { "The NVIDIA system type could not be determined.", new[] { "No se pudo determinar el tipo de sistema NVIDIA.", "Não foi possível determinar o tipo de sistema NVIDIA." } },
            { "Changing the resolution failed: {0}", new[] { "No se pudo cambiar la resolución: {0}", "Não foi possível alterar a resolução: {0}" } },
            { "Default", new[] { "Predeterminado", "Predefinido" } },
            { "Stretch", new[] { "Estirar", "Esticar" } },
            { "Center", new[] { "Centrar", "Centrar" } },
        };
        private static readonly string[] LanguageCodes = { "en", "es", "pt" };

        internal static int SelectedIndex { get; private set; }

        internal static void Load()
        {
            string code = new SettingsController().ReadLanguage();
            SelectedIndex = Math.Max(0, Array.FindIndex(LanguageCodes, candidate =>
                string.Equals(code, candidate, StringComparison.OrdinalIgnoreCase)));
        }

        internal static bool Select(int index)
        {
            if (index < 0 || index >= LanguageCodes.Length)
                return false;

            SelectedIndex = index;
            return new SettingsController().SaveLanguage(LanguageCodes[index]);
        }

        internal static string Translate(string english)
        {
            string[] translation;
            int index = SelectedIndex;
            return index != 0 && Translations.TryGetValue(english, out translation)
                ? translation[index - 1] : english;
        }
    }
}
