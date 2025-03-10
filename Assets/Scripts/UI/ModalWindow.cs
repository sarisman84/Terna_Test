using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace Terna.UI
{
	public class ModalWindow : VisualElement
	{
		private VisualElement content;
		private Action closeEvent;

		public ModalWindow()
		{
			InitializeBackground();
		}


		public ModalWindow(VisualElement newContent)
		{
			InitializeBackground();
			SetContent(newContent);

		}

		public ModalWindow(VisualElement newContent, Action onCloseEvent)
		{
			InitializeBackground();
			SetContent(newContent);
			closeEvent = onCloseEvent;

		}

		public void SetContent(VisualElement newContent)
		{
			if (content != null)
			{
				content.RemoveFromHierarchy();
			}

			content = MoveContentToCenter(newContent);
			Add(content);
		}

		public void OnCloseEvent()
		{
			closeEvent?.Invoke();
		}


		private (LengthUnit, float) CalculatePosition(StyleLength size)
		{
			float result = 0;
			switch (size.value.unit)
			{
				case LengthUnit.Percent:
					result = (100f - size.value.value) / 2.0f;
					break;
				case LengthUnit.Pixel:
					result = size.value.value / 2.0f;
					break;
				default:
					result = size.value.value / 2.0f;
					break;
			}

			return (size.value.unit, result);
		}

		private VisualElement MoveContentToCenter(VisualElement content)
		{
			content.style.position = Position.Absolute;

			var (heightUnit, topAndBottomPosition) = CalculatePosition(content.style.height);
			var (widthUnit, leftAndRightPosition) = CalculatePosition(content.style.width);

			content.style.top = new Length(topAndBottomPosition, heightUnit);
			content.style.left = new Length(leftAndRightPosition, widthUnit);
			content.style.right = new Length(leftAndRightPosition, widthUnit);
			content.style.bottom = new Length(topAndBottomPosition, heightUnit);

			return content;
		}
		private void InitializeBackground()
		{
			style.position = Position.Absolute;
			style.height = Length.Percent(100);
			style.width = Length.Percent(100);
			style.backgroundColor = Color.black;
			style.opacity = 0.5f;

			RegisterCallback<PointerDownEvent>(evt =>
			{
				evt.StopImmediatePropagation();
				ModalWindowExtensions.ClosePopup(this);
			});
		}


	}

	public static class ModalWindowExtensions
	{
		/// <summary>
		/// Shows a popup window with the given content and adds it to the parent element
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="content"></param>
		/// <returns>A popup window </returns>
		public static ModalWindow CreatePopup(this VisualElement parent, VisualElement content, Action onCloseEvent = null)
		{
			ModalWindow popup = new ModalWindow(content, onCloseEvent);
			parent.Add(popup);
			return popup;
		}

		/// <summary>
		/// Closes the popup window
		/// </summary>
		/// <param name="popup"></param>
		public static void ClosePopup(this ModalWindow popup)
		{
			popup.OnCloseEvent();
			popup.RemoveFromHierarchy();
		}

	}
}
