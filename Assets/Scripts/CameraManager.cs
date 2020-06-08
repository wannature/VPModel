using HoloToolkit.Unity;
using UnityEngine;

namespace Academy
{
    public class CameraManager: Singleton<CameraManager>
    {
        //private float expandAnimationCompletionTime;

        // Store a bool for whether our astronaut model is expanded or not.
        private bool isModelExpanding = false;

        private void Update()
        {
            if (isModelExpanding )
            {
                isModelExpanding = false;

            }
        }

        public void ResetModelCommand()
        {
            // Reset local variables.
            isModelExpanding = false;

            GameObject currentModel = ExpandModel.Instance.gameObject;
            GameObject expandedModel = ExpandModel.Instance.ExpandedModel;

            // Disable the expanded model.
            expandedModel.SetActive(false);

            // Enable the idle model.
            currentModel.SetActive(true);

            

            ExpandModel.Instance.Reset();
        }

        public void ExpandModelCommand()
        {
            // Swap out the current model for the expanded model.
            GameObject currentModel = ExpandModel.Instance.gameObject;
            GameObject expandedModel = ExpandModel.Instance.ExpandedModel;

            expandedModel.transform.position = currentModel.transform.position;
            expandedModel.transform.rotation = currentModel.transform.rotation;
            expandedModel.transform.localScale = currentModel.transform.localScale;

            currentModel.SetActive(false);
            expandedModel.SetActive(true);

            // Play animation.  Ensure the Loop Time check box is disabled in the inspector for this animation to play it once.
            Animator[] expandedAnimators = expandedModel.GetComponentsInChildren<Animator>();
            // Set local variables for disabling the animation.
            

            // Set the expand model flag.
            isModelExpanding = true;

            ExpandModel.Instance.Expand();
        }
    }
}