using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace Solitaire
{
	[RequireComponent(typeof(CanvasScaler))]
	public class PixelPerfectCanvasScaler : MonoBehaviour
	{
		private void LateUpdate()
		{
			CanvasScaler canvasScaler = GetComponent<CanvasScaler>();
			PixelPerfectCamera pixelPerfectCamera = Camera.main.GetComponent<PixelPerfectCamera>();
			canvasScaler.scaleFactor = pixelPerfectCamera.pixelRatio;
			enabled = false;	// Just run once at the end of the first frame
		}
	}
}
