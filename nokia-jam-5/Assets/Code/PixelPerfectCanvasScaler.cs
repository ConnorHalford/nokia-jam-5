using UnityEngine;
using UnityEngine.UI;

namespace Solitaire
{
	[ExecuteAlways, RequireComponent(typeof(CanvasScaler))]
	public class PixelPerfectCanvasScaler : MonoBehaviour
	{
		private CanvasScaler _canvasScaler = null;
		private PixelPerfectCameraExecuteAlways _pixelPerfectCamera = null;

		private void Awake()
		{
			ApplyScale();
		}

		private void LateUpdate()
		{
			ApplyScale();
		}

		private void ApplyScale()
		{
			// Need to always check and lookup because we want it to also work in Scene view via ExecuteAlways
			if (_canvasScaler == null)
			{
				_canvasScaler = GetComponent<CanvasScaler>();
			}
			if (_pixelPerfectCamera == null)
			{
				_pixelPerfectCamera = Camera.main.GetComponent<PixelPerfectCameraExecuteAlways>();
			}
			if (_canvasScaler != null && _pixelPerfectCamera != null)
			{
				try
				{
					_canvasScaler.scaleFactor = _pixelPerfectCamera.pixelRatio;
				}
				catch
				{ }
			}
		}
	}
}
