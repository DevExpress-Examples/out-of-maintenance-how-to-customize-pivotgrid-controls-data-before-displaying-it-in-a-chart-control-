Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Reflection
Imports System.Windows.Controls
Imports System.Xml.Serialization
Imports DevExpress.Xpf.PivotGrid

Namespace DXPivotGrid_CustomizeChartData
	Partial Public Class MainPage
		Inherits UserControl
		Private dataFileName As String = "nwind.xml"
		Private productEncodeTable_Renamed As Dictionary(Of String, String)
		Private ReadOnly Property ProductEncodeTable() As Dictionary(Of String, String)
			Get
				If productEncodeTable_Renamed Is Nothing Then
					productEncodeTable_Renamed = New Dictionary(Of String, String)()
				End If
				Return productEncodeTable_Renamed
			End Get
		End Property
		Private categoryEncodeTable_Renamed As Dictionary(Of String, String)
		Private ReadOnly Property CategoryEncodeTable() As Dictionary(Of String, String)
			Get
				If categoryEncodeTable_Renamed Is Nothing Then
					categoryEncodeTable_Renamed = New Dictionary(Of String, String)()
				End If
				Return categoryEncodeTable_Renamed
			End Get
		End Property
		Private Sub CreateEncodeTables()
			Dim customerCounter As Integer = 1
			For Each value As Object In fieldCustomer.GetUniqueValues()
				ProductEncodeTable.Add(value.ToString(), "Customer" & customerCounter)
				customerCounter += 1
			Next value

			Dim countryCounter As Integer = 1
			For Each value As Object In fieldCountry.GetUniqueValues()
				CategoryEncodeTable.Add(value.ToString(), "Country" & countryCounter)
				countryCounter += 1
			Next value
		End Sub
		Public Sub New()
			InitializeComponent()

			' Parses an XML file and creates a collection of data items.
			Dim [assembly] As System.Reflection.Assembly = System.Reflection.Assembly.GetExecutingAssembly()
			Dim stream As Stream = [assembly].GetManifestResourceStream(dataFileName)
			Dim s As New XmlSerializer(GetType(OrderData))
			Dim dataSource As Object = s.Deserialize(stream)

			' Binds a pivot grid to this collection.
			pivotGridControl1.DataSource = dataSource

			pivotGridControl1.ChartProvideRowFieldValuesAsType = GetType(String)
			pivotGridControl1.ChartProvideColumnFieldValuesAsType = GetType(String)
			pivotGridControl1.ChartProvideCellValuesAsType = GetType(Integer)

			CreateEncodeTables()

			chartControl1.Diagram.SeriesDataMember = "Series"
			chartControl1.Diagram.SeriesTemplate.ArgumentDataMember = "Arguments"
			chartControl1.Diagram.SeriesTemplate.ValueDataMember = "Values"
			chartControl1.UpdateData()

			fieldCountry.CollapseAll()
			fieldCountry.ExpandValue("Austria")
		End Sub
		#Region "#CustomChartDataSourceData"
		Private Sub pivotGridControl1_CustomChartDataSourceData(ByVal sender As Object, _
				ByVal e As PivotCustomChartDataSourceDataEventArgs)
			If e.ItemType = PivotChartItemType.RowItem Then
				If Object.ReferenceEquals(e.FieldValueInfo.Field, fieldCountry) Then
					e.Value = CategoryEncodeTable(e.FieldValueInfo.Value.ToString())
				ElseIf Object.ReferenceEquals(e.FieldValueInfo.Field, fieldCustomer) Then
					Dim product As String = ProductEncodeTable(e.FieldValueInfo.Value.ToString())
					Dim category As String = _
						CategoryEncodeTable(e.FieldValueInfo.GetHigherLevelFieldValue(fieldCountry).ToString())
					e.Value = product & "["c & category & "]"c
				End If
			End If
			If e.ItemType = PivotChartItemType.ColumnItem Then
				If e.FieldValueInfo.ValueType = FieldValueType.GrandTotal Then
					e.Value = "Total Freight"
				End If
			End If
			If e.ItemType = PivotChartItemType.CellItem Then
				e.Value = Math.Round(Convert.ToDecimal(e.CellInfo.Value), 0)
			End If
		End Sub
		#End Region ' #CustomChartDataSourceData
	End Class
End Namespace
