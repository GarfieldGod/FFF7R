#!/usr/bin/env python3
"""从 Unity 的编译输入（Assembly-CSharp.rsp）还原一份 IDE 可加载的工程。

背景：
  Unity 的 com.unity.ide.vscode 生成的是老式 csproj（TargetFrameworkVersion v4.7.1），
  C# Dev Kit 不认；而本工程根目录的 FFF7.sln / Assembly-CSharp.csproj 是生成物，
  在 .gitignore 里，克隆后不存在 → IDE 没有符号表 → 全白、无法跳转。

做法：
  不猜引用，直接读 Unity 自己产出的 Library/Bee/artifacts/*/Assembly-CSharp.rsp，
  把 -define / -r / 源文件还原成一份 SDK 风格的 netstandard2.1 工程。

用法：
  python Tools/gen_ide_projects.py
  然后在 VS Code 里执行 "Developer: Reload Window"。

注意：
  - 源文件用通配，新增 .cs 无需重新生成；
  - 只有 Unity 重新生成了 rsp（引用/宏变化，通常发生在换 Unity 版本或加包）时才需要再跑一次。
"""
import glob
import os
import sys

PROJ = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
PROJ_NAME = "Assembly-CSharp"
GUID = "B1F1F7A0-0000-4000-8000-000000000001"


def find_rsp():
    pattern = os.path.join(PROJ, "Library", "Bee", "artifacts", "*", PROJ_NAME + ".rsp")
    found = glob.glob(pattern)
    if not found:
        sys.exit("找不到 %s.rsp —— 请先用 Unity 打开一次工程，让它完成一次脚本编译。" % PROJ_NAME)
    return max(found, key=os.path.getmtime)


def unquote(s):
    s = s.strip()
    if len(s) >= 2 and s[0] == '"' and s[-1] == '"':
        return s[1:-1]
    return s


def parse(rsp_path):
    defines, refs, lang, unsafe = [], [], "9.0", False
    with open(rsp_path, "r", encoding="utf-8-sig", errors="replace") as f:
        for raw in f:
            line = raw.strip()
            if not line:
                continue
            norm = "-" + line[1:] if line.startswith("/") else line
            if norm.startswith("-define:"):
                defines.append(unquote(norm[len("-define:"):]))
            elif norm.startswith("-r:") or norm.startswith("-reference:"):
                refs.append(unquote(norm.split(":", 1)[1]))
            elif norm.startswith("-langversion:"):
                lang = unquote(norm[len("-langversion:"):])
            elif norm.startswith("-unsafe"):
                unsafe = True
    return defines, refs, lang, unsafe


def write_csproj(defines, refs, lang, unsafe):
    out = ['<Project Sdk="Microsoft.NET.Sdk">', "  <PropertyGroup>"]
    out.append("    <TargetFramework>netstandard2.1</TargetFramework>")
    out.append("    <LangVersion>%s</LangVersion>" % lang)
    out.append("    <AssemblyName>%s</AssemblyName>" % PROJ_NAME)
    out.append("    <RootNamespace>%s</RootNamespace>" % PROJ_NAME)
    out.append("    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>")
    out.append("    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>")
    out.append("    <AllowUnsafeBlocks>%s</AllowUnsafeBlocks>" % ("true" if unsafe else "false"))
    out.append("    <DefineConstants>%s</DefineConstants>" % ";".join(defines))
    out.append("    <NoWarn>CS1701;CS1702;CS0649;CS0414</NoWarn>")
    out.append("  </PropertyGroup>")
    out.append("  <ItemGroup>")
    for r in sorted(refs, key=lambda x: os.path.basename(x).lower()):
        name = os.path.splitext(os.path.basename(r))[0]
        out.append('    <Reference Include="%s">' % name)
        out.append("      <HintPath>%s</HintPath>" % r)
        out.append("      <Private>false</Private>")
        out.append("    </Reference>")
    out.append("  </ItemGroup>")
    out.append("  <ItemGroup>")
    out.append('    <Compile Include="Assets\\**\\*.cs" Exclude="Assets\\FFF7R\\obj\\**;Assets\\FFF7R\\bin\\**" />')
    out.append("  </ItemGroup>")
    out.append("</Project>")

    path = os.path.join(PROJ, PROJ_NAME + ".csproj")
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(out) + "\n")
    return path


def write_sln():
    lines = [
        "Microsoft Visual Studio Solution File, Format Version 12.00",
        "# Visual Studio Version 17",
        "VisualStudioVersion = 17.0.31903.59",
        "MinimumVisualStudioVersion = 10.0.40219.1",
        'Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}") = "%s", "%s.csproj", "{%s}"' % (PROJ_NAME, PROJ_NAME, GUID),
        "EndProject",
        "Global",
        "\tGlobalSection(SolutionConfigurationPlatforms) = preSolution",
        "\t\tDebug|Any CPU = Debug|Any CPU",
        "\t\tRelease|Any CPU = Release|Any CPU",
        "\tEndGlobalSection",
        "\tGlobalSection(ProjectConfigurationPlatforms) = postSolution",
        "\t\t{%s}.Debug|Any CPU.ActiveCfg = Debug|Any CPU" % GUID,
        "\t\t{%s}.Debug|Any CPU.Build.0 = Debug|Any CPU" % GUID,
        "\t\t{%s}.Release|Any CPU.ActiveCfg = Release|Any CPU" % GUID,
        "\t\t{%s}.Release|Any CPU.Build.0 = Release|Any CPU" % GUID,
        "\tEndGlobalSection",
        "\tGlobalSection(SolutionProperties) = preSolution",
        "\t\tHideSolutionNode = FALSE",
        "\tEndGlobalSection",
        "EndGlobal",
    ]
    path = os.path.join(PROJ, "FFF7.sln")
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(lines) + "\n")
    return path


def main():
    rsp = find_rsp()
    defines, refs, lang, unsafe = parse(rsp)
    print("编译输入:", rsp)
    print("宏 %d 条 / 引用 %d 个" % (len(defines), len(refs)))
    print("已写出:", write_csproj(defines, refs, lang, unsafe))
    print("已写出:", write_sln())
    print("下一步：VS Code 里执行 Developer: Reload Window")


if __name__ == "__main__":
    main()
