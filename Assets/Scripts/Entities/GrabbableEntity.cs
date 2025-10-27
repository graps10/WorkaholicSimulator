using System;
using Hypertonic.Modules.UltimateSockets;
using Core.Interfaces;
using Core.Utilities;
using Entities.Molds;
using Hypertonic.Modules.UltimateSockets.PlaceableItems;
using Hypertonic.Modules.UltimateSockets.Sockets;
using MelenitasDev.SoundsGood;
using Unity.VisualScripting;
using UnityEngine;

namespace Entities
{
    public class GrabbableEntity: Entity, IGrabbable
    {
        protected const float TORQUE_FOR_PLACEMENT_IN_SOCKET = 0.5f;
        
        private const float SFX_Volume = 0.6f;
        
        public event GrabbableObjectEvent OnGrabbed;
        public event GrabbableObjectEvent OnReleased;
        
        public event Action<Socket> OnEnteredPlaceableZone;
        public event Action<Socket> OnExitedPlaceableZone;

        public bool CanBeRotated => FurnitureMold != null && FurnitureMold.CanBeRotatedBeforePlaced;

        protected FurnitureMold FurnitureMold { get; set; }

        protected bool _isGrabbed;
        protected Collider _furnitureCollider;
        protected Collider _socketDetectionCollider;
        protected ColliderManager _grabColliderManager;
        protected Rigidbody _rigidbody;
        
        protected bool playerIsManuallyEquippingIntoSocket;

        private Sound _grabSound;
        private Sound _equipSound;
        private Sound _unequipSound;

        private void Awake()
        {
            if (FurnitureMold != null) SetMold(FurnitureMold);

            _furnitureCollider = gameObject.GetOrAddComponent<BoxCollider>();
            _furnitureCollider.isTrigger = true;

            _rigidbody = gameObject.GetOrAddComponent<Rigidbody>();

            var placeableItem = GetComponentInChildren<PlaceableItem>(true);
            if (placeableItem == null)
            {
                Debug.Log(gameObject.name + " doesn't have PlaceableEquipment prefab attached!");
                return;
            }

            _socketDetectionCollider = placeableItem.PlaceableItemCollider.Collider;

            _grabColliderManager = placeableItem.SocketGrabCollider.ColliderManager;
            _grabColliderManager.SetCollider(_furnitureCollider);

            /*_grabSound = new Sound(SFX.)
                .SetVolume(SFX_Volume);

            _unequipSound = new Sound(SFX.)
                .SetVolume(SFX_Volume);

            _equipSound = new Sound(SFX.)
                .SetVolume(SFX_Volume);*/
        }

        public virtual void SetMold(FurnitureMold mold)
        {
            FurnitureMold = mold;
        }

        public void EnablePhysics(bool enable)
        {
            if (_rigidbody == null)
                return;

            _rigidbody.isKinematic = !enable;
            _furnitureCollider.isTrigger = !enable;
        }

        public bool IsGrabbing() => _isGrabbed;

        public virtual void Grab()
        {
            OnGrabbed?.Invoke();
            _isGrabbed = true;
            playerIsManuallyEquippingIntoSocket = true;
            _furnitureCollider.enabled = false;
            _socketDetectionCollider.enabled = true;

            EnablePhysics(false);
        }

        public virtual void Release()
        {
            OnReleased?.Invoke();
            _isGrabbed = false;
            _furnitureCollider.enabled = true;
            _socketDetectionCollider.enabled = false;

            EnablePhysics(true);
        }

        public virtual void HandlePlacedInSocket(Socket socket, PlaceableItem placeableItem)
        {
            EnablePhysics(false);

            Entity entity = socket.GetComponentInParent<Entity>();

            if (entity == null)
                return;

            /*if (socket is EquipmentSocket equipmentSocket)
            {
                var element = equipmentSocket.EquipmentMoldsReference[socket.transform.GetSiblingIndex()];
                element.Mold = FurnitureMold;
                element.ZRotation = transform.localEulerAngles.z;
            }*/


            /*_currentEquipmentManager = entity.EquipmentManager;
            _currentEquipmentManager.Equip(this);*/

            if (playerIsManuallyEquippingIntoSocket)
            {
                _equipSound.PlayFromCamera();
                ApplyPlacementEffect(entity.GetRigidbody(), socket.transform.position);
            }

            playerIsManuallyEquippingIntoSocket = false;
        }

        public virtual void HandleRemovedFromSocket(Socket socket, PlaceableItem placeableItem)
        {
            /*if (socket is EquipmentSocket equipmentSocket)
                if (equipmentSocket.EquipmentMoldsReference.Length != 0)
                {
                    var element = equipmentSocket.EquipmentMoldsReference[socket.transform.GetSiblingIndex()];
                    element.Mold = null;
                    element.ZRotation = 0;
                }

            _currentEquipmentManager?.Unequip(this);*/

            if (socket.StackableItemController.Settings.Stackable)
                _grabSound.PlayFromCamera();
            else
                _unequipSound.PlayFromCamera();
        }

        public void HandleEnteredPlaceableZone(Socket socket, PlaceableItem placeableItem)
            => OnEnteredPlaceableZone?.Invoke(socket);

        public void HandleExitedPlaceableZone(Socket socket, PlaceableItem placeableItem)
            => OnExitedPlaceableZone?.Invoke(socket);

        private static void ApplyPlacementEffect(Rigidbody rigidbody, Vector3 socketPosition)
        {
            Vector3 pushDirection = (rigidbody.position - socketPosition).normalized;
            Vector3 torqueAxis = Vector3.Cross(Vector3.up, pushDirection).normalized;
            Vector3 torqueVector = torqueAxis * TORQUE_FOR_PLACEMENT_IN_SOCKET;

            rigidbody.AddTorque(torqueVector, ForceMode.VelocityChange);
            // play effect
        }
    }
}