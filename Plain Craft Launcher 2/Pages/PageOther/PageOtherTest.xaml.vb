' 本分支（PCL2-DLPatched）自行实现百宝箱中的常用功能。
' 官方开源版出于维护成本的考虑没有公开这部分内容（见官方 PageOtherTest 原实现），
' 此处的实现为本分支独立编写，不包含官方版的任何代码。
Public Class PageOtherTest

    Private Const JrrpLuckyThreshold As Integer = 95 ' 人品值达到此值即视为“大吉”

    Private Sub PageOtherTest_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded

    End Sub
    Private Sub ItemJrrp_Click(sender As Object, e As MouseButtonEventArgs)
        Jrrp()
        e.Handled = True
    End Sub

    Private Sub ItemMemoryOptimize_Click(sender As Object, e As MouseButtonEventArgs)
        RunInThread(Sub() MemoryOptimize(True))
        e.Handled = True
    End Sub

    Private Sub ItemRubbishClear_Click(sender As Object, e As MouseButtonEventArgs)
        RunInThread(Sub() RubbishClear())
        e.Handled = True
    End Sub

    '========================================
    ' 自定义下载（暂不支持，保留接口）
    '========================================

    Public Shared Sub StartCustomDownload(Url As String, FileName As String, Optional Folder As String = Nothing)
        Hint("本分支暂不支持自定义下载，请使用「下载」页面获取资源。")
    End Sub

    '========================================
    ' 今日人品
    '========================================

    ''' <summary>
    ''' 「今日人品」：按当天的日期与识别码生成一个 0-100 的确定值，同一天内结果不变。
    ''' </summary>
    Public Shared Sub Jrrp()
        Try
            Dim Value As Integer = GetJrrpValue()
            Dim Level As String = JrrpLevelText(Value)
            Dim Comment As String
            If Value >= JrrpLuckyThreshold Then
                Comment = "你今天的手气好得离谱，去买张彩票吧！"
            ElseIf Value >= 75 Then
                Comment = "状态不错，适合开新坑。"
            ElseIf Value >= 50 Then
                Comment = "普普通通，平庸即平安。"
            ElseIf Value >= 25 Then
                Comment = "建议今天少玩点极限模式。"
            Else
                Comment = "……要不今天先别启动游戏了？"
            End If
            MyMsgBox($"{vbCrLf}今日人品：{Value}（{Level}）{vbCrLf}{vbCrLf}{Comment}{vbCrLf}{vbCrLf}人品值由当天日期与识别码共同决定，同一天内不会变化。",
                "今日人品", "确定")
            Logger.Info($"今日人品：{Value}（{Level}）")
        Catch ex As Exception
            Logger.Error(ex, "获取今日人品失败", LogBehavior.Toast)
        End Try
    End Sub

    ''' <summary>
    ''' 计算今日人品值（0-100）。基于日期与识别码的混合哈希，保证同一天结果稳定。
    ''' </summary>
    Private Shared Function GetJrrpValue() As Integer
        Dim Seed As String = $"{Date.Now:yyyy-MM-dd}|{Identify}"
        Dim Hash As Long = 5381
        For Each C In Seed
            Hash = ((Hash << 5) + Hash) + AscW(C)
            Hash = Hash And &H7FFFFFFFL ' 保持在正数范围内
        Next
        ' 再打散一次，避免相邻日期的结果过于接近
        Hash = (Hash * 1103515245L + 12345L) And &H7FFFFFFFL
        Return CInt(Hash Mod 101)
    End Function

    Private Shared Function JrrpLevelText(Value As Integer) As String
        If Value >= JrrpLuckyThreshold Then Return "大吉"
        If Value >= 75 Then Return "吉"
        If Value >= 50 Then Return "中平"
        If Value >= 25 Then Return "小凶"
        Return "大凶"
    End Function

    '========================================
    ' 内存优化
    '========================================

    <Runtime.InteropServices.DllImport("psapi.dll")>
    Private Shared Function EmptyWorkingSet(hProcess As IntPtr) As Boolean
    End Function

    <Runtime.InteropServices.DllImport("kernel32.dll")>
    Private Shared Function GetCurrentProcess() As IntPtr
    End Function

    ''' <summary>
    ''' 内存优化：触发一次完整垃圾回收，并释放自身进程的闲置工作集。
    ''' </summary>
    Public Shared Sub MemoryOptimize(ShowHint As Boolean)
        Try
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, True, True)
            GC.WaitForPendingFinalizers()
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, True, True)
            Dim Ok As Boolean = EmptyWorkingSet(GetCurrentProcess())
            If ShowHint Then
                If Ok Then
                    Hint("内存优化完成！", HintType.Green)
                Else
                    Hint("内存优化完成，但系统未接受工作集释放请求。", HintType.Red)
                End If
            End If
            Logger.Info($"内存优化完成，EmptyWorkingSet：{Ok}")
        Catch ex As Exception
            Logger.Error(ex, "内存优化失败", LogBehavior.Toast)
        End Try
    End Sub

    ''' <summary>
    ''' 内存优化（内部调用，供启动流程使用）。
    ''' </summary>
    Public Shared Sub MemoryOptimizeInternal(ShowHint As Boolean)
        MemoryOptimize(ShowHint)
    End Sub

    '========================================
    ' 清理垃圾
    '========================================

    ''' <summary>
    ''' 可安全清理的临时目录。
    ''' 注意：PathTemp\Cache 中存有登录令牌、Uuid 映射、皮肤等关键缓存，不能整体删除，
    ''' 因此这里只清理下载与解压产生的中间文件。
    ''' </summary>
    Private Shared ReadOnly RubbishFolders As String() = {
        "Download",
        "Help"
    }

    ''' <summary>
    ''' 清理垃圾：删除 PCL 下载/解压过程中产生的临时文件。
    ''' </summary>
    Public Shared Sub RubbishClear()
        Try
            Dim Removed As New List(Of String)
            Dim Failed As New List(Of String)
            Dim TotalSize As Long = 0
            For Each SubFolder In RubbishFolders
                Dim Folder As String = PathTemp & SubFolder & "\"
                Try
                    If Not Directory.Exists(Folder) Then Continue For
                    For Each File In Directory.EnumerateFiles(Folder, "*", SearchOption.AllDirectories)
                        Try
                            TotalSize += New FileInfo(File).Length
                        Catch
                        End Try
                    Next
                    DeleteDirectory(Folder)
                    ' 只有目录确实消失才计入“已清理”
                    If Directory.Exists(Folder) Then
                        Failed.Add(SubFolder)
                    Else
                        Removed.Add(SubFolder)
                    End If
                Catch ex As Exception
                    Failed.Add(SubFolder)
                    Logger.Warn(ex, $"清理 {Folder} 失败")
                End Try
            Next
            If Removed.Any() Then
                Hint($"垃圾清理完成，已清理：{String.Join("、", Removed)}（约 {FormatFileSize(TotalSize)}）", HintType.Green)
            Else
                Hint("没有需要清理的垃圾文件。")
            End If
            If Failed.Any() Then
                Hint($"以下目录清理失败，可能正在被占用：{String.Join("、", Failed)}", HintType.Red)
            End If
            Logger.Info($"垃圾清理完成，清理 {Removed.Count} 项，约 {FormatFileSize(TotalSize)}")
        Catch ex As Exception
            Logger.Error(ex, "垃圾清理失败", LogBehavior.Toast)
        End Try
    End Sub

    Private Shared Sub DeleteDirectory(Folder As String)
        For Each File In Directory.EnumerateFiles(Folder, "*", SearchOption.AllDirectories)
            Try
                FileUtils.Delete(File)
            Catch
                ' 被占用的文件跳过，不视为失败
            End Try
        Next
        ' 目录可能因为文件被占用而删不掉，这里不抛异常，让调用方自行判断
        Try
            If Directory.Exists(Folder) Then Directory.Delete(Folder, True)
        Catch
        End Try
    End Sub

    Private Shared Function FormatFileSize(Bytes As Long) As String
        If Bytes < 1024 Then Return $"{Bytes} B"
        If Bytes < 1024 * 1024 Then Return $"{Bytes / 1024.0:0.0} KB"
        If Bytes < 1024L * 1024 * 1024 Then Return $"{Bytes / 1024.0 / 1024:0.0} MB"
        Return $"{Bytes / 1024.0 / 1024 / 1024:0.00} GB"
    End Function

    '========================================
    ' 随机提示与彩蛋
    '========================================

    Private Shared ReadOnly Random As New System.Random()

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
        Return Hints(Random.Next(Hints.Length))
    End Function

    Public Shared Function GetRandomPresetHint() As String
        Return PresetHints(Random.Next(PresetHints.Length))
    End Function

    Public Shared Function GetRandomCave() As String
        Return Caves(Random.Next(Caves.Length))
    End Function

End Class
