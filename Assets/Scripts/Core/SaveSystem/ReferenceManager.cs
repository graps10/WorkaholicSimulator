using ES3Internal;

namespace Core.SaveSystem
{
    public class ReferenceManager : ES3ReferenceMgr
    {
        public static ReferenceManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(gameObject);
            else
                Instance = this;

            DontDestroyOnLoad(gameObject);
        }

#if UNITY_EDITOR
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public override void RefreshDependencies(bool isEnteringPlayMode = false)
        {
            // Empty the refId so it has to be refreshed.
            refId = null;

            ES3ReferenceMgrBase.isEnteringPlayMode = isEnteringPlayMode;

            AddDependenciesFromFolders();
            RemoveNullOrInvalidValues();
        
            ES3ReferenceMgrBase.isEnteringPlayMode = false;
        }
#endif
    }
}