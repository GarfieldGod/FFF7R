# CardBanFX —— 卡牌禁止（Unity 侧）

一个 shader + 一个组件 + 一个菜单。挂在卡牌中心上方，卡面中央出现「禁止」标，整张卡轻微压暗。

```
CardBanFX.shader      FFF7/VFX/CardBan
CardBanFX.cs          尺寸 / 参数 / 开关，全部走 MaterialPropertyBlock
Editor/CardBanMenu.cs GameObject / VFX / Card Ban  → 一键建对象 + 建材质
Materials/CardBan.mat 第一次用菜单创建时自动生成（颜色、亮度在这里改）
```

## 1. 用法

1. 选中一张卡牌 → 菜单 **GameObject / VFX / Card Ban**。对象会挂到它下面，Y 自动抬到 **0.0265**。
   没选中任何东西时放到世界原点。
2. 拖 `CardBan` 的 Inspector：
   - **外观**（颜色、亮度）在 `Materials/CardBan.mat` 上 —— 换一个材质就能整套换色。
   - **形状 / 粗细 / 角度 / 呼吸**在 `CardBanFX` 组件上 —— 每张卡可以不一样，不进材质。
3. 开关：勾 `on`，或代码 `ban.SetOn(true)` / `SetOn(false)`，0.17 s 平滑淡入淡出。

### 播放方式（二选一，共用同一套平滑包络）

| 需求 | 调用 | 行为 |
|---|---|---|
| **持续播放直到关闭** | `SetOn(true)` / `SetOn(false)` | 一直亮，生命周期归调用方 |
| **亮一会儿自己消失** | `PlayFor(1.5f)` | 满亮度保持 1.5 s → 再花 `1/fadeSpeed`（默认 0.17 s）淡出 |
| 同上，用 Inspector 默认值 | `PlayFor()` | 用 `holdSeconds`（默认 1.2 s）|
| 硬弹出（不淡入） | `PlayFor(1.5f, true)` | 第一帧满亮，播完**仍然**淡出 |
| 查询当前状态 | `IsOn` | 限时模式看到期了没，否则看 `on`。**去重判断用这个，别看 `on`** |

五条契约：

- **`PlayFor` 的 `seconds` 是"满亮度保持时长"**，不含淡出。想要"总共 1.5 s 消失"，传 `1.5 - 1/fadeSpeed`。
- **连续调用 = 续期**。`_until = Now + seconds` 直接覆盖，不会排队播多次、也不会叠加。
- **"后写的赢"**：`on = true`（永久）时调 `PlayFor(1.5f)`，1.5 s 后照样会关。反之 `PlayFor` 中途 `SetOn(true)` 会变永久。
- **组件不在运行时 `PlayFor` 无效**（没有 `Update` 就没人推进倒计时），要"立刻显示"用 `SnapOn(true)`。
- **限时播放期间自动关掉呼吸**。呼吸是"状态一直存在"的表达，而 1.2 s 装不满一个 2.4 s 周期，看着像在闪。`PlayFor` 期间 `_Pulse` 推 0（符号恒定最亮），到期收敛成"永久关"后 `pulse` 才回到原位。关闭淡出的过程也不呼吸。

`_timed` / `_until` **故意不序列化**，`OnEnable` 里清零：对象池复用、卡牌重激活时，上一张牌留下的临时状态不会跟到下一张牌身上，一律回到 Inspector 上 `on` 决定的永久状态。

想改"淡入有多快"就调 `fadeSpeed`：6 ≈ 0.17 s（默认），40 ≈ 一帧到位（看起来就是硬切，但不用 `PlayFor(_, true)`）。

**尺寸由组件拥有，但写在网格里，不写在 Transform 上**：它按 `cardSize × (1 + 2·padding)` 建一张
"就是这么大"的 quad 网格，**Transform 恒为 (1,1,1)**。所以 `1,1,1` 就是正确比例；
想改大小只改 `cardSize` / `padding`（网格顶点跟着改，不发 `SendMessage`）。

## 2. 参数

材质上（跨卡共享）：

| 参数 | 默认 | 说明 |
|---|---|---|
| `符号颜色` | **(0.58, 0.05, 0.09)** | 暗红。别往上调太亮 —— 见 §3 ① |
| `符号亮度` | 1.00 | 乘在符号上，超过 1 会过曝成橘红 |
| `色调` / `整体亮度` / `等级` | 白 / 1 / 1 | 等级只影响亮度，不改形状 |

组件上：

| 参数 | 默认 | 说明 |
|---|---|---|
| `cardSize` | 0.32 × 0.44 | 卡牌尺寸（米），就是卡牌几何本身 |
| `cornerRadius` | 0.0174 | 卡牌圆角。**符号会被这个轮廓裁掉** |
| `padding` | 0.10 | quad 比卡牌每边大多少（按短边比例）。符号不出卡，这点余量只给辉光 |
| `symbolSize` | 0.52 | 符号直径 ÷ 卡牌短边。0.52 ≈ 16.6 cm（卡宽 0.32 时）|
| `ringThickness` | 0.085 | 环厚 ÷ 符号直径。0.085 ≈ 14 mm |
| `barScale` | 1.0 | 斜杠厚度 ÷ 环厚 |
| `slashAngle` | **42** | 斜杠自身方向（度）。42 ≈ `/`；**-42 ≈ `\`**（标准禁止符）|
| `opacity` | 0.85 | 符号不透明度。低于 0.6 就成薄雾，1 = 完全盖死 |
| `centerOffset` | (0, 0) | 符号中心偏移（按短边比例）。卡面有角标时用它避开 |
| `dim` | 0.12 | 整卡压暗。0 = 不压；0.4 以上明显发黑 |
| `glowWidth` | 0.045 | 符号外沿柔光宽度（按短边比例）≈ 14 mm |
| `glowGain` | 0.40 | 辉光亮度。辉光是加色、不遮挡 |
| `pulse` / `period` | 0.10 / 2.4 s | 呼吸幅度 / 周期。0 = 完全静止（贴纸感）。**只作用于永久状态**：`PlayFor` 期间与关闭淡出时自动不呼吸 |
| `on` / `fadeSpeed` | true / 6 | 永久开关 / 淡入淡出速度 |
| `holdSeconds` | 1.2 | `PlayFor()` 无参时用的时长（秒），**不含淡出** |

## 3. 四个必须知道的点

① **这层不能用加色，必须走预乘 over。**

它叠在**卡面这个亮底**上。加色会让红符号在红色卡背上变成粉、在白色人物上几乎看不见；
而且浅底上"白 + 红 = 白"，符号会整块消失。

所以混合是 `Blend One OneMinusSrcAlpha`，输出 `float4(rgb, a)`，其中：

```hlsl
aSym = core × opacity × k × pulse × inside × edge;
rgb  = sym × aSym;        // 预乘：rgb 与 a **同源**
rgb += sym × glow × …;    // 辉光只加色，不进 a
```

- **实心符号参与 a** → 会遮住下面的画面，读作"盖上去的标"
- **辉光 a = 0** → 只加光、不遮挡，浅底上也不会切出一圈硬边
- **a 与 rgb 同源** → `_On` / 呼吸归零后不会留下一层"透明但仍在遮挡"的黑影
  （这个仓库里踩过两次：只写 `形状 × 遮盖` 的 a、把亮度乘在 rgb 上，播完就挂一圈黑框）

② **符号被卡牌 SDF 裁掉，绝不会画到棋盘上。**

`inside = saturate(-d / 0.002·unit)`，`d` 是卡牌圆角矩形的 SDF。所有项都乘 `inside`。
所以 `symbolSize` 调过头不会溢到卡外 —— 会被卡牌的轮廓裁掉。这是有意的：
"符号比卡还大"在任何一张卡上都不成立。

③ **斜杠必须先裁到环里，否则会从环里伸出去一截。**

```hlsl
sdBar = max(sdBar, lq - R);   // R = 环的外半径
```

不裁的话，斜杠是一条**无限长**的条带（`|dot(c, n)| - h` 只约束了法向距离），
两端会戳到环外 2~3 cm，读起来像"两根交叉的棍子"而不是禁止标。

裁完之后再和环做 `smin`（圆滑并集），接缝处往外鼓出约 `k/4 ≈ 1.9 mm`，
正好是"斜杠熔进环里"的观感。要硬接缝就把 `SMin` 的 k 改成 0 —— 但会切出一个直角缺口。

④ **给 `sharedMesh` 赋值的代码不能在 `OnValidate` / `OnEnable` 里同步跑。**

`sharedMesh =` 会立刻发 `OnMeshFilterChanged`，而这三个阶段禁止 `SendMessage`，
报 `SendMessage cannot be called during Awake, CheckConsistency, or OnValidate`。
本组件已经处理成 `EditorApplication.delayCall` + 预制体资产上不建网格；
**改尺寸只改顶点**（`ReshapeQuad`），不换 `sharedMesh` 引用，所以拖 `padding` 也不会触发。

## 4. 验收

几何 / 合成全部在 `../tools/preview_cardban.py` 里量化（numpy 复现同一套数学），
跑一次打印九项，数据见 `../README.md`。三条零值是最该守住的：

```
卡外最大 alpha             0.000000
quad 四边最大 alpha        0.000000
opacity/dim/glow 全 0 时   0.000000
```

## 5. 排障

| 症状 | 原因 / 处理 |
|---|---|
| 符号看不见，或整块发白 | 大概率是**加色**了 —— 检查混合模式是不是被改成 `Blend One One`。这个效果只能用预乘 over |
| 淡出之后卡上留一圈黑框 | `a` 没跟亮度同源衰减。检查 `aSym` 与 `rgb` 是不是都乘了 `pulse × k × inside` |
| 符号边缘有一圈方块 | `padding` 太小、辉光摸到了 quad 边界。加大 `padding`（quad 边上有硬压门，但别指望它救） |
| 斜杠两端戳到环外 | `sdBar = max(sdBar, lq - R)` 这行被删了 |
| 符号跑到卡外面去了 | `inside` 那道门被删了 |
| 符号偏白 / 偏粉 | `符号亮度` 或 `等级` 太高。参考图那个红其实很暗（sRGB 0.58）|
| 符号跟着卡一起缩放变糊 | 尺寸应该走 `cardSize`，不要用父级 `localScale` 拉 |
| Shader.Find 返回 null，菜单报「找不到」 | shader 整份解析失败，八成是 `[Header(...)]` / `[Enum(...)]` 括号里写了中文。跑 `python VFX/tools/lint_shaders.py` |
| `PlayFor` 调了但一直亮着不消失 | 组件当时不在运行（对象或组件被禁用），`PlayFor` 直接返回。检查 `isActiveAndEnabled`，或改用 `SnapOn(true)` + 调用方自己计时 |
| 卡牌池化复用后带着禁止标 | `OnEnable` 的 `_timed` 清零被删掉了。临时状态必须跨启用周期丢弃，回归到 `on` |
