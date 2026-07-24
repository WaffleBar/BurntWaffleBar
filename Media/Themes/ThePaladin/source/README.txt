Drop raw ThePaladin icon sources here:
  ThePaladin_Collections.png
  ThePaladin_PVP.png
  … etc (see MOCKUPS.txt)

Then run:
  $in  = "...\Media\Themes\ThePaladin\source"
  $out = "...\Media\Themes\ThePaladin"
  $cs  = Get-Content "...\tools\ProcessThePaladinIcons.cs" -Raw
  Add-Type -TypeDefinition $cs -ReferencedAssemblies System.Drawing -IgnoreWarnings
  [ProcessThePaladinIcons]::ProcessAll($in, $out)
