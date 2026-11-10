using System;
using System.Collections.Generic;
using UnityEngine;

namespace FFF7R.VFX
{
    /// <summary>
    /// 格子升级特效 · 组合器。
    ///
    /// **这一张表就是全部时序。** 部件只管"我 0→1 长什么样"，什么时候开始、谁跟着谁，
    /// 全在这里。所以改一个部件的时长，跟着它触发的那些会自动跟上 —— 因为记的是
    /// "第一个框走到 70%"，不是"0.434 秒"。
    ///
    /// 两种触发：
    ///   Absolute   绝对时刻（秒）
    ///   AfterPart  参照部件走到百分之几时开始（百分比 × 参照的 duration）
    ///
    /// 默认的升级时序（`ApplyUpgradePreset` 生成）：
    ///   Fill    0.00 s      1.50 s  卡牌实心（常量亮面）
    ///   Edge    0.00 s      1.50 s  卡牌边框（和实心拆成两层，节奏可以各走各的）
    ///   Surge   0.02 s      0.62 s  从卡心涌到卡边（前 70% 推到边，然后整片熄灭）
    ///   Wall    0.06 s      1.40 s  涨 0.25 s、停到 0.83 s、消散到 1.46 s
    ///   Frame1  0.10 s      0.62 s  第一条框起飞
    ///   Frame2  跟随 Frame1 70%      0.62 s  第二条框
    ///   Spread  跟随 Frame1 70%      0.62 s  平面扩散（和 Frame2 同时发、同时完）
    ///
    /// 用时序全在 C# 算成 0..1 的进度写进材质，shader 里没有时间函数，
    /// 所以暂停会停、编辑器里能逐帧定格。
    /// </summary>
    [ExecuteAlways]
    [AddComponentMenu("FFF7R/VFX/Board Cell Upgrade Group")]
    public class BoardCellUpgradeGroup : MonoBehaviour
    {
        public enum Trigger
        {
            Absolute,
            AfterPart,
        }

        [Serializable]
        public class Slot
        {
            public CellFXPart part;

            public Trigger trigger = Trigger.Absolute;

            [Tooltip("Absolute：从开头算的秒数。AfterPart：参照部件走到百分之几（0..1）时开始")]
            public float at = 0f;

            [Tooltip("AfterPart 时的参照部件")]
            public CellFXPart after;

            [Tooltip("额外延迟（秒）")]
            public float delay = 0f;

            [Tooltip("覆盖这个部件的时长（秒）。0 = 用部件自己的 duration")]
            public float duration = 0f;
        }

        [Header("时间表")]
        public List<Slot> slots = new List<Slot>();

        [Header("播放")]
        public bool playOnEnable = true;
        [Tooltip("编辑器里循环播放，方便调参。上线关掉")]
        public bool loopPreview = true;
        [Tooltip("循环之间停多久")]
        public float loopGap = 0.45f;
        [Tooltip("总时长。0 = 自动算（最晚结束的那个）")]
        public float totalDuration = 0f;
        [Range(0, 3)] public int level = 1;

        [Header("配色")]
        [Tooltip("整套色调：乘到每个部件的 tint 上。改这一处 = 换一整套配色")]
        public Color colorTint = Color.white;

        float _t0 = -9999f;

        public float Elapsed { get { return Mathf.Max(0f, Now - _t0); } }

        public float TotalDuration
        {
            get
            {
                if (totalDuration > 1e-3f) return totalDuration;
                float m = 0f;
                for (int i = 0; i < slots.Count; i++)
                {
                    if (slots[i] == null || slots[i].part == null) continue;
                    m = Mathf.Max(m, StartOf(i, 0) + DurationOf(slots[i]));
                }
                return Mathf.Max(m, 1e-3f);
            }
        }

        static float Now
        {
            get { return Application.isPlaying ? Time.time : Time.realtimeSinceStartup; }
        }

        // ---- 生命周期 ------------------------------------------------

        void OnEnable()
        {
            ApplyTint();
            _t0 = playOnEnable ? Now : -9999f;
            Push();
        }

        void OnValidate()
        {
            if (!isActiveAndEnabled) return;
            ApplyTint();
            Push();
        }

        void Update()
        {
            // 编辑模式用 realtimeSinceStartup、Play 模式用 Time.time —— 两个时钟不同源。
            // 不做这个判断的话，从编辑模式点进 Play 会算出很大的负数，被 Mathf.Max 压成 0，
            // 于是特效卡在"进度 0"上一直不动。
            if (Now < _t0) _t0 = Now;

            if (loopPreview && Elapsed > TotalDuration + Mathf.Max(loopGap, 0f)) Play(level);
            Push();
        }

        void OnDisable()
        {
            // 组件关掉后不会再 Push，部件会冻在最后一帧 —— 全部归零，不留在场景里
            for (int i = 0; i < slots.Count; i++)
                if (slots[i] != null && slots[i].part != null) slots[i].part.SetProgress(0f);
        }

        /// <summary>从头播一次。</summary>
        public void Play(int lv = 1)
        {
            level = Mathf.Clamp(lv, 0, 3);
            _t0 = Now;
            ApplyTint();
            Push();
        }

        [ContextMenu("Play")]
        void PlayContextMenu() { Play(level); }

        // ---- 时间表 --------------------------------------------------

        float DurationOf(Slot s)
        {
            if (s == null) return 1e-3f;
            if (s.duration > 1e-3f) return s.duration;
            if (s.part != null) return Mathf.Max(s.part.duration, 1e-3f);
            return 1e-3f;
        }

        int IndexOf(CellFXPart p)
        {
            if (p == null) return -1;
            for (int i = 0; i < slots.Count; i++)
                if (slots[i] != null && slots[i].part == p) return i;
            return -1;
        }

        /// <summary>第 i 个槽的绝对开始时刻。AfterPart 会递归去问参照部件什么时候开始。</summary>
        float StartOf(int i, int depth)
        {
            if (i < 0 || i >= slots.Count) return 0f;
            var s = slots[i];
            if (s == null) return 0f;

            if (s.trigger == Trigger.Absolute || s.after == null) return s.at + s.delay;

            int j = IndexOf(s.after);
            // 参照不在表里、或者自己参照自己、或者链条太深 —— 都退回按绝对时刻处理，不崩
            if (j < 0 || j == i || depth > 8) return s.at + s.delay;

            return StartOf(j, depth + 1) + s.at * DurationOf(slots[j]) + s.delay;
        }

        void Push()
        {
            float el = Elapsed;
            bool live = loopPreview || el < TotalDuration;
            if (!live)
            {
                for (int i = 0; i < slots.Count; i++)
                    if (slots[i] != null && slots[i].part != null) slots[i].part.SetProgress(0f);
                return;
            }

            for (int i = 0; i < slots.Count; i++)
            {
                var s = slots[i];
                if (s == null || s.part == null) continue;
                float du = DurationOf(s);
                float p = Mathf.Clamp01((el - StartOf(i, 0)) / du);
                s.part.SetProgress(p);
            }
        }

        void ApplyTint()
        {
            for (int i = 0; i < slots.Count; i++)
                if (slots[i] != null && slots[i].part != null)
                    slots[i].part.SetExternalTint(colorTint);
        }

        // ---- 预设 ----------------------------------------------------

        /// <summary>
        /// 把子对象里的部件扫成默认的升级时序。
        /// 菜单建完之后会调一次；手动加组件时也可以用右键菜单跑一次。
        /// </summary>
        [ContextMenu("按子对象重建时间表（升级预设）")]
        public void ApplyUpgradePreset()
        {
            var parts = GetComponentsInChildren<CellFXPart>(true);

            CellFXFill fill = null;
            CellFXEdge edge = null;
            CellFXSurge surge = null;
            CellFXWall wall = null;
            CellFXSpread spread = null;
            var frames = new List<CellFXFrame>();

            for (int i = 0; i < parts.Length; i++)
            {
                if (fill == null && parts[i] is CellFXFill) fill = parts[i] as CellFXFill;
                else if (edge == null && parts[i] is CellFXEdge) edge = parts[i] as CellFXEdge;
                else if (surge == null && parts[i] is CellFXSurge) surge = parts[i] as CellFXSurge;
                else if (wall == null && parts[i] is CellFXWall) wall = parts[i] as CellFXWall;
                else if (spread == null && parts[i] is CellFXSpread) spread = parts[i] as CellFXSpread;
                else if (parts[i] is CellFXFrame) frames.Add(parts[i] as CellFXFrame);
            }

            slots.Clear();
            if (fill != null)
                slots.Add(new Slot { part = fill, trigger = Trigger.Absolute, at = 0.00f, duration = 1.50f });
            if (edge != null)
                slots.Add(new Slot { part = edge, trigger = Trigger.Absolute, at = 0.00f, duration = 1.50f });
            if (surge != null)
                slots.Add(new Slot { part = surge, trigger = Trigger.Absolute, at = 0.02f, duration = 0.62f });
            if (wall != null)
                slots.Add(new Slot { part = wall, trigger = Trigger.Absolute, at = 0.06f, duration = 1.40f });
            if (frames.Count > 0)
                slots.Add(new Slot { part = frames[0], trigger = Trigger.Absolute, at = 0.10f, duration = 0.62f });
            if (frames.Count > 1)
            {
                for (int k = 1; k < frames.Count; k++)
                    slots.Add(new Slot { part = frames[k], trigger = Trigger.AfterPart, after = frames[0], at = 0.70f, duration = 0.62f });
            }
            if (spread != null)
                slots.Add(new Slot
                {
                    part = spread,
                    trigger = frames.Count > 0 ? Trigger.AfterPart : Trigger.Absolute,
                    after = frames.Count > 0 ? (CellFXPart)frames[0] : null,
                    at = frames.Count > 0 ? 0.70f : 0.53f,
                    duration = 0.62f,
                });

            totalDuration = 0f;
        }
    }
}
