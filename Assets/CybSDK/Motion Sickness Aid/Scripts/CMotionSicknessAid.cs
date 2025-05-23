/*
 Script that creates a vignette and/or grid to decrease motion sickness in VR.
 Based on the VR Tunneling Pro plugin for Unity: https://github.com/sigtrapgames/VrTunnellingPro-Unity
 */

using System.Collections.Generic;
using UnityEngine;

namespace CybSDK
{
    [RequireComponent(typeof(Camera))]
    public class CMotionSicknessAid : MonoBehaviour
    {
        public enum VisualEffect
        {
            Vignette,
            Grid,
            VignetteGrid,
        }

        public enum EffectIntensity
        {
            Low,
            Medium,
            High,
        }

        private VisualEffect _visualEffect;
        [Tooltip("Adjust the effect shown to the user.")]
        [SerializeField]
        private VisualEffect editorVisualEffect; // Field for adjusting the visual effect in the editor
        public VisualEffect visualEffect
        {
            get { return _visualEffect; }
            set
            {
                _visualEffect = value;
                switch (_visualEffect)
                {
                    case VisualEffect.Vignette:
                        drawSkybox = false;
                        effectFeather = 0.1f;
                        break;
                    case VisualEffect.Grid:
                        drawSkybox = true;
                        effectFeather = 0.5f;
                        break;
                    case VisualEffect.VignetteGrid:
                        drawSkybox = true;
                        effectFeather = 0.1f;
                        break;
                }
            }
        }

        private EffectIntensity _effectIntensity;
        [Tooltip("Adjust the coverage of the view of the user with the effect.")]
        [SerializeField]
        private EffectIntensity editorEffectIntensity; // Field for adjusting the effect intensity in the editor
        public EffectIntensity effectIntensity
        {
            get { return _effectIntensity; }
            set
            {
                _effectIntensity = value;
                switch (_effectIntensity)
                {
                    case EffectIntensity.Low:
                        effectCoverage = 0.7f;
                        break;
                    case EffectIntensity.Medium:
                        effectCoverage = 0.85f;
                        break;
                    case EffectIntensity.High:
                        effectCoverage = 1.0f;
                        break;
                }
            }
        }
        [Tooltip("Sets the color the user sees in the periphery. Only works for the Vignette visual effect.")]
        public Color vignetteColor = Color.black;

        [Tooltip("For debug purposes.")]
        public bool forceEffect = false;

        [Tooltip("Reference to the skybox that is used for the grid effect.")]
        public Cubemap effectSkybox;
        [Tooltip("Reference to Transform of the GameObject that is moved by the player. For example, the CVirtPlayerController object.")]
        public Transform characterController;

        // Feather around cut-off as fraction of screen. Max feather should be 0.5f
        private float effectFeather = 0.1f;
        // Coverage of effect as fraction of screen. Max coverage should be 1.0f
        private float effectCoverage = 0.75f;
        // Skybox is activated for grid effect
        private bool drawSkybox;

        private Vector3 lastPos = Vector3.zero;
        private float lastFx = 0f;

        private Mesh irisMesh;
        private Material irisMatOuter, irisMatInner;
        private int propFxInner, propFxOuter;
        private int propEyeProjection, propEyeToWorld;
        private int propColor, propSkybox;
        private Matrix4x4[] eyeToWorld = new Matrix4x4[2];
        private Matrix4x4[] eyeProjection = new Matrix4x4[2];

        private List<Object> toDestroy = new List<Object>();

        private Camera cam;

        private const float FX_FILTER = 0.8f;
        private const int RQUEUE_LAST = 5000;
        private const float COVERAGE_MIN = 0.65f;
        private const float SPEED_THRESHOLD = 0.1f;
        private const float SPEED_MAX = 0.4f;
        private const string KEYWORD_SKYBOX = "TUNNEL_SKYBOX";
        private const string PROP_OUTER = "_FxOuter";
        private const string PROP_INNER = "_FxInner";
        private const string PROP_EYEPRJ = "_EyeProjection";
        private const string PROP_EYEMAT = "_EyeToWorld";
        private const string PROP_COLOR = "_Color";
        private const string PROP_SKYBOX = "_Skybox";
        private const string PATH_SHADERS = "Hidden/CybSDK/";
        private const string PATH_SHADER = "CMSAVertex";
        private const string PATH_MESHES = "Meshes/";
        private const string PATH_IRISMESH = "Iris";
        private const string PATH_SKYBOXES = "Skyboxes/";
        private const string PATH_SKYBOX = "CMSA_Skybox_CageSmall";

        private void OnValidate()
        {
            if (visualEffect != editorVisualEffect)
                visualEffect = editorVisualEffect;
            else if (editorEffectIntensity != effectIntensity)
                effectIntensity = editorEffectIntensity;
        }

        void Awake()
        {
            if (characterController == null)
            {
                CLogger.LogWarning("No character controller set for Motion Sickness Aid. Please set the character controller in the inspector.");
                characterController = GetComponentInParent<CharacterController>().transform;
                if (characterController == null)
                {
                    CLogger.LogError("No character controller found for Motion Sickness Aid. Please set the character controller in the inspector.");
                    enabled = false;
                    return;
                }
                else
                    CLogger.LogWarning("No character controller set for Motion Sickness Aid. Found a Character Controller component in one of the parent objects.");
            }

            if (effectSkybox == null)
            {
                effectSkybox = Resources.Load<Cubemap>(PATH_SKYBOXES + PATH_SKYBOX);
                CLogger.LogWarning("No skybox set for Motion Sickness Aid. Please set the skybox in the inspector.");
            }

            // Settings are loaded from a config file that should be present in the folder of the exe when built.
            // If the config file is not present, the effect is disabled.
#if !UNITY_EDITOR
            if (!CMSAConfigReader.LoadValue("motionSicknessAidActivated", false))
            {
                enabled = false;
            }
            visualEffect = CMSAConfigReader.LoadValue("motionSicknessAidEffect", VisualEffect.Vignette);
            effectIntensity = CMSAConfigReader.LoadValue("motionSicknessAidIntensity", EffectIntensity.Low);
#else
            visualEffect = editorVisualEffect;
            effectIntensity = editorEffectIntensity;
#endif

            cam = GetComponent<Camera>();

            propFxOuter = Shader.PropertyToID(PROP_OUTER);
            propFxInner = Shader.PropertyToID(PROP_INNER);

            propEyeProjection = Shader.PropertyToID(PROP_EYEPRJ);
            propEyeToWorld = Shader.PropertyToID(PROP_EYEMAT);

            irisMesh = Instantiate<Mesh>(Resources.Load<Mesh>(PATH_MESHES + PATH_IRISMESH));
            irisMatOuter = new Material(Shader.Find(PATH_SHADERS + PATH_SHADER + "Outer"));
            irisMatInner = new Material(Shader.Find(PATH_SHADERS + PATH_SHADER + "Inner"));
            toDestroy.Add(irisMesh);
            toDestroy.Add(irisMatOuter);
            toDestroy.Add(irisMatInner);

            cam = GetComponent<Camera>();

            propColor = Shader.PropertyToID(PROP_COLOR);
            propSkybox = Shader.PropertyToID(PROP_SKYBOX);
        }

        void OnEnable()
        {
            // Prevent effect thinking we've instantly teleported from 0,0,0
            ResetMotion();
        }

        void OnDestroy()
        {
            foreach (var o in toDestroy)
            {
                Destroy(o);
            }
            toDestroy.Clear();
        }

        void ResetMotion()
        {
            lastPos = characterController.position;
        }

        // Update is called once per frame
        void LateUpdate()
        {
            float fxValue;
            if (forceEffect)
                fxValue = 1f;
            else
            {
                fxValue = CalculateFxCoverage(Time.deltaTime);
                fxValue = fxValue * (1f - FX_FILTER) + lastFx * FX_FILTER;
                lastFx = fxValue;
            }
            // Clamp and scale final effect strength
            fxValue = RemapRadius(fxValue) * RemapRadius(effectCoverage);
            fxValue = Mathf.Clamp01(fxValue);

            irisMatOuter.SetFloat(propFxInner, fxValue);
            irisMatInner.SetFloat(propFxInner, fxValue);

            float outer = fxValue - effectFeather;
            irisMatOuter.SetFloat(propFxOuter, outer);
            irisMatInner.SetFloat(propFxOuter, outer);

            Color color = !drawSkybox ? vignetteColor : Color.white;
            irisMatOuter.SetColor(propColor, color);
            irisMatInner.SetColor(propColor, color);

            // Find a layer the camera will render
            int camLayer = 0;
            for (int i = 0; i < 32; ++i)
            {
                camLayer = 1 << i;
                if ((cam.cullingMask & camLayer) != 0)
                {
                    break;
                }
            }

            // Submit outer iris opaque pass
            // Immediately after opaque queue, or first in background queue
            DrawIris(irisMatOuter, 0, 1, camLayer);

            // Submit inner iris alpha blended pass
            // Immediately after opaque queue, or absolute last
            DrawIris(irisMatInner, 1, RQUEUE_LAST, camLayer);
        }

        void DrawIris(Material m, int submesh, int opaqueQueue, int camLayer)
        {
            // Draw opaque in specified order
            m.renderQueue = opaqueQueue;

            if (drawSkybox && effectSkybox != null)
            {
                m.SetTexture(propSkybox, effectSkybox);
                m.EnableKeyword(KEYWORD_SKYBOX);
            }
            else
            {
                m.SetTexture(propSkybox, null);
                m.DisableKeyword(KEYWORD_SKYBOX);
            }
            // Matrix is ignored in shader, but have to ensure mesh passes culling
            Vector3 pos = transform.position + (transform.forward * cam.nearClipPlane * 1.01f);
            Graphics.DrawMesh(irisMesh, pos, Quaternion.identity, m, camLayer, cam, submesh, null, false, false, false);
        }

        float CalculateFxCoverage(float dT)
        {
            // Calculate the velocity of the characterController
            Vector3 velocity = (characterController.position - lastPos) / dT;
            lastPos = characterController.position;
            float speed = velocity.magnitude;
            float coverage = 0;
            // Calculate the effect coverage based on the current speed of the characterController.
            // The coverage starts at 0 and increases to 1.
            // Until a speed of 0.1, the coverage is 0. From a speed of 0.1 to 0.5, the coverage increases linearly from 0 to 1. From 0.5 onwards the coverage is 1.
            if (speed > SPEED_THRESHOLD)
            {
                coverage = Mathf.Clamp01((speed - SPEED_THRESHOLD) / SPEED_MAX);
            }
            return coverage;
        }

        float RemapRadius(float radius)
        {
            return Mathf.Lerp(COVERAGE_MIN, 1, radius);
        }

        void OnPreRender()
        {
            UpdateEyeMatrices();
            ApplyEyeMatrices(irisMatOuter);
            ApplyEyeMatrices(irisMatInner);
        }

        void UpdateEyeMatrices()
        {
            Matrix4x4 local;
#if UNITY_2017_2_OR_NEWER
            if (UnityEngine.XR.XRSettings.enabled)
            {
#else
			if (UnityEngine.VR.VRSettings.enabled) {
#endif
                local = characterController.worldToLocalMatrix;
            }
            else
            {
                local = Matrix4x4.identity;
            }

            eyeProjection[0] = cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
            eyeProjection[1] = cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
            eyeProjection[0] = GL.GetGPUProjectionMatrix(eyeProjection[0], true).inverse;
            eyeProjection[1] = GL.GetGPUProjectionMatrix(eyeProjection[1], true).inverse;

            // Reverse y for D3D, PS4, XB1, Metal
            // Don't reverse on OSX or Android (but do if in-editor and build target is Android)
#if (!UNITY_STANDALONE_OSX && !UNITY_ANDROID) || UNITY_EDITOR_WIN
            var api = SystemInfo.graphicsDeviceType;
            if (
                api != UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 &&
                api != UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2 &&
                api != UnityEngine.Rendering.GraphicsDeviceType.OpenGLCore &&
                api != UnityEngine.Rendering.GraphicsDeviceType.Vulkan
            )
            {
                eyeProjection[0][1, 1] *= -1f;
                eyeProjection[1][1, 1] *= -1f;
            }
#endif

            eyeToWorld[0] = cam.GetStereoViewMatrix(Camera.StereoscopicEye.Left);
            eyeToWorld[1] = cam.GetStereoViewMatrix(Camera.StereoscopicEye.Right);

            eyeToWorld[0] = local * eyeToWorld[0].inverse;
            eyeToWorld[1] = local * eyeToWorld[1].inverse;
        }

        void ApplyEyeMatrices(Material m)
        {
            m.SetMatrixArray(propEyeProjection, eyeProjection);
            m.SetMatrixArray(propEyeToWorld, eyeToWorld);
        }
    }
}
