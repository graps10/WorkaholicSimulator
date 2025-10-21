using System;
using System.Collections.Generic;
using System.IO;
using Core;
using Core.SaveSystem;
using TMPro;
using UI.CanvasScreens;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

namespace UI.SaveSystemUI
{
    public class SaveSelectorManager : CanvasScreen
    {
        [Header("Save Selector")]
        [SerializeField] private RectTransform bodySaveSelector;
        [SerializeField] private ScrollRect saveLotScrollRect;
        [SerializeField] private RectTransform spawnSaveLotTransform;
        [SerializeField] private RectTransform defaultSaveButtonRect;
        [SerializeField] private CustomButtonController defaultSaveButton;
        [SerializeField] private GameObject scrollBar;
        [SerializeField] private CustomButtonController closeSelectPopupButton;
        
        [Header("Input Save Name")]
        [SerializeField] private GameObject saveLotPanel; 
        [SerializeField] private TMP_InputField saveNameInput; 
        [SerializeField] private CustomButtonController saveInputReaderButton; 
        [SerializeField] private CustomButtonController closeCreatePopupButton;
        
        [Space(10)]
        [SerializeField] private GameObject prefabLot; 
        [SerializeField] private GameObject deleteLotPanelPrefab;
        
        [SerializeField] private Button closeTrigger;

        private static SaveSelectorManager _instance;
        private Dictionary<string, GameObject> _saveLots = new(); 
    
        private GameObject _deleteLotPanel;

        private Action _saveChosenCallback;
        private Action _escCloseSelector;    
        private Action _escCloseInputPanel;
        private Action _escCloseDeletePanel;

        public static SaveSelectorManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<SaveSelectorManager>();
                    if (_instance == null)
                        Debug.LogError("SaveSelectorManager not found in the scene make sure it is present");
                }
                return _instance;
            }
            private set => _instance = value;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject); 
                return;
            }

            Instance = this; 
        }

        private void Start() => Initialize();
        
        public override void Initialize()
        {
            AddListenersToCanvasScreenButtons();
            UpdateSaveLots(); 
        }

        public void ShowBodySaveSelectorFirstStart(Action callback)
        {
            if (GetSaveFiles().Count == 0)
                ShowSaveLotPanel(true);
            else
                ShowBodySaveSelector(true);

            _saveChosenCallback = callback;
        }
        
        public void ShowBodySaveSelector(bool active)
        {
            if (bodySaveSelector != null)
            {
                bodySaveSelector.gameObject.SetActive(active);
                closeTrigger.gameObject.SetActive(active);
                UpdateScrollBarVisibility();
            }
              
            else Debug.LogWarning("Body SaveSelector was not initialized");

            if (active)
            {
                _escCloseSelector ??= () => ShowBodySaveSelector(false);
                ReactToCancel.SubscribeToCancel(_escCloseSelector);
            }
            else
            {
                CloseDeletePanel();
                ShowSaveLotPanel(false);
                ReactToCancel.UnsubscribeFromCancel(_escCloseSelector);
            }

            _saveChosenCallback = null;
        }
        
        private void ShowSaveLotPanel(bool show)
        {
            saveLotPanel.SetActive(show);
            closeTrigger.gameObject.SetActive(show);

            if (show)
            {
                _escCloseInputPanel ??= () => ShowSaveLotPanel(false);
                ReactToCancel.SubscribeToCancel(_escCloseInputPanel);
            }
            else
                ReactToCancel.UnsubscribeFromCancel(_escCloseInputPanel);
            
            _saveChosenCallback = null;
        }

        private void UpdateSaveLots()
        {
            foreach (var saveLot in _saveLots.Values)
                Destroy(saveLot);
            
            _saveLots.Clear();
            
            var saveFiles = GetSaveFiles();
            
            foreach (var saveFile in saveFiles)
                CreateSaveLot(Path.GetFileNameWithoutExtension(saveFile));
        }
        
        private void UpdateScrollBarVisibility()
        {
            if (bodySaveSelector == null || scrollBar == null)
                return;

            var contentHeight = bodySaveSelector.rect.height;
            var totalChildHeight = defaultSaveButtonRect.rect.height;

            foreach (var saveLot in _saveLots.Values)
            {
                if (saveLot == null) continue;

                var rect = saveLot.GetComponent<RectTransform>();
                if (rect != null)
                    totalChildHeight += rect.rect.height;
            }
            
            bool needsScrolling = totalChildHeight > contentHeight;

            if (!needsScrolling)
            {
                var anchoredPos = spawnSaveLotTransform.anchoredPosition;
                anchoredPos.y = 0f;
                spawnSaveLotTransform.anchoredPosition = anchoredPos; // Reset save lots content position
            }
            
            saveLotScrollRect.enabled = needsScrolling;
            scrollBar.SetActive(needsScrolling);
        }

        private List<string> GetSaveFiles()
        {
            var allFiles = ES3.GetFiles();
            var saveFiles = new List<string>();

            foreach (var file in allFiles)
            {
                if (file.EndsWith(".es3", StringComparison.OrdinalIgnoreCase) && file != SaveManager.LAST_SLOT_FILE)
                {
                    saveFiles.Add(file);
                }
            }

            return saveFiles;
        }
        
        private void ReadNameAndCreateLot()
        {
            string nameText = saveNameInput.text.Trim();
            if (string.IsNullOrEmpty(nameText))
            {
                Debug.LogWarning("The name of the conservation cannot be empty!");
                return;
            }
            
            CreateNewSave(nameText);
            CreateSaveLot(nameText); 
            saveNameInput.text = "";
        }

        private void CreateSaveLot(string saveName)
        {
            saveName = Path.GetFileNameWithoutExtension(saveName);

            if (_saveLots.ContainsKey(saveName))
            {
                if (SaveManager.EnableSaveLoadDebugLogs) Debug.LogWarning($"The slot '{saveName}' already exists!");
                return;
            }
            
            GameObject saveLot = Instantiate(prefabLot, spawnSaveLotTransform);
            SaveLotButtonContainer buttons = saveLot.GetComponent<SaveLotButtonContainer>();

            if (buttons != null)
            {
                InitializeSaveButton(buttons.SaveLotButton,saveName);
                InitializeDeleteButton(buttons.DeleteLotButton,saveName);
            }
            else Debug.LogError("Buttons = null");

            _saveChosenCallback?.Invoke();
            _saveChosenCallback = null;

            _saveLots.Add(saveName, saveLot);
            ShowBodySaveSelector(false);
        }
        
        private void LoadSave(string saveName)
        {
            SaveManager.SetCurrentSaveSlot(saveName);
        
            bool loaded = SaveManager.LoadProgress();

            if (loaded)
            {
               if (SaveManager.EnableSaveLoadDebugLogs)Debug.Log($"Save load: {saveName}");
                Player.Instance.ApplyLoadedProgress();
                SaveManager.SaveProgress();

                _saveChosenCallback?.Invoke();
                _saveChosenCallback = null;
            }
            else
                Debug.LogWarning($"Safe loading error: {saveName}");
        }
        
        private void CreateNewSave(string saveName)
        {
            SaveManager.SetCurrentSaveSlot(saveName);
            SaveManager.ResetProgress();
            SaveManager.SaveProgress();
            if (SaveManager.EnableSaveLoadDebugLogs)Debug.Log($" Create new save {saveName}");
        }
        
        private void CreateDeletePanel(string saveName)
        {
            _deleteLotPanel = Instantiate(deleteLotPanelPrefab, transform);
            
            DeleteLotButtonsContainer buttons = _deleteLotPanel.GetComponent<DeleteLotButtonsContainer>();

            if (buttons != null)
            {
                buttons.ConfirmButton.onClick.AddListener(() =>
                {
                    DeleteSave(saveName);
                    UpdateScrollBarVisibility();
                    CloseDeletePanel();
                });

                buttons.CancelButton.onClick.AddListener(CloseDeletePanel);
            }
            else
                Debug.LogError("Buttons = null");
            
            _escCloseDeletePanel ??= CloseDeletePanel;
            ReactToCancel.SubscribeToCancel(_escCloseDeletePanel);
        }
        
        private void CloseDeletePanel()
        {
            if (_deleteLotPanel == null) 
                return;

            Destroy(_deleteLotPanel);
            _deleteLotPanel = null;

            ReactToCancel.UnsubscribeFromCancel(_escCloseDeletePanel);
        }

        private void InitializeSaveButton(CustomButtonController customButton, string saveName)
        {
            customButton.SetText(saveName);

            if(SaveManager.IsSaveFileValid(saveName))
            {
                customButton.onClick.AddListener(() => LoadSave(saveName));
                customButton.onClick.AddListener(() => ShowBodySaveSelector(false));
            }
            else customButton.interactable = false;
        }

        private void InitializeDeleteButton(CustomButtonController customButton, string saveName)
        {
            customButton.onClick.AddListener(() => CreateDeletePanel(saveName));
        }
        
        protected override void AddListenersToCanvasScreenButtons()
        {
            defaultSaveButton.onClick.AddListener(() => ShowSaveLotPanel(true)); 
            saveInputReaderButton.onClick.AddListener(ReadNameAndCreateLot); 
            closeSelectPopupButton.onClick.AddListener(()=> ShowBodySaveSelector(false));
            closeTrigger.onClick.AddListener(() => ShowBodySaveSelector(false));
            closeCreatePopupButton.onClick.AddListener(() => ShowSaveLotPanel(false));
        }

        protected override void RemoveListenersFromCanvasScreenButtons()
        {
            defaultSaveButton.onClick.RemoveAllListeners(); 
            saveInputReaderButton.onClick.RemoveAllListeners(); 
            closeSelectPopupButton.onClick.RemoveAllListeners();
            closeCreatePopupButton.onClick.RemoveAllListeners();
            closeTrigger.onClick.RemoveAllListeners();
        }
        
        private void DeleteSave(string saveName)
        {
            if (_saveLots.ContainsKey(saveName))
            {
                string saveFilePath = $"{saveName}.es3";
                if (ES3.FileExists(saveFilePath))
                {
                    ES3.DeleteFile(saveFilePath);
                    if (SaveManager.EnableSaveLoadDebugLogs)  Debug.Log($"Save deleted: {saveName}");
                }
                else
                    Debug.LogWarning($"Save file not found: {saveName}");
            
                Destroy(_saveLots[saveName]);
                _saveLots.Remove(saveName);
            }
            else
                Debug.LogWarning($"Save lot not found: {saveName}");
        }
    }
}

