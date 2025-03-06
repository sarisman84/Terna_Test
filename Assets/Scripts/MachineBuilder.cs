using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using UnityEngine.Pool;
using Unity.VisualScripting;
using System;

namespace Terna
{
    [RequireComponent(typeof(UIDocument))]
    public class MachineBuilder : MonoBehaviour
    {
        [SerializeField] private MachinePart[] machineParts;

        [SerializeField] private InputActionProperty rotationInput;
        [SerializeField] private InputActionProperty holdInput;
        private UIDocument uiDocument;
        private VisualElement rootUI;

        private List<GameObject> assembledMachineParts = new List<GameObject>();

        void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
            rootUI = uiDocument.rootVisualElement;
            InitializeUI();
        }

        private void OnEnable()
        {
            rotationInput.action.Enable();
            holdInput.action.Enable();
        }

        private void OnDisable()
        {
            rotationInput.action.Disable();
            holdInput.action.Disable();
        }

        private void InitializeUI()
        {
            var machinePartContainer = rootUI.Q<VisualElement>("part_picker");
            foreach (var machinePart in machineParts)
            {
                var machinePartUI = new UI.MachinePartUI(machinePart, this);
                machinePartContainer.Add(machinePartUI);
            }

            var clearButton = rootUI.Q<Button>("clear_button");
            clearButton.clicked += ClearAssembly;
        }

        private Transform GetAnchorPoint()
        {
            if (assembledMachineParts.Count == 0)
            {
                return transform;
            }

            var lastPart = assembledMachineParts[assembledMachineParts.Count - 1];
            for (int i = 0; i < lastPart.transform.childCount; i++)
            {
                var child = lastPart.transform.GetChild(i);
                if (child.gameObject.name.ToLower().Contains("attachmentpoint"))
                {
                    return child;
                }
            }

            return transform;

        }

        public void ClearAssembly()
        {
            foreach (var machinePart in assembledMachineParts)
            {
                Destroy(machinePart);
            }
            assembledMachineParts.Clear();
        }

        public void AttachPart(MachinePart machinePart)
        {
            GameObject prefab = machinePart.GetPartPrefab();
            Transform anchorPoint = GetAnchorPoint();
            GameObject ins = Instantiate(prefab, anchorPoint);


            assembledMachineParts.Add(ins);
        }

        public void TryRotateAssembly()
        {

        }
    }

}
