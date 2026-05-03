using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessUI
{
    public static class LanguageManager
    {
        public static LanguageType CurrentLanguage { get; private set; } = LanguageType.English;

        public static event Action LanguageChanged;

        public static void SetLanguage(LanguageType lang)
        {
            if (CurrentLanguage != lang)
            {
                CurrentLanguage = lang;
                LanguageChanged?.Invoke();
            }
        }
    }
}