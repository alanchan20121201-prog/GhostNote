Add-Type -AssemblyName System.Drawing

$outDir = "c:\Users\alanc\Desktop\GhostNote\src\GhostNote\Assets"
if (-not (Test-Path $outDir)) { New-Item -ItemType Directory -Path $outDir | Out-Null }

$sizes = @(16, 24, 32, 48, 64, 128)
$frames = @()

function New-GhostBitmap([int]$s) {
    $bmp = New-Object System.Drawing.Bitmap($s, $s, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.Clear([System.Drawing.Color]::Transparent)

    $h = [double]$s
    $pad = $h * 0.09
    $w = $h - 2 * $pad

    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $domeH = $h * 0.70
    $bodyBottom = $h * 0.84

    $path.AddArc([single]$pad, [single]$pad, [single]$w, [single]$domeH, 180, 180)
    $path.AddLine([single]($pad + $w), [single]($pad + $domeH / 2), [single]($pad + $w), [single]$bodyBottom)

    $seg = $w / 3.0
    $dip = $h * 0.09
    $xRight = $pad + $w
    for ($i = 0; $i -lt 3; $i++) {
        $x0 = $xRight - $seg
        $path.AddBezier(
            [single]$xRight, [single]$bodyBottom,
            [single]($xRight - $seg * 0.36), [single]($bodyBottom + $dip * 2.1),
            [single]($x0 + $seg * 0.36), [single]($bodyBottom + $dip * 2.1),
            [single]$x0, [single]$bodyBottom)
        $xRight = $x0
    }
    $path.AddLine([single]$pad, [single]$bodyBottom, [single]$pad, [single]($pad + $domeH / 2))
    $path.CloseFigure()

    $bodyBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 234, 238, 245))
    $g.FillPath($bodyBrush, $path)

    $eyeBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 22, 24, 30))
    $eyeW = $w * 0.16
    $eyeH = $h * 0.20
    $eyeY = $h * 0.38
    $g.FillEllipse($eyeBrush, [single]($pad + $w * 0.23), [single]$eyeY, [single]$eyeW, [single]$eyeH)
    $g.FillEllipse($eyeBrush, [single]($pad + $w * 0.77 - $eyeW), [single]$eyeY, [single]$eyeW, [single]$eyeH)

    $bodyBrush.Dispose()
    $eyeBrush.Dispose()
    $path.Dispose()
    $g.Dispose()
    return $bmp
}

function Get-DibBytes([System.Drawing.Bitmap]$bmp) {
    $w = $bmp.Width
    $h = $bmp.Height
    $rect = New-Object System.Drawing.Rectangle(0, 0, $w, $h)
    $data = $bmp.LockBits($rect, [System.Drawing.Imaging.ImageLockMode]::ReadOnly, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $stride = $data.Stride
    $buffer = [byte[]]::new($stride * $h)
    [System.Runtime.InteropServices.Marshal]::Copy($data.Scan0, $buffer, 0, $buffer.Length)
    $bmp.UnlockBits($data)

    $ms = New-Object System.IO.MemoryStream
    $bw = New-Object System.IO.BinaryWriter($ms)

    # BITMAPINFOHEADER
    $bw.Write([UInt32]40)
    $bw.Write([Int32]$w)
    $bw.Write([Int32]($h * 2))
    $bw.Write([UInt16]1)
    $bw.Write([UInt16]32)
    $bw.Write([UInt32]0)
    $bw.Write([UInt32]($w * $h * 4))
    $bw.Write([Int32]0)
    $bw.Write([Int32]0)
    $bw.Write([UInt32]0)
    $bw.Write([UInt32]0)

    # XOR 位图：自下而上
    for ($y = $h - 1; $y -ge 0; $y--) {
        $bw.Write([byte[]]$buffer, $y * $stride, $w * 4)
    }

    # AND 掩码：全 0（靠 alpha 通道透明）
    $maskStride = [int]([Math]::Floor(($w + 31) / 32) * 4)
    $maskAll = [byte[]]::new($maskStride * $h)
    $bw.Write([byte[]]$maskAll, 0, $maskAll.Length)

    $bw.Flush()
    $bytes = $ms.ToArray()
    $bw.Dispose()
    $ms.Dispose()
    return , $bytes
}

foreach ($s in $sizes) {
    $bmp = New-GhostBitmap $s
    $frames += ,@($s, (Get-DibBytes $bmp))
    $bmp.Dispose()
}

$count = $frames.Count
$offset = 6 + 16 * $count

$fs = New-Object System.IO.FileStream((Join-Path $outDir "GhostNote.ico"), [System.IO.FileMode]::Create)
$bw = New-Object System.IO.BinaryWriter($fs)

$bw.Write([UInt16]0)
$bw.Write([UInt16]1)
$bw.Write([UInt16]$count)

foreach ($frame in $frames) {
    $sz = $frame[0]
    $data = $frame[1]
    $wb = if ($sz -ge 256) { 0 } else { $sz }
    $bw.Write([Byte]$wb)
    $bw.Write([Byte]$wb)
    $bw.Write([Byte]0)
    $bw.Write([Byte]0)
    $bw.Write([UInt16]1)
    $bw.Write([UInt16]32)
    $bw.Write([UInt32]$data.Length)
    $bw.Write([UInt32]$offset)
    $offset += $data.Length
}
foreach ($frame in $frames) { $bw.Write($frame[1]) }
$bw.Flush()
$bw.Close()
$fs.Close()

Write-Output ("ICO generated: " + (Get-Item (Join-Path $outDir "GhostNote.ico")).Length + " bytes")
