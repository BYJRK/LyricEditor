using System.Reflection;

namespace LyricEditor.Utils;

/// <summary>
/// 统一提供软件版本号信息。版本号唯一来源为仓库根目录的 version.json（由 Nerdbank.GitVersioning 包读取，
/// 自动结合 git 提交高度/commit id 生成程序集版本），其余位置（帮助信息、窗口标题等）均应通过此类读取，
/// 不要再手动硬编码版本字符串。升级版本只需修改 version.json 中的 "version" 字段。
/// </summary>
public static class AppVersion
{
    /// <summary>
    /// 用于展示给用户的版本号，例如 "3.0.0-beta"。
    /// 优先读取 AssemblyInformationalVersion（包含 -beta 等预发布标识），
    /// 若不存在则退化为程序集的数字版本号。
    /// </summary>
    public static string DisplayVersion { get; } =
        Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion
        ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString()
        ?? "unknown";
}
