Imports System.IO
Imports System.Diagnostics

Module TestUtils

	Public Function Zip(ByVal path As String) As Byte()

		' write zip file to a temp location
		Dim filename As String = IO.Path.GetTempFileName() & ".zip"

		' unzip the file to the specified location
		Dim args As String = "-min -a """ & filename & """ """ & path & """"
		Dim winzip As Process = Process.Start("C:\Program Files\WINZIP\WINZIP32.exe", args)
		Do
			winzip.WaitForExit(100)
		Loop Until winzip.HasExited()

		Return File.ReadAllBytes(filename)

	End Function

	Public Sub Unzip(ByVal path As String, ByVal file As Byte())

		' write zip file to a temp location
		Dim filename As String = IO.Path.GetTempFileName()
		System.IO.File.WriteAllBytes(filename, file)

		' unzip the file to the specified location
		Dim args As String = "-e -o """ & filename & """ """ & path & """"
		Dim winzip As Process = Process.Start("C:\Program Files\WINZIP\WINZIP32.exe", args)
		Do
			winzip.WaitForExit(100)
		Loop Until winzip.HasExited()

	End Sub

	Public Function Compare(ByVal array1 As String(), ByVal array2 As String()) As Boolean

		' 1) Compare array lengths
		If array1.Length <> array2.Length Then Return False

		' 2) Compare contents in order
		For i As Integer = 0 To array1.Length - 1

			If array1(i) <> array2(i) Then Return False

		Next

		Return True

	End Function

	Public Function Compare(ByVal array1 As Byte(), ByVal array2 As Byte()) As Boolean

		' 1) Compare array lengths
		If array1.Length <> array2.Length Then Return False

		' 2) Compare contents in order
		For i As Integer = 0 To array1.Length - 1

			If array1(i) <> array2(i) Then Return False

		Next

		Return True

	End Function

	Public Function Compare(ByVal path1 As String, ByVal path2 As String) As Boolean

		Dim dir1 As DirectoryInfo = New DirectoryInfo(path1)
		Dim fileAry1 As FileInfo() = dir1.GetFiles()
		Dim dir2 As DirectoryInfo = New DirectoryInfo(path2)
		Dim fileAry2 As FileInfo() = dir2.GetFiles()

		' 1) Compare file count
		If fileAry1.Length <> fileAry2.length Then Return False

		' 2) Compare file names and sizes
		For i As Integer = 0 To fileAry1.Length - 1

			If fileAry1(i).Name <> fileAry2(i).Name Then Return False
			If fileAry1(i).Length <> fileAry2(i).Length Then Return False

		Next

		' 3) Compare file contents
		For i As Integer = 0 To fileAry1.Length - 1

			Dim fs1 As FileStream = fileAry1(i).OpenRead()
			Dim fs2 As FileStream = fileAry2(i).OpenRead()

			Do Until fs1.Position >= fs1.Length

				Dim b1 As Integer = fs1.ReadByte()
				Dim b2 As Integer = fs2.ReadByte()

				If b1 <> b2 Then Return False

			Loop

		Next

		Return True

	End Function

End Module
