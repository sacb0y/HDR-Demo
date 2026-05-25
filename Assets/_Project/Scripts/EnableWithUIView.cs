using System;
using _Project.UI.Flow.Runtime;
using UnityEngine;

namespace _Project.Scripts
{
    public class EnableWithUIView : MonoBehaviour
    {
        public UIFlowRunner runner;
        public string viewName;
        public GameObject go;

        private void OnEnable()
        {
            if (runner != null)
                runner.OnViewChanged += OnViewChanged;
        }

        private void OnDisable()
        {
            if (runner != null)
                runner.OnViewChanged -= OnViewChanged;
        }

        private void OnViewChanged(string currentViewName)
        {
            if (go != null)
            {
                go.SetActive(currentViewName == viewName);
            }
        }
    }
}
