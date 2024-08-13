using System.Collections;
using FMOD.Studio;
using FMODUnity;
using SaintsField;
using Tulip.Data;
using Tulip.Data.Gameplay;
using UnityEngine;

namespace Tulip.Audio
{
    public class HealthAudio : MonoBehaviour
    {
        [Header("References")]
        [SerializeField, Required] SaintsInterface<Component, IHealth> health;

        [Header("FMOD Events")]
        [SerializeField] EventReference hurtEvent;

        private PARAMETER_DESCRIPTION paramAliveness;

        private IEnumerator Start()
        {
            while (!RuntimeManager.HaveAllBanksLoaded)
                yield return null;

            EventDescription description = RuntimeManager.GetEventDescription(hurtEvent);
            description.getParameterDescriptionByName("Aliveness", out paramAliveness);
        }

        private void OnEnable() => health.I.OnHurt += HandleHurt;
        private void OnDisable() => health.I.OnHurt -= HandleHurt;

        private void HandleHurt(HealthChangeEventArgs damage)
        {
            EventInstance hurtSfx = RuntimeManager.CreateInstance(hurtEvent);
            RuntimeManager.AttachInstanceToGameObject(hurtSfx, transform);

            hurtSfx.setParameterByID(paramAliveness.id, damage.Target.IsAlive.GetHashCode());

            hurtSfx.start();
            hurtSfx.release();
        }
    }
}
