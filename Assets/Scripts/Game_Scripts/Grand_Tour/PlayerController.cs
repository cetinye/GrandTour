using DG.Tweening;
using UnityEngine;

namespace GrandTour
{
	public class PlayerController : MonoBehaviour
	{
		[SerializeField] private HexController hexController;

		[Header("Car Variables")]
		[SerializeField] private int x, z;
		[SerializeField] private float rotationTweenDuration;
		[SerializeField] private AnimationCurve rotationEaseCurve;
		[SerializeField] private float moveTweenDuration;
		[SerializeField] private AnimationCurve moveEaseCurve;

		public void SetGridPosition(int x, int z)
		{
			this.x = x;
			this.z = z;
		}

		public void SetMovePosition(Vector3 destination, bool isInstant = false)
		{
			if (!isInstant)
				transform.DOMove(destination, moveTweenDuration).SetEase(moveEaseCurve);
			else
				transform.position = destination;
		}

		#region Buttons

		public void UpButton()
		{
			MoveToGrid(1, 0);
			transform.DOLocalRotate(new Vector3(0, 90, 0), rotationTweenDuration).SetEase(rotationEaseCurve);
		}

		public void DownButton()
		{
			MoveToGrid(-1, 0);
			transform.DOLocalRotate(new Vector3(0, -90, 0), rotationTweenDuration).SetEase(rotationEaseCurve);
		}

		public void UpRightButton()
		{
			if (z % 2 == 0)
				MoveToGrid(0, -1);
			else
				MoveToGrid(1, -1);

			transform.DOLocalRotate(new Vector3(0, 145, 0), rotationTweenDuration).SetEase(rotationEaseCurve);
		}

		public void UpLeftButton()
		{
			if (z % 2 == 0)
				MoveToGrid(0, 1);
			else
				MoveToGrid(1, 1);

			transform.DOLocalRotate(new Vector3(0, 35, 0), rotationTweenDuration).SetEase(rotationEaseCurve);
		}

		public void DownRightButton()
		{
			if (z % 2 == 0)
				MoveToGrid(-1, -1);
			else
				MoveToGrid(0, -1);

			transform.DOLocalRotate(new Vector3(0, -145, 0), rotationTweenDuration).SetEase(rotationEaseCurve);
		}

		public void DownLeftButton()
		{
			if (z % 2 == 0)
				MoveToGrid(-1, 1);
			else
				MoveToGrid(0, 1);

			transform.DOLocalRotate(new Vector3(0, -35, 0), rotationTweenDuration).SetEase(rotationEaseCurve);
		}

		#endregion

		private void MoveToGrid(int x, int z)
		{
			if (this.x + x < 0 || this.x + x >= hexController.gridHexXZ.GetWidth() || this.z + z < 0 || this.z + z >= hexController.gridHexXZ.GetHeight())
				return;

			SetMovePosition(hexController.gridHexXZ.GetWorldPosition(this.x + x, this.z + z));
			SetGridPosition(this.x + x, this.z + z);
		}
	}
}