Add-Type -AssemblyName System.Drawing

$cell = 34
$margin = 60
$bmpW = 26 * $cell + 2 * $margin
$bmpH = 12 * $cell + 2 * $margin + 40
$bmp = New-Object System.Drawing.Bitmap $bmpW, $bmpH
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.Clear([System.Drawing.Color]::FromArgb(255, 245, 242, 235))

$green  = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(255, 142, 196, 106))
$blue   = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(255, 86, 148, 214))
$dark   = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(255, 88, 88, 96))
$white  = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::White)
$ink    = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(255, 30, 30, 30))
$red    = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(255, 214, 84, 84))
$yellow = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(255, 244, 200, 66))
$font   = [System.Drawing.Font]::new('Segoe UI', 9)
$fontB  = [System.Drawing.Font]::new('Segoe UI', 11, [System.Drawing.FontStyle]::Bold)
$pen    = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(255, 120, 120, 120))
$penRoute = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(255, 30, 30, 30), 3)
$penRoute.DashStyle = [System.Drawing.Drawing2D.DashStyle]::Dash

function CellRect([int]$cx, [int]$cy) {
    $row = 3 - $cy
    $px = [int]($margin + ($cx + 13) * $cell)
    $py = [int]($margin + $row * $cell)
    return [System.Drawing.Rectangle]::new($px, $py, $cell, $cell)
}

# Підлога і стіни по периметру
for ($y = 3; $y -ge -6; $y--) {
    for ($x = -13; $x -le 12; $x++) {
        $r = CellRect $x $y
        if ($y -eq 3 -or $y -eq -6 -or $x -eq -13 -or $x -eq 12) { $g.FillRectangle($dark, $r) }
        else { $g.FillRectangle($green, $r) }
        $g.DrawRectangle($pen, $r)
    }
}

# Вода: перешкода внизу посередині
$g.FillRectangle($blue, (CellRect 0 -3))
$g.FillRectangle($blue, (CellRect 0 -4))
$g.DrawRectangle($pen, (CellRect 0 -3))
$g.DrawRectangle($pen, (CellRect 0 -4))

# Маршрути ворогів (пунктирні рамки)
$rA = CellRect -12 -4; $rA.Width = 3 * $cell; $g.DrawRectangle($penRoute, $rA)   # A: x -12..-9
$rB = CellRect 3 0;    $rB.Height = 3 * $cell; $g.DrawRectangle($penRoute, $rB)  # B: y 0..2
$rD = CellRect 10 -3;  $rD.Height = 3 * $cell; $g.DrawRectangle($penRoute, $rD)  # D: y -3..-1
$rF = CellRect 6 0;    $rF.Width = 3 * $cell; $g.DrawRectangle($penRoute, $rF)   # F: x 6..8

function Mark([int]$cx, [int]$cy, [System.Drawing.SolidBrush]$brush, [string]$label) {
    $r = CellRect $cx $cy
    $g.FillEllipse($brush, ($r.X + 5), ($r.Y + 5), ($cell - 10), ($cell - 10))
    $sz = $g.MeasureString($label, $fontB)
    $g.DrawString($label, $fontB, $white, [single]($r.X + ($cell - $sz.Width) / 2), [single]($r.Y + ($cell - $sz.Height) / 2))
}

Mark -12 -1 $ink 'P'    # старт робота
Mark -10 -1 $ink 'N'    # NPC
Mark -12 -4 $ink 'A'    # ворог A
Mark 3 0 $ink 'B'       # ворог B
Mark 10 -3 $ink 'D'     # ворог D
Mark 7 0 $yellow 'F'    # швидкий ворог
Mark -6 -3 $white '+'   # аптечка
Mark 4 0 $white '+'     # велика аптечка (Amount 2)
Mark -1 -4 $red 'Z'     # зона шкоди
Mark 0 1 $red 'Z'       # зона шкоди

# Підписи ділянок
$g.DrawString('Ділянка 1: знайомство', $fontB, $ink, [single]($margin + 0 * $cell), [single]8)
$g.DrawString('Ділянка 2: практика (вода, зона)', $fontB, $ink, [single]($margin + 8 * $cell), [single]8)
$g.DrawString('Ділянка 3: перевірка (швидкий ворог)', $fontB, $ink, [single]($margin + 17 * $cell), [single]8)

# Легенда
$ly = $margin + 12 * $cell + 8
$g.FillRectangle($green, $margin, $ly, 20, 20); $g.DrawString('підлога', $font, $ink, [single]($margin + 24), [single]($ly + 1))
$g.FillRectangle($blue, ($margin + 110), $ly, 20, 20); $g.DrawString('вода (блокує)', $font, $ink, [single]($margin + 134), [single]($ly + 1))
$g.FillRectangle($dark, ($margin + 260), $ly, 20, 20); $g.DrawString('стіна', $font, $ink, [single]($margin + 284), [single]($ly + 1))
$g.DrawString('A/B/D — звичайні вороги; F — швидкий (жовтий); + — аптечка; Z — зона шкоди; N — NPC; пунктир — маршрут', $font, $ink, [single]($margin + 350), [single]($ly + 1))

New-Item -ItemType Directory -Force -Path 'D:\Robot-repair-Kudriatsev\Docs' | Out-Null
$bmp.Save('D:\Robot-repair-Kudriatsev\Docs\level-plan.png', [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()
Write-Host 'PLAN SAVED'
