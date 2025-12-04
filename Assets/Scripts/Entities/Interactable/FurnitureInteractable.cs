using Components.PlacementSystem;
using Core.Interfaces;
using Entities.Molds;
using Hypertonic.Modules.UltimateSockets.PlaceableItems;
using UI.Popup;
using UnityEngine;

namespace Entities.Interactable
{
    public class FurnitureInteractable : MonoBehaviour, IInteractable
    {
        private PlaceableItem _placeableItem;
        private FurnitureEntity _entity;

        private void Awake()
        {
            _placeableItem = GetComponentInChildren<PlaceableItem>();
            _entity = GetComponent<FurnitureEntity>();
        }

        public void Interact()
        {
            /*if (!ApartmentController.Instance.IsDecorationModeActive)
                return;*/

            if (_entity == null || _entity.SourceMold is not FurnitureMold furnitureMold)
            {
                Debug.LogError("Entity or Mold configuration error!");
                return;
            }
            
            FurnitureActionPopup.Create(
                _placeableItem, 
                furnitureMold,
                (item) => PlacementManager.Instance.StartRotationMode(item),
                (item, mold) => PlacementManager.Instance.RemoveFromSocket(item, mold)
            );
        }
    }
}