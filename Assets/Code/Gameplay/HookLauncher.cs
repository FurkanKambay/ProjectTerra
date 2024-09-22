using SaintsField;
using Tulip.Data;
using Tulip.Data.Items;
using UnityEngine;

namespace Tulip.Gameplay
{
    public enum HookState
    {
        None,
        RopeTraveling,
        Attached,
        Pulling,
        Reached
    }

    public class HookLauncher : MonoBehaviour
    {
        [Header("References")]
        [SerializeField, Required] SaintsInterface<Component, IWielderBrain> wielderBrain;
        [SerializeField, Required] SaintsInterface<Component, IJumperBrain> jumperBrain;
        [SerializeField, Required] Rigidbody2D body;

        [Header("Visuals")]
        [SerializeField, Required] LineRenderer lineRenderer;

        [Header("Config")]
        [SerializeField] HookData data;
        [SerializeField] LayerMask hookableLayers;
        [SerializeField, Range(0, 3)] float reachDistance;
        [SerializeField] bool autoPullWhenHooked;
        [SerializeField] bool autoUnhookWhenReached;

        [Header("Debug")]
        [SerializeField] HookState hookState;

        public HookState HookState
        {
            get => hookState;
            private set
            {
                hookState = value;
                UpdateRope();
            }
        }

        private bool IsRopeTooLong => (RopeEnd - RopeOrigin).sqrMagnitude > rangeSquared;

        private Vector2 RopeOrigin
        {
            get => lineRenderer.GetPosition(0);
            set => lineRenderer.SetPosition(0, value);
        }

        private Vector2 RopeEnd
        {
            get => lineRenderer.GetPosition(1);
            set => lineRenderer.SetPosition(1, value);
        }

        private Vector2? attachPoint;
        private float rangeSquared;
        private float reachDistanceSquared;

        private void Awake()
        {
            rangeSquared = data.Range * data.Range;
            reachDistanceSquared = reachDistance * reachDistance;
        }

        private void Update()
        {
            UpdateRopePosition();

            switch (hookState)
            {
                case HookState.None when wielderBrain.I.WantsToHook:
                    LaunchRope();
                    break;
                case HookState.RopeTraveling or HookState.Attached when IsRopeTooLong:
                    CancelPull();
                    break;
                case HookState.RopeTraveling:
                    TickRopeTravel();
                    break;
                case HookState.Attached when autoPullWhenHooked || wielderBrain.I.WantsToHook:
                    StartPulling();
                    break;
                case HookState.Pulling when jumperBrain.I.WantsToJump:
                case HookState.Reached when autoUnhookWhenReached || jumperBrain.I.WantsToJump:
                    CancelPull();
                    break;
                default: break;
            }
        }

        private void FixedUpdate()
        {
            if (HookState is HookState.Pulling or HookState.Reached)
                TickPull();
        }

        private void LaunchRope()
        {
            attachPoint = GetHookPoint();
            UpdateRope();

            if (attachPoint.HasValue)
                HookState = HookState.RopeTraveling;
        }

        private void TickRopeTravel()
        {
            if (!attachPoint.HasValue)
                return;

            Vector2 currentPosition = RopeEnd;

            if (currentPosition != attachPoint.Value)
            {
                float maxDistanceDelta = data.RopeLaunchSpeed * Time.deltaTime;
                RopeEnd = Vector2.MoveTowards(currentPosition, attachPoint.Value, maxDistanceDelta);
                return;
            }

            HookState = HookState.Attached;
        }

        private void StartPulling() => HookState = HookState.Pulling;
        private void CancelPull() => HookState = HookState.None;

        private void TickPull()
        {
            if (!attachPoint.HasValue)
                return;

            var vector = Vector2.ClampMagnitude(attachPoint.Value - (Vector2)transform.position, 1f);
            body.linearVelocity = vector * data.PullStrength;

            if (vector.sqrMagnitude < reachDistanceSquared)
                HookState = HookState.Reached;
        }

        private Vector2? GetHookPoint()
        {
            if (!wielderBrain.I.AimPosition.HasValue)
                return null;

            Vector2 origin = transform.position;
            Vector2 direction = (wielderBrain.I.AimPosition.Value - origin).normalized;

            RaycastHit2D hit = Physics2D.Raycast(origin, direction, data.Range, hookableLayers);
            return hit ? hit.point : null;
        }

        private void UpdateRopePosition()
        {
            if (HookState == HookState.None)
                return;

            RopeOrigin = transform.position;
        }

        private void UpdateRope()
        {
            bool enableRope = hookState != HookState.None;
            lineRenderer.enabled = enableRope;

            if (enableRope)
                return;

            RopeOrigin = transform.position;
            RopeEnd = transform.position;
        }

        private void OnDrawGizmos()
        {
            if (!attachPoint.HasValue)
                return;

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, attachPoint.Value);
        }
    }
}
