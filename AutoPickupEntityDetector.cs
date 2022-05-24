using System.Collections;
//using System.Collections.Generic;
using UnityEngine;

//attach to gameplay canvas and set toggles on items

namespace MultiplayerARPG
{
    public class AutoPickupEntityDetector : MonoBehaviour
    {
        [SerializeField]
        private bool autoPickupEntityDetectorEnable;
        [SerializeField]
        protected bool sfxEnable;
        [SerializeField]
        protected bool vfxEnable;
        [SerializeField]
        protected float autoPickupDelayTime = 0.5f;
        [SerializeField]
        private bool debugEnable; 
        private int cannotAutoLootFlag = 0;        
        private bool autoPickupDelay;
        private IEnumerator coroutine;

        

        public NearbyEntityDetector ResourcesEntityDetector { get; protected set; }

        private void Awake()
        {
            if (debugEnable)
            {
                Debug.LogError("Loading _PickupExtendoEntityDetector to current scene.");
            }
            if (autoPickupEntityDetectorEnable)
            {
                GameObject tempGameObject = new GameObject("_PickupExtendoEntityDetector");
                ResourcesEntityDetector = tempGameObject.AddComponent<NearbyEntityDetector>();
                ResourcesEntityDetector.detectingRadius = GameInstance.Singleton.pickUpItemDistance;
                ResourcesEntityDetector.findItemDrop = true;
                autoPickupDelay = false;
            }            
        }

        //private void OnDestroy()
        //{
        //    if (debugEnable)
        //    {
        //        Debug.LogError("Unloading _PickupExtendoEntityDetector from current scene. " + GameInstance.PlayingCharacterEntity.name);
        //    }
        //    if (autoPickupEntityDetectorEnable)
        //    {
        //        if (ResourcesEntityDetector != null)
        //        {
        //            Destroy(ResourcesEntityDetector.gameObject);
        //        }
        //    }            
        //}

        private void Update()
        {
            if (!GameInstance.PlayingCharacterEntity.IsDead())
            {
                if (ResourcesEntityDetector.itemDrops.Count > 0 && ResourcesEntityDetector.itemDrops[0].IsAbleToLoot(GameInstance.PlayingCharacterEntity))
                {
                    BaseItem currentItem;
                    uint currentItemID = ResourcesEntityDetector.itemDrops[0].Entity.ObjectId;
                    GameInstance.Items.TryGetValue(ResourcesEntityDetector.itemDrops[0].ItemDropData.dataId, out currentItem);

                    if (!currentItem.AutoPickUpData.autoPickUp)
                    {
                        if (debugEnable)
                        {
                            Debug.LogError(currentItem.name + " not available for auto loot");
                        }
                        cannotAutoLootFlag = (int)currentItemID;
                    }
                    if (cannotAutoLootFlag == currentItemID)
                    {
                        if (debugEnable)
                        {
                            Debug.LogError("Display 'cannot autoloot this item' UI " + ResourcesEntityDetector.itemDrops[0].name);
                        }
                        //++ add UI element here - cannot loot this item UI
                        return;
                    }
                    if (ResourcesEntityDetector.itemDrops[0].ItemDropData.putOnPlaceholder || ResourcesEntityDetector.itemDrops[0].CacheRandomItems != null)
                    {
                        if (currentItem.AutoPickUpData.characterDatabases.Length > 0)
                        {
                            int currentItemDataId = 0;
                            int playerCharacterDataId = GameInstance.PlayingCharacterEntity.DataId;

                            for (int i = 0; i < currentItem.AutoPickUpData.characterDatabases.Length; i++)
                            {
                                if (currentItem.AutoPickUpData.characterDatabases[i] != null && currentItem.AutoPickUpData.characterDatabases[i].DataId == playerCharacterDataId)
                                {
                                    currentItemDataId = currentItem.AutoPickUpData.characterDatabases[i].DataId;
                                }
                            }

                            if (currentItemDataId != playerCharacterDataId)
                            {
                                if (debugEnable)
                                {
                                    Debug.LogError(playerCharacterDataId + " player doesnt match item " + currentItemDataId);
                                }
                                return;
                            }
                            else
                            {
                                if (debugEnable)
                                {
                                    Debug.LogError(playerCharacterDataId + " player matches item " + currentItemDataId);
                                }
                            }
                        }
                        else
                        {
                            if (debugEnable)
                            {
                                Debug.LogError(" characterDatabases length is " + currentItem.AutoPickUpData.characterDatabases.Length);
                            }
                        }
                        if (GameInstance.PlayingCharacterEntity.Level < currentItem.AutoPickUpData.autoPickUpByMinLevel)
                        {
                            if (debugEnable)
                            {
                                Debug.LogError(GameInstance.PlayingCharacterEntity.name + " level is too low for autopickup");
                            }
                            return;
                        }
                        if (GameInstance.PlayingCharacterEntity.Level > currentItem.AutoPickUpData.autoPickUpByMaxLevel)
                        {
                            if (debugEnable)
                            {
                                Debug.LogError(GameInstance.PlayingCharacterEntity.name + " level is passed max for autopickup");
                            }
                            return;
                        }
                        if (currentItem.AutoPickUpData.autoPickUp)
                        {
                            if (debugEnable)
                            {
                                Debug.LogError("autopickup init for: " + currentItem.name + currentItemID);
                            }
                            ItemPickup(currentItemID, currentItem);
                        }
                    }
                }
            }
        }

        private void ItemPickup(uint currentItemID, BaseItem currentItem)
        {
            if (currentItemID > 0)
            {                
                if (!autoPickupDelay)
                {
                    if (debugEnable)
                    {
                        Debug.LogError("CallServerPickupItem fired ");
                    }
                    coroutine = AutoPickupCoroutine(currentItem);
                    GameInstance.PlayingCharacterEntity.CallServerPickupItem(currentItemID);
                    autoPickupDelay = true;
                    StartCoroutine(coroutine);
                }                
                else
                {
                    if (debugEnable)
                    {
                        Debug.LogError("blocked by autoPickupDelay ");
                    }
                }                
            }
        }

        private IEnumerator AutoPickupCoroutine(BaseItem currentItem)
        {
            if (debugEnable)
            {
                Debug.LogError("AutoPickupCoroutine coroutine fired ");
            }
            if (autoPickupDelay)
            {
                if (currentItem.AutoPickUpData.autoPickUpSound != null && sfxEnable)
                {
                    AudioManager.PlaySfxClipAtPoint(currentItem.AutoPickUpData.autoPickUpSound, GameInstance.PlayingCharacterEntity.transform.position);
                    if (debugEnable)
                    {
                        Debug.LogError("APlaySfxClipAtPoint fired ");
                    }
                }
                if (currentItem.AutoPickUpData.autoPickupParticle != null && vfxEnable)
                {
                    if (debugEnable)
                    {
                        Debug.LogError("autoPickupParticle fired ");
                    }
                    GameObject tempParticle;
                    tempParticle = Instantiate(currentItem.AutoPickUpData.autoPickupParticle, GameInstance.PlayingCharacterEntity.transform);
                    Destroy(tempParticle, 2f);
                }
                yield return new WaitForSeconds(autoPickupDelayTime);
                autoPickupDelay = false;
            }
        }
    }
}

