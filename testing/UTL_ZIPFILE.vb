Imports System
Imports System.Text
Imports System.Collections.Generic
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.IO
Imports System.Data
Imports Oracle.DataAccess.Client
Imports Oracle.DataAccess.Types

Public Class UTL_ZIPFILE

	Private m_conn As OracleConnection

	Public Sub New(ByVal oracleConnection As OracleConnection)

		m_conn = oracleConnection

	End Sub

	Public ReadOnly Property OracleConnection() As OracleConnection
		Get
			Return m_conn
		End Get
	End Property

	Public Function CONVERT_TO_BLOB(ByRef p_clob As String) As Byte()

		Dim cmd As OracleCommand = m_conn.CreateCommand()
		cmd.CommandText = "UTL_ZIPFILE.CONVERT_TO_BLOB"
		cmd.CommandType = CommandType.StoredProcedure
		cmd.Parameters.Add("P_BLOB", OracleDbType.Blob, ParameterDirection.ReturnValue)
		cmd.Parameters.Add("P_CLOB", OracleDbType.Clob, p_clob, ParameterDirection.Input)
		cmd.ExecuteNonQuery()

		Dim p_blob As OracleBlob = CType(cmd.Parameters("P_BLOB").Value, OracleBlob)
		Return p_blob.Value

	End Function

	Public Function CONVERT_TO_CLOB(ByRef p_blob As Byte()) As String

		Dim cmd As OracleCommand = m_conn.CreateCommand()
		cmd.CommandText = "UTL_ZIPFILE.CONVERT_TO_CLOB"
		cmd.CommandType = CommandType.StoredProcedure
		cmd.Parameters.Add("P_CLOB", OracleDbType.Clob, ParameterDirection.ReturnValue)
		cmd.Parameters.Add("P_BLOB", OracleDbType.Blob, p_blob, ParameterDirection.Input)
		cmd.ExecuteNonQuery()

		Dim p_clob As OracleClob = CType(cmd.Parameters("P_CLOB").Value, OracleClob)
		Return p_clob.Value

	End Function

	Public Function ZIP(ByVal p_filename As String, ByRef p_file As Byte()) As Byte()

		Dim cmd As OracleCommand = m_conn.CreateCommand()
		cmd.CommandText = "UTL_ZIPFILE.ZIP"
		cmd.CommandType = CommandType.StoredProcedure
		cmd.Parameters.Add("P_ZIPFILE", OracleDbType.Blob, ParameterDirection.ReturnValue)
		cmd.Parameters.Add("P_FILENAME", OracleDbType.Varchar2, p_filename, ParameterDirection.Input)
		cmd.Parameters.Add("P_FILE", OracleDbType.Blob, p_file, ParameterDirection.Input)
		cmd.ExecuteNonQuery()

		Dim zipFile As OracleBlob = CType(cmd.Parameters("P_ZIPFILE").Value, OracleBlob)
		Return zipFile.Value

	End Function

	Public Sub APPEND_FILES(ByRef p_zipfile As Byte(), ByRef p_filenames As String(), ByRef p_files As Byte()())

		Dim cmd As OracleCommand = m_conn.CreateCommand()
		cmd.CommandText = "UTL_ZIPFILE.APPEND_FILES"
		cmd.CommandType = CommandType.StoredProcedure
		Dim param As OracleParameter
		cmd.Parameters.Add("P_ZIPFILE", OracleDbType.Blob, p_zipfile, ParameterDirection.InputOutput)
		param = cmd.Parameters.Add("P_FILENAMES", OracleDbType.Object, ParameterDirection.InputOutput)
		param.UdtTypeName = "VARCHAR2_TABLE_TYPE"
		Dim vtt As VARCHAR2_TABLE_TYPE = New VARCHAR2_TABLE_TYPE()
		vtt.Value = p_filenames
		param.Value = vtt
		param = cmd.Parameters.Add("P_FILES", OracleDbType.Object, ParameterDirection.InputOutput)
		param.UdtTypeName = "BLOB_TABLE_TYPE"
		Dim btt As BLOB_TABLE_TYPE = New BLOB_TABLE_TYPE()
		btt.Value = p_files
		param.Value = btt
		cmd.ExecuteNonQuery()

		Dim zipFile As OracleBlob = CType(cmd.Parameters("P_ZIPFILE").Value, OracleBlob)
		vtt = CType(cmd.Parameters("P_FILENAMES").Value, VARCHAR2_TABLE_TYPE)
		btt = CType(cmd.Parameters("P_FILES").Value, BLOB_TABLE_TYPE)

		p_zipfile = zipFile.Value
		p_filenames = vtt.Value
		p_files = btt.Value

	End Sub

	Public Sub REMOVE_FILE(ByRef p_zipfile As Byte(), ByVal p_index As Decimal)

		Dim cmd As OracleCommand = m_conn.CreateCommand()
		cmd.CommandText = "UTL_ZIPFILE.REMOVE_FILE"
		cmd.CommandType = CommandType.StoredProcedure
		Dim param As OracleParameter
		cmd.Parameters.Add("P_ZIPFILE", OracleDbType.Blob, p_zipfile, ParameterDirection.InputOutput)
		param = cmd.Parameters.Add("P_INDEX", OracleDbType.Decimal, p_index, ParameterDirection.InputOutput)
		cmd.ExecuteNonQuery()

		Dim zipFile As OracleBlob = CType(cmd.Parameters("P_ZIPFILE").Value, OracleBlob)
		p_zipfile = zipFile.Value

	End Sub

	Public Sub REMOVE_FILE(ByRef p_zipfile As Byte(), ByVal p_filename As String)

		Dim cmd As OracleCommand = m_conn.CreateCommand()
		cmd.CommandText = "UTL_ZIPFILE.REMOVE_FILE"
		cmd.CommandType = CommandType.StoredProcedure
		Dim param As OracleParameter
		cmd.Parameters.Add("P_ZIPFILE", OracleDbType.Blob, p_zipfile, ParameterDirection.InputOutput)
		param = cmd.Parameters.Add("P_FILENAME", OracleDbType.Varchar2, p_filename, ParameterDirection.InputOutput)
		cmd.ExecuteNonQuery()

		Dim zipFile As OracleBlob = CType(cmd.Parameters("P_ZIPFILE").Value, OracleBlob)
		p_zipfile = zipFile.Value

	End Sub

	Public Sub REMOVE_FILE(ByRef p_zipfile As Byte(), ByRef p_filenames As String())

		Dim cmd As OracleCommand = m_conn.CreateCommand()
		cmd.CommandText = "UTL_ZIPFILE.REMOVE_FILE"
		cmd.CommandType = CommandType.StoredProcedure
		Dim param As OracleParameter
		cmd.Parameters.Add("P_ZIPFILE", OracleDbType.Blob, p_zipfile, ParameterDirection.InputOutput)
		param = cmd.Parameters.Add("P_FILENAMES", OracleDbType.Object, ParameterDirection.InputOutput)
		param.UdtTypeName = "VARCHAR2_TABLE_TYPE"
		Dim vtt As VARCHAR2_TABLE_TYPE = New VARCHAR2_TABLE_TYPE()
		vtt.Value = p_filenames
		param.Value = vtt
		cmd.ExecuteNonQuery()

		Dim zipFile As OracleBlob = CType(cmd.Parameters("P_ZIPFILE").Value, OracleBlob)
		vtt = CType(cmd.Parameters("P_FILENAMES").Value, VARCHAR2_TABLE_TYPE)

		p_zipfile = zipFile.Value
		p_filenames = vtt.Value

	End Sub


	Public Sub UNZIP(ByRef p_zipfile As Byte(), ByRef p_filenames As String(), ByRef p_files As Byte()())

		Dim cmd As OracleCommand = m_conn.CreateCommand()
		cmd.CommandText = "UTL_ZIPFILE.UNZIP"
		cmd.CommandType = CommandType.StoredProcedure
		Dim param As OracleParameter
		cmd.Parameters.Add("P_ZIPFILE", OracleDbType.Blob, p_zipfile, ParameterDirection.InputOutput)
		param = cmd.Parameters.Add("P_FILENAMES", OracleDbType.Object, ParameterDirection.InputOutput)
		param.UdtTypeName = "VARCHAR2_TABLE_TYPE"
		param = cmd.Parameters.Add("P_FILES", OracleDbType.Object, ParameterDirection.InputOutput)
		param.UdtTypeName = "BLOB_TABLE_TYPE"
		cmd.ExecuteNonQuery()

		Dim zipFile As OracleBlob = CType(cmd.Parameters("P_ZIPFILE").Value, OracleBlob)
		Dim vtt As VARCHAR2_TABLE_TYPE = CType(cmd.Parameters("P_FILENAMES").Value, VARCHAR2_TABLE_TYPE)
		Dim btt As BLOB_TABLE_TYPE = CType(cmd.Parameters("P_FILES").Value, BLOB_TABLE_TYPE)

		p_zipfile = zipFile.Value
		p_filenames = vtt.Value
		p_files = btt.Value

	End Sub

	Public Sub GET_FILE(ByRef p_zipfile As Byte(), ByVal p_index As Decimal, ByRef p_file As Byte())

		Dim cmd As OracleCommand = m_conn.CreateCommand()
		cmd.CommandText = "UTL_ZIPFILE.GET_FILE"
		cmd.CommandType = CommandType.StoredProcedure
		Dim param As OracleParameter
		cmd.Parameters.Add("P_ZIPFILE", OracleDbType.Blob, p_zipfile, ParameterDirection.InputOutput)
		param = cmd.Parameters.Add("P_INDEX", OracleDbType.Decimal, p_index, ParameterDirection.Input)
		param = cmd.Parameters.Add("P_FILE", OracleDbType.Blob, ParameterDirection.Output)
		cmd.ExecuteNonQuery()

		Dim zipFile As OracleBlob = CType(cmd.Parameters("P_ZIPFILE").Value, OracleBlob)
		Dim bfile As OracleBlob = CType(cmd.Parameters("P_FILE").Value, OracleBlob)

		p_zipfile = zipFile.Value
		p_file = bfile.Value

	End Sub

	Public Sub GET_FILE(ByRef p_zipfile As Byte(), ByVal p_filename As String, ByRef p_file As Byte())

		Dim cmd As OracleCommand = m_conn.CreateCommand()
		cmd.CommandText = "UTL_ZIPFILE.GET_FILE"
		cmd.CommandType = CommandType.StoredProcedure
		Dim param As OracleParameter
		cmd.Parameters.Add("P_ZIPFILE", OracleDbType.Blob, p_zipfile, ParameterDirection.InputOutput)
		param = cmd.Parameters.Add("P_FILENAME", OracleDbType.Varchar2, p_filename, ParameterDirection.Input)
		param = cmd.Parameters.Add("P_FILE", OracleDbType.Blob, ParameterDirection.Output)
		cmd.ExecuteNonQuery()

		Dim zipFile As OracleBlob = CType(cmd.Parameters("P_ZIPFILE").Value, OracleBlob)
		Dim bfile As OracleBlob = CType(cmd.Parameters("P_FILE").Value, OracleBlob)

		p_zipfile = zipFile.Value
		p_file = bfile.Value

	End Sub

	Public Function GET_FILE(ByRef p_zipfile As Byte(), ByVal p_index As Decimal) As Byte()

		Dim cmd As OracleCommand = m_conn.CreateCommand()
		cmd.CommandText = "UTL_ZIPFILE.GET_FILE"
		cmd.CommandType = CommandType.StoredProcedure
		Dim param As OracleParameter
		param = cmd.Parameters.Add("P_FILE", OracleDbType.Blob, ParameterDirection.ReturnValue)
		cmd.Parameters.Add("P_ZIPFILE", OracleDbType.Blob, p_zipfile, ParameterDirection.InputOutput)
		param = cmd.Parameters.Add("P_INDEX", OracleDbType.Decimal, p_index, ParameterDirection.Input)
		cmd.ExecuteNonQuery()

		Dim zipFile As OracleBlob = CType(cmd.Parameters("P_ZIPFILE").Value, OracleBlob)
		Dim bfile As OracleBlob = CType(cmd.Parameters("P_FILE").Value, OracleBlob)

		p_zipfile = zipFile.Value
		Return bfile.Value

	End Function

	Public Function GET_FILE(ByRef p_zipfile As Byte(), ByVal p_filename As String) As Byte()

		Dim cmd As OracleCommand = m_conn.CreateCommand()
		cmd.CommandText = "UTL_ZIPFILE.GET_FILE"
		cmd.CommandType = CommandType.StoredProcedure
		Dim param As OracleParameter
		param = cmd.Parameters.Add("P_FILE", OracleDbType.Blob, ParameterDirection.ReturnValue)
		cmd.Parameters.Add("P_ZIPFILE", OracleDbType.Blob, p_zipfile, ParameterDirection.InputOutput)
		param = cmd.Parameters.Add("P_FILENAME", OracleDbType.Varchar2, p_filename, ParameterDirection.Input)
		cmd.ExecuteNonQuery()

		Dim zipFile As OracleBlob = CType(cmd.Parameters("P_ZIPFILE").Value, OracleBlob)
		Dim bfile As OracleBlob = CType(cmd.Parameters("P_FILE").Value, OracleBlob)

		p_zipfile = zipFile.Value
		Return bfile.Value

	End Function

	'	-- Gets a list of the files contained in a zip file
	'	function get_files_list(p_zipfile in out nocopy blob) return varchar2_table_type;
	Public Function GET_FILES_LIST(ByRef p_zipfile As Byte()) As String()

		Dim cmd As OracleCommand = m_conn.CreateCommand()
		cmd.CommandText = "UTL_ZIPFILE.GET_FILES_LIST"
		cmd.CommandType = CommandType.StoredProcedure
		Dim param As OracleParameter
		param = cmd.Parameters.Add("P_FILENAMES", OracleDbType.Object, ParameterDirection.ReturnValue)
		param.UdtTypeName = "VARCHAR2_TABLE_TYPE"
		cmd.Parameters.Add("P_ZIPFILE", OracleDbType.Blob, p_zipfile, ParameterDirection.InputOutput)
		cmd.ExecuteNonQuery()

		Dim zipFile As OracleBlob = CType(cmd.Parameters("P_ZIPFILE").Value, OracleBlob)
		Dim vtt As VARCHAR2_TABLE_TYPE = CType(cmd.Parameters("P_FILENAMES").Value, VARCHAR2_TABLE_TYPE)

		p_zipfile = zipFile.Value
		Return vtt.Value

	End Function

End Class
