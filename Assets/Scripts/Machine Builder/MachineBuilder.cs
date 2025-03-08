using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using System;
using Terna.UI;
using Terna.Gizmos;
using Terna.Serializer;
using System.Linq;


namespace Terna
{
    public class AssembledMachinePart
    {
        public GameObject part;
        public Transform anchorPoint;
        public Bounds anchorPointBounds;
        public Bounds bounds;
        public MachinePart.PartType type;
        public Anchor anchorIndicator;
        public Select selectIndicator;
        public int machinePartIndex;
        public int parentIndex;
        public int childIndex;
    }

    [RequireComponent(typeof(UIDocument))]
    public class MachineBuilder : MonoBehaviour
    {
        [SerializeField] private MachinePart[] machineParts;
        [Header("Input Actions")]
        [SerializeField] private InputActionProperty mouseInput;
        [SerializeField] private InputActionProperty selectInput;
        [SerializeField] private InputActionProperty deleteInput;

        [Header("Camera")]
        [SerializeField] private CameraOrbitController cameraOrbitController;
        [Header("Gizmos")]
        [SerializeField] private Anchor anchorIndicatorPrefab;
        [SerializeField] private Select selectIndicatorPrefab;

        private UIDocument uiDocument;
        private VisualElement rootUI;
        private MachineBuilderUI machinePartPicker;
        private MachineSerializer machineSerializer;
        private Camera cam;

        private int selectedAssembledPartIndex = -1;
        private Transform targetAnchorPoint;
        private bool selectedAnchorPointFlag = true;

        public event Action updateEvent;


        private List<AssembledMachinePart> assembledMachineParts = new List<AssembledMachinePart>();

        private static int[] ToIndexArray<T>(T[] array, int startingIndex = 0, int endingIndex = -1)
        {
            int[] indices = new int[array.Length - Math.Max(endingIndex, 0)];
            for (int i = startingIndex; i < Math.Max(array.Length, endingIndex); i++)
            {
                indices[i - startingIndex] = i;
            }
            return indices;
        }

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
            rootUI = uiDocument.rootVisualElement;
            cam = Camera.main;
            machineSerializer = new MachineSerializer(this);

            targetAnchorPoint = transform;

            InitializeUI();
        }

        private void InitializeUI()
        {
            machinePartPicker = new MachineBuilderUI(this, rootUI);
            machinePartPicker.UpdatePartSelection(ToIndexArray(machineParts));
        }


        private void OnEnable()
        {
            SetInputActive(true);
        }

        private void OnDisable()
        {
            SetInputActive(false);
        }

        private void Update()
        {
            var (resultIndex, hasSelectedAnchorPoint) = TrySelectPartOfAssembly();
            for (int i = 0; i < assembledMachineParts.Count; ++i)
            {
                AssembledMachinePart part = assembledMachineParts[i];
                Color color = resultIndex == i ? Color.yellow : Color.grey;
                color = selectedAssembledPartIndex == i ? Color.green : color;
                part.selectIndicator?.SetColor(color);
            }
            if (HasTriggeredSelectInput())
            {
                if (resultIndex != -1)
                {
                    UpdateSelection(resultIndex, hasSelectedAnchorPoint);
                }
                else
                {
                    ResetSelection();
                }
            }

            if (selectedAssembledPartIndex != -1 && HasTriggeredDeleteInput())
            {
                ClearAssemblyPart(selectedAssembledPartIndex);
            }

            updateEvent?.Invoke();
        }

        public int GetSelectedAssembledPartIndex() => selectedAssembledPartIndex;
        public MachinePart GetMachinePart(int machinePart) => machineParts[machinePart];
        public MachinePart[] GetMachineParts() => machineParts;
        public bool HasSelectedAnchorPoint() => selectedAnchorPointFlag;
        public List<AssembledMachinePart> GetAssembledMachineParts() => assembledMachineParts;
        public MachineSerializer GetMachineSerializer() => machineSerializer;


        private bool HasTriggeredSelectInput() => selectInput.action.ReadValue<float>() > 0 && selectInput.action.triggered;
        private bool HasTriggeredDeleteInput() => deleteInput.action.ReadValue<float>() > 0 && deleteInput.action.triggered;
        private bool IsRootAnchor(int parentIndex) => parentIndex < 0 || parentIndex >= assembledMachineParts.Count;
        private bool HasParent(AssembledMachinePart part) => !IsRootAnchor(part.parentIndex);
        private bool IsIndexValid(int holderIndex) => holderIndex >= 0 && holderIndex < assembledMachineParts.Count;



        public void SetInputActive(bool value)
        {
            if (value)
            {
                mouseInput.action.Enable();
                selectInput.action.Enable();
                deleteInput.action.Enable();
                return;
            }

            mouseInput.action.Disable();
            selectInput.action.Disable();
            deleteInput.action.Disable();
        }

        private Transform GetPartAnchorPoint(GameObject part)
        {
            for (int i = 0; i < part.transform.childCount; ++i)
            {
                Transform child = part.transform.GetChild(i);
                if (child.CompareTag("AnchorPoint"))
                {
                    return child;
                }
            }

            return null;
        }



        public void ClearAssembly()
        {
            foreach (var machinePart in assembledMachineParts)
            {
                Destroy(machinePart.part);
            }
            assembledMachineParts.Clear();
            cameraOrbitController.SetTargetFocus(transform.position);
        }


        /// <summary>
        /// Build the assembly from the serialized machine parts
        /// </summary>
        /// <param name="serializedMachineParts"> A collection of machine part instance ids to create an assembly from </param>
        public void BuildAssembly(List<SerializedMachinePart> serializedMachineParts)
        {
            ClearAssembly();
            int currentIndex = -1;
            foreach (SerializedMachinePart serializablePart in serializedMachineParts)
            {
                int machinePartIndex = Array.FindIndex(machineParts, (part) => part.GetInstanceID() == serializablePart.partID);
                currentIndex = CreateNewAssemblyPart(machinePartIndex, currentIndex);
            }
        }

        /// <summary>
        /// Clear an assembly part from the assembly
        /// </summary>
        /// <param name="assemblyPartIndexToClear"> The index of a part to remove from the assembly </param>
        public void ClearAssemblyPart(int assemblyPartIndexToClear)
        {
            AssembledMachinePart assemblyPart = assembledMachineParts[assemblyPartIndexToClear];
            Destroy(assemblyPart.part);

            ClearChildAssembly(assemblyPart.childIndex);
            assembledMachineParts.RemoveAt(assemblyPartIndexToClear);

            int parentIndex = assemblyPart.parentIndex;
            if (parentIndex >= 0 && parentIndex < assembledMachineParts.Count)
            {
                assembledMachineParts[parentIndex].childIndex = -1;
                UpdateSelection(parentIndex, true);
            }
            else
            {
                ResetSelection();
            }

        }

        /// <summary>
        /// Update an assembly part with a new part
        /// </summary>
        /// <param name="newPart"> The index of the new machine part </param>
        /// <param name="assembledPartIndex">The index of the assembled part to replace </param>
        public void UpdateAssemblyPart(int newPart, int assembledPartIndex)
        {
            AssembledMachinePart assembledPart = assembledMachineParts[assembledPartIndex];
            MachinePart partDesc = machineParts[newPart];

            //Destroy the current part
            Destroy(assembledPart.part);

            //Reassemble the part with the new part
            assembledPart = ReassembleMachinePart(assembledPart, partDesc);

            //Rebuild the child assembly
            RebuildChildAssembly(assembledPart.childIndex);

            //Update the assembled part entry
            assembledMachineParts[assembledPartIndex] = assembledPart;
        }

        /// <summary>
        /// Create a new assembly part
        /// </summary>
        /// <param name="newPart">The index of the new machine part</param>
        /// <param name="holderIndex">An optional index indicating the parent of the new part</param>
        /// <returns>The index of the newrly assembled part (or -1 if it failed to create a part) </returns>
        public int CreateNewAssemblyPart(int newPart, int holderIndex)
        {
            if (assembledMachineParts.Count > 0 && IsIndexValid(holderIndex) && !HasPartAnAnchor(assembledMachineParts[holderIndex].machinePartIndex))
            {
                return -1;
            }

            MachinePart partDesc = machineParts[newPart];

            AssembledMachinePart assemblyPart = new AssembledMachinePart();
            Transform point = IsRootAnchor(holderIndex) ? transform : assembledMachineParts[holderIndex].anchorPoint;

            GameObject instancedPart = Instantiate(partDesc.GetPartPrefab(), point);

            assemblyPart.part = instancedPart;
            assemblyPart.anchorPoint = GetPartAnchorPoint(instancedPart);
            if (assemblyPart.anchorPoint)
            {
                assemblyPart.anchorPointBounds = new Bounds(assemblyPart.anchorPoint.position, Vector3.one);
                assemblyPart.anchorIndicator = Instantiate(anchorIndicatorPrefab, assemblyPart.anchorPoint);
            }
            else
            {
                assemblyPart.anchorPointBounds = new Bounds(Vector3.zero, Vector3.zero);
                assemblyPart.anchorIndicator = null;
            }

            assemblyPart.bounds = instancedPart.GetComponent<Collider>().bounds;
            assemblyPart.type = partDesc.GetPartType();

            assemblyPart.machinePartIndex = newPart;
            assemblyPart.parentIndex = holderIndex;
            assemblyPart.childIndex = -1;
            assemblyPart.machinePartIndex = newPart;

            assemblyPart.selectIndicator = Instantiate(selectIndicatorPrefab, instancedPart.transform);
            assemblyPart.selectIndicator.SetTransform(assemblyPart.bounds.center, assemblyPart.bounds.size);

            if (!IsRootAnchor(holderIndex))
            {
                assembledMachineParts[holderIndex].childIndex = assembledMachineParts.Count;
            }


            assembledMachineParts.Add(assemblyPart);

            UpdateSelection(assembledMachineParts.Count - 1, true);

            return assembledMachineParts.Count - 1;

        }




        /// <summary>
        /// Checks if a part contains an object that is tagged "AnchorPoint"
        /// </summary>
        /// <param name="newPart">The part to check with</param>
        /// <returns>True if the inputed part has an object that is tagged "AnchorPoint" </returns>
        private bool HasPartAnAnchor(int newPart)
        {
            if (newPart <= 0 || newPart >= machineParts.Length)
            {
                return false;
            }

            GameObject part = machineParts[newPart].GetPartPrefab();
            for (int i = 0; i < part.transform.childCount; ++i)
            {
                Transform child = part.transform.GetChild(i);
                if (child.CompareTag("AnchorPoint"))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Reassemble a machine part with a new or existing part
        /// </summary>
        /// <param name="assembledPart">
        /// An assembled machine part to reassemble
        /// </param>
        /// <param name="newPartDesc">
        /// An optional new machine part to reassemble the part with
        /// </param>
        /// <returns> A reassembled machine part </returns>
        private AssembledMachinePart ReassembleMachinePart(AssembledMachinePart assembledPart, MachinePart newPartDesc = null)
        {
            MachinePart partDesc = newPartDesc ?? machineParts[assembledPart.machinePartIndex];
            Transform anchorPoint = HasParent(assembledPart) ? assembledMachineParts[assembledPart.parentIndex].anchorPoint : transform;

            assembledPart.part = Instantiate(partDesc.GetPartPrefab(), anchorPoint);
            assembledPart.anchorPoint = GetPartAnchorPoint(assembledPart.part);
            if (assembledPart.anchorPoint)
            {
                assembledPart.anchorPointBounds = new Bounds(assembledPart.anchorPoint.position, Vector3.one);
                assembledPart.anchorIndicator = Instantiate(anchorIndicatorPrefab, assembledPart.anchorPoint);
            }
            else
            {
                assembledPart.anchorPointBounds = new Bounds(Vector3.zero, Vector3.zero);
                assembledPart.anchorIndicator = null;
            }

            assembledPart.bounds = assembledPart.part.GetComponent<Collider>().bounds;
            assembledPart.selectIndicator = Instantiate(selectIndicatorPrefab, assembledPart.part.transform);
            assembledPart.selectIndicator.SetTransform(assembledPart.bounds.center, assembledPart.bounds.size);

            Debug.Log("Reassembled part: " + partDesc.GetPartPrefab().name);


            return assembledPart;
        }

        /// <summary>
        /// Clear a child assembly from the assembly collection using recursion
        /// </summary>
        /// <param name="index">
        /// An index of an assembly to clear from the collection
        /// </param>
        /// <param name="maxDepth">
        /// An optional maximum depth to rebuild the child assembly (so that the recursion doesnt cause a stack overflow)
        /// </param>
        private void ClearChildAssembly(int index, int maxDepth = 100)
        {
            if (index < 0 || index >= assembledMachineParts.Count || maxDepth <= 0)
            {
                return;
            }

            AssembledMachinePart childPart = assembledMachineParts[index];
            //Recursively clear the child assembly
            ClearChildAssembly(childPart.childIndex, maxDepth - 1);

            assembledMachineParts.RemoveAt(index);
        }

        /// <summary>
        /// Rebuild a child assembly from the assembly collection using recursion
        /// </summary>
        /// <param name="index">
        /// An index of an assembly to rebuild from the collection
        /// </param>
        /// <param name="maxDepth">
        /// An optional maximum depth to rebuild the child assembly (so that the recursion doesnt cause a stack overflow)
        /// </param>
        private void RebuildChildAssembly(int index, int maxDepth = 100)
        {
            if (index < 0 || index >= assembledMachineParts.Count || maxDepth <= 0)
            {
                return;
            }

            AssembledMachinePart assembledPart = ReassembleMachinePart(assembledMachineParts[index]);
            RebuildChildAssembly(assembledPart.childIndex, maxDepth - 1);
        }

        /// <summary>
        /// Attemps to select a part of the assembly
        /// </summary>
        /// <returns>
        /// A tuple containing the index of the selected part and a flag indicating if the selected part is an anchor point
        /// </returns>
        private (int, bool) TrySelectPartOfAssembly()
        {
            Vector2 mousePosition = mouseInput.action.ReadValue<Vector2>();
            Ray ray = cam.ScreenPointToRay(mousePosition);
            float closestDist = float.MaxValue;
            int closestIndex = -1;
            bool hasSelectedAnchorPoint = false;

            //Iterate through the assembled machine parts and select the closest part
            for (int i = 0; i < assembledMachineParts.Count; i++)
            {
                AssembledMachinePart machinePart = assembledMachineParts[i];
                float dist = Vector3.Distance(machinePart.bounds.center, cam.transform.position);

                if (dist < closestDist)
                {
                    closestDist = dist;
                    //Check if the anchor point bounds intersect with the ray first
                    if (machinePart.anchorPointBounds.IntersectRay(ray))
                    {
                        closestDist = dist;
                        closestIndex = i;
                        hasSelectedAnchorPoint = true;
                        continue;
                    }

                    //Check if the part bounds intersect with the ray
                    if (machinePart.bounds.IntersectRay(ray))
                    {
                        closestDist = dist;
                        closestIndex = i;
                        hasSelectedAnchorPoint = false;
                        continue;
                    }
                }
            }

            // Return the closest index and the flag indicating if the selected part is an anchor point
            return (closestIndex, hasSelectedAnchorPoint);
        }

        /// <summary>
        /// Update the selection of the assembly
        /// </summary>
        /// <param name="index">
        /// The index of the part to select
        /// </param>
        /// <param name="hasSelectedAnchorPoint">
        /// A flag indicating if the selected part is an anchor point
        /// </param>
        private void UpdateSelection(int index, bool hasSelectedAnchorPoint = false)
        {
            selectedAssembledPartIndex = index;
            selectedAnchorPointFlag = hasSelectedAnchorPoint;

            AssembledMachinePart selectedPart = assembledMachineParts[index];
            AssembledMachinePart parentPart = HasParent(selectedPart) ? assembledMachineParts[selectedPart.parentIndex] : null;
            targetAnchorPoint = parentPart?.anchorPoint ?? transform;
            cameraOrbitController.SetTargetFocus(targetAnchorPoint.position);


            assembledMachineParts.ForEach((part) =>
            {
                part.anchorIndicator?.SetVisibility(false);

                part.anchorIndicator?.SetColor(Color.grey);
                part.selectIndicator?.SetColor(Color.grey);
            });
            selectedPart.anchorIndicator?.SetVisibility(true);
            if (hasSelectedAnchorPoint)
            {
                selectedPart.anchorIndicator?.SetColor(Color.green);
            }
            selectedPart.selectIndicator?.SetColor(Color.green);
        }

        /// <summary>
        /// Reset the selection of the assembly
        /// </summary>
        private void ResetSelection()
        {
            selectedAssembledPartIndex = -1;
            selectedAnchorPointFlag = true;
            targetAnchorPoint = transform;
            cameraOrbitController.SetTargetFocus(targetAnchorPoint.position);
        }

        /// <summary>
        /// Draw gizmos for the machine builder (for debug purposes)
        /// </summary>
        private void OnDrawGizmos()
        {
            cam = cam ?? Camera.main;
            var (resultIndex, hasSelectedAnchorPoint) = TrySelectPartOfAssembly();

            for (int i = 0; i < assembledMachineParts.Count; ++i)
            {
                AssembledMachinePart part = assembledMachineParts[i];

                UnityEngine.Gizmos.color = resultIndex == i && !hasSelectedAnchorPoint ? Color.yellow : Color.red;
                UnityEngine.Gizmos.DrawWireCube(part.bounds.center, part.bounds.size);

                UnityEngine.Gizmos.color = resultIndex == i && hasSelectedAnchorPoint ? Color.yellow : Color.blue;
                UnityEngine.Gizmos.DrawWireCube(part.anchorPointBounds.center, part.anchorPointBounds.size);
            }
        }


    }

}
