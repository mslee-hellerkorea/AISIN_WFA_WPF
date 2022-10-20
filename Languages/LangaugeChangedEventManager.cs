using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AISIN_WFA.Languages
{
    class LangaugeChangedEventManager : WeakEventManager
    {
        public static void AddListener(TranslationManager source, IWeakEventListener listener)
        {
            CurrentManager.ProtectedAddListener(source, listener);
        }

        public static void RemoveListener(TranslationManager source, IWeakEventListener listener)
        {
            CurrentManager.ProtectedRemoveListener(source, listener);
        }

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            DeliverEvent(sender, e);
        }

        protected override void StartListening(object source)
        {
            var manager = (TranslationManager)source;
            manager.LanguageChanged += OnLanguageChanged;
        }

        protected override void StopListening(object source)
        {
            var manager = (TranslationManager)source;
            manager.LanguageChanged -= OnLanguageChanged;
        }

        private static LangaugeChangedEventManager CurrentManager
        {
            get
            {
                Type managerType = typeof(LangaugeChangedEventManager);
                var manager = (LangaugeChangedEventManager)GetCurrentManager(managerType);
                if (manager == null)
                {
                    manager = new LangaugeChangedEventManager();
                    SetCurrentManager(managerType, manager);
                }
                return manager;
            }
        }
    }
}
