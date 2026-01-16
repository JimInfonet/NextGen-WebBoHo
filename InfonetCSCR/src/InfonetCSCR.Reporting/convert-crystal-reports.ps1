$ErrorActionPreference = 'Stop'

$sameNameReports = @(
    'AC0100', 'AR0050', 'AR0051', 'AR0120', 'AR0130', 'AR0150',
    'ArchCC0200', 'ArchCOM0200', 'ArchHOT0200', 'ArchPC0200',
    'ArchPRO0200', 'ArchST0200', 'SA7012', 'SA7013', 'SA7014',
    'SA7016', 'SA7062', 'SA8000', 'SK1010', 'SK1011', 'SK1020',
    'SK1060', 'SK2010', 'SK2011', 'SK2013', 'SK2015', 'SK2020',
    'SK2021', 'SK2030', 'SK3010', 'SK3011', 'SK4045', 'SK5011',
    'SL0200', 'SL0202', 'SL0204'
)

$renamedReports = [ordered]@{
    'CC0200'  = 'CostChangeReport'
    'COM0200' = 'CommoditytransferReport'
    'EJ0001'  = 'ElectronicJournalReport'
    'EJ0002'  = 'ElectronicJournalSummaryReport'
    'EJ0003'  = 'ElectronicJournalDetailsReport'
    'FL0010'  = 'FuelPriceChangeReport'
    'HOT0200' = 'HotButtonsReport'
    'PC0200'  = 'PriceChangeReport'
    'PRO0200' = 'PromotionalDiscountTransferReport'
    'ST0200'  = 'StockTransferReport'
}

# CC0300 is the only Crystal definition without a historic RDLC counterpart.
# Its purchase-cost-change data is the same report family as CC0200, so use
# that established layout and bind it to the category report's runtime name.
$renamedReports['CC0300'] = 'CostChangeReport'

function Set-CscDataSource([string] $path) {
    [xml] $document = Get-Content -LiteralPath $path
    $namespace = $document.DocumentElement.NamespaceURI
    $manager = [Xml.XmlNamespaceManager]::new($document.NameTable)
    $manager.AddNamespace('r', $namespace)
    $manager.AddNamespace('rd', 'http://schemas.microsoft.com/SQLServer/reporting/reportdesigner')

    $dataSources = @($document.SelectNodes('//r:DataSources/r:DataSource', $manager))
    if ($dataSources.Count -eq 0) {
        throw "The report has no data source: $path"
    }

    $primary = $dataSources[0]
    $primary.SetAttribute('Name', 'CSCTransDataSet')
    foreach ($extra in $dataSources | Select-Object -Skip 1) {
        [void] $extra.ParentNode.RemoveChild($extra)
    }

    foreach ($querySource in $document.SelectNodes('//r:Query/r:DataSourceName', $manager)) {
        $querySource.InnerText = 'CSCTransDataSet'
    }
    foreach ($designerDataSet in $document.SelectNodes('//rd:DataSetInfo/rd:DataSetName', $manager)) {
        $designerDataSet.InnerText = 'CSCTransDataSet'
    }
    foreach ($schemaPath in $document.SelectNodes('//rd:DataSetInfo/rd:SchemaPath', $manager)) {
        $schemaPath.InnerText = '..\CSCTransDataSet.xsd'
    }
    foreach ($dataSet in $document.SelectNodes('//r:DataSets/r:DataSet', $manager)) {
        $dataSetInfo = $dataSet.SelectSingleNode('rd:DataSetInfo', $manager)
        if ($null -eq $dataSetInfo) {
            $dataSetInfo = $document.CreateElement('rd', 'DataSetInfo', 'http://schemas.microsoft.com/SQLServer/reporting/reportdesigner')
            [void] $dataSet.AppendChild($dataSetInfo)
        }
        $designerName = $dataSetInfo.SelectSingleNode('rd:DataSetName', $manager)
        if ($null -eq $designerName) {
            $designerName = $document.CreateElement('rd', 'DataSetName', 'http://schemas.microsoft.com/SQLServer/reporting/reportdesigner')
            [void] $dataSetInfo.AppendChild($designerName)
        }
        $designerName.InnerText = 'CSCTransDataSet'
        $schemaPath = $dataSetInfo.SelectSingleNode('rd:SchemaPath', $manager)
        if ($null -eq $schemaPath) {
            $schemaPath = $document.CreateElement('rd', 'SchemaPath', 'http://schemas.microsoft.com/SQLServer/reporting/reportdesigner')
            [void] $dataSetInfo.AppendChild($schemaPath)
        }
        $schemaPath.InnerText = '..\CSCTransDataSet.xsd'
    }

    $settings = [Xml.XmlWriterSettings]::new()
    $settings.Indent = $true
    $settings.IndentChars = '  '
    $settings.Encoding = [Text.UTF8Encoding]::new($false)
    $writer = [Xml.XmlWriter]::Create($path, $settings)
    try { $document.Save($writer) } finally { $writer.Dispose() }
}

foreach ($name in $sameNameReports) {
    $source = Join-Path 'BoReports' ($name + '.rdlc')
    $destination = Join-Path 'HoReports' ($name + '.rdlc')
    Copy-Item -LiteralPath $source -Destination $destination -Force
    Set-CscDataSource $destination
}

foreach ($entry in $renamedReports.GetEnumerator()) {
    $source = Join-Path 'BoReports' ($entry.Value + '.rdlc')
    $destination = Join-Path 'HoReports' ($entry.Key + '.rdlc')
    Copy-Item -LiteralPath $source -Destination $destination -Force
    Set-CscDataSource $destination

    if ($entry.Key -eq 'CC0300') {
        [xml] $document = Get-Content -LiteralPath $destination
        $manager = [Xml.XmlNamespaceManager]::new($document.NameTable)
        $manager.AddNamespace('r', $document.DocumentElement.NamespaceURI)
        foreach ($dataSet in $document.SelectNodes('//r:DataSets/r:DataSet', $manager)) {
            $oldName = $dataSet.GetAttribute('Name')
            $dataSet.SetAttribute('Name', 'CC0300')
            foreach ($binding in $document.SelectNodes("//r:DataSetName[text()='$oldName']", $manager)) {
                $binding.InnerText = 'CC0300'
            }
        }
        $document.Save((Resolve-Path -LiteralPath $destination))
    }
}
