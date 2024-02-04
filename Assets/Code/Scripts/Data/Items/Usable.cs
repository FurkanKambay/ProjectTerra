using Game.Data.Interfaces;
using UnityEngine;

namespace Game.Data.Items
{
    /// <summary>
    /// A basic item that can be used.
    /// </summary>
    [CreateAssetMenu(fileName = "Usable Item", menuName = "Items/Usable Item")]
    public class Usable : Item, IUsable
    {
        public float Cooldown => cooldown;
        public float ChargeTime => chargeTime;
        public float SwingTime => swingTime;

        [Header("Usable Data")]
        [SerializeField, Min(0)] float cooldown = 0.5f;
        [SerializeField, Min(0)] float chargeTime = 0.1f;
        [SerializeField, Min(0)] float swingTime = 0.1f;
    }
}
