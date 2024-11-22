using System;
using Furkan.Common;
using SaintsField;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;

namespace Tulip.GameWorld
{
    public class RealmShader : MonoBehaviour
    {
        [Header("Tilemaps")]
        [SerializeField, Required] Tilemap wallTilemap;
        [SerializeField, Required] Tilemap blockTilemap;
        [SerializeField, Required] Tilemap curtainTilemap;

        [Header("Shader Properties")]
        [SerializeField, Required] EntityLocationDeterminer playerLocation;
        [SerializeField] Transform portalTop;
        [SerializeField] Transform portalBottom;

        [Header("Config")]
        [SerializeField, Min(0)] float curtainRevealSpeed;
        [SerializeField, Range(0, 1)] float inactiveSaturation;
        [SerializeField, Range(0, 10)] float saturationChangeSpeed;
        [SerializeField] bool activeRealm;

        [Header("Shader Keywords")]
        [SerializeField] bool portalInSight;

        [Header("Shader Parameters")]
        [SerializeField, Range(0, 1)] float saturation;
        [SerializeField] bool shaderActiveRealm;

        private float curtainRevealProgress;

        private TilemapRenderer wallRenderer;
        private TilemapRenderer blockRenderer;
        private TilemapRenderer curtainRenderer;

        private Shader realmShader;
        private LocalKeyword keywordPortalInSight;
        private LocalKeyword keywordActiveRealm;
        private MaterialPropertyBlock propertyBlock;

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
            AssignRenderers();
            CacheKeywords();
        }

        private void Update()
        {
            TickCurtainReveal();
            TickSaturationShift();

#if UNITY_EDITOR
            CacheKeywords();
#endif
            SetKeywords();
            SetProperties();
        }

        public void SetActiveRealm(bool value) => activeRealm = value;

        // TODO: look into culling the portal inside the shader itself
        // then assign the closest portal and let the shader decide whether to use it
        public void SetPortalInSight(bool value) => portalInSight = value;

        private void AssignRenderers()
        {
            wallRenderer = wallTilemap.GetComponent<TilemapRenderer>();
            blockRenderer = blockTilemap.GetComponent<TilemapRenderer>();
            curtainRenderer = curtainTilemap.GetComponent<TilemapRenderer>();
        }

        private void CacheKeywords()
        {
            realmShader = blockRenderer.sharedMaterial.shader;

            if (realmShader != wallRenderer.sharedMaterial.shader || realmShader != blockRenderer.sharedMaterial.shader)
                Debug.LogError("Realm tilemaps have different shaders!", this);

            keywordPortalInSight = new LocalKeyword(realmShader, ShaderParams.PortalInSight);
            keywordActiveRealm = new LocalKeyword(realmShader, ShaderParams.ActiveRealm);

            var shaderIsCurtain = new LocalKeyword(realmShader, ShaderParams.IsCurtain);
            curtainRenderer.material.SetKeyword(shaderIsCurtain, true);
        }

        private void TickCurtainReveal()
        {
            float maxDeltaTime = Time.deltaTime * curtainRevealSpeed;

            float targetRevealValue = playerLocation.Location switch
            {
                EntityLocation.Outdoors => 0f,
                EntityLocation.Indoors => 1f,
                _ => throw new ArgumentOutOfRangeException()
            };

            curtainRevealProgress = Mathf.MoveTowards(curtainRevealProgress, targetRevealValue, maxDeltaTime);
        }

        private void TickSaturationShift()
        {
            if (activeRealm == shaderActiveRealm)
                return;

            float targetSaturation = activeRealm ? 1 : inactiveSaturation;
            saturation = saturation.ExpDecay(targetSaturation, saturationChangeSpeed, Time.deltaTime);

            // don't change saturation while transitioning!
            if (saturation <= inactiveSaturation + 0.01f)
                shaderActiveRealm = false;

            if (saturation >= 0.99f)
                shaderActiveRealm = true;
        }

        private void SetKeywords()
        {
            // Portal In Sight
            wallRenderer.material.SetKeyword(keywordPortalInSight, portalInSight);
            blockRenderer.material.SetKeyword(keywordPortalInSight, portalInSight);
            curtainRenderer.material.SetKeyword(keywordPortalInSight, portalInSight);

            // Active Realm
            wallRenderer.material.SetKeyword(keywordActiveRealm, shaderActiveRealm);
            blockRenderer.material.SetKeyword(keywordActiveRealm, shaderActiveRealm);
            curtainRenderer.material.SetKeyword(keywordActiveRealm, shaderActiveRealm);
        }

        private void SetProperties()
        {
            if (portalTop && portalBottom)
            {
                // only update these if there's a portal
                propertyBlock.SetVector(ShaderParams.PortalCeiling, portalTop.position);
                propertyBlock.SetVector(ShaderParams.PortalFloor, portalBottom.position);
                propertyBlock.SetFloat(ShaderParams.Saturation, saturation);
            }

            propertyBlock.SetFloat(ShaderParams.RevealProgress, curtainRevealProgress);

            wallRenderer.SetPropertyBlock(propertyBlock);
            blockRenderer.SetPropertyBlock(propertyBlock);
            curtainRenderer.SetPropertyBlock(propertyBlock);
        }

        private static class ShaderParams
        {
            // Keywords
            internal const string PortalInSight = "_PORTAL_IN_SIGHT";
            internal const string ActiveRealm = "_ACTIVE_REALM";
            internal const string IsCurtain = "_IS_CURTAIN";

            // Properties
            internal static readonly int PortalCeiling = Shader.PropertyToID("_Portal_Ceiling");
            internal static readonly int PortalFloor = Shader.PropertyToID("_Portal_Floor");
            internal static readonly int Saturation = Shader.PropertyToID("_Saturation");
            internal static readonly int RevealProgress = Shader.PropertyToID("_Reveal_Progress");
        }
    }
}
