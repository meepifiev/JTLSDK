using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Simulation
{
    public class GameViewHost
    {
        private const string RootName = "jtl-simulation-root";
        private const string RootClass = "jtl-simulation-root";

        private readonly Type _gameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");
        private readonly StyleSheet _styleSheet;
        private readonly List<VisualElement> _attached = new List<VisualElement>();
        private readonly List<EditorWindow> _windows = new List<EditorWindow>();

        public GameViewHost(StyleSheet styleSheet)
        {
            _styleSheet = styleSheet;
            Refresh();
        }

        public bool HasGameView => _windows.Count > 0;

        public void Attach(VisualElement element)
        {
            if (_attached.Contains(element))
            {
                return;
            }

            _attached.Add(element);

            foreach (EditorWindow window in _windows)
            {
                Root(window).Add(element);
            }
        }

        public void Detach(VisualElement element)
        {
            _attached.Remove(element);
            element.RemoveFromHierarchy();
        }

        public void Refresh()
        {
            _windows.Clear();

            if (_gameViewType == null)
            {
                return;
            }

            foreach (UnityEngine.Object window in Resources.FindObjectsOfTypeAll(_gameViewType))
            {
                if (window is EditorWindow editorWindow)
                {
                    _windows.Add(editorWindow);
                }
            }

            if (_windows.Count == 0)
            {
                return;
            }

            EditorWindow primary = _windows[0];
            VisualElement root = Root(primary);

            foreach (VisualElement element in _attached)
            {
                if (element.parent != root)
                {
                    root.Add(element);
                }
            }
        }

        private VisualElement Root(EditorWindow window)
        {
            VisualElement root = window.rootVisualElement.Q<VisualElement>(RootName);

            if (root != null)
            {
                return root;
            }

            root = new VisualElement { name = RootName, pickingMode = PickingMode.Ignore };
            root.AddToClassList(RootClass);

            if (_styleSheet != null)
            {
                root.styleSheets.Add(_styleSheet);
            }

            window.rootVisualElement.Add(root);
            return root;
        }
    }
}
