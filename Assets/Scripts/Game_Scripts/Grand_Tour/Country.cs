using DG.Tweening;
using UnityEngine;

namespace GrandTour
{
	public class Country : MonoBehaviour
	{
		public int width;
		public int height;
		public SoundType countryMusic;

		[Header("Animation Variables")]
		[SerializeField] private SpriteRenderer outline;
		[SerializeField] private float timeToMoveUp;
		[SerializeField] private float timeToFade;
		private Sequence sequence;

		public Sequence SpawnAnim()
		{
			transform.position = new Vector3(transform.position.x, -2, transform.position.z);

			sequence = DOTween.Sequence();
			sequence.Append(transform.DOMoveY(0f, timeToMoveUp));
			sequence.AppendInterval(1f);
			sequence.Append(outline.DOFade(0f, timeToFade));
			return sequence;
		}

		public void SetOutline(bool state)
		{
			outline.enabled = state;
		}
	}
}