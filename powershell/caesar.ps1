param(
    [Parameter(Position = 0, Mandatory)][int]   $Shift,
    [Parameter(Position = 1, Mandatory)][string] $File,
    [Alias('d')][switch]  $Decrypt,
    [Alias('o')][string]  $Output
)

$upper = [char[]]("ABCDEFGHIJKLMNOPQRSTUVWXYZ" + [char]0xC6 + [char]0xD8 + [char]0xC5)
$lower = [char[]]("abcdefghijklmnopqrstuvwxyz" + [char]0xE6 + [char]0xF8 + [char]0xE5)
$size  = $upper.Length  # 29

# Build index — lower indices stored as (-i - 1), mirroring the ~i trick
$idx = [System.Collections.Generic.Dictionary[char, int]]::new(58)
for ($i = 0; $i -lt $size; $i++) {
    $idx[$upper[$i]] = $i
    $idx[$lower[$i]] = -$i - 1
}

function Invoke-Cipher([string]$Text, [int]$n) {
    $n = (($n % $size) + $size) % $size
    $chars = $Text.ToCharArray()
    $pos   = 0
    for ($i = 0; $i -lt $chars.Length; $i++) {
        if ($idx.TryGetValue($chars[$i], [ref]$pos)) {
            $chars[$i] = if ($pos -ge 0) { $upper[($pos + $n) % $size] }
                         else            { $lower[(-$pos - 1 + $n) % $size] }
        }
    }
    return [string]::new($chars)
}

if ($Decrypt) { $Shift = -$Shift }

$text   = [System.IO.File]::ReadAllText($File, [System.Text.Encoding]::UTF8)
$result = Invoke-Cipher -Text $text -n $Shift

if ($Output) {
    [System.IO.File]::WriteAllText($Output, $result, [System.Text.Encoding]::UTF8)
} else {
    [Console]::OutputEncoding = [System.Text.Encoding]::UTF8
    [Console]::Write($result)
}
