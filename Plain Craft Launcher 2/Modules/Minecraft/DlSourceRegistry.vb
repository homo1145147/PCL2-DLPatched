' 本分支（PCL2-DLPatched）：内置镜像源定义。
'
' 域名替换规则与原版 ModDownload 中各 DlSource* 函数保持一致，只是
' 由「按类型写死」改为「按源组织」，以便在设置中增删与排序。
Namespace Minecraft

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
        ''' 取某个源的 maven 版本。BMCLAPI 对 maven 与 assets/libraries 使用不同的子路径，
        ''' 原版在 DlSourceLibraryGet 中按资源类型分别处理，这里单独提供。
        ''' </summary>
        Public Function GetMavenCompatible(Source As DlSourceRule) As DlSourceRule
            If Source.BuiltinKey <> KeyBmclapi Then Return Source
            Return New DlSourceRule With {
                .Name = Source.Name,
                .BuiltinKey = Source.BuiltinKey,
                .VersionManifest = Source.VersionManifest,
                .Replacements = New List(Of KeyValuePair(Of String, String)) From {
                    MakeRule("https://piston-data.mojang.com", "https://bmclapi2.bangbang93.com/maven"),
                    MakeRule("https://piston-meta.mojang.com", "https://bmclapi2.bangbang93.com/maven"),
                    MakeRule("https://libraries.minecraft.net", "https://bmclapi2.bangbang93.com/maven"),
                    MakeRule("https://maven.minecraftforge.net", "https://bmclapi2.bangbang93.com/maven"),
                    MakeRule("https://maven.neoforged.net/releases", "https://bmclapi2.bangbang93.com/maven"),
                    MakeRule("https://maven.fabricmc.net", "https://bmclapi2.bangbang93.com/maven")
                }
            }
        End Function

        Public Function MakeRule(From As String, To As String) As KeyValuePair(Of String, String)
            Return New KeyValuePair(Of String, String)(From, To)
        End Function

        ''' <summary>
        ''' 按标识取内置源，找不到时返回 Nothing。
        ''' </summary>
        Public Function GetBuiltin(Key As String) As DlSourceRule
            Return BuiltinSources.FirstOrDefault(Function(s) s.BuiltinKey = Key)
        End Function

    End Module

End Namespace
