using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GrandTour
{
	public class GameManager : MonoBehaviour
	{
		[SerializeField] private RenderPipelineAsset defaultRenderPipelineAsset;
		[SerializeField] private RenderPipelineAsset overrideRenderPipelineAsset;
		[SerializeField] private UniversalAdditionalCameraData mainCameraAdditionalData;

		//switch render pipeline to enable post processing
		#region RenderPipeline Switch        

		void Awake()
		{
			SwitchDefaultRenderPipeline();
		}

		void OnDestroy()
		{
			SwitchDefaultRenderPipeline();
		}

		void SwitchDefaultRenderPipeline()
		{
			GraphicsSettings.defaultRenderPipeline = overrideRenderPipelineAsset;
			QualitySettings.renderPipeline = overrideRenderPipelineAsset;

			mainCameraAdditionalData.SetRenderer(0);
			mainCameraAdditionalData.renderPostProcessing = true;
		}

		#endregion
	}
}