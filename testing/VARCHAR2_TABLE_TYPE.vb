'------------------------------------------------------------------------------
' <auto-generated>
'     This code was generated by a tool.
'     Runtime Version:2.0.50727.1433
'
'     Changes to this file may cause incorrect behavior and will be lost if
'     the code is regenerated.
' </auto-generated>
'------------------------------------------------------------------------------

Option Strict Off
Option Explicit On

Imports Oracle.DataAccess.Client
Imports Oracle.DataAccess.Types
Imports System
Imports System.Xml.Schema
Imports System.Xml.Serialization


Public Class VARCHAR2_TABLE_TYPE
	Inherits [Object]
	Implements INullable, IOracleCustomType, IXmlSerializable

	Private m_IsNull As Boolean

	Private m_VARCHAR2_TABLE_TYPE() As String

	Private m_statusArray() As OracleUdtStatus

	Public Sub New()
		MyBase.New()
		'TODO : Add code to initialise the object
	End Sub

	Public Sub New(ByVal str As String)
		MyBase.New()
		'TODO : Add code to initialise the object based on the given string 
	End Sub

	Public Overridable ReadOnly Property IsNull() As Boolean Implements INullable.IsNull
		Get
			Return Me.m_IsNull
		End Get
	End Property

	Public Shared ReadOnly Property Null() As VARCHAR2_TABLE_TYPE
		Get
			Dim obj As VARCHAR2_TABLE_TYPE = New VARCHAR2_TABLE_TYPE
			obj.m_IsNull = True
			Return obj
		End Get
	End Property

	<OracleArrayMappingAttribute()> _
	Public Overridable Property Value() As String()
		Get
			Return Me.m_VARCHAR2_TABLE_TYPE
		End Get
		Set(ByVal value As String())
			Me.m_VARCHAR2_TABLE_TYPE = value
		End Set
	End Property

	Public Overridable Property StatusArray() As OracleUdtStatus()
		Get
			Return Me.m_statusArray
		End Get
		Set(ByVal value As OracleUdtStatus())
			Me.m_statusArray = value
		End Set
	End Property

	Public Overridable Sub FromCustomObject(ByVal con As Oracle.DataAccess.Client.OracleConnection, ByVal pUdt As System.IntPtr) Implements IOracleCustomType.FromCustomObject
		Dim objectStatusArray As Object = CType(m_statusArray, Object)
		OracleUdt.SetValue(con, pUdt, 0, Me.m_VARCHAR2_TABLE_TYPE, objectStatusArray)
	End Sub

	Public Overridable Sub ToCustomObject(ByVal con As Oracle.DataAccess.Client.OracleConnection, ByVal pUdt As System.IntPtr) Implements IOracleCustomType.ToCustomObject
		Dim objectStatusArray As Object = Nothing
		Me.m_VARCHAR2_TABLE_TYPE = CType(OracleUdt.GetValue(con, pUdt, 0, objectStatusArray), String())
		Me.m_statusArray = CType(objectStatusArray, OracleUdtStatus())
	End Sub

	Public Overridable Sub ReadXml(ByVal reader As System.Xml.XmlReader) Implements IXmlSerializable.ReadXml
		'TODO : Read Serialized Xml Data
	End Sub

	Public Overridable Sub WriteXml(ByVal writer As System.Xml.XmlWriter) Implements IXmlSerializable.WriteXml
		'TODO : Serialize object to xml data
	End Sub

	Public Overridable Function GetSchema() As XmlSchema Implements IXmlSerializable.GetSchema
		'TODO : Implement GetSchema
		Return Nothing
	End Function

	Public Overrides Function ToString() As String
		'TODO : Return a string that represents the current object
		Return ""
	End Function

	Public Shared Function Parse(ByVal str As String) As VARCHAR2_TABLE_TYPE
		'TODO : Add code needed to parse the string and get the object represented by the string
		Return New VARCHAR2_TABLE_TYPE
	End Function
End Class

'Factory to create an object for the above class
<OracleCustomTypeMappingAttribute("DAYNEO.VARCHAR2_TABLE_TYPE")> _
Public Class VARCHAR2_TABLE_TYPEFactory
	Inherits [Object]
	Implements IOracleCustomTypeFactory, IOracleArrayTypeFactory

	Public Overridable Function CreateObject() As IOracleCustomType Implements IOracleCustomTypeFactory.CreateObject
		Dim obj As VARCHAR2_TABLE_TYPE = New VARCHAR2_TABLE_TYPE
		Return obj
	End Function

	Public Overridable Function CreateArray(ByVal length As Integer) As System.Array Implements IOracleArrayTypeFactory.CreateArray
		Dim collElem((length) - 1) As [String]
		Return collElem
	End Function

	Public Overridable Function CreateStatusArray(ByVal length As Integer) As System.Array Implements IOracleArrayTypeFactory.CreateStatusArray
		Dim udtStatus((length) - 1) As OracleUdtStatus
		Return udtStatus
	End Function
End Class
