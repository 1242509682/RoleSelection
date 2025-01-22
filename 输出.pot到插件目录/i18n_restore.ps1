param(
    [Parameter(Mandatory)]
    [string]$t
)

# ���� "dotnet tool restore" ��ʹ��GetText.Extractor���������
Write-Host "Running 'dotnet tool restore'..."
dotnet tool restore

# ��ȡ�ļ���չ��
$ext = [System.IO.Path]::GetExtension($t)

# ����ͨ��� *
if ($t -eq "*") {
    # ��ȡ���� .csproj �ļ�
    $projects = Get-ChildItem src/**/*.csproj -Recurse
    foreach ($p in $projects) {
        Write-Host "Processing project: $($p.FullName)"
        $pot = [System.IO.Path]::Combine($p.DirectoryName, "i18n", "template.pot")
        New-Item -Path $p.DirectoryName -Name i18n -ItemType Directory -Force
        dotnet tool run GetText.Extractor -u -o -s $p.FullName -t $pot
        Write-Host "�밴Enter����..."
        Read-Host
    }
}
# ������ .csproj �ļ�
elseif ((Test-Path $t) -and $ext -eq ".csproj") {
    Write-Host "Processing project: $t"
    $FullName = [System.IO.Path]::GetFullPath($t)
    $dirName = [System.IO.Path]::GetDirectoryName($t)
    $pot = [System.IO.Path]::Combine($dirName, "i18n", "template.pot")
    New-Item -Path $dirName -Name i18n -ItemType Directory -Force
    dotnet tool run GetText.Extractor -u -o -s $FullName -t $pot
    Write-Host "�밴Enter����..."
    Read-Host
}
else {
    Write-Host "csproj file path does not exist or is not a valid .csproj file!"
}