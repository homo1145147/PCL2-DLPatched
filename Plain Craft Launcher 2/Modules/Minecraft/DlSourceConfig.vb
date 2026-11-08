' 本分支（PCL2-DLPatched）：自定义下载源配置。
'
' 原版各镜像域名硬编码在 ModDownload 的 DlSource* 函数里。这里提供：
'   - 内置源（Mojang 官方、BMCLAPI），不可删除，只能排序与启停
'   - 自定义源：用户填写一个镜像根地址，按统一规则匹配官方域名
' 配置以 JSON 形式存入设置项 ToolDownloadSourceConfig。

Public Module DlSourceConfig

    ''' <summary>
    ''' 一条来源记录：内置源只存标识，自定义源存完整信息。
    ''' </summary>
    Public Class SourceEntry
        ''' <summary>
        ''' 内置源标识（Official / Bmclapi）；自定义源为空。
        ''' </summary>
        Public Property BuiltinKey As String = ""
        ''' <summary>
        ''' 自定义源的显示名。
        ''' </summary>
        Public Property Name As String = ""
        ''' <summary>
        ''' 自定义源的根地址，例如 https://cdn.example.com 或 https://cdn.example.com/bmcl/
        ''' </summary>
        Public Property Url As String = ""
        ''' <summary>
        ''' 是否启用。禁用的源不参与候选地址生成。
        ''' </summary>
        Public Property Enabled As Boolean = True

        Public ReadOnly Property IsBuiltin As Boolean
            Get
                Return Not String.IsNullOrWhiteSpace(BuiltinKey)
            End Get
        End Property

        Public Function Clone() As SourceEntry
            Return New SourceEntry With {.BuiltinKey = BuiltinKey, .Name = Name, .Url = Url, .Enabled = Enabled}
        End Function
    End Class

    ''' <summary>
    ''' 读取当前配置。配置为空或损坏时返回默认列表（官方源 + BMCLAPI）。
    ''' </summary>
    Public Function Load() As List(Of SourceEntry)
        Try
            Dim Raw As String = Settings.Get(Of String)("ToolDownloadSourceConfig")
            If String.IsNullOrWhiteSpace(Raw) Then Return DefaultEntries()
            Dim Array = JArray.Parse(Raw)
            Dim Result As New List(Of SourceEntry)
            For Each Item In Array
                Dim Entry As New SourceEntry With {
                    .BuiltinKey = If(Item("builtin")?.ToString(), ""),
                    .Name = If(Item("name")?.ToString(), ""),
                    .Url = If(Item("url")?.ToString(), ""),
                    .Enabled = If(Item("enabled") Is Nothing, True, Item("enabled").ToObject(Of Boolean)())
                }
                If Entry.IsBuiltin Then
                    '内置源校验标识是否有效
                    If DlSourceRegistry.GetBuiltin(Entry.BuiltinKey) Is Nothing Then Continue For
                    Entry.Name = DlSourceRegistry.GetBuiltin(Entry.BuiltinKey).Name
                    Entry.Url = ""
                Else
                    If String.IsNullOrWhiteSpace(Entry.Url) Then Continue For
                End If
                '去重
                If Result.Any(Function(e) If(Entry.IsBuiltin, e.BuiltinKey = Entry.BuiltinKey, String.Equals(e.Url, Entry.Url, StringComparison.OrdinalIgnoreCase))) Then Continue For
                Result.Add(Entry)
            Next
            '确保内置源齐全：缺失的补在末尾
            For Each BuiltinDef In DlSourceRegistry.BuiltinSources
                If Not Result.Any(Function(e) e.BuiltinKey = BuiltinDef.BuiltinKey) Then
                    Result.Add(New SourceEntry With {.BuiltinKey = BuiltinDef.BuiltinKey, .Name = BuiltinDef.Name, .Enabled = True})
                End If
            Next
            Return Result
        Catch ex As Exception
            Logger.Warn(ex, "解析自定义下载源配置失败，已使用默认配置")
            Return DefaultEntries()
        End Try
    End Function

    ''' <summary>
    ''' 保存配置。内置源始终写入，顺序即优先级。
    ''' </summary>
    Public Sub Save(Entries As List(Of SourceEntry))
        Try
            Dim Array As New JArray
            For Each Entry In Entries
                Dim Obj As New JObject
                If Entry.IsBuiltin Then
                    Obj("builtin") = Entry.BuiltinKey
                Else
                    Obj("name") = Entry.Name
                    Obj("url") = Entry.Url
                End If
                Obj("enabled") = Entry.Enabled
                Array.Add(Obj)
            Next
            Settings.Set("ToolDownloadSourceConfig", Array.ToString(Newtonsoft.Json.Formatting.None))
        Catch ex As Exception
            Logger.Error(ex, "保存自定义下载源配置失败", LogBehavior.Toast)
        End Try
    End Sub

    Public Function DefaultEntries() As List(Of SourceEntry)
        Return DlSourceRegistry.BuiltinSources.
            Select(Function(s) New SourceEntry With {.BuiltinKey = s.BuiltinKey, .Name = s.Name, .Enabled = True}).
            ToList()
    End Function

    ''' <summary>
    ''' 把配置项转换为运行时使用的规则列表（跳过停用项）。
    ''' </summary>
    Public Function ToRules(Entries As List(Of SourceEntry)) As List(Of DlSourceRule)
        Dim Result As New List(Of DlSourceRule)
        For Each Entry In Entries
            If Not Entry.Enabled Then Continue For
            If Entry.IsBuiltin Then
                Dim Builtin = DlSourceRegistry.GetBuiltin(Entry.BuiltinKey)
                If Builtin IsNot Nothing Then Result.Add(Builtin)
            Else
                Dim Custom = MakeCustomRule(Entry)
                If Custom IsNot Nothing Then Result.Add(Custom)
            End If
        Next
        If Result.Count = 0 Then Result.AddRange(DlSourceRegistry.BuiltinSources)
        Return Result
    End Function

    ''' <summary>
    ''' 用自定义源生成 DlSourceRule。
    ''' 自定义源采用「清单入口」模型：只提供版本清单地址，版本 JSON 与所有
    ''' 文件地址一律沿用清单/版本 JSON 里携带的 URL，不做域名替换。
    ''' 这适用于自身完成全部资源映射的 CDN（例如玄剑 CDN）。
    ''' </summary>
    Public Function MakeCustomRule(Entry As SourceEntry) As DlSourceRule
        Dim Root As String = If(Entry.Url, "").Trim().TrimEnd("/"c)
        If String.IsNullOrWhiteSpace(Root) Then Return Nothing
        If Not (Root.StartsWithF("http://", True) OrElse Root.StartsWithF("https://", True)) Then Return Nothing
        Return New DlSourceRule With {
            .Name = If(String.IsNullOrWhiteSpace(Entry.Name), Root, Entry.Name),
            .BuiltinKey = "",
            .VersionManifest = GetVersionManifest(Entry),
            .Replacements = New List(Of KeyValuePair(Of String, String))
        }
    End Function

    ''' <summary>
    ''' 取出某个源的版本清单地址。
    ''' 内置源用内置地址；自定义源默认按 {根}/mc/game/version_manifest_v2.json
    ''' 取清单，取不到时回退到 {根}/mc/game/version_manifest.json。
    ''' </summary>
    Public Function GetVersionManifest(Entry As SourceEntry) As String
        If Entry Is Nothing Then Return Nothing
        If Entry.IsBuiltin Then
            Dim Builtin = DlSourceRegistry.GetBuiltin(Entry.BuiltinKey)
            Return Builtin?.VersionManifest
        End If
        Dim Root As String = If(Entry.Url, "").Trim().TrimEnd("/"c)
        If String.IsNullOrWhiteSpace(Root) Then Return Nothing
        '自定义源允许直接填写清单完整地址（以 .json 结尾）
        If Root.EndsWithF(".json", True) Then Return Root
        Return Root & "/mc/game/version_manifest_v2.json"
    End Function

    ''' <summary>
    ''' 自定义源的清单地址回退列表：v2 取不到时再试不带 _v2 的命名。
    ''' </summary>
    Public Function GetVersionManifestCandidates(Entry As SourceEntry) As String()
        Dim Primary As String = GetVersionManifest(Entry)
        If String.IsNullOrWhiteSpace(Primary) Then Return {}
        If Entry.IsBuiltin OrElse Primary.EndsWithF("version_manifest.json", True) Then Return {Primary}
        Return {Primary, Primary.Replace("version_manifest_v2.json", "version_manifest.json")}
    End Function

End Module
