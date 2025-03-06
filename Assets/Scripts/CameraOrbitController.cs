using UnityEngine;
using UnityEngine.InputSystem;

namespace Terna
{
	public class CameraOrbitController : MonoBehaviour
	{
		[SerializeField] private float cameraSpeed = 5f;
		[SerializeField] private float zoomSpeed = 2f;
		[SerializeField] private float minZoom = 5f;
		[SerializeField] private float maxZoom = 20f;
		[SerializeField] private float panSpeed = 20f;

		[Header("Input Actions")]
		[SerializeField] private InputActionProperty panInput;
		[SerializeField] private InputActionProperty holdInput;
		[SerializeField] private InputActionProperty zoomInput;

		private Camera camera;
		private Vector3 targetFocus;
		private Vector3 previousPosition;
		private float zoomLevel = 10.0f;

		private bool IsHoldInputTriggered() => IsHoldInputHeld() && holdInput.action.triggered;
		private bool IsHoldInputHeld() => holdInput.action.ReadValue<float>() > 0;
		private Vector3 GetViewportPointFromPanInput() => camera.ScreenToViewportPoint(panInput.action.ReadValue<Vector2>());
		private float GetZoomInputValue() => zoomInput.action.ReadValue<Vector2>().y;

		private void Awake()
		{
			camera = Camera.main;
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

				camera.transform.position = targetFocus;

				camera.transform.Rotate(new Vector3(1, 0, 0), direction.y * 180);
				camera.transform.Rotate(new Vector3(0, 1, 0), -direction.x * 180, Space.World);
				camera.transform.Translate(new Vector3(0, 0, zoomLevel));
			}

			zoomLevel -= GetZoomInputValue() * zoomSpeed;
			zoomLevel = Mathf.Clamp(zoomLevel, minZoom, maxZoom);
			camera.transform.position = targetFocus - (camera.transform.forward.normalized * zoomLevel);
		}



		public void SetTargetFocus(Vector3 targetPosition)
		{
			targetFocus = targetPosition;
		}
	}
}
