using DG.Tweening;
using UnityEngine;

namespace Components.JobSystem.Jobs.Cleaning
{
    public class DirtStain: MonoBehaviour
    {
        [SerializeField] private float maxCleanliness = 100f;
        [SerializeField] private float cleanedDisappearDuration = 0.3f;
        [SerializeField] private SpriteRenderer spriteRenderer; // or MeshRenderer
        
        public bool IsCleaned { get; private set; }
        
        private float _currentDirtLevel;

        private void Awake()
        {
            _currentDirtLevel = maxCleanliness;
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Clean(float cleanAmount)
        {
            if (IsCleaned) return;

            _currentDirtLevel -= cleanAmount;
            
            float alpha = Mathf.Clamp01(_currentDirtLevel / maxCleanliness);
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = alpha;
                spriteRenderer.color = c;
            }

            if (_currentDirtLevel <= 0)
            {
                IsCleaned = true;
                OnFullyCleaned();
            }
        }

        private void OnFullyCleaned()
        {
            transform.DOScale(0f, cleanedDisappearDuration).OnComplete(() => gameObject.SetActive(false));
        }
    }
}