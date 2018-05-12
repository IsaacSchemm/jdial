Imports de.w3is.jdial

Public Class Form1
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim devices = New Discovery().discover()
        ListBox1.Items.Clear()
        PropertyGrid1.SelectedObject = Nothing
        ListBox1.Items.AddRange(devices.ToArray())
    End Sub

    Private Sub ListBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListBox1.SelectedIndexChanged
        PropertyGrid1.SelectedObject = ListBox1.SelectedItem
    End Sub
End Class
