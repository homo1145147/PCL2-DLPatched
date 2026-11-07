' 本分支（PCL2-DLPatched）：镜像下载源的结构定义。
'
' 原版中各镜像域名以字符串替换的形式硬编码在 ModDownload 的各个
' DlSource* 函数中，既无法增删，也无法调整顺序。这里把「一个镜像源
' 包含哪些域名替换规则」抽取成数据，由设置中的源列表决定使用哪些、
' 以什么顺序使用。

''' <summary>
''' 单个镜像源的规则集合。每个字段对应一类资源；为空表示该源不代理此类资源。
''' </summary>
Public Class DlSourceRule

    Public Property Name As String = ""
    ''' <summary>
    ''' 内置源标识，如 Official / Bmclapi。自定义源为空。
    ''' </summary>
    Public Property BuiltinKey As String = ""
    ''' <summary>
    ''' 版本清单（version_manifest.json）的地址。为空表示该源不提供版本清单。
    ''' </summary>
    Public Property VersionManifest As String = ""
    ''' <summary>
    ''' 文件下载所需的域名替换表：原域名、目标域名。
    ''' </summary>
    Public Property Replacements As New List(Of KeyValuePair(Of String, String))
    ''' <summary>
    ''' 是否用于 Mod / 资源下载（CurseForge、Modrinth 等）。
    ''' </summary>
    Public Property IsModMirror As Boolean = False

    Public ReadOnly Property IsOfficial As Boolean
        Get
            Return BuiltinKey = DlSourceRegistry.KeyOfficial
        End Get
    End Property

    ''' <summary>
    ''' 按本源的替换表处理一个原始 URL。若没有命中任何规则则返回原地址。
    ''' </summary>
    Public Function Map(Original As String) As String
        If Original Is Nothing Then Return Nothing
        Dim Result As String = Original
        For Each Rule In Replacements
            Result = Result.Replace(Rule.Key, Rule.Value)
        Next
        Return Result
    End Function

    ''' <summary>
    ''' 判断该 URL 是否需要经过本源的替换才能得到不同地址。
    ''' </summary>
    Public Function CanHandle(Original As String) As Boolean
        If Original Is Nothing Then Return False
        If IsOfficial Then Return True
        Return Map(Original) <> Original
    End Function

    Public Overrides Function ToString() As String
        Return If(String.IsNullOrWhiteSpace(Name), BuiltinKey, Name)
    End Function

End Class
