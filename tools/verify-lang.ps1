$root = Split-Path -Parent $PSScriptRoot
$files = @{
    "zh-cn" = Join-Path $root "publish\zh-cn\GhostNote-zh-cn.exe"
    "zh-tw" = Join-Path $root "publish\zh-tw\GhostNote-zh-tw.exe"
    "en"    = Join-Path $root "publish\en\GhostNote-en.exe"
    "ja"    = Join-Path $root "publish\ja\GhostNote-ja.exe"
}

$probes = @{
    "zh-cn" = "隐藏水印"
    "zh-tw" = "隱藏浮水印"
    "en"    = "Hide watermark"
    "ja"    = "透かしを隠す"
}

function Test-ContainsUtf16([string]$path, [string]$text) {
    $bytes = [System.IO.File]::ReadAllBytes($path)
    $needle = [System.Text.Encoding]::Unicode.GetBytes($text)

    $limit = $bytes.Length - $needle.Length
    for ($i = 0; $i -le $limit; $i++) {
        if ($bytes[$i] -ne $needle[0]) { continue }
        $hit = $true
        for ($j = 1; $j -lt $needle.Length; $j++) {
            if ($bytes[$i + $j] -ne $needle[$j]) { $hit = $false; break }
        }
        if ($hit) { return $true }
    }
    return $false
}

foreach ($lang in $files.Keys) {
    $own = Test-ContainsUtf16 $files[$lang] $probes[$lang]
    $others = @()
    foreach ($other in $files.Keys) {
        if ($other -eq $lang) { continue }
        if (Test-ContainsUtf16 $files[$lang] $probes[$other]) { $others += $other }
    }
    $verdict = if ($own) { "OK" } else { "MISSING" }
    $leak = if ($others.Count -eq 0) { "无其它语言" } else { "混入: " + ($others -join ",") }
    Write-Output ("{0,-6} 本语言文案: {1,-8} {2}" -f $lang, $verdict, $leak)
}
