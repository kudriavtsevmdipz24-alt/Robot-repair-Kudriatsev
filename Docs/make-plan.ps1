Add-Type -AssemblyName System.Drawing

$bitmap = [Drawing.Bitmap]::new(1320, 655)
$graphics = [Drawing.Graphics]::FromImage($bitmap)
$graphics.SmoothingMode = [Drawing.Drawing2D.SmoothingMode]::AntiAlias
$graphics.Clear([Drawing.Color]::White)
$originX = 650
$originY = 284
$cell = 41

function Brush([string]$color) {
    return [Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml($color))
}

function Text([string]$caption, [float]$x, [float]$y, [float]$size, [string]$color, [switch]$Bold) {
    $weight = if ($Bold) { [Drawing.FontStyle]::Bold } else { [Drawing.FontStyle]::Regular }
    $font = [Drawing.Font]::new('Segoe UI', $size, $weight)
    $brush = Brush $color
    $graphics.DrawString($caption, $font, $brush, $x, $y)
    $brush.Dispose()
    $font.Dispose()
}

function TileRect([float]$x, [float]$y, [float]$width, [float]$height, [string]$color) {
    $brush = Brush $color
    $graphics.FillRectangle($brush, [single]($originX + $x * $cell), [single]($originY - ($y + $height) * $cell), [single]($width * $cell), [single]($height * $cell))
    $brush.Dispose()
}

function Marker([float]$x, [float]$y, [string]$caption, [string]$color) {
    $px = $originX + $x * $cell
    $py = $originY - $y * $cell
    $brush = Brush $color
    $graphics.FillEllipse($brush, [single]($px - 13), [single]($py - 13), [single]26, [single]26)
    $brush.Dispose()
    Text $caption ($px - 8) ($py - 12.5) 11 '#ffffff' -Bold
}

function Route([Drawing.Pen]$pen, [array]$points) {
    $graphics.DrawLines($pen, [Drawing.PointF[]]@(
        foreach ($point in $points) {
            [Drawing.PointF]::new($originX + $point[0] * $cell, $originY - $point[1] * $cell)
        }
    ))
}

Text 'Robot Repair: Timed Gate' 74 16 26 '#172d27' -Bold
Text 'План MyLevel  /  v2.1.0' 75 66 13 '#456059'
Text '1  Знайомство' 78 95 15 '#172d27' -Bold
Text '2  Практика' 395 95 15 '#172d27' -Bold
Text '3  Перевірка' 940 95 15 '#172d27' -Bold

TileRect -13.9 -5.35 7.9 9.35 '#e9f2e3'
TileRect -6 -5.35 11.15 9.35 '#e1eddb'
TileRect 5.15 -5.35 8.75 9.35 '#eaf2e5'
TileRect -13.9 3.85 27.8 .15 '#547477'
TileRect -13.9 -5.35 27.8 .15 '#547477'
TileRect -10.5 1.1 3 2.75 '#c1dbe3'
TileRect 4.7 -5.35 .9 2.85 '#485550'
TileRect 4.7 -.8 .9 2.75 '#485550'
TileRect 4.7 -2.5 .9 1.7 '#b64e16'
TileRect -1.17 -1.93 .94 .56 '#b64e16'
TileRect -2.02 -5 2.04 1.98 '#eab6a6'
TileRect 4.08 1.96 2.14 1.98 '#eab6a6'

$repair = [Drawing.Pen]::new([Drawing.ColorTranslator]::FromHtml('#41645c'), 2)
$repair.DashStyle = [Drawing.Drawing2D.DashStyle]::Dash
Route $repair @(@(-11.2,-2.8),@(-7.84,-2.8))
Route $repair @(@(-3.8,1.1),@(-3.8,2.8))
Route $repair @(@(1.4,-3.1),@(4.6,-3.1))
Route $repair @(@(10.6,1.2),@(10.6,2.8))
Route $repair @(@(8.6,-1.1),@(11.72,-1.1))
$repair.Dispose()

$direct = [Drawing.Pen]::new([Drawing.ColorTranslator]::FromHtml('#156e4c'), 4)
$direct.EndCap = [Drawing.Drawing2D.LineCap]::ArrowAnchor
Route $direct @(@(-.7,-1.65),@(7.25,-1.65))
$direct.Dispose()
$bypass = [Drawing.Pen]::new([Drawing.ColorTranslator]::FromHtml('#a73526'), 3)
$bypass.DashStyle = [Drawing.Drawing2D.DashStyle]::Dash
$bypass.EndCap = [Drawing.Drawing2D.LineCap]::ArrowAnchor
Route $bypass @(@(1.25,-1.2),@(1.25,2.85),@(7.35,2.85),@(7.35,1.0))
$bypass.Dispose()

Marker -12 -1 'Г' '#285a50'
Marker -10.5 -1 'N' '#285a50'
Marker -11.2 -2.8 '1' '#475a82'
Marker -3.8 1.1 '2' '#475a82'
Marker 1.4 -3.1 '3' '#9f491d'
Marker 8.6 -1.1 '4' '#a3343c'
Marker 10.6 1.2 '5' '#475a82'
Marker -6 -3 '+' '#387643'
Marker 3.4 .6 '+' '#387643'
Text 'Старт' 104 350 10 '#172d27'
Text 'Плита' 593 309 10 '#813612' -Bold
Text 'Двері' 891 373 10 '#813612' -Bold
Text 'Верхній обхід' 680 140 10 '#8a2b22' -Bold
Text 'Г — старт гравця    N — NPC    1–5 — роботи та їхні маршрути    + — аптечки' 76 515 14 '#172d27'
Text 'Суцільна стрілка — шлях через двері за 3,5 с. Пунктирна стрілка — верхній обхід через пастку.' 76 551 13 '#36534a'
Text 'Червоні клітини — зони шкоди, блакитні — вода.' 76 587 13 '#36534a'

$bitmap.Save((Join-Path $PSScriptRoot 'level-plan.png'), [Drawing.Imaging.ImageFormat]::Png)
$graphics.Dispose()
$bitmap.Dispose()
