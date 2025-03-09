using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Terna
{
	public class CameraOrbitController : MonoBehaviour
	{

		[SerializeField] private float zoomSpeed = 2f;
		[SerializeField] private float minZoom = 5f;
		[SerializeField] private float maxZoom = 20f;


		[Header("Input Actions")]
		[SerializeField] private InputActionProperty panInput;
		[SerializeField] private InputActionProperty holdInput;
		[SerializeField] private InputActionProperty zoomInput;

		private Camera cam;
		private Vector3 targetFocus;
		private Vector3 previousPosition;
		private float zoomLevel = 10.0f;

		private bool IsHoldInputTriggered() => IsHoldInputHeld() && holdInput.action.triggered;
		private bool IsHoldInputHeld() => holdInput.action.ReadValue<float>() > 0;
		private Vector3 GetViewportPointFromPanInput() => cam.ScreenToViewportPoint(panInput.action.ReadValue<Vector2>());
		private float GetZoomInputValue() => zoomInput.action.ReadValue<Vector2>().y;

		public string GetPanKey() => holdInput.action.GetBindingDisplayString(0);

		public string GetZoomKey() => zoomInput.action.GetBindingDisplayString(0);

		private void Awake()
		{
			cam = Camera.main;
		}

		private void OnEnable()
		{
			panInput.action.Enable();
			holdInput.action.Enable();
			zoomInput.action.Enable();
		}

		private void OnDisable()
		{
			panInput.action.Disable();
			holdInput.action.Disable();
			zoomInput.action.Disable();
		}



		private void Update()
		{

			if (IsHoldInputTriggered())
			{
				previousPosition = GetViewportPointFromPanInput();
			}

			if (IsHoldInputHeld())
			{
				Vector3 currentPosition = GetViewportPointFromPanInput();
				Vector3 direction = previousPosition - currentPosition;
				previousPosition = currentPosition;

				cam.transform.position = targetFocus;

				cam.transform.Rotate(new Vector3(1, 0, 0), direction.y * 180);
				cam.transform.Rotate(new Vector3(0, 1, 0), -direction.x * 180, Space.World);
				cam.transform.Translate(new Vector3(0, 0, zoomLevel));
			}

			zoomLevel -= GetZoomInputValue() * zoomSpeed;
			zoomLevel = Mathf.Clamp(zoomLevel, minZoom, maxZoom);
			cam.transform.position = targetFocus - (cam.transform.forward.normalized * zoomLevel);
		}


		/// <summary>
		/// Set the target focus for the camera to orbit around
		/// </summary>
		/// <param name="targetPosition">
		/// The position to focus the camera on
		/// </param>
		public void SetTargetFocus(Vector3 targetPosition)
		{
			targetFocus = targetPosition;
		}


	}
}
