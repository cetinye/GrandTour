using UnityEngine;

namespace GrandTour
{
	public class PlayerController : MonoBehaviour
	{
		public void SetMovePosition(Vector3 destination)
		{
			transform.position = destination;
		}
	}
}