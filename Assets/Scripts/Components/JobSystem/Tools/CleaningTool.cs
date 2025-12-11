using Components.JobSystem.Jobs.Cleaning;
using UnityEngine;

namespace Components.JobSystem.Tools
{
    public class CleaningTool : JobTool
    {
        [Header("Cleaning Settings")] 
        [SerializeField] private float cleaningRate = 50f;
        [SerializeField] private float reachDistance = 2.5f;
        [SerializeField] private LayerMask dirtLayer;
        [SerializeField] private Transform rayOrigin;

        protected override void OnToolUsage(bool isInputActive)
        {
            // If we don't move and don't press the button - we don't clean
            if (!isInputActive && !IsMoving()) return;
            if (Camera.main == null) return;
            
            Transform originT = rayOrigin != null ? rayOrigin : Camera.main.transform;
            Ray ray = new Ray(originT.position, originT.forward);

            // TODO: Add visual effect

            if (Physics.Raycast(ray, out RaycastHit hit, reachDistance, dirtLayer))
            {
                if (hit.collider.TryGetComponent(out DirtStain dirt))
                    dirt.Clean(cleaningRate * Time.deltaTime);
            }
        }
    }
}