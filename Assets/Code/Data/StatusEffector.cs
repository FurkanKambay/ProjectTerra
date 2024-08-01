using System.Collections.Generic;
using UnityEngine;

namespace Tulip.Data
{
    public class StatusEffector : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] HealthBase health;

        [Header("Config")]
        [SerializeField] StatusEffectData[] startingEffects;

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private List<StatusEffect> effects = new();

        private void Awake()
        {
            foreach (StatusEffectData effect in startingEffects)
                effects.Add(effect.Create(health, health));
        }

        private void Update()
        {
            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i] is not { } effect)
                    return;

                effect.Tick(Time.deltaTime);

                if (effect.IsDone)
                    effects.RemoveAt(i);
            }
        }
    }
}
