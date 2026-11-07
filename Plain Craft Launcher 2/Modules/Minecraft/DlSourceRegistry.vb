' 本分支（PCL2-DLPatched）：内置镜像源定义。
'
' 域名替换规则与原版 ModDownload 中各 DlSource* 函数保持一致，只是
' 由「按类型写死」改为「按源组织」，以便在设置中增删与排序。

Public Module DlSourceRegistry

    Public Const KeyOfficial As String = "Official"
    Public Const KeyBmclapi As String = "Bmclapi"

    ''' <summary>
    ''' 所有内置源。顺序即默认顺序，排在前面表示优先使用。
    ''' </summary>
    Public ReadOnly Property BuiltinSources As List(Of DlSourceRule) = New List(Of DlSourceRule) From {
        New DlSourceRule With {
            .Name = "Mojang 官方源",
            .BuiltinKey = KeyOfficial,
            .VersionManifest = "https://launchermeta.mojang.com/mc/game/version_manifest.json"
        },
        New DlSourceRule With {
            .Name = "BMCLAPI",
            .BuiltinKey = KeyBmclapi,
            .VersionManifest = "https://bmclapi2.bangbang93.com/mc/game/version_manifest.json",
            .Replacements = New List(Of KeyValuePair(Of String, String)) From {
                MakeRule("https://piston-data.mojang.com", "https://bmclapi2.bangbang93.com"),
                MakeRule("https://piston-meta.mojang.com", "https://bmclapi2.bangbang93.com"),
                MakeRule("https://launcher.mojang.com", "https://bmclapi2.bangbang93.com"),
                MakeRule("https://launchermeta.mojang.com", "https://bmclapi2.bangbang93.com"),
                MakeRule("https://libraries.minecraft.net", "https://bmclapi2.bangbang93.com/libraries"),
                MakeRule("https://resources.download.minecraft.net", "https://bmclapi2.bangbang93.com/assets")
            }
        }
    }

    ''' <summary>
    ''' 取某个源用于 Libraries 与 maven 的版本。BMCLAPI 对 maven 与 assets
    ''' 使用不同的子路径，原版在 DlSourceLibraryGet 中分别处理，这里单独提供。
    ''' </summary>
    Public Function GetMavenCompatible(Source As DlSourceRule) As DlSourceRule
        If Source Is Nothing Then Return Nothing
        If Source.BuiltinKey <> KeyBmclapi Then Return Source
        Return New DlSourceRule With {
            .Name = Source.Name,
            .BuiltinKey = Source.BuiltinKey,
            .VersionManifest = Source.VersionManifest,
            .Replacements = New List(Of KeyValuePair(Of String, String)) From {
                MakeRule("https://piston-data.mojang.com", "https://bmclapi2.bangbang93.com/maven"),
                MakeRule("https://piston-meta.mojang.com", "https://bmclapi2.bangbang93.com/maven"),
                MakeRule("https://libraries.minecraft.net", "https://bmclapi2.bangbang93.com/maven"),
                MakeRule("https://resources.download.minecraft.net", "https://bmclapi2.bangbang93.com/assets"),
                MakeRule("https://maven.minecraftforge.net", "https://bmclapi2.bangbang93.com/maven"),
                MakeRule("https://maven.neoforged.net/releases", "https://bmclapi2.bangbang93.com/maven"),
                MakeRule("https://maven.fabricmc.net", "https://bmclapi2.bangbang93.com/maven")
            }
        }
    End Function

    ''' <summary>
    ''' 构造一条域名替换规则。注意 From / To 是 VB 保留字，因此参数用 SourceUrl / TargetUrl。
    ''' </summary>
    Public Function MakeRule(SourceUrl As String, TargetUrl As String) As KeyValuePair(Of String, String)
        Return New KeyValuePair(Of String, String)(SourceUrl, TargetUrl)
    End Function

    ''' <summary>
    ''' 按标识取内置源，找不到时返回 Nothing。
    ''' </summary>
    Public Function GetBuiltin(Key As String) As DlSourceRule
        Return BuiltinSources.FirstOrDefault(Function(s) s.BuiltinKey = Key)
    End Function

End Module
