Module OraUtils

	Public Function CleanPLSQL(ByVal cmdText As String) As String

		Return cmdText.Replace(vbCr, "")

	End Function

End Module
