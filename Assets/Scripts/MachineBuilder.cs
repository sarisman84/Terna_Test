using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using System;
using Terna.UI;


namespace Terna
{
    public class AssembledMachinePart
    {
        public int childAssembledMachinePartIndex;
        public GameObject part;
        public Bounds bounds;
        public MachinePart.PartType type;
    }

    [RequireComponent(typeof(UIDocument))]
    public class MachineBuilder : MonoBehaviour
    {
        [SerializeField] private MachinePart[] machineParts;
        [Header("Input Actions")]
        [SerializeField] private InputActionProperty mouseInput;
        [SerializeField] private InputActionProperty selectInput;
        [Header("Camera")]
        [SerializeField] private CameraOrbitController cameraOrbitController;
        private UIDocument uiDocument;
        private VisualElement rootUI;
        private MachineBuilderUI machinePartPicker;
        private Camera cam;

        private int selectedAssembledPartIndex = -1;



        private List<AssembledMachinePart> assembledMachineParts = new List<AssembledMachinePart>();

        public int GetSelectedAssembledPartIndex() => selectedAssembledPartIndex;

        void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
            rootUI = uiDocument.rootVisualElement;
            cam = Camera.main;

            InitializeUI();
        }

        private void OnEnable()
        {
            mouseInput.action.Enable();
            selectInput.action.Enable();
        }

        private void OnDisable()
        {
            mouseInput.action.Disable();
            selectInput.action.Disable();
        }

        private void Update()
        {
            if (HasTriggeredHoldInput())
            {
                TrySelectPartOfAssembly();
            }
        }

        private bool HasTriggeredHoldInput()
        {
            return selectInput.action.ReadValue<float>() > 0 && selectInput.action.triggered;
        }

        private void InitializeUI()
        {
            machinePartPicker = new MachineBuilderUI(this, rootUI, ClearAssembly);
            machinePartPicker.UpdatePartSelection(machineParts);
        }

        private Transform GetAnchorPoint()
        {
            if (assembledMachineParts.Count == 0)
            {
                return transform;
            }
            var targetIndex = selectedAssembledPartIndex >= 0 ? selectedAssembledPartIndex : assembledMachineParts.Count - 1;
            var targetPart = assembledMachineParts[targetIndex].part;
            for (int i = 0; i < targetPart.transform.childCount; i++)
            {
                var child = targetPart.transform.GetChild(i);
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
                Destroy(machinePart.part);
            }
            assembledMachineParts.Clear();
            machinePartPicker.ResetSelectPartLabel();
            cameraOrbitController.SetTargetFocus(transform.position);
        }

        // Attach a new part to the selected part
        public void AttachPart(MachinePart newPart, int parentPart)
        {
            bool parentExists = parentPart >= 0 && parentPart < assembledMachineParts.Count;
            // If the parent part has a child, destroy the child before attaching the new part
            if(parentExists && HasChildAssembledPart(parentPart))
            {
                int childIndex = assembledMachineParts[parentPart].childAssembledMachinePartIndex;
                Destroy(assembledMachineParts[childIndex].part);
                assembledMachineParts.RemoveAt(childIndex);
            }

            // Attach the new part to the parent part
            GameObject prefab = newPart.GetPartPrefab();
            Transform anchorPoint = GetAnchorPoint();
            GameObject ins = Instantiate(prefab, anchorPoint);

            AssembledMachinePart assembledMachinePart = new AssembledMachinePart
            {
                part = ins,
                bounds = ins.GetComponent<Collider>().bounds,
                childAssembledMachinePartIndex = -1,
                type = newPart.GetPartType()
            };
            assembledMachineParts.Add(assembledMachinePart);
            // Update the parent part's child index
            if (parentExists)
            {
                assembledMachineParts[parentPart].childAssembledMachinePartIndex = assembledMachineParts.Count - 1;
            }

            UpdateSelection(assembledMachineParts.Count - 1);
        }

        private bool HasChildAssembledPart(int parentPart)
        {
            int childIndex = assembledMachineParts[parentPart].childAssembledMachinePartIndex;
            return childIndex >= 0 && childIndex < assembledMachineParts.Count;
        }

        private void TrySelectPartOfAssembly()
        {
            Vector2 mousePosition = mouseInput.action.ReadValue<Vector2>();
            Ray ray = cam.ScreenPointToRay(mousePosition);
            for (int i = 0; i < assembledMachineParts.Count; i++)
            {
                AssembledMachinePart machinePart = assembledMachineParts[i];
                if (machinePart.bounds.IntersectRay(ray))
                {
                    UpdateSelection(i);
                    return;
                }
            }
        }

        private void UpdateSelection(int index)
        {
            selectedAssembledPartIndex = index;
            AssembledMachinePart selectedPart = assembledMachineParts[index];
            machinePartPicker.UpdateSelectedPartLabel(selectedPart);
            cameraOrbitController.SetTargetFocus(selectedPart.bounds.center);
        }


    }

}
