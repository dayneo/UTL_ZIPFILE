Imports System
Imports System.Text
Imports System.Collections.Generic
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.IO
Imports System.Data
Imports Oracle.DataAccess.Client
Imports Oracle.DataAccess.Types

''' <summary>
''' Test the functional behaviour of the UTL_ZIPFILE package
''' </summary>
''' <remarks>
''' Requirements:
''' 1) No directory support yet
''' 2) File access by name  (or index???)
''' 3) Write file  (by name,(return index???))
''' 4) Delete file (by name (or index???))
''' 5) Read file   (by name (or index???))
''' 6) Get filename(by index???)  | \  
'''                               |==> This can be implemented using an array of entries. It will give a count
''' 7) Count files                | /  of files and their filename at a given index.
''' 8) Zip (single or multiple returns zip blob)
''' 9) Unzip (returns all files)
''' 10) file entry with name only at the moment
''' 11) Order? Not guaranteed.
''' 
''' Performance will be an issue with zipping... so we want to zip as many files as possible in a single
''' call. So adding files should be array based perhaps... this could have memory implications. Files could
''' be of either clob or blob.
''' 
''' To test, we will compare files in the db with files in our test set. Comparison will be byte for byte on
''' blob files and character for character on clob files. A successful zip will be one in which the JSProc 
''' zips the content, returning the binary zip file, which is then successfully unzipped in the test code
''' and compared successfully with it's test result files.
''' </remarks>
<TestClass()> Public Class FunctionalTests

#Region "Shared attributes and methods"

	Private Shared m_conn As OracleConnection

	<ClassInitialize()> Public Shared Sub MyClassInitialize(ByVal testContext As TestContext)

		m_conn = New OracleConnection(My.Settings.Orcl)
		m_conn.Open()

	End Sub

	<ClassCleanup()> Public Shared Sub MyClassCleanup()

		m_conn.Close()

	End Sub

#End Region

	Private m_testContext As TestContext

	Public Property TestContext() As TestContext
		Get
			Return m_testContext
		End Get
		Set(ByVal value As TestContext)
			m_testContext = value
		End Set
	End Property

#Region "Test initialization and termination"

	Private m_tx As OracleTransaction

	''' <summary>
	''' Initialize each test
	''' </summary>
	''' <remarks>
	''' Every test must be autonomous. We will use transaction rollback to revert back to the 
	''' original dataset
	''' </remarks>
	<TestInitialize()> Public Sub MyTestInitialize()

		m_tx = m_conn.BeginTransaction()

	End Sub

	<TestCleanup()> Public Sub MyTestCleanup()

		m_tx.Rollback()

	End Sub

#End Region

#Region "Utility methods"

	Private Sub ReadFiles(ByVal path As String, ByRef filenames As String(), ByRef filecontents As Byte()())

		Dim dirInfo As DirectoryInfo = New DirectoryInfo(path)
		Dim files As FileInfo() = dirInfo.GetFiles()
		filenames = New String(files.Length - 1) {}
		filecontents = New Byte(files.Length - 1)() {}

		' Execute
		For i As Integer = 0 To files.Length - 1

			filenames(i) = files(i).Name
			filecontents(i) = File.ReadAllBytes(IO.Path.Combine(path, files(i).Name))

		Next

	End Sub

	Private Sub WriteFiles(ByVal path As String, ByVal filenames As String(), ByVal filecontents As Byte()())

		Directory.CreateDirectory(path)
		If filenames Is Nothing Or filecontents Is Nothing Then Exit Sub

		For i As Integer = 0 To filenames.Length - 1

			Dim s As String = IO.Path.Combine(path, filenames(i))
			File.WriteAllBytes(s, filecontents(i))

		Next

	End Sub

#End Region

	<TestMethod(), _
	DeploymentItem("testing\testfiles", "FunctionalTests.MultifileZip\input"), _
	Description("Zip multiple files together. New zip file.")> _
	Public Sub MultifileZip()

		Const srcpath As String = "FunctionalTests.MultifileZip\input"
		Const destpath As String = "FunctionalTests.MultifileZip\output"

		Dim filenames As String()
		Dim filecontents As Byte()()
		ReadFiles(srcpath, filenames, filecontents)

		' Execute
		Dim obj As UTL_ZIPFILE = New UTL_ZIPFILE(m_conn)
		Dim bary As Byte()
		obj.APPEND_FILES(bary, filenames, filecontents)

		' Compare
		TestUtils.Unzip(destpath, bary)
		Dim path1 As String = TestContext.TestDeploymentDir & "\" & srcpath
		Dim path2 As String = TestContext.TestDeploymentDir & "\" & destpath
		Assert.IsTrue(TestUtils.Compare(path1, path2), "File output differs")

	End Sub

	<TestMethod(), _
	DeploymentItem("testing\testfiles", "FunctionalTests.MultifileUnzip\input"), _
	Description("Unzip multiple files")> _
	Public Sub MultifileUnzip()

		Const srcpath As String = "FunctionalTests.MultifileUnzip\input"
		Const destpath As String = "FunctionalTests.MultifileUnzip\output"
		Dim zipFile As Byte() = TestUtils.Zip(Path.Combine(srcpath, "*.*"))
		Dim filenames As String()
		Dim filecontents As Byte()()

		' Execute
		Dim obj As UTL_ZIPFILE = New UTL_ZIPFILE(m_conn)
		obj.UNZIP(zipFile, filenames, filecontents)

		' Compare
		WriteFiles(destpath, filenames, filecontents)
		Dim path1 As String = Path.Combine(TestContext.TestDeploymentDir, srcpath)
		Dim path2 As String = Path.Combine(TestContext.TestDeploymentDir, destpath)
		Assert.IsTrue(TestUtils.Compare(path1, path2), "File output differs")

	End Sub

	<TestMethod(), _
	  DeploymentItem("testing\testfiles", "FunctionalTests.SinglefileUnzip\input"), _
	  Description("Unzip multiple files")> _
	  Public Sub SinglefileUnzip()

		Const srcpath As String = "FunctionalTests.SinglefileUnzip\input"
		Dim zipFile As Byte() = TestUtils.Zip(Path.Combine(srcpath, "*.*"))
		Dim filenames As String()
		Dim filecontents As Byte()()
		ReadFiles(srcpath, filenames, filecontents)

		' Execute
		Dim obj As UTL_ZIPFILE = New UTL_ZIPFILE(m_conn)
		For i As Integer = 0 To filenames.Length - 1

			Dim output As Byte()
			obj.GET_FILE(zipFile, filenames(i), output)
			Assert.IsTrue(TestUtils.Compare(filecontents(i), output), "File content differs")

		Next

	End Sub

	<TestMethod(), _
	  DeploymentItem("testing\testfiles", "FunctionalTests.SinglefileUnzip\input"), _
	  Description("Unzip multiple files")> _
	  Public Sub SinglefileUnzip_Nonexistant()

		Const srcpath As String = "FunctionalTests.SinglefileUnzip\input"
		Dim zipFile As Byte() = TestUtils.Zip(Path.Combine(srcpath, "*.*"))
		Dim filenames As String()
		Dim filecontents As Byte()()
		ReadFiles(srcpath, filenames, filecontents)

		' Execute
		Dim obj As UTL_ZIPFILE = New UTL_ZIPFILE(m_conn)
		Dim output As Byte()
		Dim filename As String = "wrongname" & filenames(0)
		Try

			obj.GET_FILE(zipFile, filename, output)

			Assert.Fail("Did not raise NO_DATA_FOUND exception")

		Catch ex As OracleException

			' should result in NO_DATA_FOUND exception
			Const NO_DATA_FOUND As Integer = 1403
			If ex.Number <> NO_DATA_FOUND Then

				Throw ex

			End If

		End Try

	End Sub

	<TestMethod(), _
   DeploymentItem("testing\testfiles", "FunctionalTests.FilesList\input"), _
   Description("List the contents of a zip file.")> _
   Public Sub FilesList()

		Const srcpath As String = "FunctionalTests.FilesList\input"

		Dim zipFile As Byte() = TestUtils.Zip(Path.Combine(srcpath, "*.*"))
		Dim filenames As String()
		Dim filecontents As Byte()()
		ReadFiles(srcpath, filenames, filecontents)

		' Execute
		Dim obj As UTL_ZIPFILE = New UTL_ZIPFILE(m_conn)
		Dim sary As String() = obj.GET_FILES_LIST(zipFile)

		' Compare filenames
		Assert.IsTrue(TestUtils.Compare(filenames, sary), "File list is not the same")

	End Sub

End Class
