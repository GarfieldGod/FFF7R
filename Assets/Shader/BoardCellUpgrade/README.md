# 放进 Unity 工程 · 参数地图

把**这个文件夹整个**拷到 `Assets/Shader/` 下，路径正好是 `Assets/Shader/BoardCellUpgrade/`：

```
Assets/Shader/BoardCellUpgrade/
├── BoardCellUpgradeGroup.cs        时间表（唯一需要配时序的地方）
├── Parts/
│   ├── CellFXShape.cs              卡牌轮廓（0.32 × 0.44 + 圆角），五个部件共用
│   ├── CellFXPart.cs               基类：进度入口 / 网格 / 公共参数
│   ├── CellFXFill.cs   + CellFXFill.shader     卡牌实心（内部亮面）
│   ├── CellFXEdge.cs   + CellFXEdge.shader     卡牌边框（更亮的那一圈）
│   ├── CellFXWall.cs   + CellFXWall.shader     光墙：涨 → 停 → 消散
│   ├── CellFXSpread.cs + CellFXSpread.shader   平面扩散（线宽/透明度可变）
│   ├── CellFXFrame.cs  + CellFXFrame.shader    一条垂直边框（线宽/透明度可变）
│   └── Materials/                  菜单生成的五个 .mat（只放颜色和亮度）
├── Editor/BoardCellUpgradeMenu.cs  一键创建菜单（必须留在 Editor/ 里）
└── README.md                       本文档
```

**三条不能动**：`Editor/` 这个文件夹名；`.cs` 文件名要和类名一致；`.shader` 不要放进 `Editor/`。

菜单：**GameObject → VFX → Board Cell Upgrade**（选中一个格子对象再点，会自动挂到它下面）。

---

## 参数分三层（这条最重要）

| 想改的事 | 去哪儿改 |
|---|---|
| 什么时候开始、谁跟着谁、整段多长 | **Group 的 `slots` 表** |
| 形状、端点、节奏：卡牌多大、圆角多少、墙多高、框离墙多远、**起始/末尾宽度、起始/末尾透明度、飞多高、淡入淡出点、脉冲** | **部件组件**（Fill / Edge / Wall / Spread / Frame 各自的 Inspector） |
| 颜色、亮度、质感曲线（由实变虚、由深变浅、墙根白热、顶端软边） | **材质**（`Parts/Materials/` 下的 `.mat`） |
| 整套换配色 | **Group 的 `colorTint`** |
| 进度曲线（先慢后快） | **部件的 `curve`** |

**时间表管"何时"，组件管"多大/多亮到多少"，材质管"什么颜色、什么质感"。**

### 为什么宽度/透明度放在组件而不是材质

因为 `Frame1` 和 `Frame2` **共用同一个 `CellFXFrame.mat`**。如果端点写在材质上，
两条框的起始宽度就只能一样，"第一条粗、第二条细"这件事根本做不到。

所以约定是：**凡是"每个实例可能不同"的量 → 组件字段，由 MPB 写进 shader；
凡是"整套皮肤共享"的量 → 材质。** 材质上那些被组件接管的属性已经加了
`[HideInInspector]`，Inspector 里看不到，也就不会出现"改这里没反应"的困惑。

---

## 形状：卡牌，不是格子

| | 以前 | 现在 |
|---|---|---|
| 轮廓 | 格子矩形 0.3636 × 0.5 | **卡牌 0.32 × 0.44（8:11）** |
| 圆角 | 无 | **r 0.0174**（就是卡牌几何上的圆角） |
| 平面部件怎么画 | 矩形 quad | 片元里算圆角矩形的**有符号距离**，圆角是解析的 |
| 立体部件怎么画 | 4 段直边 | 顺轮廓 4 条直边 + 4 个圆弧（每角 `cornerSegments` 段） |

所以现在边框、墙根、扩散环都在**卡牌边**上，而不是格子上 —— 卡牌和格子之间那圈留白里
不会再出现一条"格子框"。

五个部件上的 `cardSize` / `cornerRadius` **要填一样的值**，否则各层的尺寸对不上（默认值已正确）。

---

## 基类 CellFXPart（五个部件都有）

| 字段 | 默认 | 说明 |
|---|---|---|
| `tint` | 白 | 色调，乘在材质颜色上 |
| `intensity` | 1.00 | 亮度倍率。**1.0 = 材质原样** |
| `level` | 1 | 0–3，只抬高亮度 |
| `cardSize` | 0.32 × 0.44 | 卡牌尺寸（米）。五个部件保持一致 |
| `cornerRadius` | 0.0174 | 圆角半径（米）。卡牌几何上的值 |
| `cornerSegments` | 6 | 每个圆角分几段（只影响 Wall / Frame 的网格） |
| `duration` | 0.60 | 自身走完 0→1 用多久。**时间表槽上的 `duration` 优先，槽上是 0 才读这里** |
| `curve` | 线性 | 进度重映射，只改节奏，不改总时长 |
| `material` | 空 | 空时编辑器会自动接上 `Materials/` 下同名的 `.mat`；找不到才临时建一个（不落盘） |

---

## 各部件自己的字段

### CellFXFill —— 只做实心，不管边框

| 字段 | 默认 | 作用 |
|---|---|---|
| `pad` | 0.02 | quad 比卡牌大多少（每边），只为边缘软边留像素 |
| `fillLift` | 0.028 | 实心高度。**必须高于卡牌顶面 0.020**，否则整块被卡牌挡掉 |
| `edgeSoft` | 0.012 | 轮廓处软边宽度（× 卡牌短边），防锯齿 |
| `fadeIn` | 0.10 | 淡入占的进度 |
| `fadeOut` | 0.68 | 开始熄灭的进度（× 1.50 s = 1.02 s） |
| `pulseDepth` | **0** | 0 = 常量亮着；0.4 = 在 60%~100% 之间呼吸 |
| `pulseSpeed` | 3.90 | 整段进度里呼吸几次（只在 `pulseDepth > 0` 时有意义） |

### CellFXEdge —— 只做边框

| 字段 | 默认 | 作用 |
|---|---|---|
| `pad` | 0.08 | quad 留白，要给亮带外半圈留空间 |
| `lift` | 0.029 | 亮带高度，比 Fill 高一点点免得抢深度 |
| `edgeWidth` | 0.050 | **边框宽度**（× 卡牌短边）。亮带跨边界，一半在卡牌上、一半在格子面上 |
| `fadeIn` | 0.08 | 淡入占的进度 |
| `fadeOut` | 0.70 | 开始熄灭的进度。**比 Fill 的 0.68 晚一点**，就是"实心先暗、边还留一下" |
| `pulseDepth` | **0** | 边框单独脉冲，实心不动 |
| `pulseSpeed` | 2.60 | |

### CellFXWall —— 涨起来，然后消散

| 字段 | 默认 | 作用 |
|---|---|---|
| `heightScale` | 0.90 | 墙高 = **卡牌深** × 这个 |
| `outset` | 0 | 轮廓整体外扩（米）。0 = 正好贴卡牌边 |
| `lift` | 0.002 | 墙根离格子面 |
| `riseEnd` | 0.18 | 涨满时的进度（前 18% 用来升起） |
| `headScale` | 1.18 | 升满时顶端多出 18%，留余量顶端才是"漫上来"而不是被切一刀 |
| `dissolveStart` | 0.55 | 开始消散的进度。**这段之前墙是满的** |
| `dissolveSoft` | 0.28 | 消散前沿的软边厚度 |
| `dissolveSweep` | 1.0 | **0 = 整体淡出，1 = 从下往上抽走** |

进度 → 形态：`0 → riseEnd` 涨满，`riseEnd → dissolveStart` 立着，`dissolveStart → 1` 散掉。
想让墙只升不散，把驱动方给的进度停在 `dissolveStart` 之前。

### CellFXSpread —— 往外推，越推越细越淡

| 字段 | 默认 | 作用 |
|---|---|---|
| `pad` | 0.62 | quad 留白。**必须 ≥ `reach`**，否则环被切掉（有 warning） |
| `lift` | 0.004 | 贴地高度。低于卡牌顶面，所以环在卡牌范围内那半圈会被卡牌挡住 |
| `reach` | 0.45 | **扩散到多远**（× 卡牌短边 ≈ 14 cm） |
| `widthStart` | 0.050 | **起始宽度**（× 卡牌短边 ≈ 1.6 cm） |
| `widthEnd` | 0.014 | **末尾宽度**。比起始小就是"边推边细" |
| `alphaStart` | 1.00 | **起始透明度** |
| `alphaEnd` | 0.00 | **末尾透明度**。0 = 推到终点就化没了 |
| `fadeIn` | 0.05 | 刚发出那一下的淡入长度（占总进度） |

宽度和透明度都按进度**线性**插值；想要非线性就给组件的 `curve` 换条曲线（会同时作用于两者）。

### CellFXFrame —— 贴着轮廓往上飞

| 字段 | 默认 | 作用 |
|---|---|---|
| `outset` | 0.014 | 离墙那条缝。**0 时框和墙共面，会看成墙上的横纹** |
| `lift` | 0.002 | 起飞前贴地的高度 |
| `rise` | 1.30 | **飞多高**（× 卡牌短边 ≈ 0.42 m）。顶点真的搬这么远 |
| `widthStart` | 0.090 | **起飞时的线宽**（× 卡牌短边 ≈ 2.9 cm） |
| `widthEnd` | 0.040 | **到顶时的线宽**。在 vert 里算，是真实的几何变细 |
| `soft` | 0.32 | 线宽内的高斯柔和度。越小越像一条硬线，越大越像一团光 |
| `alphaStart` | 1.00 | **起始透明度** |
| `alphaEnd` | 0.00 | **末尾透明度** |
| `fadeIn` | 0.08 | 起飞那一下的淡入长度 |

**两条框各调各的**：`Frame1` 与 `Frame2` 都是 `CellFXFrame`，字段互相独立。
想让第二条更细，只改 `Frame2.widthStart` 就行。

---

## 材质上还剩什么（只有配色与质感）

公共：`_Tint` `_Intensity` `_Level` `_ZTest`（3D 用 4）。
其余全部由组件通过 MPB 写入，在材质上已 `[HideInInspector]`。

| 材质 | 保留的可调属性 |
|---|---|
| `CellFXFill.mat` | `实心颜色`、`实心亮度`（0.42，比边框低是刻意的） |
| `CellFXEdge.mat` | `边框颜色`、`边框亮度`（1.40，要"边框比实心亮"就拉到 2–3 倍） |
| `CellFXWall.mat` | `底部深色` `顶部浅色` `墙根白热`、`墙亮度` 1.05、`由实变虚` 1.55、`由深变浅的快慢` 1.15、`墙根白热亮度` 0.14、`贴地淡入高度` 0.12、`顶端软边` 0.22 |
| `CellFXSpread.mat` | `扩散颜色`、`扩散亮度` 2.20 |
| `CellFXFrame.mat` | `线框颜色`、`线框亮度` 0.80 |

换一套配色最省事的做法：改 Group 的 `colorTint`（乘在每个部件的 `tint` 上），
只有想彻底换色相时才动这五个 `.mat`。

---

## 时间表 BoardCellUpgradeGroup

| 字段 | 说明 |
|---|---|
| `slots` | 槽列表。每槽：`part` + `trigger` + `at` + `after` + `delay` + `duration` |
| `playOnEnable` | |
| `loopPreview` | 编辑器循环播放，**上线关掉** |
| `loopGap` | 循环之间停多久，默认 0.45 |
| `totalDuration` | 0 = 自动（最晚结束的那个） |
| `level` | 0–3，`Play(level)` 用 |
| `colorTint` | 整套配色，乘到每个部件的 `tint` 上 |

右键菜单 **按子对象重建时间表（升级预设）** 会按类型扫子对象，重排成默认升级时序
（Fill / Edge / Wall / Frame1 / Frame2 / Spread）。

---

## 换了形状之后要重新生成一次

旧场景里的对象是"按格子"建的，网格是矩形。形状改成卡牌之后：

1. 删掉旧的 `BoardCellUpgrade` 对象；
2. 重新点 **GameObject → VFX → Board Cell Upgrade**（会多出一个 `Edge` 子对象）；
3. `Materials/` 下已有的 `.mat` 会被复用，旧材质里缺的新属性一律用 shader 默认值或组件值，没问题。

---

## 已知限制 / 排查

| 症状 | 原因 |
|---|---|
| 实心完全没生效 | `fillLift` 低于卡牌顶面 0.020，被卡牌挡掉了。调亮度没用 |
| 改了起始宽度/透明度没反应 | 确认改的是**组件**上的字段。材质里同名属性已隐藏（组件会覆盖它） |
| 两条框粗细一样、改不动 | 以前端点在共用的 `.mat` 上；现在改 `Frame1` / `Frame2` 各自的 `widthStart` |
| 边框看起来偏外/偏里 | 五个部件的 `cardSize` / `cornerRadius` 不一致 |
| 扩散环看不见 | `pad` < `reach`，环最后一段被 quad 切掉；或 `alphaEnd` 太小导致一开始就几乎没有 |
| 框像墙上的两道横纹 | `outset` = 0，框和墙面共面 |
| 墙像玻璃缸 | `_WallFalloff` 太小 + `顶端软边` 太小。加大试试 |
| 墙散得很突然 | `dissolveSoft` 太小；或把 `dissolveSweep` 调低改成整体淡出 |
| 亮底上看不见 | 纯加色在亮底上叠不出亮度，这是固有限制 |
| 拖参数时刷 `SendMessage cannot be called during Awake, CheckConsistency, or OnValidate` | 在 `OnValidate` 里改 `sharedMesh` 会立刻触发 `OnMeshFilterChanged`，那个阶段禁止发消息。基类已用 `EditorApplication.delayCall` 延后一帧重建 |
| 都看不见 | 检查材质 `深度测试` 是不是 4（LEqual），以及 `loopPreview` 有没有停住；进度为 0 时 renderer 是关掉的 |
| **菜单弹「找不到 FFF7/VFX/CellFX/Fill」** | 文件在、但整份 shader 解析失败，`Shader.Find` 就返回 null。九成是 `[Header(...)]` / `[Enum(...)]` 里写了中文 —— **attribute 括号里只许 ASCII**，中文放到属性自己的引号标签里。查 Console 的 `Shader error`，或直接跑 `python VFX/tools/lint_shaders.py`（退出码 1 = 有致命问题） |
| 改完 shader 立刻弹上面那条 | Unity 还没重新导入完。等编译转圈结束再点菜单；不行就 `Assets → Reimport` 那个 .shader |
