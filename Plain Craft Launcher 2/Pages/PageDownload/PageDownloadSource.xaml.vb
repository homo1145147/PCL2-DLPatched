' 本分支（PCL2-DLPatched）：自定义下载源配置页面。
Partial Public Class PageDownloadSource

    Private Sub PageDownloadSource_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        RefreshList()
    End Sub

    ''' <summary>
    ''' 按当前配置重建源列表。
    ''' </summary>
    Private Sub RefreshList()
        Try
            PanSourceList.Children.Clear()
            Dim Entries = DlSourceConfig.Load()
            For i = 0 To Entries.Count - 1
                Dim Entry = Entries(i)
                Dim Item As New MyListItem With {
                    .Title = If(Entry.IsBuiltin, Entry.Name, Entry.Name),
                    .Info = If(Entry.IsBuiltin,
                              If(Entry.Enabled, "内置源 · 已启用", "内置源 · 已停用"),
                              If(Entry.Enabled, Entry.Url, "已停用 · " & Entry.Url)),
                    .Type = MyListItem.CheckType.CheckBox,
                    .Checked = Entry.Enabled,
                    .Tag = Entry,
                    .Margin = New Thickness(0, 0, 0, 8)
                }
                '上下移动按钮
                Dim BtnUp As New MyIconButton With {.Tag = i, .ToolTip = "上移", .LogoScale = 0.7, .Logo = Logo.IconButtonArrowUp}
                Dim BtnDown As New MyIconButton With {.Tag = i, .ToolTip = "下移", .LogoScale = 0.7, .Logo = Logo.IconButtonArrowDown}
                If Not Entry.IsBuiltin Then
                    Dim BtnDel As New MyIconButton With {.Tag = i, .ToolTip = "删除", .LogoScale = 0.7, .Logo = Logo.IconButtonCross}
                    AddHandler BtnDel.Click, AddressOf BtnDelete_Click
                    Item.Buttons = New MyIconButton() {BtnUp, BtnDown, BtnDel}
                Else
                    Item.Buttons = New MyIconButton() {BtnUp, BtnDown}
                End If
                AddHandler BtnUp.Click, AddressOf BtnUp_Click
                AddHandler BtnDown.Click, AddressOf BtnDown_Click
                AddHandler Item.Click, AddressOf ItemSource_Click
                PanSourceList.Children.Add(Item)
            Next
        Catch ex As Exception
            Logger.Error(ex, "刷新自定义下载源列表失败", LogBehavior.Toast)
        End Try
    End Sub

    Private Sub ItemSource_Click(sender As Object, e As MouseButtonEventArgs)
        Try
            Dim Item = CType(sender, MyListItem)
            Dim Entry = CType(Item.Tag, DlSourceConfig.SourceEntry)
            Dim Entries = DlSourceConfig.Load()
            Dim Target = Entries.FirstOrDefault(
                Function(x) If(Entry.IsBuiltin, x.BuiltinKey = Entry.BuiltinKey, String.Equals(x.Url, Entry.Url, StringComparison.OrdinalIgnoreCase)))
            If Target Is Nothing Then Return
            Target.Enabled = Not Target.Enabled
            DlSourceConfig.Save(Entries)
            RefreshList()
            e.Handled = True
        Catch ex As Exception
            Logger.Error(ex, "切换下载源启用状态失败", LogBehavior.Toast)
        End Try
    End Sub

    Private Sub BtnUp_Click(sender As Object, e As EventArgs)
        MoveSource(CInt(CType(sender, MyIconButton).Tag), -1)
    End Sub

    Private Sub BtnDown_Click(sender As Object, e As EventArgs)
        MoveSource(CInt(CType(sender, MyIconButton).Tag), 1)
    End Sub

    Private Sub MoveSource(Index As Integer, Delta As Integer)
        Try
            Dim Entries = DlSourceConfig.Load()
            Dim NewIndex As Integer = Index + Delta
            If NewIndex < 0 OrElse NewIndex >= Entries.Count Then Return
            Dim Temp = Entries(Index)
            Entries(Index) = Entries(NewIndex)
            Entries(NewIndex) = Temp
            DlSourceConfig.Save(Entries)
            RefreshList()
        Catch ex As Exception
            Logger.Error(ex, "移动下载源失败", LogBehavior.Toast)
        End Try
    End Sub

    Private Sub BtnDelete_Click(sender As Object, e As EventArgs)
        Try
            Dim Index = CInt(CType(sender, MyIconButton).Tag)
            Dim Entries = DlSourceConfig.Load()
            If Index < 0 OrElse Index >= Entries.Count Then Return
            If Entries(Index).IsBuiltin Then Return
            Entries.RemoveAt(Index)
            DlSourceConfig.Save(Entries)
            RefreshList()
        Catch ex As Exception
            Logger.Error(ex, "删除下载源失败", LogBehavior.Toast)
        End Try
    End Sub

    Private Sub BtnAddSource_Click(sender As Object, e As MouseButtonEventArgs)
        Try
            Dim Url As String = If(TextSourceUrl.Text, "").Trim()
            If String.IsNullOrWhiteSpace(Url) Then
                Hint("请填写源的地址。", HintType.Red)
                Return
            End If
            If Not (Url.StartsWithF("http://", True) OrElse Url.StartsWithF("https://", True)) Then
                Hint("地址需要以 http:// 或 https:// 开头。", HintType.Red)
                Return
            End If
            Dim Entries = DlSourceConfig.Load()
            If Entries.Any(Function(x) Not x.IsBuiltin AndAlso String.Equals(x.Url.TrimEnd("/"c), Url.TrimEnd("/"c), StringComparison.OrdinalIgnoreCase)) Then
                Hint("该地址已经在列表中。", HintType.Red)
                Return
            End If
            Entries.Add(New DlSourceConfig.SourceEntry With {
                .Name = If(String.IsNullOrWhiteSpace(TextSourceName.Text), Url, TextSourceName.Text.Trim()),
                .Url = Url,
                .Enabled = True
            })
            DlSourceConfig.Save(Entries)
            TextSourceName.Text = ""
            TextSourceUrl.Text = ""
            RefreshList()
            Hint("已添加下载源。", HintType.Green)
        Catch ex As Exception
            Logger.Error(ex, "添加下载源失败", LogBehavior.Toast)
        End Try
    End Sub

    Private Sub BtnReset_Click(sender As Object, e As MouseButtonEventArgs)
        Try
            DlSourceConfig.Save(DlSourceConfig.DefaultEntries())
            RefreshList()
            Hint("已恢复默认下载源。", HintType.Green)
        Catch ex As Exception
            Logger.Error(ex, "恢复默认下载源失败", LogBehavior.Toast)
        End Try
    End Sub

    Private Sub BtnTest_Click(sender As Object, e As MouseButtonEventArgs)
        LabTestResult.Visibility = Visibility.Visible
        LabTestResult.Text = "正在测试……"
        RunInThread(
        Sub()
            Try
                Dim Entries = DlSourceConfig.Load().Where(Function(x) x.Enabled).ToList()
                If Entries.Count = 0 Then
                    RunInUi(Sub()
                                LabTestResult.Text = "没有已启用的源。"
                                LabTestResult.Foreground = New MyColor(200, 60, 60)
                            End Sub)
                    Return
                End If
                Dim Entry = Entries(0)
                Dim Url As String = DlSourceConfig.GetVersionManifest(Entry)
                Dim StartTime As Long = GetTimeMs()
                Dim Json As JObject = NetRequestByClient(Url, RequireJson:=True).DeserializeJson()
                Dim Cost As Long = GetTimeMs() - StartTime
                Dim Count As Integer = 0
                If Json("versions") IsNot Nothing Then Count = CType(Json("versions"), JArray).Count
                RunInUi(Sub()
                            LabTestResult.Foreground = New MyColor(40, 150, 60)
                            LabTestResult.Text = $"{If(Entry.Name, Entry.Url)}：成功，用时 {Cost} ms，版本数 {Count}。"
                        End Sub)
            Catch ex As Exception
                RunInUi(Sub()
                            LabTestResult.Foreground = New MyColor(200, 60, 60)
                            LabTestResult.Text = "测试失败：" & ex.Message
                        End Sub)
            End Try
        End Sub)
    End Sub

End Class
