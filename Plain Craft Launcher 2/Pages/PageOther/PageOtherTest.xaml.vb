' 本分支（PCL2-DLPatched）：该页面目前是空壳，后续重做为「自定义下载源」。
' 原有百宝箱功能（今日人品 / 内存优化 / 清理垃圾）已移除，仅保留其他代码仍会调用的入口，
' 保证编译与启动流程不受影响。
Public Class PageOtherTest

    '========================================
    ' 自定义下载（后续实现）
    '========================================

    Public Shared Sub StartCustomDownload(Url As String, FileName As String, Optional Folder As String = Nothing)
        Hint("自定义下载源功能尚在开发中。")
    End Sub

    '========================================
    ' 已移除的功能（保留空实现）
    '========================================

    Public Shared Sub Jrrp()
        Hint("该功能已移除。")
    End Sub

    Public Shared Sub RubbishClear()
        Hint("该功能已移除。")
    End Sub

    ''' <summary>
    ''' 启动流程仍会调用此处，保留空实现。
    ''' </summary>
    Public Shared Sub MemoryOptimize(ShowHint As Boolean)
    End Sub

    Public Shared Sub MemoryOptimizeInternal(ShowHint As Boolean)
    End Sub

    '========================================
    ' 提示与彩蛋文本（暂时留空）
    '========================================

    '========================================
    ' 提示与彩蛋文本（UI 用，保留）
    '========================================

    Private Shared ReadOnly RandomSource As New System.Random()

    Private Shared ReadOnly Hints As String() = {
        "按 F3 可以在游戏里查看坐标，按 F3 + G 可以显示区块边界。",
        "材质包放在 resourcepacks 文件夹里，光影放在 shaderpacks 文件夹里。",
        "在启动设置里调高优先级的 Java 可以让启动快一点。",
        "整合包可以整个文件夹拖进 PCL 直接导入。",
        "按 Ctrl + 滚轮可以快速缩放游戏界面（调 GUI 缩放档位）。",
        "如果游戏启动很慢，可以试试减少整合包里同时启用的 Mod。",
        "把游戏目录放在固态硬盘上，加载速度会有明显提升。",
        "遇到游戏崩溃，先看看崩溃报告里第一处 Exception，通常就是元凶。",
        "版本设置里的「高级」可以添加自定义启动参数。",
        "多个版本共用一个存档，可以在版本隔离设置里调整。",
        "按 Shift + 左键可以快速搬运背包里的整组物品。",
        "在服务器里输入 /trigger 可以查看服务端提供的自定义计分板功能。",
        "备份存档比什么都重要——尤其是在用整合包的时候。",
        "下载失败时换一个下载源，往往就能解决。",
        "显卡驱动更新有时能解决游戏里的画面异常。",
        "内存分配不是越大越好，超过 8 GB 对原版游戏帮助有限。",
        "在 PCL 里按住 Ctrl 点「启动」可以跳过启动前检查。",
        "版本名称可以是中文的，方便区分不同的整合包。",
        "游戏日志在 logs 文件夹里，崩溃报告在 crash-reports 文件夹里。",
        "Java 参数里的 -Xmx 就是分配的内存上限。"
    }

    Private Shared ReadOnly PresetHints As String() = {
        "你注意到了今天的云长得有点像什么东西吗？",
        "屏幕右下角的像素，是不是比刚才多了一个？",
        "如果把游戏关掉再打开，世界会不会不一样？",
        "有时候，最好的 Mod 就是那个你还没装的。",
        "进度条在走，世界在等你。",
        "别急，加载也是游戏的一部分。"
    }

    Private Shared ReadOnly Caves As String() = {
        "这是 PCL 的某一个不为人知的小角落。",
        "你找到了一段没什么用的文字。恭喜。",
        "这里没有金子，只有这一句话。",
        "如果游戏打不开，试试重启电脑（真的有用）。",
        "你确定要继续往下翻吗？",
        "据说对着屏幕说三次「能启动」，游戏会更快启动。"
    }

    Public Shared Function GetRandomHint() As String
        Return Hints(RandomSource.Next(Hints.Length))
    End Function

    Public Shared Function GetRandomPresetHint() As String
        Return PresetHints(RandomSource.Next(PresetHints.Length))
    End Function

    Public Shared Function GetRandomCave() As String
        Return Caves(RandomSource.Next(Caves.Length))
    End Function

End Class
