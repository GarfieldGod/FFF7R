using UnityEngine;
using UnityEngine.UI;

namespace FFF7R.VFX
{
    /// <summary>
    /// 把 RectTransform 的实际像素尺寸喂给 GoldenFrameUI.shader。
    ///
    /// Shader 内部按"设计单位"工作（quad 高 = 0.52），需要知道真实宽高比才能正确展开线宽，
    /// 所以必须有这个组件。挂在 Image 同一个 GameObject 上即可，不需要手动填任何参数。
    ///
    /// 注意：这里走 IMaterialModifier 生成材质实例，所以每个框一个材质实例，不参与 UGUI 合批。
    /// 卡牌高亮框数量有限（同屏几张到十几张），这个代价可以接受；
    /// 如果要几百个同屏，应该改成把尺寸写进顶点 uv1，但那需要自定义 Graphic。
    /// </summary>
    [RequireComponent(typeof(Image))]
    [ExecuteAlways]
    [AddComponentMenu("FFF7R/VFX/Golden Frame UI")]
    public class GoldenFrameUI : MonoBehaviour, IMaterialModifier
    {
        static readonly int QuadWId = Shader.PropertyToID("_QuadW");
        static readonly int QuadHId = Shader.PropertyToID("_QuadH");

        Graphic _graphic;
        Material _instance;

        Graphic Graphic
        {
            get
            {
                if (_graphic == null) _graphic = GetComponent<Graphic>();
                return _graphic;
            }
        }

        void OnEnable()
        {
            if (Graphic != null) Graphic.SetMaterialDirty();
        }

        void OnDisable()
        {
            ReleaseInstance();
        }

        void OnDestroy()
        {
            ReleaseInstance();
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (Graphic != null) Graphic.SetMaterialDirty();
        }
#endif

        void OnRectTransformDimensionsChange()
        {
            if (Graphic != null) Graphic.SetMaterialDirty();
        }

        /// <summary>
        /// 换过材质、或者运行时改过共享材质的参数之后调用，让材质实例重新同步一次。
        /// 例：image.material = redMat; frame.RefreshMaterial();
        /// </summary>
        public void RefreshMaterial()
        {
            if (Graphic != null) Graphic.SetMaterialDirty();
        }

        public Material GetModifiedMaterial(Material baseMaterial)
        {
            if (!isActiveAndEnabled || baseMaterial == null) return baseMaterial;

            if (_instance == null || _instance.shader != baseMaterial.shader)
            {
                ReleaseInstance();
                _instance = new Material(baseMaterial)
                {
                    name = baseMaterial.name + " (Instance)",
                    hideFlags = HideFlags.HideAndDontSave
                };
            }
            else
            {
                // 让 Inspector 里对 .mat 的改动能实时同步到实例
                _instance.CopyPropertiesFromMaterial(baseMaterial);
            }

            var rect = ((RectTransform)transform).rect;
            _instance.SetFloat(QuadWId, Mathf.Max(1f, rect.width));
            _instance.SetFloat(QuadHId, Mathf.Max(1f, rect.height));
            return _instance;
        }

        void ReleaseInstance()
        {
            if (_instance == null) return;
            if (Application.isPlaying) Destroy(_instance);
            else DestroyImmediate(_instance);
            _instance = null;
        }
    }
}
