using SaintsField;
using Tulip.Data;
using UnityEngine;

namespace Tulip.Character
{
    public class CharacterFacer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField, Required] SpriteRenderer sprite;
        [SerializeField, Required] SaintsInterface<Component, ICharacterBrain> brain;

        private void Update()
        {
            if (brain.I.HorizontalMovement != 0)
            {
                sprite.flipX = brain.I.HorizontalMovement < 0;
                return;
            }

            if (!brain.I.AimPosition.HasValue)
                return;

            Vector2 targetVector = brain.I.AimPosition.Value - (Vector2)transform.position;
            sprite.flipX = targetVector.x < 0;
        }
    }
}
