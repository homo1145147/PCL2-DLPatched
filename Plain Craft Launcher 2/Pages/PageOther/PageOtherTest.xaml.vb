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
        "在版本选择页面，右键某个版本也能进入版本设置页面！",
        "你可以在版本设置中调整分类，以将特定版本隐藏！",
        "使用 --memory 参数启动 PCL 可以静默进行内存优化！",
        "在第一次启动游戏时，PCL 会自动将语言设置为中文！",
        "在版本设置中可以开启第三方登录验证，例如 Little Skin 登录！",
        "自动配置游戏内存时，PCL 将会根据剩余内存与 Mod 数量动态决定分配的内存！",
        "主页可以使用特定的预设，在设置中看看吧！",
        "在高级启动选项中，可以设置在游戏启动前运行特定程序！",
        "将鼠标悬浮在设置页的左边栏，可以找到重置设置按钮！",
        "版本设置只对当前版本生效，而设置页面的设置对所有版本生效！",
        "要将已有的 MC 文件夹加入 PCL，可以在版本选择页的左侧选择「添加已有文件夹」！",
        "如果同时安装了 OptiFine 与对应的原版，PCL 会展示 OptiFine 版本，折叠原版！",
        "版本选择的「常规版本」分类中，只会列出最新的一个快照或预发布版。",
        "在版本选择页面，右键游戏文件夹可以进行打开、重命名、删除等操作！",
        "如果你在其他地方修改了皮肤，需要手动选择「刷新皮肤」才能更新登录页面的头像……",
        "下载 Mod 时，PCL 会自动定位对应版本的 Mod 文件夹！",
        "如果缺少 Java，PCL 也能自动下载，不必自己安装啦！",
        "如果你打开了调试模式，启动页右侧就会显示启动日志！",
        "将鼠标悬浮在下载页的左边栏，可以找到刷新按钮！",
        "将鼠标指向下载页的 MC 版本，可以在右侧找到「更新日志」选项！",
        "直接把 Mod 或整合包拖进 PCL 窗口就能安装了！",
        "在 PCL 文件夹下新建 hints.txt，可以自定义「你知道吗」的内容！",
        "设置中可以自定义离线皮肤，但这只对单人游戏有效！",
        "如果不想用某项功能，可以在个性化设置中把它隐藏掉！",
        "如果打开了「游戏更新提示」功能，当 MC 更新时 PCL 会弹窗进行提醒！",
        "你可以使用主页来自定义快捷方式！",
        "点击版本右侧的心形就能将该版本加入收藏夹，便于查找。",
        "PCL 的第一个内部版本制作于 2018 年 8 月 13 日！",
        "据调查，有 90.3% 的用户点击了百宝箱中的千万别点按钮！",
        "PCL 的绝大多数代码都在 GitHub 开源了！",
        "PCL 的开发者龙腾猫跃经常被简称为龙猫，但和那只龙猫没有任何关系！"
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
