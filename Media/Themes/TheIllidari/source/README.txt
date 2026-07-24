Drop raw TheIllidari icon sources here:
  TheIllidari_Collections.png
  TheIllidari_PVP.png
  … etc.

Process:
  $in  = "...\Media\Themes\TheIllidari\source"
  $out = "...\Media\Themes\TheIllidari"
  $csc = "${env:WINDIR}\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
  & $csc /nologo /target:library /r:System.Drawing.dll /out:"...\tools\ProcessIllidari.dll" "...\tools\ProcessTheIllidariIcons.cs"
  Add-Type -Path "...\tools\ProcessIllidari.dll"
  [ProcessTheIllidariIcons]::ProcessAll($in, $out)
