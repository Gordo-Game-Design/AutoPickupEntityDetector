using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract partial class BaseItem
    {
        [Category("Auto Pick Up Extension ~ GG")]
        public AutoPickUpData AutoPickUpData;
    }

    [System.Serializable]
    public class AutoPickUpData
    {
        [Header("Auto Pickup Options")]
        [Tooltip("Unless modelForAutoPickup is chosen then Drop Model from this Item asset will be used by default")]
        public bool autoPickUp = false;

        [Tooltip("Affects only classes in this list")]
        public PlayerCharacter[] characterDatabases;

        [Tooltip("Restrict autopickup to minimum character level - 0 to disable")]
        public int autoPickUpByMinLevel = 1;

        [Tooltip("Restrict autopickup to maximum character level- 0 to disable")]
        public int autoPickUpByMaxLevel = 99;

        [Tooltip("Optional particle effect to play on pickup")]
        public GameObject autoPickupParticle;

        [Tooltip("Optional sound effect to play on pickup")]
        public AudioClip autoPickUpSound;
    }
}